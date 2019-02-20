using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace NureTimetable.UI.Views.Groups
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AddGroupPage : ContentPage
    {

        public AddGroupPage()
        {
            InitializeComponent();
        }

        async void Handle_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            ((ListView)sender).SelectedItem = null;
        }



    }
}
