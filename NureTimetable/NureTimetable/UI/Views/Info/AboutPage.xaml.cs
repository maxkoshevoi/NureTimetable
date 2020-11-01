using NureTimetable.UI.ViewModels.Info;
using Xamarin.Forms;

namespace NureTimetable.UI.Views
{
    public partial class AboutPage : ContentPage
    {
        public AboutPage()
        {
            InitializeComponent();
            BindingContext = new AboutViewModel();
        }
    }
}