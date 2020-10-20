using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using NureTimetable.Core.Localization;
using NureTimetable.Core.Models.Consts;
using NureTimetable.UI.Views;
using Syncfusion.Licensing;
using System.Globalization;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace NureTimetable
{
    public partial class App : Application
    {
        public static bool IsDebugMode { get; set; }
#if DEBUG
            = true;
#else
            = false;
#endif

        public App()
        {
            //Register Syncfusion license
            SyncfusionLicenseProvider.RegisterLicense(Keys.SyncfusionLicenseKey);
            
            // Force Russian language for Ukraine
            // TODO: Translate application to ukrainian
            if (CultureInfo.CurrentCulture.TwoLetterISOLanguageName == "uk")
            {
                LN.Culture = CultureInfo.CurrentCulture = new CultureInfo("ru");
            }
            Bugfix.InitCalendarCrashFix();
            VersionTracking.Track();

            InitializeComponent();
            MainPage = new MainPage();
        }

        protected override void OnStart()
        {
#if !DEBUG
            //Register Microsoft App Center metrics
            if (DeviceInfo.DeviceType != DeviceType.Virtual)
            {
                AppCenter.Start(Keys.MicrosoftAppCenterKey, typeof(Analytics), typeof(Crashes));
            }
#endif
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
