using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace NureTimetable.UI.Views.Lessons
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
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
