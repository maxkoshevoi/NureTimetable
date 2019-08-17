using System;
using System.Collections.Generic;
using System.Linq;

namespace NureTimetable.DAL.Models.Local
{
    public class TimetableInfo
    {
        private SavedEntity entity = new SavedEntity();
        public SavedEntity Entity
        {
            get => entity;
            set => entity = value ?? throw new NullReferenceException($"Attemt to set {Entity} to null");
        }

        private List<Event> events = new List<Event>();
        public List<Event> Events
        {
            get => events;
            set => events = value ?? throw new NullReferenceException($"Attemt to set {Events} to null");
        }

        private List<LessonInfo> lessonsInfo = new List<LessonInfo>();
        public List<LessonInfo> LessonsInfo
        {
            get => lessonsInfo;
            set => lessonsInfo = value ?? throw new NullReferenceException($"Attemt to set {LessonsInfo} to null");
        } 

        public TimetableInfo()
        { }
        
        public TimetableInfo(SavedEntity entity)
        {
            Entity = entity;
        }

        #region Statistics
        public int Count => Events.Count;

        public IEnumerable<Lesson> Lessons()
            => Events.Select(e => e.Lesson).Distinct();

        public IEnumerable<string> Rooms()
            => Events.Select(e => e.RoomName).Distinct();

        public IEnumerable<EventType> EventTypes()
            => Events.Select(e => e.Type).Distinct();

        public IEnumerable<EventType> EventTypes(long lessonId)
            => Events
                .Where(e => e.Lesson.ID == lessonId)
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
        
        public void ApplyLessonSettings()
        {
            foreach (LessonInfo lInfo in LessonsInfo.Where(ls => ls.Settings.IsSomeSettingsApplied))
            {
                // Hidding settings
                if (lInfo.Settings.Hiding.ShowLesson == false)
                {
                    Events.RemoveAll(ev => ev.Lesson == lInfo.Lesson);
                }
                else if (lInfo.Settings.Hiding.ShowLesson == null)
                {
                    Events.RemoveAll(ev => ev.Lesson == lInfo.Lesson && lInfo.Settings.Hiding.HideOnlyThisEventTypes.Contains(ev.Type.ID));
                }
            }
        }
    }
}
