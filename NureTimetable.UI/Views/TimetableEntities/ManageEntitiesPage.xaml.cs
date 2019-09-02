using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace NureTimetable.UI.Views.TimetableEntities
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ManageEntitiesPage : ContentPage
    {
        public ManageEntitiesPage()
        {
            InitializeComponent();
        }
        
        private void EntitiesList_OnItemTapped(object sender, ItemTappedEventArgs e)
        {
            ((ListView)sender).SelectedItem = null;
        }
    }
}
