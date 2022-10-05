using Microsoft.AppCenter.Analytics;
using NureTimetable.BL;
using NureTimetable.Core.Extensions;
using NureTimetable.Core.Localization;
using NureTimetable.DAL.Models;
using NureTimetable.DAL.Settings;
using NureTimetable.UI.Views;
using Rg.Plugins.Popup.Services;
using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace NureTimetable.UI.ViewModels;

public class EventPopupViewModel : BaseViewModel
{
    public Event Event { get; }
    public LessonInfo LessonInfo { get; }
    public TimetableInfo? Timetable { get; }
    public ITimetablePageCommands TimetablePage { get; }

    public int EventNumber { get; }

    public int EventsCount { get; }

    public string Details { get; }

    public string? Notes { get; }

    public IAsyncCommand OptionsCommand { get; }

    public EventPopupViewModel(Event ev, TimetableInfoList timetables, ITimetablePageCommands timetablePage)
    {
        Event = ev;
        TimetablePage = timetablePage;
        if (timetables.Timetables.Count == 1)
        {
            Timetable = timetables.Timetables.Single();
            LessonInfo = Timetable.GetAndAddLessonsInfo(ev.Lesson);
        }
        else
        {
            LessonInfo = new(ev.Lesson);
        }

        EventNumber = timetables.Events
            .Where(e => e.Lesson == ev.Lesson && e.Type == ev.Type && e.Start < ev.Start)
            .DistinctBy(e => e.Start)
            .Count() + 1;
        EventsCount = timetables.Events
            .Where(e => e.Lesson == ev.Lesson && e.Type == ev.Type)
            .DistinctBy(e => e.Start)
            .Count();
        Details = $"{string.Format(LN.EventType, ev.Type.FullName)} ({EventNumber}/{EventsCount})\n" +
          $"{string.Format(LN.EventClassroom, ev.RoomName)}\n" +
          $"{string.Format(LN.EventTeachers, string.Join(", ", ev.Teachers.Select(t => t.Name)))}\n" +
          $"{string.Format(LN.EventGroups, string.Join(", ", Event.Groups.Select(t => t.Name).GroupBasedOnLastPart()))}\n" +
          $"{string.Format(LN.EventDay, ev.Start.ToString("ddd, dd.MM.yy"))}\n" +
          $"{string.Format(LN.EventTime, ev.Start.ToString("HH:mm"), ev.End.ToString("HH:mm"))}";
        Notes = LessonInfo.Notes?.Trim();

        OptionsCommand = CommandFactory.Create(ShowOptions, allowsMultipleExecutions: false);
    }

    private async Task ShowOptions()
    {
        List<string> options = new()
        {
            LN.AddToCalendar
        };
        if (Timetable != null)
        {
            options.Add(LN.LessonManagement);
            options.Add(LN.LessonInfo);
        }
        if (SettingsRepository.Settings.DlNureUser != null)
        {
            options.Add(LN.OpenAttendance);
        }

        string result = await Shell.Current.DisplayActionSheet(LN.ChooseAction, LN.Cancel, null, options.ToArray());
        if (result == null || result == LN.Cancel)
        {
            return;
        }

        if (result == LN.LessonManagement)
        {
            await ClosePopup();
            await Navigation.PushAsync(new LessonSettingsPage
            {
                BindingContext = new LessonSettingsViewModel(LessonInfo, Timetable!, true)
            });
        }
        else if (result == LN.LessonInfo)
        {
            await ClosePopup();
            await Navigation.PushAsync(new LessonInfoPage
            {
                BindingContext = new LessonInfoViewModel(LessonInfo, Timetable!)
            });
        }
        else if (result == LN.AddToCalendar)
        {
            await AddEventToCalendarAsync();
        }
        else if (result == LN.OpenAttendance)
        {
            await OpenAttendanceAsync();
        }
    }

    private async Task AddEventToCalendarAsync()
    {
        if (!await CalendarService.RequestPermissionsAsync())
        {
            await Shell.Current.DisplayAlert(LN.AddingToCalendarTitle, LN.InsufficientRights, LN.Ok);
            return;
        }

        var calendar = await CalendarService.GetCalendarAsync();
        if (calendar == null)
        {
            // User didn't choose calendar
            return;
        }

        var calendarEvent = CalendarService.GenerateCalendarEvent(Event, EventNumber, EventsCount, Notes);
        bool isAdded = await CalendarService.AddOrUpdateEventAsync(calendar, calendarEvent);
        if (!isAdded)
        {
            await Shell.Current.CurrentPage.DisplayAlert(LN.AddingToCalendarTitle, LN.AddingEventToCalendarFail, LN.Ok);
            return;
        }
        TimetablePage.DisplayToastAsync(LN.AddingEventToCalendarSuccess).Forget();

        await ClosePopup();
    }

    private async Task OpenAttendanceAsync()
    {
        Analytics.TrackEvent("Moodle: Open attendance");

        var (attendanceUrl, errorMessage) = await DlNureService.GetAttendanceUrlAsync(LessonInfo.Lesson, Timetable);
        if (attendanceUrl == null)
        {
            await Shell.Current.DisplayAlert(LN.SomethingWentWrong, errorMessage!, LN.Ok);
            return;
        }

        await Browser.OpenAsync(attendanceUrl);
    }

    private static async Task ClosePopup()
    {
        if (PopupNavigation.Instance.PopupStack.Any())
        {
            await PopupNavigation.Instance.PopAsync();
        }
    }
}
