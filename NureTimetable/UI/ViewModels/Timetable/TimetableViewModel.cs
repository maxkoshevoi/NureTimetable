using NureTimetable.UI.Models.Consts;
using NureTimetable.UI.ViewModels.Timetable;
using Rg.Plugins.Popup.Services;
using Syncfusion.Maui.Scheduler;

namespace NureTimetable.UI.ViewModels;

public partial class TimetableViewModel : BaseViewModel
{
    #region Variables
    private readonly ITimetablePageCommands timetablePage;

    private bool isFirstLoad = true;
    private bool isPageVisible = false;
    private (Entity[] entities, DateTime updateStart) lastAutoupdateInfo = (Array.Empty<Entity>(), default);

    private List<DateTime> visibleDates = new();
    private bool needToUpdateEventsUI = false;

    // Lock objects
    private readonly object enumeratingEvents = new();
    private readonly object updatingEventsUI = new();
    #endregion

    #region Properties

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(HideSelectedEventsCommand))]
    [NotifyCanExecuteChangedFor(nameof(ScheduleModeCommand))]
    [NotifyCanExecuteChangedFor(nameof(UpdateTimetableCommand))]
    private TimetableInfoList _timetableInfoList = TimetableInfoList.Empty;

    // Toolbar
    [ObservableProperty]
    private string _hideSelectedEventsIcon = MaterialIconsFont.Filter;
    private bool applyHiddingSettings = true;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(UpdateTimetableCommand))]
    private bool _isTimetableUpdating = false;
    private readonly List<Entity> updatingTimetables = new();

    // Timetable
    public TimeSpan TimetableTimeInterval => TimeSpan.FromHours(1);

    [ObservableProperty] private DateTime? _timetableSelectedDate;
    [ObservableProperty] private SchedulerView _timetableScheduleView = SchedulerView.Week;
    [ObservableProperty] private string _timetableLocale = string.Empty;
    [ObservableProperty] private List<EventViewModel>? _timetableDataSource;
    [ObservableProperty] private DateTime _timetableMaxDisplayDate;
    [ObservableProperty] private DateTime _timetableMinDisplayDate;
    [ObservableProperty] private double _timetableStartHour = 0;
    [ObservableProperty] private double _timetableEndHour = 24;

    // TimeLeft
    private bool lastTimeLeftVisible;
    [ObservableProperty] private bool _isTimeLeftVisible;
    [ObservableProperty] private string? _timeLeftText;

    public double AttendanceOpacity => AttendanceClickCommand.CanExecute(null) ? 1 : 0.5;

    // Layouts
    [ObservableProperty]
    private bool _isProgressLayoutVisible;

    // bToday
    [ObservableProperty] private string _bTodayText = string.Empty;
    [ObservableProperty] private double _bTodayScale = 0;

    public IRelayCommand PageAppearingCommand { get; }
    public IRelayCommand PageDisappearingCommand { get; }
    public IRelayCommand HideSelectedEventsCommand { get; }
    public IRelayCommand ScheduleModeCommand { get; }
    public IRelayCommand<SchedulerTappedEventArgs> TimetableCellTappedCommand { get; }
    public IRelayCommand<SchedulerViewChangedEventArgs> TimetableVisibleDatesChangedCommand { get; }
    public IRelayCommand BTodayClickedCommand { get; }
    public IRelayCommand UpdateTimetableCommand { get; }
    public IRelayCommand AttendanceClickCommand { get; }
    #endregion

    public TimetableViewModel(ITimetablePageCommands timetablePage)
    {
        this.timetablePage = timetablePage;

        Title = new(() => LN.AppName);
        lastTimeLeftVisible = IsTimeLeftVisible;

        SetTimetableLocale();
        LocalizationResourceManager.Current.PropertyChanged += (_, _) => SetTimetableLocale();

        TimetableScheduleView = SettingsRepository.Settings.TimetableViewMode switch
        {
            TimetableViewMode.Day => SchedulerView.Day,
            TimetableViewMode.Week => SchedulerView.Week,
            TimetableViewMode.Month => SchedulerView.Month,
            _ => TimetableScheduleView
        };

        MessagingCenter.Subscribe<Application, Entity>(this, MessageTypes.TimetableUpdating, (_, entity) =>
        {
            IsTimetableUpdating = TimetableInfoList.Entities.Contains(entity);
            lock (updatingTimetables)
            {
                updatingTimetables.Add(entity);
            }
        });
        MessagingCenter.Subscribe<Application, Entity>(this, MessageTypes.TimetableUpdated, async (_, entity) =>
        {
            lock (updatingTimetables)
            {
                updatingTimetables.Remove(entity);
                IsTimetableUpdating = TimetableInfoList.Entities.Intersect(updatingTimetables).Any();
            }

            if (TimetableInfoList.Entities.Contains(entity) && !IsTimetableUpdating)
                await UpdateEvents();
        });
        MessagingCenter.Subscribe<Application, Entity>(this, MessageTypes.LessonSettingsChanged, async (_, entity) =>
        {
            if (TimetableInfoList.Entities.Contains(entity))
                await UpdateEvents();
        });
        MessagingCenter.Subscribe<Application, AppTheme>(this, MessageTypes.ThemeChanged, async (_, _) =>
        {
            if (TimetableInfoList.Events.None())
                return;

            needToUpdateEventsUI = true;
            await UpdateEventsWithUI();
        });
        MessagingCenter.Subscribe<Application, List<SavedEntity>>(this, MessageTypes.SelectedEntitiesChanged, async (_, newSelectedEntities) =>
        {
            List<Entity> newEntities = newSelectedEntities.Select(e => e.Entity).ToList();
            lock (updatingTimetables)
            {
                IsTimetableUpdating = newEntities.Intersect(updatingTimetables).Any();
            }
            await UpdateEvents(newEntities);
            await UpdateTimetableIfNeeded();
        });

        PageAppearingCommand = CommandFactory.Create(PageAppearing);
        HideSelectedEventsCommand = CommandFactory.Create(HideSelectedEventsClicked, () => TimetableInfoList.Events.Any());
        ScheduleModeCommand = CommandFactory.Create(ScheduleModeClicked, () => TimetableInfoList.Events.Any());
        BTodayClickedCommand = CommandFactory.Create(BTodayClicked);
        PageDisappearingCommand = CommandFactory.Create(() => isPageVisible = false);
        TimetableCellTappedCommand = CommandFactory.Create<SchedulerTappedEventArgs>(e => DisplayEventDetails((Event)e!.Appointments!.Single()));
        TimetableVisibleDatesChangedCommand = CommandFactory.Create<SchedulerViewChangedEventArgs>(async (e) =>
        {
            if (e?.NewVisibleDates == null)
                return;

            visibleDates = e.NewVisibleDates;
            await UpdateTodayButton(false);
        });
        UpdateTimetableCommand = CommandFactory.Create(
            () => TimetableService.UpdateAndDisplayResultAsync(TimetableInfoList.Entities.ToArray()),
            () => TimetableInfoList.Timetables.Any() && !IsTimetableUpdating
        );
        AttendanceClickCommand = CommandFactory.Create(OpenAttendancePage);
        AttendanceClickCommand.CanExecuteChanged += (_, args) => OnPropertyChanged(nameof(AttendanceOpacity));

        IsProgressLayoutVisible = true;

        void SetTimetableLocale()
        {
            string activeCultureCode = Cultures.SupportedCultures[0].TwoLetterISOLanguageName;
            if (Cultures.SupportedCultures.Any(c => c.TwoLetterISOLanguageName == CultureInfo.CurrentCulture.TwoLetterISOLanguageName))
            {
                // Set custom TimetableLocale only if it is one of supported cultures
                activeCultureCode = LocalizationResourceManager.Current.CurrentCulture.TwoLetterISOLanguageName;
            }
            TimetableLocale = activeCultureCode;
        }
    }

    private async Task PageAppearing()
    {
        isPageVisible = true;

        if (isFirstLoad)
        {
            isFirstLoad = false;

            await AppShell.PerformMigrations();

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

            // Updating current date if it's changed
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
        await UpdateTimetableIfNeeded();
    }

    private async Task UpdateTodayButton(bool isForceUpdate)
    {
        if (visibleDates.None())
            return;

        // Updating Today button
        if (visibleDates.Any(d => d.Date == DateTime.Today))
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

    private void UpdateTimeLeft()
    {
        if (TimetableInfoList.Events.None())
        {
            IsTimeLeftVisible = false;
            return;
        }

        string? text = null;
        lock (enumeratingEvents)
        {
            Event? currentEvent = TimetableInfoList.CurrentEvent();
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
                Event? nextEvent = TimetableInfoList.NextEvent();
                if (nextEvent != null && nextEvent.Start.Date == DateTime.Today)
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

        lastTimeLeftVisible = IsTimeLeftVisible;
    }

    private Task UpdateEventsWithUI(bool reloadSavedEntities = false) => Task.Run(async () =>
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
                List<Entity> selectedEntities = (await UniversityEntitiesRepository.GetSavedAsync())
                    .Where(e => e.IsSelected)
                    .Select(e => e.Entity)
                    .ToList();
                await UpdateEvents(selectedEntities);
            }
            else
            {
                await UpdateEvents();
            }
        }

        IsProgressLayoutVisible = false;
    });

    /// <summary>
    /// Updates events for already displayed entities
    /// </summary>
    private Task UpdateEvents() =>
        UpdateEvents(TimetableInfoList.Entities.ToList());

    private async Task UpdateEvents(List<Entity> selectedEntities)
    {
        if (selectedEntities == null || selectedEntities.None())
        {
            Title = new(() => LN.AppName);
            TimetableInfoList = TimetableInfoList.Empty;
            return;
        }

        Title = new(() => string.Join(", ", selectedEntities.Select(se => se.Name).GroupBasedOnLastPart()));

        List<TimetableInfo> timetableInfos = new();
        foreach (var entity in selectedEntities)
        {
            TimetableInfo? timetableInfo = await EventsRepository.GetTimetableLocalAsync(entity);
            timetableInfos.Add(timetableInfo ?? new(entity));
        }
        lock (enumeratingEvents)
        {
            TimetableInfoList = TimetableInfoList.Build(timetableInfos, applyHiddingSettings);
            if (TimetableInfoList.Events.Any())
            {
                needToUpdateEventsUI = true;
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

            if (TimetableInfoList.Events.None())
            {
                TimetableDataSource = null;
                return;
            }

            lock (enumeratingEvents)
            {
                TimetableMinDisplayDate = TimetableInfoList.StartDate();
                TimetableMaxDisplayDate = TimetableInfoList.EndDate();

                TimetableEndHour = 24;
                TimetableStartHour = TimetableInfoList.StartTime().Hours;
                TimetableEndHour = ((TimetableStartHour * 60d) + TimetableTimeInterval.TotalMinutes * Math.Ceiling((TimetableInfoList.EndTime().TotalMinutes - (TimetableStartHour * 60d)) / TimetableTimeInterval.TotalMinutes)) / 60d;
            }

            TimetableDataSource = TimetableInfoList.Events
                .Select(ev => new EventViewModel(ev) { ShowTime = TimetableScheduleView != SchedulerView.Month })
                .ToList();

            UpdateTimeLeft();
        }
    }

    private async Task UpdateTimetableIfNeeded()
    {
        if (!SettingsRepository.Settings.Autoupdate || !isPageVisible)
            return;

        // Try to update the same entities at most every 15 minutes
        Entity[] entitiesToUpdate = TimetableInfoList.Entities.ToArray();
        if (lastAutoupdateInfo.entities.SequenceEqual(entitiesToUpdate) && (DateTime.Now - lastAutoupdateInfo.updateStart).TotalMinutes < 15)
            return;

        lastAutoupdateInfo = (entitiesToUpdate, DateTime.Now);

        var updateResult = await TimetableService.UpdateAsync(entitiesToUpdate);
        if (updateResult.Any(e => e.exception != null))
        {
            timetablePage.DisplayToastAsync(LN.AutoupdateFailed).Forget();
        }
    }

    private async Task ScheduleModeClicked()
    {
        string displayMode = await Shell.Current.DisplayActionSheet(LN.ChooseDisplayMode, LN.Cancel, null, LN.Day, LN.Week, LN.Month);

        DateTime? selected = TimetableSelectedDate;
        TimetableSelectedDate = null;
        if (displayMode == LN.Day)
        {
            TimetableScheduleView = SchedulerView.Day;
            SettingsRepository.Settings.TimetableViewMode = TimetableViewMode.Day;
        }
        else if (displayMode == LN.Week)
        {
            TimetableScheduleView = SchedulerView.Week;
            SettingsRepository.Settings.TimetableViewMode = TimetableViewMode.Week;
        }
        else if (displayMode == LN.Month)
        {
            TimetableScheduleView = SchedulerView.Month;
            SettingsRepository.Settings.TimetableViewMode = TimetableViewMode.Month;
        }

        if (selected != null)
        {
            TimetableSelectedDate = selected;
            TimetableSelectedDate = null;
        }

        needToUpdateEventsUI = true;
        UpdateEventsUI();
    }

    private async Task DisplayEventDetails(Event ev)
    {
        if (ev == null)
            return;

        await PopupNavigation.Instance.PushAsync(new EventPopupPage
        {
            BindingContext = new EventPopupViewModel(ev, TimetableInfoList, timetablePage)
        });
    }

    private async Task HideSelectedEventsClicked()
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

        await UpdateEventsWithUI();

        timetablePage.DisplayToastAsync(message, 1500).Forget();
    }

    private void BTodayClicked()
    {
        DateTime moveTo = DateTime.Today;
        if (TimetableMinDisplayDate > moveTo)
        {
            moveTo = TimetableMinDisplayDate;
        }
        else if (TimetableMaxDisplayDate < moveTo)
        {
            moveTo = TimetableMaxDisplayDate;
        }
        if (moveTo != DateTime.Today && visibleDates.Contains(moveTo))
        {
            timetablePage.DisplayToastAsync(LN.TimetableEndReached).Forget();
            return;
        }

        timetablePage.TimetableNavigateTo(moveTo);
    }

    private async Task OpenAttendancePage()
    {
        Analytics.TrackEvent("Moodle: Open attendance");

        var currentEvent = TimetableInfoList.CurrentEvent() ?? TimetableInfoList.NextEvent();
        if (currentEvent == null)
        {
            await Shell.Current.DisplayAlert(LN.SomethingWentWrong, LN.NoUpcomingEvent, LN.Ok);
            return;
        }

        var currentTimetable = TimetableInfoList.Timetables.Count == 1 ? TimetableInfoList.Timetables.Single() : null;
        var (attendanceUrl, errorMessage) = await DlNureService.GetAttendanceUrlAsync(currentEvent.Lesson, currentTimetable);
        if (attendanceUrl == null)
        {
            await Shell.Current.DisplayAlert(LN.SomethingWentWrong, errorMessage!, LN.Ok);
            return;
        }

        await Browser.OpenAsync(attendanceUrl);
    }
}
