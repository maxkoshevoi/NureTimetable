using Xamarin.Forms;

namespace NureTimetable.UI.Views.Lessons
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
    }
}