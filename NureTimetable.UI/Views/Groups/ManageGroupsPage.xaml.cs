using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace NureTimetable.UI.Views.Groups
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ManageGroupsPage : ContentPage
    {

        public ManageGroupsPage()
        {
            InitializeComponent();
        }
        
        private void GroupsList_OnItemTapped(object sender, ItemTappedEventArgs e)
        {
            ((ListView)sender).SelectedItem = null;
        }
    }
}
