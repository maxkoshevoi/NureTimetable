using Xamarin.Forms;

namespace NureTimetable.UI.Views.Lessons
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
    }
}
