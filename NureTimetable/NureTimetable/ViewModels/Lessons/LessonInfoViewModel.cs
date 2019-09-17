using NureTimetable.DAL.Models.Local;
using NureTimetable.ViewModels.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms;

namespace NureTimetable.ViewModels.Lessons
{
    public class LessonInfoViewModel : BaseViewModel
    {
        #region Variables
        private string _title;
        private TimetableInfo timetableInfo;
        #endregion

        #region Properties
        public string Title { get => _title; set => SetProperty(ref _title, value); }

        public LessonInfo LessonInfo { get; }

        public string Statistics { get; }
        #endregion

        public LessonInfoViewModel(INavigation navigation, LessonInfo lessonInfo, TimetableInfo timetableInfo) : base(navigation)
        {
            LessonInfo = lessonInfo;
            this.timetableInfo = timetableInfo;

            Title = lessonInfo.Lesson.FullName;
            Statistics = GetStatistics(timetableInfo.Events.Where(e => e.Lesson == lessonInfo.Lesson));
        }
        
        private string GetStatistics(IEnumerable<Event> events)
        {
            var statForTypes = timetableInfo.EventTypes(LessonInfo.Lesson.ID).OrderBy(et => et.ShortName).Select(et =>
            {
                var eventsWithType = events.Where(e => e.Type == et).ToList();
                return $"{et.ShortName}:\n" +
                    $"- {eventsWithType.Count} classes total, {eventsWithType.Where(e => e.Start > DateTime.Now).DefaultIfEmpty().Count()} left\n" +
                    $"- Next: {eventsWithType.Where(e => e.Start > DateTime.Now).FirstOrDefault()?.Start.Date.ToShortDateString() ?? "-" }\n" +
                    $"- Teachers: {string.Join(", ", eventsWithType.SelectMany(e => e.Teachers).Distinct().Select(t => t.ShortName).OrderBy(tn => tn).DefaultIfEmpty("-"))}";
            });
            return string.Join("\n", statForTypes);
        }
    }
}
