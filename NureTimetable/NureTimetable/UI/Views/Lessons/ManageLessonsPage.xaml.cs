using NureTimetable.DAL.Settings;
using NureTimetable.UI.ViewModels;
using Xamarin.Forms;

namespace NureTimetable.UI.Views
{
    public partial class ManageLessonsPage : ContentPage
    {
        public ManageLessonsPage()
        {
            InitializeComponent();

            // Remove SyncDl button if no DL user
            if (SettingsRepository.Settings.DlNureUser == null)
            {
                ToolbarItems.RemoveAt(0);
            }
        }

        protected override bool OnBackButtonPressed()
        {
            (BindingContext as ManageLessonsViewModel)!.BackButtonPressedCommand.Execute(null);
            return true;
        }
    }
}
