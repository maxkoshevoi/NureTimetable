using NureTimetable.Core.Localization;
using NureTimetable.DAL.Models;
using Xamarin.CommunityToolkit.Helpers;

namespace NureTimetable.UI.ViewModels;

public class LessonInfoViewModel : BaseViewModel
{
    private readonly TimetableInfo timetableInfo;

    #region Properties
    public LessonInfo LessonInfo { get; }

    public LocalizedString Statistics { get; }
    #endregion

    public LessonInfoViewModel(LessonInfo lessonInfo, TimetableInfo timetableInfo)
    {
        LessonInfo = lessonInfo;
        this.timetableInfo = timetableInfo;
        Statistics = new(GetStatistics);

        LocalizationResourceManager.Current.PropertyChanged += (_, _) => OnPropertyChanged(nameof(Statistics));
    }

    private string GetStatistics()
    {
        IEnumerable<Event> events = timetableInfo.Events.Where(e => e.Lesson == LessonInfo.Lesson);

        var statForTypes = timetableInfo.EventTypes(LessonInfo.Lesson.ID).OrderBy(et => et.ShortName).Select(et =>
        {
            var eventsWithType = events.Where(e => e.Type == et).ToList();
            return $"{et.ShortName}:\n" +
                $"- {LN.EventsTotal} {eventsWithType.Count}, {eventsWithType.Count(e => e.Start > DateTime.Now)} {LN.EventsLeft}\n" +
                $"- {LN.NextEvent}: {eventsWithType.FirstOrDefault(e => e.Start > DateTime.Now)?.Start.ToShortDateString() ?? "-"}\n" +
                $"- {LN.Teachers}: {string.Join(", ", eventsWithType.SelectMany(e => e.Teachers).Distinct().Select(t => t.ShortName).OrderBy(tn => tn).DefaultIfEmpty("-"))}";
        });
        return string.Join("\n", statForTypes);
    }
}
