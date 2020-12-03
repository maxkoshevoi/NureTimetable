using NureTimetable.Core.Localization;
using NureTimetable.DAL.Models.Local;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NureTimetable.UI.ViewModels.Lessons
{
    public class LessonInfoViewModel : BaseViewModel
    {
        #region Variables
        private readonly TimetableInfo timetableInfo;
        #endregion

        #region Properties
        public LessonInfo LessonInfo { get; }

        public string Statistics => GetStatistics();
        #endregion

        public LessonInfoViewModel(LessonInfo lessonInfo, TimetableInfo timetableInfo)
        {
            LessonInfo = lessonInfo;
            this.timetableInfo = timetableInfo;

            Title = LN.LessonInfo;
        }
        
        private string GetStatistics()
        {
            IEnumerable<Event> events = timetableInfo.Events.Where(e => e.Lesson == LessonInfo.Lesson);

            var statForTypes = timetableInfo.EventTypes(LessonInfo.Lesson.ID).OrderBy(et => et.ShortName).Select(et =>
            {
                var eventsWithType = events.Where(e => e.Type == et).ToList();
                return $"{et.ShortName}:\n" +
                    $"- {LN.EventsTotal} {eventsWithType.Count}, {eventsWithType.Where(e => e.Start > DateTime.Now).Count()} {LN.EventsLeft}\n" +
                    $"- {LN.NextEvent}: {eventsWithType.Where(e => e.Start > DateTime.Now).FirstOrDefault()?.Start.Date.ToShortDateString() ?? "-" }\n" +
                    $"- {LN.Teachers}: {string.Join(", ", eventsWithType.SelectMany(e => e.Teachers).Distinct().Select(t => t.ShortName).OrderBy(tn => tn).DefaultIfEmpty("-"))}";
            });
            return string.Join("\n", statForTypes);
        }
    }
}
