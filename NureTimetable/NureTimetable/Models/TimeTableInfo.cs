using NureTimetable.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NureTimetable.ViewModels
{
    public class TimetableInfo
    {
        private Group group = new Group();
        public Group Group
        {
            get => group;
            set => group = value ?? throw new NullReferenceException($"Attemt to set {Group} to null");
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
        
        public TimetableInfo(Group group)
        {
            Group = group;
        }

        #region Event extensions
        public List<string> GetLessonTeachers(string lessonShortName, string eventType)
            => LessonsInfo
                .FirstOrDefault(li => li.ShortName == lessonShortName)?
                .EventTypesInfo.FirstOrDefault(et => et.Name == eventType)?
                .Teachers ?? new List<string>();

        public string GetLessonLongName(string lessonShortName)
            => LessonsInfo
                .FirstOrDefault(li => li.ShortName == lessonShortName)?
                .LongName;
        #endregion

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
        
        public void ApplyLessonSettings()
        {
            foreach (LessonInfo lInfo in LessonsInfo.Where(ls => ls.Settings.IsSomeSettingsApplied))
            {
                // Hidding settings
                if (lInfo.Settings.Hiding.ShowLesson == false)
                {
                    Events.RemoveAll(ev => ev.Lesson == lInfo.ShortName);
                }
                else if (lInfo.Settings.Hiding.ShowLesson == null)
                {
                    Events.RemoveAll(ev => ev.Lesson == lInfo.ShortName && lInfo.Settings.Hiding.HideOnlyThisEventTypes.Contains(ev.Type));
                }
            }
        }

        public void UpdateLessonsInfo(List<LessonInfo> newLessonsInfo)
        {
            foreach (LessonInfo newLessonInfo in newLessonsInfo)
            {
                LessonInfo oldLessonInfo = LessonsInfo.FirstOrDefault(li => li.ShortName == newLessonInfo.ShortName);
                if (oldLessonInfo == null)
                {
                    oldLessonInfo = new LessonInfo();
                    LessonsInfo.Add(oldLessonInfo);
                }
                oldLessonInfo.ShortName = newLessonInfo.ShortName;
                oldLessonInfo.LongName = newLessonInfo.LongName;
                oldLessonInfo.EventTypesInfo = newLessonInfo.EventTypesInfo;
            }
        }
    }
}
