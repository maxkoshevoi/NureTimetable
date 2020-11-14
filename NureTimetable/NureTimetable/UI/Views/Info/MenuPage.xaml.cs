using NureTimetable.UI.ViewModels.Info;
using Xamarin.Forms;

namespace NureTimetable.UI.Views
{
    public partial class MenuPage : ContentPage
    {
        public MenuPage()
        {
            InitializeComponent();
            BindingContext = new MenuViewModel();
        }
    }
}