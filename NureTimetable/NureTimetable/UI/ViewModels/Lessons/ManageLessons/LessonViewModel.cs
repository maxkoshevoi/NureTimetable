using NureTimetable.DAL.Models.Local;
using NureTimetable.UI.Helpers;
using NureTimetable.UI.ViewModels.Core;
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
        #endregion

        #region Properties
        public LessonInfo LessonInfo { get; set; }

        public ICommand SettingsClickedCommand { get; }

        public ICommand InfoClickedCommand { get; }
        #endregion

        public LessonViewModel(INavigation navigation, LessonInfo lessonInfo, TimetableInfo timetableInfo) : base(navigation)
        {
            LessonInfo = lessonInfo;
            this.timetableInfo = timetableInfo;

            SettingsClickedCommand = CommandHelper.CreateCommand(SettingsClicked);
            InfoClickedCommand = CommandHelper.CreateCommand(InfoClicked);
        }

        private async Task SettingsClicked()
        {
            Navigation.PushAsync(new LessonSettingsPage
            {
                BindingContext = new LessonSettingsViewModel(Navigation, LessonInfo, timetableInfo)
            });
        }

        private async Task InfoClicked()
        {
            Navigation.PushAsync(new LessonInfoPage
            {
                BindingContext = new LessonInfoViewModel(Navigation, LessonInfo, timetableInfo)
            });
        }
    }
}
