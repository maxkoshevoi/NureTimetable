namespace NureTimetable.UI.ViewModels;

public class ManageLessonsViewModel : BaseViewModel
{
    private readonly Entity entity;

    #region Properties
    public bool HasUnsavedChanges { get; set; } = false;

    public ObservableRangeCollection<LessonViewModel> Lessons { get; } = new();

    public IRelayCommand PageAppearingCommand { get; }
    public IRelayCommand SaveClickedCommand { get; }
    public IRelayCommand SyncDlNureClickedCommand { get; }
    public IRelayCommand BackButtonPressedCommand { get; }
    #endregion

    public ManageLessonsViewModel(Entity entity)
    {
        Title = new(() => $"{LN.Lessons}: {entity.Name}");

        this.entity = entity;
        PageAppearingCommand = CommandFactory.Create(PageAppearing);
        BackButtonPressedCommand = CommandFactory.Create(BackButtonPressed);
        SaveClickedCommand = CommandFactory.Create(SaveClicked, () => Lessons.Any());
        SyncDlNureClickedCommand = CommandFactory.Create(SyncDlNureClecked, () => SettingsRepository.Settings.DlNureUser != null);

        Lessons.CollectionChanged += (_, _) =>
        {
            SaveClickedCommand.NotifyCanExecuteChanged();
            OnPropertyChanged(nameof(Lessons));
        };
    }

    private async Task PageAppearing()
    {
        if (Lessons.Any())
            return;

        TimetableInfo? timetable = await EventsRepository.GetTimetableLocalAsync(entity);
        if (timetable == null)
        {
            await Shell.Current.DisplayAlert(LN.LessonsManagement, LN.AtFirstLoadTimetable, LN.Ok);
            await Navigation.PopAsync();
            return;
        }

        Lessons.ReplaceRange
        (
            timetable.Lessons()
                .Select(lesson => timetable.GetAndAddLessonsInfo(lesson))
                .Select(lessonInfo => (lessonInfo, hasUpcomingEvents: timetable.Events.Where(e => e.Start >= DateTime.Today).Any(e => e.Lesson == lessonInfo.Lesson)))
                .OrderByDescending(model => model.hasUpcomingEvents).ThenBy(model => model.lessonInfo.Lesson.ShortName)
                .Select(model => new LessonViewModel(model.lessonInfo, model.hasUpcomingEvents, timetable, this))
        );
    }

    private async Task SaveClicked()
    {
        await EventsRepository.UpdateLessonsInfo(entity, Lessons.Select(l => l.LessonInfo).ToList());
        HasUnsavedChanges = false;

        await Shell.Current.GoToAsync("..", true);
        Toast.Make(string.Format(LN.EntityLessonSettingsSaved, entity.Name)).Show().Forget();
    }

    private async Task SyncDlNureClecked()
    {
        bool canProceed = await Shell.Current.DisplayAlert(LN.SyncDlNureLessonsTitle, LN.SyncDlNureLessonsMessage, LN.Yes, LN.Cancel);
        if (!canProceed)
        {
            return;
        }

        Analytics.TrackEvent("Moodle: Sync lessons");

        TimetableInfo? timetable = await EventsRepository.GetTimetableLocalAsync(entity);
        var lessonInfos = await DlNureService.AddMissingLessonIdsAsync(timetable!);

        foreach (var lesson in Lessons)
        {
            var lessonInfo = lessonInfos.FirstOrDefault(li => li.Lesson == lesson.LessonInfo.Lesson);
            if (lessonInfo?.DlNureInfo.LessonId == null)
            {
                lesson.IsChecked = false;
            }
        }
    }

    private async Task BackButtonPressed()
    {
        bool canClose = true;
        if (HasUnsavedChanges)
        {
            canClose = await Shell.Current.DisplayAlert(LN.UnsavedChangesTitle, LN.UnsavedChangesMessage, LN.Yes, LN.Cancel);
        }

        if (canClose)
        {
            await Shell.Current.GoToAsync("..", true);
        }
    }
}
