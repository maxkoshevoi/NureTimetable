using NureTimetable.BL;
using NureTimetable.Core.Extensions;
using NureTimetable.Core.Localization;
using NureTimetable.DAL.Models.Local;
using NureTimetable.UI.Helpers;
using NureTimetable.UI.ViewModels.Lessons;
using NureTimetable.UI.ViewModels.Lessons.LessonSettings;
using NureTimetable.UI.Views;
using Rg.Plugins.Popup.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.CommunityToolkit.Extensions;
using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.Forms;

namespace NureTimetable.UI.ViewModels
{
    public class EventPopupViewModel : BaseViewModel
    {
        public Event Event { get; }
        public LessonInfo LessonInfo { get; }
        public TimetableInfo Timetable { get; }

        public int EventNumber { get; }

        public int EventsCount { get; }

        public string Details { get; }

        public string Notes { get; }
        
        public IAsyncCommand OptionsCommand { get; }

        public EventPopupViewModel(Event ev, TimetableInfoList timetables)
        {
            Event = ev;
            Timetable = timetables.Timetables.SingleOrDefault();
            LessonInfo = timetables.LessonsInfo.FirstOrDefault(li => li.Lesson == ev.Lesson) ?? new() { Lesson = ev.Lesson };

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
              $"{string.Format(LN.EventGroups, string.Join(", ", Event.Groups.Select(t => t.Name).GroupBasedOnLastPart("-")))}\n" +
              $"{string.Format(LN.EventDay, ev.Start.ToString("ddd, dd.MM.yy"))}\n" +
              $"{string.Format(LN.EventTime, ev.Start.ToString("HH:mm"), ev.End.ToString("HH:mm"))}";
            Notes = LessonInfo.Notes?.Trim();

            OptionsCommand = CommandFactory.Create(ShowOptions);
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

            string result = await Shell.Current.DisplayActionSheet(LN.ChooseAction , LN.Cancel, null, options.ToArray());
            if (result == null || result == LN.Cancel)
            {
                return;
            }

            if (result == LN.LessonManagement)
            {
                await ClosePopup();
                await Navigation.PushAsync(new LessonSettingsPage
                {
                    BindingContext = new LessonSettingsViewModel(LessonInfo, Timetable, true)
                });
            }
            else if (result == LN.LessonInfo)
            {
                await ClosePopup();
                await Navigation.PushAsync(new LessonInfoPage
                {
                    BindingContext = new LessonInfoViewModel(LessonInfo, Timetable)
                });
            }
            else if(result == "Add to calendar")
            {
                await AddEventToCalendar();
            }
        }

        private async Task AddEventToCalendar()
        {
            if (!await CalendarService.RequestPermissions())
            {
                await Shell.Current.DisplayAlert(LN.AddingToCalendarTitle, LN.InsufficientRights, LN.Ok);
                return;
            }

            var calendar = await CalendarService.GetCalendar();
            if (calendar == null)
            {
                // User didn't choose calendar
                return;
            }

            var calendarEvent = CalendarService.GenerateCalendarEvent(Event, EventNumber, EventsCount);
            bool isAdded = await CalendarService.AddOrUpdateEvent(calendar, calendarEvent);
            if (!isAdded)
            {
                await Shell.Current.CurrentPage.DisplayAlert(LN.AddingToCalendarTitle, LN.AddingEventToCalendarFail, LN.Ok);
                return;
            }

            // Displaying toast and closing popup at the same time
            await Task.WhenAll(
                Shell.Current.CurrentPage.DisplayToastAsync(LN.AddingEventToCalendarSuccess), 
                ClosePopup()
            );
        }

        private static async Task ClosePopup()
        {
            if (PopupNavigation.Instance.PopupStack.Any())
            {
                await PopupNavigation.Instance.PopAsync();
            }
        }
    }
}
