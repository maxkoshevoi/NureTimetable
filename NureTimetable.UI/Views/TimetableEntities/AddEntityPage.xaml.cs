using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace NureTimetable.UI.Views.TimetableEntities
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AddEntityPage : ContentPage
    {

        public AddEntityPage()
        {
            InitializeComponent();
        }

        void Handle_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            ((ListView)sender).SelectedItem = null;
        }
    }
}
