﻿using NureTimetable.BL;
using NureTimetable.Core.Localization;
using NureTimetable.Core.Models.Consts;
using NureTimetable.Core.Models.Settings;
using NureTimetable.DAL;
using NureTimetable.DAL.Models.Local;
using NureTimetable.Models.Consts;
using NureTimetable.Models.Consts.Fonts;
using NureTimetable.UI.Helpers;
using NureTimetable.UI.Views;
using Rg.Plugins.Popup.Services;
using Syncfusion.SfSchedule.XForms;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.CommunityToolkit.Extensions;
using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.Forms;
using AppTheme = NureTimetable.Core.Models.Settings.AppTheme;

namespace NureTimetable.UI.ViewModels.Timetable
{
    public class TimetableViewModel : BaseViewModel
    {
        #region Variables
        private readonly ITimetablePageCommands timetablePage;

        private bool isFirstLoad = true;
        private bool isPageVisible = false;
        
        private TimetableInfoList timetableInfoList = TimetableInfoList.Empty;
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
        private bool _isTimeLeftVisible;
        public bool IsTimeLeftVisible { get => _isTimeLeftVisible; set => SetProperty(ref _isTimeLeftVisible, value); }
        
        private string _timeLeftText;
        public string TimeLeftText { get => _timeLeftText; set => SetProperty(ref _timeLeftText, value); }

        // Layouts
        private bool _isNoSourceLayoutVisible = true;
        public bool IsNoSourceLayoutVisible { get => _isNoSourceLayoutVisible; set => SetProperty(ref _isNoSourceLayoutVisible, value, () =>
        {
            HideSelectedEventsCommand.RaiseCanExecuteChanged();
            ScheduleModeCommand.RaiseCanExecuteChanged();
            UpdateTimetableCommand.ChangeCanExecute();
        }); }
        
        private string _noSourceLayoutText;
        public string NoSourceLayoutText { get => _noSourceLayoutText; set => SetProperty(ref _noSourceLayoutText, value); }
        
        private bool _isProgressLayoutVisible;
        public bool IsProgressLayoutVisible { get => _isProgressLayoutVisible; set => SetProperty(ref _isProgressLayoutVisible, value); }
        
        // bToday
        private string _bTodayText;
        public string BTodayText { get => _bTodayText; set => SetProperty(ref _bTodayText, value); }
        
        private double _bTodayScale = 0;
        public double BTodayScale { get => _bTodayScale; set => SetProperty(ref _bTodayScale, value); }

        public IAsyncCommand PageAppearingCommand { get; }
        public Command PageDisappearingCommand { get; }
        public IAsyncCommand HideSelectedEventsCommand { get; }
        public IAsyncCommand ScheduleModeCommand { get; }
        public IAsyncCommand<CellTappedEventArgs> TimetableCellTappedCommand { get; }
        public IAsyncCommand<MonthInlineAppointmentTappedEventArgs> TimetableMonthInlineAppointmentTappedCommand { get; }
        public Command<MonthInlineLoadedEventArgs> TimetableMonthInlineLoadedCommand { get; }
        public IAsyncCommand<VisibleDatesChangedEventArgs> TimetableVisibleDatesChangedCommand { get; }
        public IAsyncCommand BTodayClickedCommand { get; }
        public Command UpdateTimetableCommand { get; }
        #endregion

        public TimetableViewModel(ITimetablePageCommands timetablePage)
        {
            this.timetablePage = timetablePage;

            Title = LN.AppName;
            lastTimeLeftVisible = IsTimeLeftVisible;
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

            MessagingCenter.Subscribe<Application, Entity>(this, MessageTypes.LessonSettingsChanged, EntityChanged);
            MessagingCenter.Subscribe<Application, Entity>(this, MessageTypes.TimetableUpdated, EntityChanged);
            void EntityChanged(Application app, Entity entity)
            {
                if (timetableInfoList.Entities.Contains(entity))
                    UpdateEvents();
            }
            MessagingCenter.Subscribe<Application, AppTheme>(this, MessageTypes.ThemeChanged, async (sender, newTheme) =>
            {
                if (timetableInfoList.Events.Count == 0)
                    return;

                needToUpdateEventsUI = true;
                await UpdateEventsWithUI();
            });
            MessagingCenter.Subscribe<Application, List<SavedEntity>>(this, MessageTypes.SelectedEntitiesChanged, (sender, newSelectedEntities) =>
            {
                IsTimetableUpdating = updatingTimetables.Intersect(newSelectedEntities.Cast<Entity>()).Any();
                UpdateEvents(newSelectedEntities.ToList<Entity>());
            });
            MessagingCenter.Subscribe<Application, Entity>(this, MessageTypes.TimetableUpdating, (sender, entity) =>
            {
                updatingTimetables.Add(entity);
                IsTimetableUpdating = updatingTimetables.Contains(entity);
            });
            MessagingCenter.Subscribe<Application, Entity>(this, MessageTypes.TimetableUpdated, (sender, entity) =>
            {
                updatingTimetables.Remove(entity);
                IsTimetableUpdating = updatingTimetables.Contains(entity);
            });

            PageAppearingCommand = CommandHelper.Create(PageAppearing);
            HideSelectedEventsCommand = CommandHelper.Create(HideSelectedEventsClicked, _ => !IsNoSourceLayoutVisible);
            ScheduleModeCommand = CommandHelper.Create(ScheduleModeClicked, _ => !IsNoSourceLayoutVisible);
            BTodayClickedCommand = CommandHelper.Create(BTodayClicked);
            PageDisappearingCommand = CommandHelper.Create(() => isPageVisible = false);
            TimetableCellTappedCommand = CommandHelper.Create<CellTappedEventArgs>(e => DisplayEventDetails((Event)e.Appointment));
            TimetableMonthInlineAppointmentTappedCommand = CommandHelper.Create<MonthInlineAppointmentTappedEventArgs>((e) => DisplayEventDetails((Event)e.Appointment));
            TimetableMonthInlineLoadedCommand = CommandHelper.Create<MonthInlineLoadedEventArgs>(e =>
            {
                e.monthInlineViewStyle = new()
                {
                    BackgroundColor = ResourceManager.PageBackgroundColor,
                    TimeTextFormat = "HH:mm",
                };
            });
            TimetableVisibleDatesChangedCommand = CommandHelper.Create<VisibleDatesChangedEventArgs>(async (e) =>
            {
                if (e is null)
                    return;

                visibleDates = e.visibleDates;
                await UpdateTodayButton(false);
            });
            UpdateTimetableCommand = CommandHelper.Create(async () => 
            {
                string responce = await TimetableService.Update(timetableInfoList.Entities.ToList());
                if (responce is null)
                    return;

                await Shell.Current.DisplayAlert(LN.TimetableUpdate, responce, LN.Ok);
            }, () => timetableInfoList.Timetables.Any() && !IsTimetableUpdating);
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
                    timetablePage.TimetableNavigateTo(visibleDates.First());
                }
            }

            Device.StartTimer(TimeSpan.FromSeconds(1), () => { UpdateTimeLeft(); return isPageVisible; });
        }

        private void UpdateTimeLeft()
        {
            if (timetableInfoList.Events.Count == 0)
            {
                IsTimeLeftVisible = false;
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
                if (string.IsNullOrEmpty(text) && IsTimeLeftVisible)
                {
                    IsTimeLeftVisible = false;
                }
            }
            else
            {
                TimeLeftText = text;
                IsTimeLeftVisible = true;
            }

            if (IsTimeLeftVisible != lastTimeLeftVisible)
            {
                lastTimeLeftVisible = IsTimeLeftVisible;
            }
        }

        private async Task UpdateEventsWithUI(bool reloadSavedEntities = false)
        {
            await Task.Run(async () =>
            {
                IsProgressLayoutVisible = true;

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

                IsProgressLayoutVisible = false;
            });
        }

        /// <summary>
        /// Updates events for already displayed entities
        /// </summary>
        private void UpdateEvents() => 
            UpdateEvents(timetableInfoList.Entities.ToList());

        private void UpdateEvents(List<Entity> selectedEntities)
        {
            if (selectedEntities is null || !selectedEntities.Any())
            {
                Title = LN.AppName;
                timetableInfoList = TimetableInfoList.Empty;
                NoSourceLayoutText = LN.NoTimetable;
                IsNoSourceLayoutVisible = true;
                return;
            }

            Title = string.Join(", ", selectedEntities.Select(se => se.Name));

            List<TimetableInfo> timetableInfos = new();
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
                    needToUpdateEventsUI = true;
                    IsNoSourceLayoutVisible = false;
                }
                else
                {
                    NoSourceLayoutText = LN.TimetableIsEmpty;
                    IsNoSourceLayoutVisible = true;
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

                if (timetableInfoList.Events.Count == 0)
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
                    .Select(ev => new EventViewModel(ev) { ShowTime = TimetableScheduleView != ScheduleView.MonthView })
                    .ToList();

                UpdateTimeLeft();
            }
        }

        private async Task ScheduleModeClicked()
        {
            if (IsNoSourceLayoutVisible)
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

            needToUpdateEventsUI = true;
            UpdateEventsUI();
        }

        private async Task DisplayEventDetails(Event ev)
        {
            if (ev is null)
                return;

            await PopupNavigation.Instance.PushAsync(new EventPopupPage
            {
                BindingContext = new EventPopupViewModel(ev, timetableInfoList)
            });
        }

        private async Task HideSelectedEventsClicked()
        {
            if (IsNoSourceLayoutVisible)
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
            _ = Shell.Current.CurrentPage.DisplayToastAsync(message, 1500);

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
