using NureTimetable.BL;
using NureTimetable.Core.Extensions;
using NureTimetable.Core.Localization;
using NureTimetable.DAL.Helpers;
using NureTimetable.DAL.Models.Local;
using NureTimetable.UI.Helpers;
using Rg.Plugins.Popup.Services;
using Shiny;
using Shiny.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.Forms;

namespace NureTimetable.UI.ViewModels.Timetable
{
    public class EventPopupViewModel : BaseViewModel
    {
        public Event Event { get; }
        
        public int EventNumber { get; }

        public int EventsCount { get; }

        public string Details { get; }

        public string Notes { get; }

        public IAsyncCommand ClosePopupCommand { get; }
        
        public IAsyncCommand AddToCalendarCommand { get; }

        public IAsyncCommand AddNotificationCommand { get; }

        public EventPopupViewModel(Event ev, TimetableInfoList timetable)
        {
            Event = ev;

            ClosePopupCommand = CommandHelper.Create(ClosePopup);
            AddToCalendarCommand = CommandHelper.Create(AddEventToCalendar);
            AddNotificationCommand = CommandHelper.Create(AddNotification);

            LessonInfo lessonInfo = timetable.LessonsInfo.FirstOrDefault(li => li.Lesson.Equals(ev.Lesson));
            Notes = lessonInfo?.Notes;

            EventNumber = timetable.Events
                .Where(e => e.Lesson == ev.Lesson && e.Type == ev.Type && e.Start < ev.Start)
                .DistinctBy(e => e.Start)
                .Count() + 1;
            EventsCount = timetable.Events
                .Where(e => e.Lesson == ev.Lesson && e.Type == ev.Type)
                .DistinctBy(e => e.Start)
                .Count();

            Details = $"{string.Format(LN.EventType, ev.Type.FullName)} ({EventNumber}/{EventsCount})\n" +
              $"{string.Format(LN.EventClassroom, ev.RoomName)}\n" +
              $"{string.Format(LN.EventTeachers, string.Join(", ", ev.Teachers.Select(t => t.Name)))}\n" +
              $"{string.Format(LN.EventGroups, string.Join(", ", ev.Groups.Select(t => t.Name)))}\n" +
              $"{string.Format(LN.EventDay, ev.Start.ToString("ddd, dd.MM.yy"))}\n" +
              $"{string.Format(LN.EventTime, ev.Start.ToString("HH:mm"), ev.End.ToString("HH:mm"))}";
        }

        private async Task AddNotification()
        {
            TimeSpan delay = TimeSpan.FromMinutes(10);
            var calendarEvent = CalendarService.GenerateCalendarEvent(Event, EventNumber, EventsCount);
            int notificationId = Event.GetHashCode();

            INotificationManager notificationManager = ShinyHost.Resolve<INotificationManager>();
            var pending = await notificationManager.GetPending();
            
            if (pending.Any(n => n.Id == notificationId))
            {
                await Shell.Current.DisplayAlert("Notification", "Already set", LN.Ok);
                return;
            }

            Notification notification = new()
            {
                Id = notificationId,
                Title = calendarEvent.Name,
                Message = calendarEvent.Description,
                Payload = new Dictionary<string, string> { { "Event", Serialisation.ToJson(Event) } },
                //ScheduleDate = DateTimeOffset.UtcNow.Add(Event.Start - DateTime.Now - delay)
                ScheduleDate = DateTimeOffset.UtcNow.AddSeconds(2)
            };
            await notificationManager.Send(notification);

            await ClosePopup();
        }

        private async Task AddEventToCalendar()
        {
            if (!await CalendarService.RequestPermissions())
            {
                await Shell.Current.DisplayAlert(LN.AddingToCalendarTitle, LN.InsufficientRights, LN.Ok);
                return;
            }

            var calendar = await CalendarService.GetCalendar();
            if (calendar is null)
            {
                // User didn't choose calendar
                return;
            }

            var calendarEvent = CalendarService.GenerateCalendarEvent(Event, EventNumber, EventsCount);
            bool isAdded = await CalendarService.AddOrUpdateEvent(calendar, calendarEvent);

            string message = isAdded ? LN.AddingEventToCalendarSuccess : LN.AddingEventToCalendarFail;
            await Shell.Current.DisplayAlert(LN.AddingToCalendarTitle, message, LN.Ok);
            
            await ClosePopup();
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
