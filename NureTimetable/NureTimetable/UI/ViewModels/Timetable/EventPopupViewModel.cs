using NureTimetable.BL;
using NureTimetable.Core.Extensions;
using NureTimetable.Core.Localization;
using NureTimetable.DAL.Models.Local;
using NureTimetable.UI.Helpers;
using Rg.Plugins.Popup.Services;
using System;
using System.Linq;
using System.Threading.Tasks;
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

        public Command ClosePopupCommand { get; }
        
        public Command AddToCalendarCommand { get; }

        public EventPopupViewModel(Event ev, TimetableInfoList timetable)
        {
            Event = ev;

            ClosePopupCommand = CommandHelper.Create(async () => await PopupNavigation.Instance.PopAsync());
            AddToCalendarCommand = CommandHelper.Create(AddEventToCalendar);

            LessonInfo lessonInfo = timetable.LessonsInfo?.FirstOrDefault(li => li.Lesson == ev.Lesson);
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

        private async Task AddEventToCalendar()
        {
            bool isAdded = await CalendarService.AddEvent(Event, EventNumber, EventsCount);

            string message = isAdded ? LN.AddingEventToCalendarSuccess : LN.AddingEventToCalendarFail;
            await Shell.Current.DisplayAlert(LN.AddingToCalendarTitle, message, LN.Ok);
        }
    }
}
