using NureTimetable.Models.InterplatformCommunication;
using System;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace NureTimetable.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AboutPage : ContentPage
    {
        public AboutPage()
        {
            InitializeComponent();

            var version = DependencyService.Get<IAppVersionProvider>();
            SVersion.Text = version?.AppVersion ?? "1.0";
            SwDebugMode.IsToggled = App.IsDebugMode;
        }

        private void Button_Clicked(object sender, EventArgs e)
        {
            Device.OpenUri(new Uri("https://github.com/maxkoshevoi/NureTimetable"));
        }

        private void SwDebugMode_Toggled(object sender, ToggledEventArgs e)
        {
            App.IsDebugMode = SwDebugMode.IsToggled;
        }
    }
}