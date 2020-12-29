using Microsoft.AppCenter.Analytics;
using NureTimetable.Core.Localization;
using NureTimetable.Core.Models.Consts;
using NureTimetable.DAL.Models.Local;
using Plugin.Calendars;
using Plugin.Calendars.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Calendar = Plugin.Calendars.Abstractions.Calendar;

namespace NureTimetable.BL
{
    public static class CalendarService
    {
        public static async Task<bool> AddEvent(Event ev, int eventNumber, int eventsCount)
        {
            PermissionStatus? readStatus = null;
            PermissionStatus? writeStatus = null;
            try
            {
                (readStatus, writeStatus) = await RequestPermissions();
                if (readStatus != PermissionStatus.Granted || writeStatus != PermissionStatus.Granted)
                {
                    return false;
                }

                Calendar targetCalendar = await GetCalendar();
                if (targetCalendar is null)
                {
                    return false;
                }

                Analytics.TrackEvent("Add To Calendar");

                CalendarEvent calendarEvent = GetCalendarEvent(ev, eventNumber, eventsCount);
                await CrossCalendars.Current.AddOrUpdateEventAsync(targetCalendar, calendarEvent);
            }
            catch (Exception ex)
            {
                ex.Data.Add("Read Status", readStatus?.ToString());
                ex.Data.Add("Write Status", writeStatus?.ToString());
                MessagingCenter.Send(Application.Current, MessageTypes.ExceptionOccurred, ex);

                return false;
            }

            return true;
        }

        private static CalendarEvent GetCalendarEvent(Event ev, int eventNumber, int eventsCount)
        {
            CalendarEvent calendarEvent = new()
            {
                Start = ev.StartUtc,
                End = ev.EndUtc,
                Name = $"{ev.Lesson.ShortName} ({ev.Type.ShortName} {eventNumber}/{eventsCount})",
                Description = $"{string.Format(LN.EventClassroom, ev.RoomName)}\n" +
                    $"{string.Format(LN.EventTeachers, string.Join(", ", ev.Teachers.Select(t => t.Name)))}\n" +
                    $"{string.Format(LN.EventGroups, string.Join(", ", ev.Groups.Select(t => t.Name)))}\n",
                Location = $"KHNURE -\"{ev.RoomName}\"",
                Reminders = new CalendarEventReminder[]
                {
                    new()
                    {
                        Method = CalendarReminderMethod.Alert,
                        TimeBefore = TimeSpan.FromMinutes(30)
                    }
                }
            };
            return calendarEvent;
        }

        private static async Task<(PermissionStatus read, PermissionStatus write)> RequestPermissions()
        {
            PermissionStatus readStatus = await Permissions.CheckStatusAsync<Permissions.CalendarRead>();
            PermissionStatus writeStatus = await Permissions.CheckStatusAsync<Permissions.CalendarWrite>();
            if (readStatus != PermissionStatus.Granted)
            {
                readStatus = await Permissions.RequestAsync<Permissions.CalendarRead>();
            }
            if (writeStatus != PermissionStatus.Granted && readStatus == PermissionStatus.Granted)
            {
                writeStatus = await Permissions.RequestAsync<Permissions.CalendarWrite>();
            }
            return (readStatus, writeStatus);
        }

        private static async Task<Calendar> GetCalendar()
        {
            const string customCalendarName = "NURE Timetable";

            // Getting Calendar list
            IList<Calendar> calendars = await CrossCalendars.Current.GetCalendarsAsync();
            calendars = calendars
                .Where(c => c.Name.ToLower() == c.AccountName.ToLower() || c.AccountName.ToLower() == customCalendarName.ToLower())
                .ToList();

            // Getting our custom calendar
            bool isCustomCalendarExists = true;
            Calendar customCalendar = calendars
                .Where(c => c.AccountName.ToLower() == customCalendarName.ToLower())
                .FirstOrDefault();
            if (customCalendar is null)
            {
                isCustomCalendarExists = false;
                customCalendar = new Calendar
                {
                    Name = customCalendarName
                };
                calendars.Add(customCalendar);
            }
            else if (calendars.Where(c => c.AccountName == customCalendar.AccountName).Count() > 1)
            {
                MessagingCenter.Send(Application.Current, MessageTypes.ExceptionOccurred, new IndexOutOfRangeException($"There are {calendars.Where(c => c.AccountName == customCalendar.AccountName).Count()} calendars with AccountName {customCalendar.AccountName}"));
            }

            // Getting calendar to add event into
            Calendar targetCalendar = customCalendar;
            if (calendars.Count > 1)
            {
                string targetCalendarName = await Shell.Current.DisplayActionSheet(
                    LN.ChooseCalendar,
                    LN.Cancel,
                    null,
                    calendars.Select(c => c.Name).ToArray());

                if (string.IsNullOrEmpty(targetCalendarName) || targetCalendarName == LN.Cancel)
                {
                    return null;
                }
                targetCalendar = calendars.First(c => c.Name == targetCalendarName);
            }

            if (!isCustomCalendarExists && targetCalendar == customCalendar)
            {
                await CrossCalendars.Current.AddOrUpdateCalendarAsync(customCalendar);
            }

            return targetCalendar;
        }
    }
}
