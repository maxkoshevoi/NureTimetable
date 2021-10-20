using System;
using System.Collections.Generic;
using System.Linq;

namespace NureTimetable.DAL.Models
{
    public class TimetableInfo : TimetableStatistics
    {
        public Entity Entity { get; }

        public List<Event> Events
        {
            get => events;
            set => events = value;
        }

        public List<LessonInfo> LessonsInfo { get; set; } = new();

        public TimetableInfo(Entity entity)
        {
            Entity = entity ?? throw new ArgumentNullException(nameof(entity));
        }

        public void ApplyLessonSettings()
        {
            foreach (var lInfo in LessonsInfo.Where(ls => ls.Settings.IsSomeSettingsApplied))
            {
                // Hiding settings
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
