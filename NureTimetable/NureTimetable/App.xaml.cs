using NureTimetable.Core.Models.Consts;
using NureTimetable.Views;
using Syncfusion.Licensing;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using NureTimetable.DAL;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace NureTimetable
{
    public partial class App : Application
    {
        public static bool IsDebugMode
#if DEBUG
            = true;
#else
            = false;
#endif

        public App()
        {
            //Register Syncfusion license
            SyncfusionLicenseProvider.RegisterLicense(Keys.SyncfusionLicenseKey);
            
            InitializeComponent();
            MainPage = new MainPage();
        }

        protected override void OnStart()
        {
            // Handle when your app starts
            DALBase.Init();
#if !DEBUG
            //Register Microsoft App Center metrics
            AppCenter.Start(Keys.MicrosoftAppCenterKey, typeof(Analytics), typeof(Crashes));
#endif
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
