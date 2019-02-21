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

        private void NavigateUrl_Handler(object sender, EventArgs e)
        {
            TappedEventArgs tappedEventArgs = (TappedEventArgs)e;
            NavigateUrl(tappedEventArgs.Parameter.ToString());
        }

        private void NavigateUrl(string url)
        {
            Device.OpenUri(new Uri(url));
        }

        private void SwDebugMode_Toggled(object sender, ToggledEventArgs e)
        {
            App.IsDebugMode = SwDebugMode.IsToggled;
        }
    }
}