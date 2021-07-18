using Microsoft.Maui.Controls;
using NureTimetable.UI.ViewModels;

namespace NureTimetable.UI.Views
{
    public partial class ManageLessonsPage : ContentPage
    {
        public ManageLessonsPage()
        {
            InitializeComponent();
        }

        private void Handle_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            ((ListView)sender).SelectedItem = null;
        }

        protected override bool OnBackButtonPressed()
        {
            (BindingContext as ManageLessonsViewModel)!.BackButtonPressedCommand.Execute(null);
            return true;
        }
    }
}
