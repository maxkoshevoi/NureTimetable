using NureTimetable.Models;
using NureTimetable.Models.Consts;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Xamarin.Forms;

namespace NureTimetable.ViewModels
{
    public class EventList
    {
        public List<Event> Events { get; } = new List<Event>();
        
        public EventList()
        { }

        public EventList(string cistCsvDataForOneGroup)
        {
            Events = Parse(cistCsvDataForOneGroup);
        }

        public EventList(List<Event> events)
        {
            Events = events;
        }

        #region Statistics
        public int Count => Events.Count;

        public IEnumerable<string> Lessons()
            => Events.Select(e => e.Lesson).Distinct();

        public IEnumerable<string> Rooms()
            => Events.Select(e => e.Room).Distinct();

        public IEnumerable<string> EventTypes()
            => Events.Select(e => e.Type).Distinct();

        public IEnumerable<string> EventTypes(string lessonName)
            => Events
                .Where(e => e.Lesson == lessonName)
                .Select(e => e.Type)
                .Distinct();

        public DateTime StartDate()
            => Events.Min(e => e.Start.Date);

        public DateTime EndDate()
            => Events.Max(e => e.End.Date);

        public TimeSpan StartTime()
            => Events.Min(e => e.Start.TimeOfDay);

        public TimeSpan EndTime()
            => Events.Max(e => e.End.TimeOfDay);
        #endregion

        #region static
        public static List<Event> Parse(string cistCsvDataForOneGroup)
            => Parse(cistCsvDataForOneGroup, false).Values.FirstOrDefault() ?? new List<Event>();

        public static Dictionary<string, List<Event>> Parse(string cistCsvData, bool isManyGroups)
        {
            string defaultKey = string.Empty;
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
                    string[] eventDescription = rawEvent[0].Split(' ');

                    /*
                     * Data structure
                     * 
                     * 0  - Тема ([Group_Name - ]Lesson Type Room...)
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

                    Event ev = new Event
                    {
                        Lesson = eventDescription[isManyGroups ? 2 : 0],
                        Type = eventDescription[isManyGroups ? 3 : 1],
                        Room = eventDescription[isManyGroups ? 4 : 2],

                        Start = DateTime.ParseExact(rawEvent[1], "dd.MM.yyyy", CultureInfo.InvariantCulture)
                                    .Add(TimeSpan.ParseExact(rawEvent[2], "hh\\:mm\\:ss", CultureInfo.InvariantCulture)),
                        End = DateTime.ParseExact(rawEvent[3], "dd.MM.yyyy", CultureInfo.InvariantCulture)
                                    .Add(TimeSpan.ParseExact(rawEvent[4], "hh\\:mm\\:ss", CultureInfo.InvariantCulture)),
                    };
                    string groupName = defaultKey;
                    if (isManyGroups)
                    {
                        groupName = eventDescription[0];
                    }

                    if (!timetables.ContainsKey(groupName))
                    {
                        timetables.Add(groupName, new List<Event>());
                    }
                    timetables[groupName].Add(ev);
                }
                catch (Exception ex)
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        MessagingCenter.Send(Application.Current, MessageTypes.ExceptionOccurred, ex);
                    });
                }
            }
            return timetables;
        }
        #endregion
    }
}
