using System;
using System.Collections.Generic;
using System.Linq;

namespace NureTimetable.DAL.Models
{
    public class TimetableStatistics
    {
        private protected List<Event> events = new();

        #region Statistics
        public IEnumerable<Lesson> Lessons() =>
            events.Select(e => e.Lesson).Distinct();

        public IEnumerable<string> Rooms() =>
            events.Select(e => e.RoomName).Distinct();

        public IEnumerable<EventType> EventTypes() =>
            events.Select(e => e.Type).Distinct();

        public IEnumerable<EventType> EventTypes(long lessonId) =>
            events.Where(e => e.Lesson.ID == lessonId)
                .Select(e => e.Type)
                .Distinct();

        public IEnumerable<Teacher> Teachers() =>
            events.SelectMany(e => e.Teachers).Distinct();

        public IEnumerable<Teacher> Teachers(long lessonId) =>
            events.Where(e => e.Lesson.ID == lessonId)
                .SelectMany(e => e.Teachers)
                .Distinct();

        public DateTime StartDate() =>
            events.Min(e => e.Start.Date);

        public DateTime EndDate() =>
            events.Max(e => e.End.Date);

        public TimeSpan StartTime() =>
            events.Min(e => e.Start.TimeOfDay);

        public TimeSpan EndTime() =>
            events.Max(e => e.End.TimeOfDay);

        public Event? CurrentEvent() =>
            events.FirstOrDefault(e => e.Start <= DateTime.Now && e.End >= DateTime.Now);

        public Event? NextEvent() => events
            .Where(e => e.Start > DateTime.Now)
            .OrderBy(e => e.Start)
            .FirstOrDefault();
        #endregion
    }
}
