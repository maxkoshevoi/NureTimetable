using System;
using System.Collections.Generic;
using System.Linq;

namespace NureTimetable.DAL.Models.Local
{
    public class TimetableInfo : TimetableStatistics
    {
        private SavedEntity entity = new SavedEntity();
        public SavedEntity Entity
        {
            get => entity;
            set => entity = value ?? throw new NullReferenceException($"Attemt to set {Entity} to null");
        }
        
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
                    Events.RemoveAll(ev => ev.Lesson == lInfo.Lesson && 
                        (lInfo.Settings.Hiding.EventTypesToHide.Contains(ev.Type.ID) || 
                        lInfo.Settings.Hiding.TeachersToHide.Intersect(ev.Teachers.Select(t => t.ID)).Any())
                    );
                }
            }
        }
    }
}
