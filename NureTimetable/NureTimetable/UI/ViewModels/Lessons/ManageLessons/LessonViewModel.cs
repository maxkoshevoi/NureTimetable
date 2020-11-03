using NureTimetable.Core.Models.Consts;
using NureTimetable.DAL.Models.Local;
using NureTimetable.UI.Helpers;
using NureTimetable.UI.ViewModels.Lessons.LessonSettings;
using NureTimetable.UI.Views.Lessons;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace NureTimetable.UI.ViewModels.Lessons.ManageLessons
{
    public class LessonViewModel : BaseViewModel
    {
        #region Variables
        private readonly TimetableInfo timetableInfo;
        private readonly ManageLessonsViewModel manageLessonsViewModel;
        private LessonInfo lessonInfo;
        #endregion

        #region Properties
        public LessonInfo LessonInfo { get => lessonInfo; set { lessonInfo = value; OnPropertyChanged(nameof(IsChecked)); } }

        public bool? IsChecked 
        { 
            get => LessonInfo.Settings.Hiding.ShowLesson;
            set
            {
                if (IsChecked == value)
                {
                    return;
                }

                LessonInfo.Settings.Hiding.ShowLesson = value;
                OnPropertyChanged();
                manageLessonsViewModel.HasUnsavedChanes = true;
            } 
        }

        public ICommand SettingsClickedCommand { get; }

        public ICommand InfoClickedCommand { get; }
        #endregion

        public LessonViewModel(LessonInfo lessonInfo, TimetableInfo timetableInfo, ManageLessonsViewModel manageLessonsViewModel)
        {
            this.LessonInfo = lessonInfo;
            this.timetableInfo = timetableInfo;
            this.manageLessonsViewModel = manageLessonsViewModel;

            MessagingCenter.Subscribe<LessonSettingsViewModel, LessonInfo>(this, MessageTypes.OneLessonSettingsChanged, (sender, newLessonSettings) =>
            {
                if (LessonInfo.Lesson != newLessonSettings.Lesson)
                {
                    return;
                }

                LessonInfo = newLessonSettings;
                manageLessonsViewModel.HasUnsavedChanes = true;
            });

            SettingsClickedCommand = CommandHelper.CreateCommand(SettingsClicked);
            InfoClickedCommand = CommandHelper.CreateCommand(InfoClicked);
        }

        private async Task SettingsClicked()
        {
            await Navigation.PushAsync(new LessonSettingsPage
            {
                BindingContext = new LessonSettingsViewModel(LessonInfo, timetableInfo)
            });
        }

        private async Task InfoClicked()
        {
            await Navigation.PushAsync(new LessonInfoPage
            {
                BindingContext = new LessonInfoViewModel(LessonInfo, timetableInfo)
            });
        }
    }
}
