using NureTimetable.UI.ViewModels.Info;
using Xamarin.Forms;

namespace NureTimetable.UI.Views
{
    public partial class SettingsPage : ContentPage
    {
        public SettingsPage()
        {
            InitializeComponent();
            BindingContext = new SettingsViewModel();
        }
    }
}