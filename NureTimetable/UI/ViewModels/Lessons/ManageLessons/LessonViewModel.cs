namespace NureTimetable.UI.ViewModels;

public class LessonViewModel : BaseViewModel
{
    private readonly TimetableInfo timetableInfo;
    private readonly ManageLessonsViewModel manageLessonsViewModel;

    #region Properties
    private LessonInfo _lessonInfo;
    public LessonInfo LessonInfo { get => _lessonInfo; set { _lessonInfo = value; OnPropertyChanged(nameof(IsChecked)); } }

    public bool HasUpcomingEvents { get; }

    public string MainTeacherNames { get; } = "-";

    public bool? IsChecked
    {
        get => LessonInfo.Settings.Hiding.ShowLesson;
        set
        {
            if (IsChecked == value)
                return;

            LessonInfo.Settings.Hiding.ShowLesson = value;
            OnPropertyChanged();
            manageLessonsViewModel.HasUnsavedChanges = true;
        }
    }

    public IRelayCommand SettingsClickedCommand { get; }
    public IRelayCommand InfoClickedCommand { get; }
    #endregion

    public LessonViewModel(LessonInfo lessonInfo, bool hasUpcomingEvents, TimetableInfo timetableInfo, ManageLessonsViewModel manageLessonsViewModel)
    {
        this._lessonInfo = lessonInfo;
        this.HasUpcomingEvents = hasUpcomingEvents;
        var mainTeachers = timetableInfo
            .Events.FirstOrDefault(e => e.Lesson == LessonInfo.Lesson && e.Type.EnglishBaseName == "lecture")?
            .Teachers.Select(t => t.ShortName);
        if (mainTeachers != null)
        {
            this.MainTeacherNames = string.Join(", ", mainTeachers);
        }

        this.timetableInfo = timetableInfo;
        this.manageLessonsViewModel = manageLessonsViewModel;

        MessagingCenter.Subscribe<LessonSettingsViewModel, LessonInfo>(this, MessageTypes.OneLessonSettingsChanged, (sender, newLessonSettings) =>
        {
            if (LessonInfo.Lesson != newLessonSettings.Lesson)
                return;

            LessonInfo = newLessonSettings;
            manageLessonsViewModel.HasUnsavedChanges = true;
        });

        SettingsClickedCommand = CommandFactory.Create(SettingsClicked);
        InfoClickedCommand = CommandFactory.Create(InfoClicked);
    }

    private Task SettingsClicked() =>
        Navigation.PushAsync(new LessonSettingsPage
        {
            BindingContext = new LessonSettingsViewModel(LessonInfo, timetableInfo, false)
        });

    private Task InfoClicked() =>
        Navigation.PushAsync(new LessonInfoPage
        {
            BindingContext = new LessonInfoViewModel(LessonInfo, timetableInfo)
        });
}
