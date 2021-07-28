using NureTimetable.DAL.Legacy.Models;
using NureTimetable.DAL.Legacy.Models.Consts;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;

namespace NureTimetable.DAL.Legacy
{
#pragma warning disable CS8600, CS8603, CS8604 // Possible null reference
    [Obsolete("", true)]
    static class EventsDataStore
    {
        public static TimetableInfo GetTimetableFromCist(DateTime dateStart, DateTime dateEnd, int groupID) =>
            GetTimetableFromCist(dateStart, dateEnd, new Group { ID = groupID })?.FirstOrDefault();

        public static List<TimetableInfo> GetTimetableFromCist(DateTime dateStart, DateTime dateEnd, params Group[] groups)
        {
            List<Group> groupsAllowed = groups.ToList(); //SettingsDataStore.CheckCistTimetableUpdateRights(groups);

            using (var client = new WebClient())
            {
                client.Encoding = Encoding.GetEncoding("Windows-1251");
                try
                {
                    List<TimetableInfo> timetables = new List<TimetableInfo>(); //GetTimetableLocal(groupsAllowed.ToArray());

                    // Getting events
                    Uri uri = new Uri(Urls.CistGroupTimetableUrl(Urls.CistTimetableFormat.Csv, dateStart, dateEnd, groupsAllowed.Select(g => g.ID).ToArray()));
                    string data = client.DownloadString(uri);
                    Dictionary<string, List<Event>> newEvents = ParseCistCsvTimetable(data, groupsAllowed.Count > 1);
                    if (newEvents == null)
                    {
                        // Parsing error
                        return null;
                    }

                    // Updating events and adding new timetables
                    foreach (var group in groupsAllowed)
                    {
                        var groupEvents = new List<Event>();
                        if (newEvents.Keys.Contains(group.Name))
                        {
                            groupEvents = newEvents[group.Name];
                        }
                        else if (groupsAllowed.Count == 1 && newEvents.Count == 1)
                        {
                            groupEvents = newEvents.Values.First();
                        }
                        TimetableInfo groupTimetable = timetables.FirstOrDefault(tt => tt.Group.ID == group.ID);
                        if (groupTimetable == null)
                        {
                            groupTimetable = new TimetableInfo(group);
                            timetables.Add(groupTimetable);
                        }
                        groupTimetable.Events = groupEvents;
                    }

                    // Updating lesson info if needed
                    List<Group> groupsLessonInfoAllowed = groupsAllowed; //SettingsDataStore.CheckCistLessonsInfoUpdateRights(groupsAllowed.ToArray());
                    if (groupsLessonInfoAllowed.Any())
                    {
                        uri = new Uri(Urls.CistGroupTimetableUrl(Urls.CistTimetableFormat.Xls, DateTime.Now, dateEnd, groupsLessonInfoAllowed.Select(g => g.ID).ToArray()));
                        data = client.DownloadString(uri);
                        Dictionary<string, List<LessonInfo>> groupsLessons = ParseCistXlsLessonInfo(data, groupsLessonInfoAllowed.ToArray());
                        if (groupsLessons != null)
                        {
                            foreach (var group in groupsLessonInfoAllowed)
                            {
                                TimetableInfo timetable = timetables.First(tt => tt.Group.ID == group.ID);
                                // Updating lesson info excluding lesson settings
                                foreach (var newLessonInfo in groupsLessons[group.Name])
                                {
                                    LessonInfo oldLessonInfo = timetable.LessonsInfo.FirstOrDefault(li => li.ShortName == newLessonInfo.ShortName);
                                    if (oldLessonInfo == null)
                                    {
                                        oldLessonInfo = new LessonInfo();
                                        timetable.LessonsInfo.Add(oldLessonInfo);
                                    }
                                    oldLessonInfo.ShortName = newLessonInfo.ShortName;
                                    oldLessonInfo.LongName = newLessonInfo.LongName;
                                    oldLessonInfo.EventTypesInfo = newLessonInfo.EventTypesInfo;
                                    oldLessonInfo.LastUpdated = DateTime.Now;
                                }
                            }
                        }
                    }

                    // Updating LastUpdated for saved groups 
                    //List<SavedGroup> AllSavedGroups = GroupsDataStore.GetSaved();
                    //foreach (var group in AllSavedGroups)
                    //{
                    //    if (groupsAllowed.Exists(g => g.ID == group.ID))
                    //    {
                    //        group.LastUpdated = DateTime.Now;
                    //    }
                    //}
                    //GroupsDataStore.UpdateSaved(AllSavedGroups);

                    // Saving timetables
                    //foreach (var newTimetable in timetables)
                    //{
                    //    UpdateTimetableLocal(newTimetable);
                    //    MessagingCenter.Send(Application.Current, MessageTypes.TimetableUpdated, newTimetable.Group.ID);
                    //}

                    return timetables;
                }
                catch (Exception)
                {
                    //ExceptionService.LogException(ex);
                }
            }
            return null;
        }

        #region Parsers
        public static List<Event> ParseCistCsvTimetable(string cistCsvDataForOneGroup) => 
            ParseCistCsvTimetable(cistCsvDataForOneGroup, false)?.Values.FirstOrDefault();

        public static Dictionary<string, List<Event>> ParseCistCsvTimetable(string cistCsvData, bool isManyGroups)
        {
            string defaultKey = "default";
            IEnumerable<string> rawData = cistCsvData
                .Split(new char[] { '\r' }, StringSplitOptions.RemoveEmptyEntries)
                .Skip(1); // Skipping headers

            var timetables = new Dictionary<string, List<Event>>();
            foreach (string eventStr in rawData)
            {
                try
                {
                    string[] rawEvent = eventStr
                        .Split(new string[] { "\",\"" }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(str => str.Trim('"'))
                        .ToArray();

                    /*
                     * Data structure
                     * 
                     * 0  - Тема ([Group_Name - ]Lesson Type Room[, Room2]; [Lesson2...; ][LessonN...])
                     * 1  - Дата начала 
                     * 2  - Время начала 
                     * 3  - Дата завершения 
                     * 4  - Время завершения 
                     * 5  - Ежедневное событие 
                     * 6  - Оповещение вкл / выкл 
                     * 7  - Дата оповещения 
                     * 8  - Время оповещения    
                     * 9  - В это время 
                     * 10 - Важность    
                     * 11 - Описание 
                     * 12 - Пометка
                    */

                    string[] concurrentEventsList = rawEvent[0].Split(new string[] { "; " }, StringSplitOptions.RemoveEmptyEntries);

                    string groupName = defaultKey;
                    if (isManyGroups)
                    {
                        groupName = concurrentEventsList[0].Remove(concurrentEventsList[0].IndexOf(' '));
                        concurrentEventsList[0] = concurrentEventsList[0][(concurrentEventsList[0].IndexOf(" - ") + 3)..];
                    }

                    foreach (string eventDescriptionStr in concurrentEventsList)
                    {
                        List<string> eventDescription = eventDescriptionStr.Split(' ').ToList();

                        // Checking for lessons with spaces
                        int typeIndex = -1;
                        for (int i = eventDescription.Count - 1; i >= 0; i--)
                        {
                            if (KnownEventTypes.Values.Contains(eventDescription[i].ToLower()))
                            {
                                typeIndex = i;
                                break;
                            }
                        }
                        while (typeIndex > 1)
                        {
                            eventDescription[0] += $" {eventDescription[1]}";
                            eventDescription.RemoveAt(1);
                            typeIndex--;
                        }

                        // Checking for multiple rooms
                        while (eventDescription[2].EndsWith(",") && eventDescription.Count > 2)
                        {
                            eventDescription[2] += $" {eventDescription[3]}";
                            eventDescription.RemoveAt(3);
                        }

                        Event ev = new Event
                        {
                            Lesson = eventDescription[0],
                            Type = eventDescription[1],
                            Room = eventDescription[2],

                            Start = DateTime.ParseExact(rawEvent[1], "dd.MM.yyyy", CultureInfo.InvariantCulture)
                                        .Add(TimeSpan.ParseExact(rawEvent[2], "hh\\:mm\\:ss", CultureInfo.InvariantCulture)),
                            End = DateTime.ParseExact(rawEvent[3], "dd.MM.yyyy", CultureInfo.InvariantCulture)
                                        .Add(TimeSpan.ParseExact(rawEvent[4], "hh\\:mm\\:ss", CultureInfo.InvariantCulture)),
                        };

                        if (!timetables.ContainsKey(groupName))
                        {
                            timetables.Add(groupName, new List<Event>());
                        }
                        timetables[groupName].Add(ev);
                    }
                }
                catch (Exception)
                {
                    //ExceptionService.LogException(ex);
                    return null;
                }
            }
            return timetables;
        }

        public static Dictionary<string, List<LessonInfo>> ParseCistXlsLessonInfo(string cistXlsTimetableData, params Group[] searchGroups)
        {
            var groupsLessons = new Dictionary<string, List<LessonInfo>>();
            if (searchGroups.Length == 0)
            {
                return groupsLessons;
            }
            
            try
            {
                // Setting default values
                foreach (string searchGroup in searchGroups.Select(sg => sg.Name))
                {
                    if (string.IsNullOrEmpty(searchGroup))
                    {
                        throw new ArgumentNullException(nameof(searchGroups));
                    }
                    groupsLessons.Add(searchGroup, new List<LessonInfo>());
                }

                cistXlsTimetableData = cistXlsTimetableData[cistXlsTimetableData.IndexOf("\n\n")..];
                List<string> timetableInfoRaw = cistXlsTimetableData
                    .Split(new string[] { @"ss:Type=""String"">" }, StringSplitOptions.RemoveEmptyEntries)
                    .Skip(1)
                    .ToList();

                for (int i = 0; i < timetableInfoRaw.Count; i += 2)
                {
                    List<string> infoRaw = timetableInfoRaw[i + 1]
                        .Remove(timetableInfoRaw[i + 1].IndexOf("</Data>"))
                        .Split(new string[] { ": ", "\n" }, StringSplitOptions.RemoveEmptyEntries)
                        .ToList();

                    string lessonShortName = timetableInfoRaw[i].Remove(timetableInfoRaw[i].IndexOf("</Data>"));
                    string lessonLongName = infoRaw[0].Trim();
                    foreach (string groupName in groupsLessons.Keys)
                    {
                        groupsLessons[groupName].Add(new LessonInfo
                        {
                            ShortName = lessonShortName,
                            LongName = lessonLongName
                        });
                    }

                    foreach (string eventTypeInfoRaw in infoRaw.Skip(1))
                    {
                        List<string> eventTypeInfo = eventTypeInfoRaw.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                        if (eventTypeInfo.Count <= 4)
                        {
                            // Event type doesn't have teacher
                            continue;
                        }
                        
                        // Checking for groups with spaces
                        while (!eventTypeInfo[3].EndsWith(",") && eventTypeInfo.Count > 3)
                        {
                            eventTypeInfo[3] += $" {eventTypeInfo[4]}";
                            eventTypeInfo.RemoveAt(4);
                        }

                        foreach (string groupName in groupsLessons.Keys)
                        {
                            if (!IsGroupsListFromXlsContains(eventTypeInfo[3], groupName))
                            {
                                // Teachers for different groups
                                continue;
                            }
                            LessonInfo lessonInfo = groupsLessons[groupName].First(li => li.ShortName == lessonShortName);

                            string type = eventTypeInfo[0];
                            string teacher = $"{eventTypeInfo[4]} {eventTypeInfo[5]}{eventTypeInfo[6]}".TrimEnd(',');
                                
                            EventTypeInfo eventType = lessonInfo.EventTypesInfo.FirstOrDefault(et => et.Name == type);
                            if (eventType == null)
                            {
                                eventType = new EventTypeInfo
                                {
                                    Name = type
                                };
                                lessonInfo.EventTypesInfo.Add(eventType);
                            }
                            if (!eventType.Teachers.Contains(teacher))
                            {
                                eventType.Teachers.Add(teacher);
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                //ExceptionService.LogException(ex);
                groupsLessons = null;
            }
            return groupsLessons;
        }

        private static bool IsGroupsListFromXlsContains(string groupsStr, string searchGroup)
        {
            //*РNet(ПЗПІ-16-)-2,
            //*РNet(ПЗПІ-16-)-1;*РNet(ПЗПІ-16-)-2,
            if (groupsStr.Contains("("))
            {
                return true;
            }

            //ПЗПІ-16-5,
            //ПЗПІ-16-4,5,6,7,8,
            //ПЗПІи-16-4;ПЗПІ-16-4,5,6,7,8,
            foreach (string groupSection in groupsStr.Split(';'))
            {
                List<string> groupList = groupSection.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                if (groupList[0] == searchGroup)
                {
                    return true;
                }
                else if (groupList.Count == 1)
                {
                    continue;
                }
                string groupTemplate = groupList[0].Remove(groupList[0].LastIndexOf('-') + 1);
                foreach (string groupLeftPart in groupList.Skip(1))
                {
                    if (groupTemplate + groupLeftPart == searchGroup)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
        #endregion
    }
#pragma warning restore CS8600, CS8603, CS8604 // Possible null reference
}