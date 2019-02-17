using NureTimetable.Helpers;
using NureTimetable.Models;
using NureTimetable.Models.Consts;
using NureTimetable.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Xamarin.Forms;

namespace NureTimetable.DataStores
{
    public static class EventsDataStore
    {
        /// <summary>
        /// Returns events for one group. Null if error occurs 
        /// </summary>
        public static EventList GetEvents(int groupID, bool tryUpdate = false, DateTime? dateStart = null, DateTime? dateEnd = null)
        {
            EventList eventList;
            if (tryUpdate)
            {
                if (dateStart == null || dateEnd == null)
                {
                    throw new ArgumentNullException("dateStart and dateEnd should be set");
                }

                eventList = GetEventsFromCist(dateStart.Value, dateEnd.Value, groupID);
                if (eventList != null)
                {
                    return eventList;
                }
            }
            eventList = GetEventsLocal(groupID);
            return eventList;
        }
        
        public static bool CheckUpdateTimetableFromCistRights()
        {
            TimeSpan? timePass = DateTime.Now - SettingsDataStore.GetLastTimetableUpdate();
            if (timePass != null && timePass <= Config.TimetableManualUpdateMinInterval)
            {
                return false;
            }
            return true;
        }

        public static EventList GetEventsFromCist(DateTime dateStart, DateTime dateEnd, int groupID)
            => GetEventsFromCist(dateStart, dateEnd, new Group { ID = groupID }).Values.FirstOrDefault();

        public static Dictionary<int, EventList> GetEventsFromCist(DateTime dateStart, DateTime dateEnd, params Group[] groups)
        {
            if (groups == null || groups.Length == 0 || CheckUpdateTimetableFromCistRights() == false)
            {
                return null;
            }

            using (var client = new WebClient())
            {
                client.Encoding = Encoding.GetEncoding("Windows-1251");
                try
                {
                    var timetables = new Dictionary<int, EventList>();
                    Uri uri = new Uri(Urls.CistTimetableUrl(dateStart, dateEnd, groups.Select(g => g.ID).ToArray()));
                    string data = client.DownloadString(uri);

                    SettingsDataStore.UpdateLastTimetableUpdate();

                    if (groups.Length == 1)
                    {
                        List<Event> events = EventList.Parse(data);
                        if (events == null)
                        {
                            return null;
                        }
                        timetables.Add(groups[0].ID, new EventList(events));
                    }
                    else
                    {
                        Dictionary<string, List<Event>> timetablesStr = EventList.Parse(data, true);
                        foreach (Group group in groups)
                        {
                            List<Event> groupEvents = new List<Event>();
                            if (timetablesStr.Keys.Contains(group.Name))
                            {
                                groupEvents = timetablesStr[group.Name];
                            }
                            timetables.Add(group.ID, new EventList(groupEvents));
                        }
                    }
                    foreach (int groupID in timetables.Keys)
                    {
                        SerializationHelper.ToJsonFile(timetables[groupID].Events, FilePath.SavedTimetable(groupID));
                        Device.BeginInvokeOnMainThread(() =>
                        {
                            MessagingCenter.Send(Application.Current, MessageTypes.TimetableUpdated, groupID);
                        });
                    }

                    return timetables;
                }
                catch (Exception ex)
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        MessagingCenter.Send(Application.Current, MessageTypes.ExceptionOccurred, ex);
                    });
                }
            }
            return null;
        }

        public static EventList GetEventsLocal(int groupID)
        {
            List<Event> localEvents = SerializationHelper.FromJsonFile<List<Event>>(FilePath.SavedTimetable(groupID));
            if (localEvents == null)
            {
                return null;
            }
            var events = new EventList(localEvents);
            return events;
        }

        public static EventList GetEventsFromCistCsv(string filePath, bool isMultipleGroups = false)
        {
            if (!File.Exists(filePath))
            {
                return null;
            }

            try
            {
                string data = File.ReadAllText(filePath, Encoding.GetEncoding(1251));
                var events = new EventList(data);
                return events;
            }
            catch (Exception ex)
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    MessagingCenter.Send(Application.Current, MessageTypes.ExceptionOccurred, ex);
                });
            }
            return null;
        }
    }
}