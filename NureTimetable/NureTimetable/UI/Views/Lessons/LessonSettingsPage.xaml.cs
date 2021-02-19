using NureTimetable.UI.ViewModels;
using Xamarin.Forms;

namespace NureTimetable.UI.Views
{
    public partial class LessonSettingsPage : ContentPage
    {
        public LessonSettingsPage()
        {
            InitializeComponent();
        }

        private void ListView_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            ((ListView)sender).SelectedItem = null;
        }

        protected override bool OnBackButtonPressed()
        {
            (BindingContext as LessonSettingsViewModel).BackButtonPressedCommand.Execute(null);
            return true;
        }
    }
}