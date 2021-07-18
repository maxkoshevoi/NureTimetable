using Microsoft.Maui.Controls;
using NureTimetable.UI.ViewModels;

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
            (BindingContext as LessonSettingsViewModel)!.BackButtonPressedCommand.Execute(null);
            return true;
        }
    }
}