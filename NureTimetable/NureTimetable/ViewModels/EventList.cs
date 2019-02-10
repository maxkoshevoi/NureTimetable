using NureTimetable.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace NureTimetable.ViewModels
{
    public class EventList
    {
        public List<Event> Events = new List<Event>();
        
        public EventList()
        { }

        public EventList(string cistCsvData)
        {
            Events = Parse(cistCsvData);
        }

        #region Statistics
        public int Count => Events.Count;

        public IEnumerable<string> Lessons()
            => Events.Select(e => e.Lesson).Distinct();

        public IEnumerable<string> Rooms()
            => Events.Select(e => e.Room).Distinct();

        public IEnumerable<string> EventTypes()
            => Events.Select(e => e.Type).Distinct();

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
        protected static List<Event> Parse(string cistCsvData)
        {
            var rawData = cistCsvData
                .Split(new char[] { '\r' }, StringSplitOptions.RemoveEmptyEntries)
                .Skip(1); // Skipping headers

            List<Event> events = new List<Event>();
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
                     * 0  - Тема (Lesson Type Room...)
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
                        Lesson = eventDescription[0],
                        Type = eventDescription[1],
                        Room = eventDescription[2],

                        Start = DateTime.ParseExact(rawEvent[1], "dd.MM.yyyy", CultureInfo.InvariantCulture)
                                    .Add(TimeSpan.ParseExact(rawEvent[2], "hh\\:mm\\:ss", CultureInfo.InvariantCulture)),
                        End = DateTime.ParseExact(rawEvent[3], "dd.MM.yyyy", CultureInfo.InvariantCulture)
                                    .Add(TimeSpan.ParseExact(rawEvent[4], "hh\\:mm\\:ss", CultureInfo.InvariantCulture)),
                    };
                    events.Add(ev);
                }
                catch (Exception ex)
                {
                    // Log error
                }
            }
            return events;
        }
        #endregion
    }
}
