﻿using NureTimetable.Core.Localization;
using NureTimetable.Core.Models;
using NureTimetable.Core.Models.Consts;
using NureTimetable.Core.Models.InterplatformCommunication;
using NureTimetable.DAL;
using NureTimetable.DAL.Models.Local;
using NureTimetable.Models;
using NureTimetable.Models.Consts.Fonts;
using NureTimetable.Services.Helpers;
using NureTimetable.UI.Views.TimetableEntities;
using NureTimetable.ViewModels.Core;
using NureTimetable.ViewModels.TimetableEntities;
using NureTimetable.Views;
using Syncfusion.SfSchedule.XForms;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace NureTimetable.ViewModels
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

        private ITimetablePageCommands _timetablePage;
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
        public static int TimetableTimeInterval => 60;

        public bool TimeLeftIsVisible { get => _timeLeftIsVisible; set => SetProperty(ref _timeLeftIsVisible, value); }
        public string TimeLeftText { get => _timeLeftText; set => SetProperty(ref _timeLeftText, value); }

        public bool TimetableLayoutIsVisible { get => _timetableLayoutIsVisible; set => SetProperty(ref _timetableLayoutIsVisible, value); }
        public bool NoSourceLayoutIsVisible { get => _noSourceLayoutIsVisible; set => SetProperty(ref _noSourceLayoutIsVisible, value); }
        public bool ProgressLayoutIsVisible { get => _progressLayoutIsVisible; set => SetProperty(ref _progressLayoutIsVisible, value); }
        public string BTodayText { get => _bTodayText; set => SetProperty(ref _bTodayText, value); }
        public double BTodayScale { get => _bTodayScale; set => SetProperty(ref _bTodayScale, value); }

        public ICommand PageAppearingCommand { get; }
        public ICommand PageDisappearingCommand { get; }
        public ICommand PageSizeChangedCommand { get; }
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
            PageSizeChangedCommand = CommandHelper.CreateCommand(PageSizeChanged);
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
            await UpdateTodayButton(false, e);
        }

        private async Task UpdateTodayButton(bool isForceUpdate, VisibleDatesChangedEventArgs visibleDatesChangedArgs = null)
        {
            if (visibleDatesChangedArgs != null)
            {
                visibleDates = visibleDatesChangedArgs.visibleDates;
            }
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
            else if (isForceUpdate || BTodayScale == 0)
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
                        text = string.Format(LN.TimeUntilBreak, (currentEvent.End - DateTime.Now).ToString("hh\\:mm\\:ss"));
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
                        _timetablePage.UpdateTimetableHeight();
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
                    _timetablePage.UpdateTimetableHeight();
                }
            }
            else
            {
                TimeLeftIsVisible = false;
                _timetablePage.UpdateTimetableHeight();
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
                Device.BeginInvokeOnMainThread(() =>
                {
                    Title = LN.AppName;
                });
                TimetableLayoutIsVisible = false;
                NoSourceLayoutIsVisible = true;
                return;
            }
            else
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    Title = string.Join(", ", selectedEntities.Select(se => se.Name));
                });
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

                                TimetableStartHour = 0;
                                TimetableEndHour = 24;
                                TimetableStartHour = timetableInfoList.StartTime().TotalHours;
                                TimetableEndHour = timetableInfoList.EndTime().TotalHours + (TimetableTimeInterval / 60 / 2);
                            }

                            _timetablePage.UpdateTimetableHeight();

                            // Fix for bug when view header isn`t updating on first swipe
                            try
                            {
                                TimetableVisibleDatesChangedCommand = null;

                                DateTime currebtDate = visibleDates.Count > 0 ? visibleDates[0] : DateTime.Now;
                                _timetablePage.TimetableNavigateTo(currebtDate.AddDays(7));
                                _timetablePage.TimetableNavigateTo(currebtDate);
                            }
                            catch { }
                            finally
                            {
                                TimetableVisibleDatesChangedCommand = CommandHelper.CreateCommand<VisibleDatesChangedEventArgs>(TimetableVisibleDatesChanged);
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

        private void PageSizeChanged()
        {
            _timetablePage.UpdateTimetableHeight();
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

            string displayMode = await App.Current.MainPage.DisplayActionSheet(LN.ChooseDisplayMode, LN.Cancel, null, LN.Day, LN.Week, LN.Month);

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
            await App.Current.MainPage.DisplayAlert($"{ev.Lesson.FullName}", string.Format(LN.EventType, ev.Type.FullName) + nl +
                string.Format(LN.EventClassroom, ev.RoomName) + nl +
                string.Format(LN.EventTeachers, string.Join(", ", ev.Teachers.Select(t => t.Name))) + nl +
                string.Format(LN.EventGroups, string.Join(", ", ev.Groups.Select(t => t.Name))) + nl +
                string.Format(LN.EventDay, ev.Start.ToString("ddd, dd.MM.yy")) + nl +
                string.Format(LN.EventTime, ev.Start.ToString("HH:mm"), ev.End.ToString("HH:mm")) +
                notes, LN.Ok);
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
