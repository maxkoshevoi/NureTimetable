using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using NureTimetable.Views;
using Syncfusion.Licensing;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace NureTimetable
{
    public partial class App : Application
    {

        public App()
        {
            //Register Syncfusion license
            SyncfusionLicenseProvider.RegisterLicense("Njc3NDVAMzEzNjJlMzQyZTMwYUJReFEwbjdlU1BNSndTYTcramc0cmdxL2dDdEFVT0syOU5xa2hlLzdhOD0=");

            InitializeComponent();

            MainPage = new MainPage();
        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
