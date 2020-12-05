using Microsoft.AppCenter.Analytics;
using NureTimetable.BL;
using NureTimetable.Core.Extensions;
using NureTimetable.Core.Localization;
using NureTimetable.Core.Models.Consts;
using NureTimetable.Core.Models.InterplatformCommunication;
using NureTimetable.Core.Models.Settings;
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
using AppTheme = NureTimetable.Core.Models.Settings.AppTheme;
using Calendar = Plugin.Calendars.Abstractions.Calendar;
using Plugin.Calendars.Abstractions;

namespace NureTimetable.UI.ViewModels.Timetable
{
    public class TimetableViewModel : BaseViewModel
    {
        #region Variables
        private readonly ITimetablePageCommands timetablePage;

        private bool isFirstLoad = true;
        private bool isPageVisible = false;
        
        private TimetableInfoList timetableInfoList = null;
        private List<DateTime> visibleDates = new();
        private bool needToUpdateEventsUI = false;

        // Lock objects
        private readonly object enumeratingEvents = new();
        private readonly object updatingEventsUI = new();
        #endregion

        #region Properties
        // Toolbar
        private bool applyHiddingSettings = true;
        private string _hideSelectedEventsIcon = MaterialIconsFont.Filter;
        public string HideSelectedEventsIcon { get => _hideSelectedEventsIcon; set => SetProperty(ref _hideSelectedEventsIcon, value); }

        private readonly List<Entity> updatingTimetables = new();
        private bool _isTimetableUpdating = false;
        public bool IsTimetableUpdating { get => _isTimetableUpdating; set => SetProperty(ref _isTimetableUpdating, value, () => UpdateTimetableCommand.ChangeCanExecute()); }

        // Timetable
        public int TimetableTimeInterval => 60;

        private DateTime? _timetableSelectedDate;
        public DateTime? TimetableSelectedDate { get => _timetableSelectedDate; set => SetProperty(ref _timetableSelectedDate, value); }
        
        private ScheduleView _timetableScheduleView = ScheduleView.WeekView;
        public ScheduleView TimetableScheduleView { get => _timetableScheduleView; set => SetProperty(ref _timetableScheduleView, value); }
        
        private bool _timetableIsEnabled = true;
        public bool TimetableIsEnabled { get => _timetableIsEnabled; set => SetProperty(ref _timetableIsEnabled, value); }
        
        private string _timetableLocale;
        public string TimetableLocale { get => _timetableLocale; set => SetProperty(ref _timetableLocale, value); }
        
        private List<EventViewModel> _timetableDataSource;
        public List<EventViewModel> TimetableDataSource { get => _timetableDataSource; private set => SetProperty(ref _timetableDataSource, value); }
        
        private DateTime _timetableMaxDisplayDate;
        public DateTime TimetableMaxDisplayDate { get => _timetableMaxDisplayDate; set => SetProperty(ref _timetableMaxDisplayDate, value); }
        
        private DateTime _timetableMinDisplayDate;
        public DateTime TimetableMinDisplayDate { get => _timetableMinDisplayDate; set => SetProperty(ref _timetableMinDisplayDate, value); }
        
        private double _timetableStartHour = 0;
        public double TimetableStartHour { get => _timetableStartHour; set => SetProperty(ref _timetableStartHour, value); }
        
        private double _timetableEndHour = 24;
        public double TimetableEndHour { get => _timetableEndHour; set => SetProperty(ref _timetableEndHour, value); }

        // TimeLeft
        private bool lastTimeLeftVisible;
        private bool _timeLeftIsVisible;
        public bool TimeLeftIsVisible { get => _timeLeftIsVisible; set => SetProperty(ref _timeLeftIsVisible, value); }
        
        private string _timeLeftText;
        public string TimeLeftText { get => _timeLeftText; set => SetProperty(ref _timeLeftText, value); }

        // Layouts
        private bool _timetableLayoutIsVisible = false;
        public bool TimetableLayoutIsVisible { get => _timetableLayoutIsVisible; set => SetProperty(ref _timetableLayoutIsVisible, value); }
        
        private bool _noSourceLayoutIsVisible;
        public bool NoSourceLayoutIsVisible { get => _noSourceLayoutIsVisible; set => SetProperty(ref _noSourceLayoutIsVisible, value); }
        
        private string _noSourceLayoutText;
        public string NoSourceLayoutText { get => _noSourceLayoutText; set => SetProperty(ref _noSourceLayoutText, value); }
        
        private bool _progressLayoutIsVisible;
        public bool ProgressLayoutIsVisible { get => _progressLayoutIsVisible; set => SetProperty(ref _progressLayoutIsVisible, value); }
        
        // bToday
        private string _bTodayText;
        public string BTodayText { get => _bTodayText; set => SetProperty(ref _bTodayText, value); }
        
        private double _bTodayScale = 0;
        public double BTodayScale { get => _bTodayScale; set => SetProperty(ref _bTodayScale, value); }

        public Command PageAppearingCommand { get; }
        public Command PageDisappearingCommand { get; }
        public Command HideSelectedEventsCommand { get; }
        public Command ScheduleModeCommand { get; }
        public Command TimetableCellTappedCommand { get; }
        public Command TimetableMonthInlineAppointmentTappedCommand { get; }
        public Command TimetableVisibleDatesChangedCommand { get; }
        public Command BTodayClickedCommand { get; }
        public Command UpdateTimetableCommand { get; }
        #endregion

        public TimetableViewModel(ITimetablePageCommands timetablePage)
        {
            this.timetablePage = timetablePage;

            Title = LN.AppName;
            lastTimeLeftVisible = TimeLeftIsVisible;
            // Set custom TimetableLocale only if it is one of supported cultures
            string activeCultureCode = Cultures.SupportedCultures[0].TwoLetterISOLanguageName;
            if (Cultures.SupportedCultures.Any(c => c.TwoLetterISOLanguageName == CultureInfo.CurrentCulture.TwoLetterISOLanguageName))
            {
                activeCultureCode = LN.Culture.TwoLetterISOLanguageName;
            }
            TimetableLocale = activeCultureCode;
            TimetableScheduleView = SettingsRepository.Settings.TimetableViewMode switch
            {
                TimetableViewMode.Day => ScheduleView.DayView,
                TimetableViewMode.Week => ScheduleView.WeekView,
                TimetableViewMode.Month => ScheduleView.MonthView,
                _ => TimetableScheduleView
            };

            MessagingCenter.Subscribe<Application, AppTheme>(this, MessageTypes.ThemeChanged, async (sender, newTheme) =>
            {
                if (timetableInfoList is null)
                {
                    return;
                }

                needToUpdateEventsUI = true;
                await UpdateEventsWithUI();
            });
            MessagingCenter.Subscribe<Application, List<SavedEntity>>(this, MessageTypes.SelectedEntitiesChanged, (sender, newSelectedEntities) =>
            {
                UpdateEvents(newSelectedEntities.ToList<Entity>());
            });
            MessagingCenter.Subscribe<Application, Entity>(this, MessageTypes.TimetableUpdated, (sender, entity) =>
            {
                if (timetableInfoList.Entities.Contains(entity))
                    UpdateEvents();
            });
            MessagingCenter.Subscribe<Application, Entity>(this, MessageTypes.LessonSettingsChanged, (sender, entity) =>
            {
                if (timetableInfoList.Entities.Contains(entity))
                    UpdateEvents();
            }); 
            MessagingCenter.Subscribe<Application, Entity>(this, MessageTypes.TimetableUpdating, (sender, entity) =>
            {
                if (!timetableInfoList.Entities.Contains(entity))
                    return;

                updatingTimetables.Add(entity);
                IsTimetableUpdating = true;
            });
            MessagingCenter.Subscribe<Application, Entity>(this, MessageTypes.TimetableUpdated, (sender, entity) =>
            {
                if (!updatingTimetables.Contains(entity))
                    return;

                updatingTimetables.Remove(entity);
                if (updatingTimetables.Count == 0)
                    IsTimetableUpdating = false;
            });

            PageAppearingCommand = CommandHelper.Create(PageAppearing);
            HideSelectedEventsCommand = CommandHelper.Create(HideSelectedEventsClicked);
            ScheduleModeCommand = CommandHelper.Create(ScheduleModeClicked);
            BTodayClickedCommand = CommandHelper.Create(BTodayClicked);
            PageDisappearingCommand = CommandHelper.Create(() => isPageVisible = false);
            TimetableCellTappedCommand = CommandHelper.Create<CellTappedEventArgs>((e) => DisplayEventDetails((Event)e.Appointment));
            TimetableMonthInlineAppointmentTappedCommand = CommandHelper.Create<MonthInlineAppointmentTappedEventArgs>((e) => DisplayEventDetails((Event)e.Appointment));
            TimetableVisibleDatesChangedCommand = CommandHelper.Create<VisibleDatesChangedEventArgs>(async (e) =>
            {
                if (e != null)
                    visibleDates = e.visibleDates;
                await UpdateTodayButton(false);
            });
            UpdateTimetableCommand = CommandHelper.Create(async () => 
            {
                string responce = await TimetableService.Update(timetableInfoList?.Entities.ToList());
                if (responce is null)
                    return;

                await Shell.Current.DisplayAlert(LN.TimetableUpdate, responce, LN.Ok);
            }, () => !_isTimetableUpdating);
        }

        private async Task UpdateTodayButton(bool isForceUpdate)
        {
            if (visibleDates.Count == 0)
                return;

            // Updating Today button
            if (visibleDates.Any(d => d.Date == DateTime.Now.Date))
            {
                if (BTodayScale == 1)
                {
                    await timetablePage.ScaleTodayButtonTo(0);
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
                await timetablePage.ScaleTodayButtonTo(1);
            }
        }

        private async Task PageAppearing()
        {
            isPageVisible = true;

            if (isFirstLoad)
            {
                isFirstLoad = false;
                await UpdateEventsWithUI(true);
            }
            else
            {
                UpdateTimeLeft();
                await UpdateTodayButton(true);
                
                if (needToUpdateEventsUI)
                {
                    await UpdateEventsWithUI();
                }

                // Updaing current date if it's changed
                if (visibleDates.Any())
                {
                    await Task.Delay(100);
                    try
                    {
                        timetablePage.TimetableNavigateTo(visibleDates.First());
                    }
                    catch (ObjectDisposedException)
                    { }
                    catch (NullReferenceException)
                    { }
                }
            }

            Device.StartTimer(TimeSpan.FromSeconds(1), () => { UpdateTimeLeft(); return isPageVisible; });
        }

        private void UpdateTimeLeft()
        {
            if (timetableInfoList is null || timetableInfoList.EventCount == 0)
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

        private async Task UpdateEventsWithUI(bool reloadSavedEntities = false)
        {
            await Task.Run(async () =>
            {
                TimetableIsEnabled = false;
                ProgressLayoutIsVisible = true;

                if (needToUpdateEventsUI)
                {
                    await Task.Delay(250);
                    UpdateEventsUI();
                }
                else
                {
                    if (reloadSavedEntities)
                    {
                        List<Entity> selectedEntities = UniversityEntitiesRepository.GetSaved().Where(e => e.IsSelected).ToList<Entity>();
                        UpdateEvents(selectedEntities);
                    }
                    else
                    {
                        UpdateEvents();
                    }
                }

                ProgressLayoutIsVisible = false;
                TimetableIsEnabled = true;
            });
        }

        /// <summary>
        /// Updates events for already displayed entities
        /// </summary>
        private void UpdateEvents()
            => UpdateEvents(timetableInfoList?.Entities.ToList());

        private void UpdateEvents(List<Entity> selectedEntities)
        {
            if (selectedEntities is null || !selectedEntities.Any())
            {
                Title = LN.AppName;
                TimetableLayoutIsVisible = false;
                NoSourceLayoutText = LN.NoTimetable;
                NoSourceLayoutIsVisible = true;
                timetableInfoList = null;
                return;
            }

            Title = string.Join(", ", selectedEntities.Select(se => se.Name));

            var timetableInfos = new List<TimetableInfo>();
            foreach (Entity entity in selectedEntities)
            {
                TimetableInfo timetableInfo = EventsRepository.GetTimetableLocal(entity);
                timetableInfos.Add(timetableInfo ?? new(entity));
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

                if (timetableInfoList.EventCount == 0)
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
            }
        }

        private async Task ScheduleModeClicked()
        {
            if (!TimetableLayoutIsVisible)
                return;

            string displayMode = await Shell.Current.DisplayActionSheet(LN.ChooseDisplayMode, LN.Cancel, null, LN.Day, LN.Week, LN.Month);

            DateTime? selected = TimetableSelectedDate;
            TimetableSelectedDate = null;
            if (displayMode == LN.Day)
            {
                TimetableScheduleView = ScheduleView.DayView;
                SettingsRepository.Settings.TimetableViewMode = TimetableViewMode.Day;
            }
            else if (displayMode == LN.Week)
            {
                TimetableScheduleView = ScheduleView.WeekView;
                SettingsRepository.Settings.TimetableViewMode = TimetableViewMode.Week;
            }
            else if (displayMode == LN.Month)
            {
                TimetableScheduleView = ScheduleView.MonthView;
                SettingsRepository.Settings.TimetableViewMode = TimetableViewMode.Month;
            }
            if (selected != null)
            {
                //_timetablePage.TimetableNavigateTo(selected.Value);
                TimetableSelectedDate = selected;
                TimetableSelectedDate = null;
            }
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
                    readStatus = await Permissions.RequestAsync<Permissions.CalendarRead>();
                }
                if (writeStatus != PermissionStatus.Granted && readStatus == PermissionStatus.Granted)
                {
                    writeStatus = await Permissions.RequestAsync<Permissions.CalendarWrite>();
                }
                if (readStatus != PermissionStatus.Granted || writeStatus != PermissionStatus.Granted)
                {
                    return false;
                }

                // Getting Calendar list
                IList<Calendar> calendars = await CrossCalendars.Current.GetCalendarsAsync();
                calendars = calendars
                    .Where(c => c.Name.ToLower() == c.AccountName.ToLower() || c.AccountName.ToLower() == customCalendarName.ToLower())
                    .ToList();

                // Getting our custom calendar
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
                var calendarEvent = new CalendarEvent
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
                        new CalendarEventReminder
                        {
                            Method = CalendarReminderMethod.Alert,
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
                ex.Data.Add("Read Status", readStatus?.ToString());
                ex.Data.Add("Write Status", writeStatus?.ToString());
                MessagingCenter.Send(Application.Current, MessageTypes.ExceptionOccurred, ex);

                return false;
            }

            return true;
        }

        private async Task HideSelectedEventsClicked()
        {
            if (!TimetableLayoutIsVisible)
                return;

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
            DependencyService.Get<IMessageManager>().ShortAlert(message);

            await UpdateEventsWithUI();
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

            timetablePage.TimetableNavigateTo(moveTo);
        }
    }
}
