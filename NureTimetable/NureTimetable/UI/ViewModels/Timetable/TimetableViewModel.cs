using NureTimetable.Core.Extensions;
using NureTimetable.Core.Localization;
using NureTimetable.Core.Models;
using NureTimetable.Core.Models.Consts;
using NureTimetable.Core.Models.InterplatformCommunication;
using NureTimetable.DAL;
using NureTimetable.DAL.Models.Local;
using NureTimetable.Models.Consts.Fonts;
using NureTimetable.Services.Helpers;
using NureTimetable.UI.ViewModels.Core;
using NureTimetable.UI.ViewModels.TimetableEntities.ManageEntities;
using NureTimetable.UI.Views;
using NureTimetable.UI.Views.TimetableEntities;
using Plugin.Calendars;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using Syncfusion.SfSchedule.XForms;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
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

        private string _hideSelectedEventsIcon = "filter";
        private DateTime? _timetableSelectedDate;
        private string _title = LN.AppName;
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
        private bool _progressLayoutIsVisible;
        private string _bTodayText;
        private double _bTodayScale = 0;

        private readonly ITimetablePageCommands _timetablePage;
        #endregion

        #region Properties
        public string Title { get => _title; set => SetProperty(ref _title, value); }

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

        public bool TimetableLayoutIsVisible { get => _timetableLayoutIsVisible; set => SetProperty(ref _timetableLayoutIsVisible, value); }
        public bool NoSourceLayoutIsVisible { get => _noSourceLayoutIsVisible; set => SetProperty(ref _noSourceLayoutIsVisible, value); }
        public bool ProgressLayoutIsVisible { get => _progressLayoutIsVisible; set => SetProperty(ref _progressLayoutIsVisible, value); }
        public string BTodayText { get => _bTodayText; set => SetProperty(ref _bTodayText, value); }
        public double BTodayScale { get => _bTodayScale; set => SetProperty(ref _bTodayScale, value); }

        public ICommand PageAppearingCommand { get; }
        public ICommand PageDisappearingCommand { get; }
        public ICommand HideSelectedEventsClickedCommand { get; }
        public ICommand ScheduleModeClickedCommand { get; }
        public ICommand ManageGroupsClickedCommand { get; }
        public ICommand TimetableCellTappedCommand { get; }
        public ICommand TimetableMonthInlineAppointmentTappedCommand { get; }
        public ICommand TimetableVisibleDatesChangedCommand { get; private set; }
        public ICommand BTodayClickedCommand { get; }
        #endregion

        public TimetableViewModel(INavigation navigation, ITimetablePageCommands timetablePage) : base(navigation)
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
            switch (settings.TimetableViewMode)
            {
                case TimetableViewMode.Day:
                    TimetableScheduleView = ScheduleView.DayView;
                    break;
                case TimetableViewMode.Week:
                    TimetableScheduleView = ScheduleView.WeekView;
                    break;
                case TimetableViewMode.Month:
                    TimetableScheduleView = ScheduleView.MonthView;
                    break;
            }

            MessagingCenter.Subscribe<Application, List<SavedEntity>>(this, MessageTypes.SelectedEntitiesChanged, (sender, newSelectedEntities) =>
            {
                UpdateEvents(newSelectedEntities);
            });
            MessagingCenter.Subscribe<Application, SavedEntity>(this, MessageTypes.TimetableUpdated, (sender, entity) =>
            {
                List<SavedEntity> selectedEntities = UniversityEntitiesRepository.GetSelected();
                if (selectedEntities == null || !selectedEntities.Contains(entity))
                {
                    return;
                }
                UpdateEvents(selectedEntities);
            });
            MessagingCenter.Subscribe<Application, SavedEntity>(this, MessageTypes.LessonSettingsChanged, (sender, entity) =>
            {
                List<SavedEntity> selectedEntities = UniversityEntitiesRepository.GetSelected();
                if (selectedEntities.Count == 0 || !selectedEntities.Contains(entity))
                {
                    return;
                }
                UpdateEvents(selectedEntities);
            });

            PageAppearingCommand = CommandHelper.CreateCommand(PageAppearing);
            PageDisappearingCommand = CommandHelper.CreateCommand(PageDisappearing);
            HideSelectedEventsClickedCommand = CommandHelper.CreateCommand(HideSelectedEventsClicked);
            ScheduleModeClickedCommand = CommandHelper.CreateCommand(ScheduleModeClicked);
            ManageGroupsClickedCommand = CommandHelper.CreateCommand(ManageGroupsClicked);
            TimetableCellTappedCommand = CommandHelper.CreateCommand<CellTappedEventArgs>(TimetableCellTapped);
            TimetableMonthInlineAppointmentTappedCommand = CommandHelper.CreateCommand<MonthInlineAppointmentTappedEventArgs>(TimetableMonthInlineAppointmentTapped);
            TimetableVisibleDatesChangedCommand = CommandHelper.CreateCommand<VisibleDatesChangedEventArgs>(TimetableVisibleDatesChanged);
            BTodayClickedCommand = CommandHelper.CreateCommand(BTodayClicked);
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
                    BTodayText = MaterialFont.ChevronLeft;
                }
                else
                {
                    BTodayText = MaterialFont.ChevronRight;
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
            }

            Device.StartTimer(TimeSpan.FromSeconds(1), UpdateTimeLeft);

            if (needToUpdateEventsUI)
            {
                UpdateEventsWithUI();
            }
        }

        private void PageDisappearing()
        {
            isPageVisible = false;
        }

        private bool UpdateTimeLeft()
        {
            if (timetableInfoList != null && timetableInfoList.Count > 0)
            {
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
            else
            {
                TimeLeftIsVisible = false;
            }
            return isPageVisible;
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
                Device.BeginInvokeOnMainThread(() =>
                {
                    ProgressLayoutIsVisible = false;
                    TimetableIsEnabled = true;
                });
            });
        }

        private void UpdateEvents(List<SavedEntity> selectedEntities)
        {
            if (selectedEntities == null || selectedEntities.Count == 0)
            {
                Title = LN.AppName;
                TimetableLayoutIsVisible = false;
                NoSourceLayoutIsVisible = true;
                return;
            }
            else
            {
                Title = string.Join(", ", selectedEntities.Select(se => se.Name));
                NoSourceLayoutIsVisible = false;
                TimetableLayoutIsVisible = true;
            }

            lock (enumeratingEvents)
            {
                var timetableInfos = new List<TimetableInfo>();
                foreach (SavedEntity entity in selectedEntities)
                {
                    TimetableInfo timetableInfo = EventsRepository.GetEvents(entity);
                    if (timetableInfo != null)
                    {
                        timetableInfos.Add(timetableInfo);
                    }
                }
                timetableInfoList = TimetableInfoList.Build(timetableInfos, applyHiddingSettings);
                needToUpdateEventsUI = true;
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
                Device.BeginInvokeOnMainThread(async () =>
                {
                    if (timetableInfoList.Count == 0)
                    {
                        TimetableDataSource = null;
                        return;
                    }

                    int retriesLeft = 25;
                    Exception exception = null;
                    do
                    {
                        try
                        {
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
                            break;
                        }
                        catch (Exception ex)
                        {
                            // Potential error with the SfSchedule control. Needs investigation!
                            exception = ex;

                            // This is temporary to check if this error is still occuring. TODO: Remove while loop if it doesn't
                            MessagingCenter.Send(Application.Current, MessageTypes.ExceptionOccurred, exception);
                        }

                        retriesLeft--;
                    } while (retriesLeft > 0);
                    if (retriesLeft == 0 && exception != null)
                    {
                        MessagingCenter.Send(Application.Current, MessageTypes.ExceptionOccurred, exception);

                        await App.Current.MainPage.DisplayAlert(LN.TimetableDisplay, LN.TimetableDisplayError, LN.Ok);
                        return;
                    }
                });
            }
        }
        
        private async Task ManageGroupsClicked()
        {
            await Navigation.PushAsync(new ManageEntitiesPage
            {
                BindingContext = new ManageEntitiesViewModel(Navigation)
            });
        }

        private async Task ScheduleModeClicked()
        {
            if (!TimetableLayoutIsVisible)
            {
                return;
            }

            string displayMode = await App.Current.MainPage.DisplayActionSheet(LN.ChooseDisplayMode, LN.Cancel, null, LN.Day, LN.Week, LN.Timeline, LN.Month);

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
            if (ev == null)
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
            bool isNotAddToCalendar = await App.Current.MainPage.DisplayAlert($"{ev.Lesson.FullName}", string.Format(LN.EventType, ev.Type.FullName + $" ({eventNumber}/{eventsCount})") + nl +
                string.Format(LN.EventClassroom, ev.RoomName) + nl +
                string.Format(LN.EventTeachers, string.Join(", ", ev.Teachers.Select(t => t.Name))) + nl +
                string.Format(LN.EventGroups, string.Join(", ", ev.Groups.Select(t => t.Name))) + nl +
                string.Format(LN.EventDay, ev.Start.ToString("ddd, dd.MM.yy")) + nl +
                string.Format(LN.EventTime, ev.Start.ToString("HH:mm"), ev.End.ToString("HH:mm")) +
                notes, LN.Ok, LN.AddToCalendar);

            if (!isNotAddToCalendar)
            {
                PermissionStatus? status = null;
                try
                {
                    status = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Calendar);
                    if (status != PermissionStatus.Granted)
                    {
                        status = (await CrossPermissions.Current.RequestPermissionsAsync(Permission.Calendar)).Single().Value;
                    }

                    var calendar = (await CrossCalendars.Current.GetCalendarsAsync())
                        .Where(c => c.Name == LN.AppName)
                        .FirstOrDefault();

                    if (calendar == null)
                    {
                        calendar = new Calendars.Calendar
                        {
                            Name = LN.AppName
                        };
                        await CrossCalendars.Current.AddOrUpdateCalendarAsync(calendar);
                    }

                    var calendarEvent = new Calendars.CalendarEvent
                    {
                        AllDay = false,
                        Start = ev.StartUtc,
                        End = ev.EndUtc,
                        Name = $"{ev.Lesson.ShortName} ({ev.Type.ShortName} {eventNumber}/{eventsCount})",
                        Description = string.Format(LN.EventClassroom, ev.RoomName) + nl +
                            string.Format(LN.EventTeachers, string.Join(", ", ev.Teachers.Select(t => t.Name))) + nl +
                            string.Format(LN.EventGroups, string.Join(", ", ev.Groups.Select(t => t.Name))),
                        Location = "9G2R267H+W9",
                        Reminders = new[] { 
                            new Calendars.CalendarEventReminder 
                            { 
                                Method = Calendars.CalendarReminderMethod.Alert, 
                                TimeBefore = TimeSpan.FromMinutes(30) 
                            } 
                        }
                    };
                    await CrossCalendars.Current.AddOrUpdateEventAsync(calendar, calendarEvent);
                    
                    await App.Current.MainPage.DisplayAlert(LN.AddingToCalendarTitle, LN.AddingEventToCalendarSuccess, LN.Ok);
                }
                catch (Exception ex)
                {
                    if (status == null && status == PermissionStatus.Granted)
                    {
                        Device.BeginInvokeOnMainThread(() =>
                        {
                            MessagingCenter.Send(Application.Current, MessageTypes.ExceptionOccurred, ex);
                        });
                    }
                    await App.Current.MainPage.DisplayAlert(LN.AddingToCalendarTitle, LN.AddingEventToCalendarFail, LN.Ok);
                }
            }
        }

        private void HideSelectedEventsClicked()
        {
            if (!TimetableLayoutIsVisible)
            {
                return;
            }

            applyHiddingSettings = !applyHiddingSettings;

            string message, icon;
            if (applyHiddingSettings)
            {
                icon = "filter";
                message = LN.SelectedEventsShown;
            }
            else
            {
                icon = "filter_outline";
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
                await App.Current.MainPage.DisplayAlert(LN.ShowToday, LN.NoTodayTimetable, LN.Ok);
                return;
            }

            _timetablePage.TimetableNavigateTo(moveTo);
        }
    }
}
