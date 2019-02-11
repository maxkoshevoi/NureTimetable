using NureTimetable.Helpers;
using NureTimetable.Models;
using NureTimetable.Models.Consts;
using NureTimetable.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using Xamarin.Forms;

namespace NureTimetable.DAL
{
    public static class TimetableDataStore
    {
        public static EventList GetEvents(int groupID, bool tryUpdate = false, DateTime? dateStart = null, DateTime? dateEnd = null)
        {
            EventList eventList;
            if (tryUpdate)
            {
                if (dateStart == null || dateEnd == null)
                {
                    throw new ArgumentNullException("dateStart and dateEnd should be set");
                }

                eventList = GetEventsFromCist(groupID, dateStart.Value, dateEnd.Value);
                if (eventList != null)
                {
                    return eventList;
                }
            }
            eventList = GetEventsLocal(groupID);
            return eventList;
        }

        public static EventList GetEventsFromCist(int groupID, DateTime dateStart, DateTime dateEnd)
        {
            using (var client = new WebClient())
            {
                client.Encoding = Encoding.GetEncoding("Windows-1251");
                try
                {
                    Uri uri = new Uri(string.Format(Urls.CistTimetableUrlPattern, groupID, dateStart.ToString("dd.MM.yyyy"), dateEnd.ToString("dd.MM.yyyy")));
                    string data = client.DownloadString(uri);
                    var events = new EventList(data);
                    Serialisation.ToJsonFile(events.Events, FilePath.SavedTimetableFilename(groupID));
                    
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        MessagingCenter.Send(Application.Current, MessageTypes.TimetableUpdated, groupID);
                    });
                    return events;
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
            var events = new EventList();
            events.Events.AddRange(Serialisation.FromJsonFile<List<Event>>(FilePath.SavedTimetableFilename(groupID)) ?? new List<Event>());
            return events;
        }

        public static EventList GetEventsFromCistCsv(string filePath)
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