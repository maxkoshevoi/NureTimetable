using Microsoft.Maui.Controls;

namespace NureTimetable.UI.Views
{
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
