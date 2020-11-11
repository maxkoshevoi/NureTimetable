using Microsoft.AppCenter.Analytics;
using NureTimetable.Core.Extensions;
using NureTimetable.Core.Localization;
using NureTimetable.Core.Models;
using NureTimetable.Core.Models.Consts;
using NureTimetable.Core.Models.InterplatformCommunication;
using NureTimetable.DAL;
using NureTimetable.DAL.Models.Local;
using NureTimetable.Models.Consts.Fonts;
using NureTimetable.UI.Helpers;
using NureTimetable.UI.Views;
using Plugin.Calendars;
using Syncfusion.SfSchedule.XForms;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Calendars = Plugin.Calendars.Abstractions;

namespace NureTimetable.UI.ViewModels.Timetable
{
    public class TimetableViewModel : BaseViewModel
    {
        #region Variables
        private TimetableInfoList timetableInfoList = null;
        private bool applyHiddingSettings = true;

        private bool isFirstLoad = true;
        private bool isPageVisible = false;
        private List<DateTime> visibleDates = new List<DateTime>();
        private readonly object enumeratingEvents = new object();
        private readonly object updatingEventsUI = new object();
        private bool needToUpdateEventsUI = false;
        private bool lastTimeLeftVisible;

        private string _hideSelectedEventsIcon = MaterialIconsFont.Filter;
        private DateTime? _timetableSelectedDate;
        private ScheduleView _timetableScheduleView = ScheduleView.WeekView;
        private string _timetableLocale;
        private bool _timetableIsEnabled = true;
        private List<EventViewModel> _timetableDataSource;
        private DateTime _timetableMinDisplayDate;
        private DateTime _timetableMaxDisplayDate;
        private double _timetableStartHour = 0;
        private double _timetableEndHour = 24;

        private bool _timeLeftIsVisible;
        private string _timeLeftText;

        private bool _timetableLayoutIsVisible = false;
        private bool _noSourceLayoutIsVisible;
        private string _noSourceLayoutText;
        private bool _progressLayoutIsVisible;
        private string _bTodayText;
        private double _bTodayScale = 0;

        private readonly ITimetablePageCommands _timetablePage;
        #endregion

        #region Properties
        public string HideSelectedEventsIcon { get => _hideSelectedEventsIcon; set => SetProperty(ref _hideSelectedEventsIcon, value); }

        public DateTime? TimetableSelectedDate { get => _timetableSelectedDate; set => SetProperty(ref _timetableSelectedDate, value); }
        public ScheduleView TimetableScheduleView { get => _timetableScheduleView; set => SetProperty(ref _timetableScheduleView, value); }
        public bool TimetableIsEnabled { get => _timetableIsEnabled; set => SetProperty(ref _timetableIsEnabled, value); }
        public string TimetableLocale { get => _timetableLocale; set => SetProperty(ref _timetableLocale, value); }
        public List<EventViewModel> TimetableDataSource { get => _timetableDataSource; private set => SetProperty(ref _timetableDataSource, value); }
        public DateTime TimetableMaxDisplayDate { get => _timetableMaxDisplayDate; set => SetProperty(ref _timetableMaxDisplayDate, value); }
        public DateTime TimetableMinDisplayDate { get => _timetableMinDisplayDate; set => SetProperty(ref _timetableMinDisplayDate, value); }
        public double TimetableStartHour { get => _timetableStartHour; set => SetProperty(ref _timetableStartHour, value); }
        public double TimetableEndHour { get => _timetableEndHour; set => SetProperty(ref _timetableEndHour, value); }
        public int TimetableTimeInterval => 60;

        public bool TimeLeftIsVisible { get => _timeLeftIsVisible; set => SetProperty(ref _timeLeftIsVisible, value); }
        public string TimeLeftText { get => _timeLeftText; set => SetProperty(ref _timeLeftText, value); }

        public bool TimetableLayoutIsVisible { get => _timetableLayoutIsVisible; set => SetProperty(ref _timetableLayoutIsVisible, value, () => { HideSelectedEventsCommand.ChangeCanExecute(); ScheduleModeCommand.ChangeCanExecute(); }); }
        public bool NoSourceLayoutIsVisible { get => _noSourceLayoutIsVisible; set => SetProperty(ref _noSourceLayoutIsVisible, value); }
        public string NoSourceLayoutText { get => _noSourceLayoutText; set => SetProperty(ref _noSourceLayoutText, value); }
        public bool ProgressLayoutIsVisible { get => _progressLayoutIsVisible; set => SetProperty(ref _progressLayoutIsVisible, value); }
        public string BTodayText { get => _bTodayText; set => SetProperty(ref _bTodayText, value); }
        public double BTodayScale { get => _bTodayScale; set => SetProperty(ref _bTodayScale, value); }

        public Command PageAppearingCommand { get; }
        public Command PageDisappearingCommand { get; }
        public Command HideSelectedEventsCommand { get; }
        public Command ScheduleModeCommand { get; }
        public Command TimetableCellTappedCommand { get; }
        public Command TimetableMonthInlineAppointmentTappedCommand { get; }
        public Command TimetableVisibleDatesChangedCommand { get; private set; }
        public Command BTodayClickedCommand { get; }
        #endregion

        public TimetableViewModel(ITimetablePageCommands timetablePage)
        {
            _timetablePage = timetablePage;

            AppSettings settings = SettingsRepository.GetSettings();

            Title = LN.AppName;
            lastTimeLeftVisible = TimeLeftIsVisible;
            string activeCultureCode = Cultures.SupportedCultures[0].TwoLetterISOLanguageName;
            if (Cultures.SupportedCultures.Any(c => c.TwoLetterISOLanguageName == CultureInfo.CurrentCulture.TwoLetterISOLanguageName))
            {
                activeCultureCode = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
            }
            TimetableLocale = activeCultureCode;
            TimetableScheduleView = settings.TimetableViewMode switch
            {
                TimetableViewMode.Day => ScheduleView.DayView,
                TimetableViewMode.Week => ScheduleView.WeekView,
                TimetableViewMode.Month => ScheduleView.MonthView,
                TimetableViewMode.Timeline => ScheduleView.TimelineView,
                _ => TimetableScheduleView
            };

            MessagingCenter.Subscribe<Application, OSAppTheme>(this, MessageTypes.ThemeChanged, (sender, newTheme) =>
            {
                if (timetableInfoList is null)
                {
                    return;
                }

                needToUpdateEventsUI = true;
                UpdateEventsWithUI();
            });
            MessagingCenter.Subscribe<Application, List<SavedEntity>>(this, MessageTypes.SelectedEntitiesChanged, (sender, newSelectedEntities) =>
            {
                UpdateEvents(newSelectedEntities);
            });
            MessagingCenter.Subscribe<Application, SavedEntity>(this, MessageTypes.TimetableUpdated, (sender, entity) =>
            {
                List<SavedEntity> selectedEntities = UniversityEntitiesRepository.GetSelected();
                if (!selectedEntities.Contains(entity))
                {
                    return;
                }
                UpdateEvents(selectedEntities);
            });
            MessagingCenter.Subscribe<Application, SavedEntity>(this, MessageTypes.LessonSettingsChanged, (sender, entity) =>
            {
                List<SavedEntity> selectedEntities = UniversityEntitiesRepository.GetSelected();
                if (!selectedEntities.Contains(entity))
                {
                    return;
                }
                UpdateEvents(selectedEntities);
            });

            PageAppearingCommand = CommandHelper.Create(PageAppearing);
            PageDisappearingCommand = CommandHelper.Create(PageDisappearing);
            HideSelectedEventsCommand = CommandHelper.Create(HideSelectedEventsClicked, () => TimetableLayoutIsVisible);
            ScheduleModeCommand = CommandHelper.Create(ScheduleModeClicked, () => TimetableLayoutIsVisible);
            TimetableCellTappedCommand = CommandHelper.Create<CellTappedEventArgs>(TimetableCellTapped);
            TimetableMonthInlineAppointmentTappedCommand = CommandHelper.Create<MonthInlineAppointmentTappedEventArgs>(TimetableMonthInlineAppointmentTapped);
            TimetableVisibleDatesChangedCommand = CommandHelper.Create<VisibleDatesChangedEventArgs>(TimetableVisibleDatesChanged);
            BTodayClickedCommand = CommandHelper.Create(BTodayClicked);
        }

        private async Task TimetableVisibleDatesChanged(VisibleDatesChangedEventArgs e)
        {
            if (e != null)
            {
                visibleDates = e.visibleDates;
            }
            await UpdateTodayButton(false);
        }

        private async Task UpdateTodayButton(bool isForceUpdate)
        {
            if (visibleDates.Count == 0)
            {
                return;
            }

            // Updating Today button
            if (visibleDates.Any(d => d.Date == DateTime.Now.Date))
            {
                if (BTodayScale == 1)
                {
                    await _timetablePage.ScaleTodayButtonTo(0);
                }
            }
            else if (isForceUpdate || (BTodayScale == 0 && visibleDates.Any(d => d.Year > 1)))
            {
                if (visibleDates[0].Date > DateTime.Now)
                {
                    BTodayText = MaterialIconsFont.ChevronLeft;
                }
                else
                {
                    BTodayText = MaterialIconsFont.ChevronRight;
                }
                await _timetablePage.ScaleTodayButtonTo(1);
            }
        }

        private async Task PageAppearing()
        {
            isPageVisible = true;

            if (isFirstLoad)
            {
                isFirstLoad = false;
                UpdateEventsWithUI();
            }
            else
            {
                UpdateTimeLeft();
                await UpdateTodayButton(true);
                
                if (needToUpdateEventsUI)
                {
                    UpdateEventsWithUI();
                }

                // Updaing current date if it's changed
                if (visibleDates.Any())
                {
                    await Task.Delay(100);
                    try
                    {
                        _timetablePage.TimetableNavigateTo(visibleDates.First());
                    }
                    catch (ObjectDisposedException)
                    { }
                    catch (NullReferenceException)
                    { }
                }
            }

            Device.StartTimer(TimeSpan.FromSeconds(1), () => { UpdateTimeLeft(); return isPageVisible; });
        }

        private void PageDisappearing()
        {
            isPageVisible = false;
        }

        private void UpdateTimeLeft()
        {
            if (timetableInfoList is null || timetableInfoList.Count == 0)
            {
                TimeLeftIsVisible = false;
                return;
            }

            string text = null;
            lock (enumeratingEvents)
            {
                Event currentEvent = timetableInfoList.Events.FirstOrDefault(e => e.Start <= DateTime.Now && e.End >= DateTime.Now);
                if (currentEvent != null)
                {
                    text = string.Format(
                        LN.TimeUntilBreak,
                        currentEvent.RoomName,
                        (currentEvent.End - DateTime.Now).ToString("hh\\:mm\\:ss")
                    );
                }
                else
                {
                    Event nextEvent = timetableInfoList.Events
                        .Where(e => e.Start > DateTime.Now)
                        .OrderBy(e => e.Start)
                        .FirstOrDefault();
                    if (nextEvent != null && nextEvent.Start.Date == DateTime.Now.Date)
                    {
                        text = string.Format(
                            LN.TimeUntilLesson,
                            nextEvent.Lesson.ShortName,
                            nextEvent.RoomName,
                            (nextEvent.Start - DateTime.Now).ToString("hh\\:mm\\:ss")
                        );
                    }
                }
            }
            if (string.IsNullOrEmpty(text) || !isPageVisible)
            {
                TimeLeftText = null;
                if (string.IsNullOrEmpty(text) && TimeLeftIsVisible)
                {
                    TimeLeftIsVisible = false;
                }
            }
            else
            {
                TimeLeftText = text;
                TimeLeftIsVisible = true;
            }

            if (TimeLeftIsVisible != lastTimeLeftVisible)
            {
                lastTimeLeftVisible = TimeLeftIsVisible;
            }
        }

        private void UpdateEventsWithUI()
        {
            TimetableIsEnabled = false;
            ProgressLayoutIsVisible = true;

            Task.Run(async () =>
            {
                if (needToUpdateEventsUI)
                {
                    await Task.Delay(250);
                    UpdateEventsUI();
                }
                else
                {
                    UpdateEvents(UniversityEntitiesRepository.GetSelected());
                }
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    ProgressLayoutIsVisible = false;
                    TimetableIsEnabled = true;
                });
            });
        }

        private void UpdateEvents(List<SavedEntity> selectedEntities)
        {
            if (selectedEntities is null || !selectedEntities.Any())
            {
                Title = LN.AppName;
                TimetableLayoutIsVisible = false;
                NoSourceLayoutText = LN.NoTimetable;
                NoSourceLayoutIsVisible = true;
                return;
            }
            Title = string.Join(", ", selectedEntities.Select(se => se.Name));

            var timetableInfos = new List<TimetableInfo>();
            foreach (SavedEntity entity in selectedEntities)
            {
                TimetableInfo timetableInfo = EventsRepository.GetTimetableLocal(entity);
                if (timetableInfo != null)
                {
                    timetableInfos.Add(timetableInfo);
                }
            }
            lock (enumeratingEvents)
            {
                timetableInfoList = TimetableInfoList.Build(timetableInfos, applyHiddingSettings);
                if (timetableInfoList.Events.Any())
                {
                    NoSourceLayoutIsVisible = false;
                    TimetableLayoutIsVisible = true;
                    needToUpdateEventsUI = true;
                }
                else
                {
                    TimetableLayoutIsVisible = false;
                    NoSourceLayoutText = LN.TimetableIsEmpty;
                    NoSourceLayoutIsVisible = true;
                }
            }

            UpdateEventsUI();
        }

        private void UpdateEventsUI()
        {
            if (!needToUpdateEventsUI || !isPageVisible)
            {
                return;
            }

            lock (updatingEventsUI)
            {
                if (!needToUpdateEventsUI || !isPageVisible)
                {
                    return;
                }
                needToUpdateEventsUI = false;
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    if (timetableInfoList.Count == 0)
                    {
                        TimetableDataSource = null;
                        return;
                    }

                    lock (enumeratingEvents)
                    {
                        TimetableMinDisplayDate = timetableInfoList.StartDate();
                        TimetableMaxDisplayDate = timetableInfoList.EndDate();

                        TimetableEndHour = 24;
                        TimetableStartHour = timetableInfoList.StartTime().Hours;
                        TimetableEndHour = ((TimetableStartHour * 60d) + TimetableTimeInterval * Math.Ceiling((timetableInfoList.EndTime().TotalMinutes - (TimetableStartHour * 60d)) / TimetableTimeInterval)) / 60d;
                    }

                    TimetableDataSource = timetableInfoList.Events
                        .Select(ev => new EventViewModel(ev))
                        .ToList();

                    UpdateTimeLeft();
                });
            }
        }

        private async Task ScheduleModeClicked()
        {
            if (!TimetableLayoutIsVisible)
            {
                return;
            }

            string displayMode = await Shell.Current.DisplayActionSheet(LN.ChooseDisplayMode, LN.Cancel, null, LN.Day, LN.Week, LN.Timeline, LN.Month);

            AppSettings settings = SettingsRepository.GetSettings();
            DateTime? selected = TimetableSelectedDate;
            TimetableSelectedDate = null;
            if (displayMode == LN.Day)
            {
                TimetableScheduleView = ScheduleView.DayView;
                settings.TimetableViewMode = TimetableViewMode.Day;
            }
            else if (displayMode == LN.Week)
            {
                TimetableScheduleView = ScheduleView.WeekView;
                settings.TimetableViewMode = TimetableViewMode.Week;
            }
            else if (displayMode == LN.Timeline)
            {
                TimetableScheduleView = ScheduleView.TimelineView;
                settings.TimetableViewMode = TimetableViewMode.Timeline;
            }
            else if (displayMode == LN.Month)
            {
                TimetableScheduleView = ScheduleView.MonthView;
                settings.TimetableViewMode = TimetableViewMode.Month;
            }
            if (selected != null)
            {
                //_timetablePage.TimetableNavigateTo(selected.Value);
                TimetableSelectedDate = selected;
                TimetableSelectedDate = null;
            }

            SettingsRepository.UpdateSettings(settings);
        }

        private async Task TimetableCellTapped(CellTappedEventArgs e)
        {
            await DisplayEventDetails((Event)e.Appointment);
        }

        private async Task TimetableMonthInlineAppointmentTapped(MonthInlineAppointmentTappedEventArgs e)
        {
            await DisplayEventDetails((Event)e.Appointment);
        }

        private async Task DisplayEventDetails(Event ev)
        {
            if (ev is null)
            {
                return;
            }
            string nl = Environment.NewLine;

            LessonInfo lessonInfo = timetableInfoList.LessonsInfo?.FirstOrDefault(li => li.Lesson == ev.Lesson);
            string notes = null;
            if (lessonInfo != null)
            {
                if (!string.IsNullOrEmpty(lessonInfo.Notes))
                {
                    notes = nl + nl + lessonInfo.Notes;
                }
            }
            int eventNumber = timetableInfoList.Events
                .Where(e => e.Lesson == ev.Lesson && e.Type == ev.Type && e.Start < ev.Start)
                .DistinctBy(e => e.Start)
                .Count() + 1;
            int eventsCount = timetableInfoList.Events
                .Where(e => e.Lesson == ev.Lesson && e.Type == ev.Type)
                .DistinctBy(e => e.Start)
                .Count();
            bool isAddToCalendar = await Shell.Current.DisplayAlert($"{ev.Lesson.FullName}", string.Format(LN.EventType, ev.Type.FullName + $" ({eventNumber}/{eventsCount})") + nl +
                string.Format(LN.EventClassroom, ev.RoomName) + nl +
                string.Format(LN.EventTeachers, string.Join(", ", ev.Teachers.Select(t => t.Name))) + nl +
                string.Format(LN.EventGroups, string.Join(", ", ev.Groups.Select(t => t.Name))) + nl +
                string.Format(LN.EventDay, ev.Start.ToString("ddd, dd.MM.yy")) + nl +
                string.Format(LN.EventTime, ev.Start.ToString("HH:mm"), ev.End.ToString("HH:mm")) +
                notes, LN.AddToCalendar, LN.Ok);

            if (isAddToCalendar)
            {
                ProgressLayoutIsVisible = true;

                bool isAdded = await AddEventToCalendar(ev, eventNumber, eventsCount);
                if (isAdded)
                {
                    await Shell.Current.DisplayAlert(LN.AddingToCalendarTitle, LN.AddingEventToCalendarSuccess, LN.Ok);
                }
                else
                {
                    await Shell.Current.DisplayAlert(LN.AddingToCalendarTitle, LN.AddingEventToCalendarFail, LN.Ok);
                }

                ProgressLayoutIsVisible = false;
            }
        }

        private static async Task<bool> AddEventToCalendar(Event ev, int eventNumber, int eventsCount)
        {
            PermissionStatus? readStatus = null;
            PermissionStatus? writeStatus = null;
            try
            {
                const string customCalendarName = "NURE Timetable";
                bool isCustomCalendarExists = true;

                // Getting permissions
                readStatus = await Permissions.CheckStatusAsync<Permissions.CalendarRead>();
                writeStatus = await Permissions.CheckStatusAsync<Permissions.CalendarWrite>();
                if (readStatus != PermissionStatus.Granted)
                {
                    readStatus = await MainThread.InvokeOnMainThreadAsync(Permissions.RequestAsync<Permissions.CalendarRead>);
                }
                if (writeStatus != PermissionStatus.Granted && readStatus == PermissionStatus.Granted)
                {
                    writeStatus = await MainThread.InvokeOnMainThreadAsync(Permissions.RequestAsync<Permissions.CalendarWrite>);
                }
                if (readStatus != PermissionStatus.Granted || writeStatus != PermissionStatus.Granted)
                {
                    return false;
                }

                // Getting Calendar list
                IList<Calendars.Calendar> calendars = await MainThread.InvokeOnMainThreadAsync(CrossCalendars.Current.GetCalendarsAsync);
                calendars = calendars
                    .Where(c => c.Name.ToLower() == c.AccountName.ToLower() || c.AccountName.ToLower() == customCalendarName.ToLower())
                    .ToList();

                // Getting our custom calendar
                Calendars.Calendar customCalendar = calendars
                    .Where(c => c.AccountName.ToLower() == customCalendarName.ToLower())
                    .FirstOrDefault();
                if (customCalendar is null)
                {
                    isCustomCalendarExists = false;
                    customCalendar = new Calendars.Calendar
                    {
                        Name = customCalendarName
                    };
                    calendars.Add(customCalendar);
                }
                else if (calendars.Where(c => c.AccountName == customCalendar.AccountName).Count() > 1)
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        MessagingCenter.Send(Application.Current, MessageTypes.ExceptionOccurred, new IndexOutOfRangeException($"There are {calendars.Where(c => c.AccountName == customCalendar.AccountName).Count()} calendars with AccountName {customCalendar.AccountName}"));
                    });
                }

                // Getting calendar to add event into
                Calendars.Calendar targetCalendar = customCalendar;
                if (calendars.Count > 1)
                {
                    string targetCalendarName = await MainThread.InvokeOnMainThreadAsync(() => Shell.Current.DisplayActionSheet(
                        LN.ChooseCalendar,
                        LN.Cancel,
                        null,
                        calendars.Select(c => c.Name).ToArray()));

                    if (string.IsNullOrEmpty(targetCalendarName) || targetCalendarName == LN.Cancel)
                    {
                        return false;
                    }
                    targetCalendar = calendars.First(c => c.Name == targetCalendarName);
                }

                Analytics.TrackEvent("Add To Calendar", new Dictionary<string, string>
                {
                    { "Type", ev.Type.ShortName },
                    { "Room", ev.RoomName },
                    { "Groups", string.Join(", ", ev.Groups.Select(t => t.Name)) },
                    { "Teachers", string.Join(", ", ev.Teachers.Select(t => t.Name)) },
                });

                // Adding event to calendar
                string nl = Environment.NewLine;
                var calendarEvent = new Calendars.CalendarEvent
                {
                    AllDay = false,
                    Start = ev.StartUtc,
                    End = ev.EndUtc,
                    Name = $"{ev.Lesson.ShortName} ({ev.Type.ShortName} {eventNumber}/{eventsCount})",
                    Description = string.Format(LN.EventClassroom, ev.RoomName) + nl +
                        string.Format(LN.EventTeachers, string.Join(", ", ev.Teachers.Select(t => t.Name))) + nl +
                        string.Format(LN.EventGroups, string.Join(", ", ev.Groups.Select(t => t.Name))),
                    Location = $"KHNURE -\"{ev.RoomName}\"",
                    Reminders = new[] 
                    {
                        new Calendars.CalendarEventReminder
                        {
                            Method = Calendars.CalendarReminderMethod.Alert,
                            TimeBefore = TimeSpan.FromMinutes(30)
                        }
                    }
                };

                if (!isCustomCalendarExists && targetCalendar == customCalendar)
                {
                    await CrossCalendars.Current.AddOrUpdateCalendarAsync(customCalendar);
                }
                await CrossCalendars.Current.AddOrUpdateEventAsync(targetCalendar, calendarEvent);
            }
            catch (Exception ex)
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    ex.Data.Add("Read Status", readStatus?.ToString());
                    ex.Data.Add("Write Status", writeStatus?.ToString());
                    MessagingCenter.Send(Application.Current, MessageTypes.ExceptionOccurred, ex);
                });
                return false;
            }

            return true;
        }

        private void HideSelectedEventsClicked()
        {
            applyHiddingSettings = !applyHiddingSettings;

            string message, icon;
            if (applyHiddingSettings)
            {
                icon = MaterialIconsFont.Filter;
                message = LN.SelectedEventsShown;
            }
            else
            {
                icon = MaterialIconsFont.FilterOff;
                message = LN.AllEventsShown;
            }
            HideSelectedEventsIcon = icon;
            DependencyService.Get<IMessage>().LongAlert(message);

            UpdateEventsWithUI();
        }

        private async Task BTodayClicked()
        {
            DateTime today = DateTime.Now.Date;

            DateTime moveTo = today;
            if (TimetableMinDisplayDate > moveTo)
            {
                moveTo = TimetableMinDisplayDate;
            }
            else if (TimetableMaxDisplayDate < moveTo)
            {
                moveTo = TimetableMaxDisplayDate;
            }
            if (moveTo != today && visibleDates.Contains(moveTo))
            {
                await Shell.Current.DisplayAlert(LN.ShowToday, LN.NoTodayTimetable, LN.Ok);
                return;
            }

            _timetablePage.TimetableNavigateTo(moveTo);
        }
    }
}
