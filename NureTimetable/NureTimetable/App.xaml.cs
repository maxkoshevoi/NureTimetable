using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using NureTimetable.Core.Localization;
using NureTimetable.Core.Models.Consts;
using NureTimetable.Core.Models.Settings;
using NureTimetable.DAL;
using NureTimetable.UI.Views;
using Syncfusion.Licensing;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Threading.Tasks;
using Xamarin.CommunityToolkit.Helpers;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace NureTimetable
{
    public partial class App : Application
    {
        public App()
        {
            //Register Syncfusion license
            SyncfusionLicenseProvider.RegisterLicense(Keys.SyncfusionLicenseKey);

            // Set user selected language for the app
            CultureInfo culture = CultureInfo.CurrentCulture;
            if (SettingsRepository.Settings.Language != AppLanguage.FollowSystem)
            {
                culture = new CultureInfo((int)SettingsRepository.Settings.Language);
            }
            LN.Culture = culture; // TODO: Romove once https://github.com/xamarin/XamarinCommunityToolkit/pull/915 is released
            LocalizationResourceManager.Current.PropertyChanged += (_, _) => LN.Culture = LocalizationResourceManager.Current.CurrentCulture;
            LocalizationResourceManager.Current.Init(LN.ResourceManager, culture);

            Bugfix.InitCalendarCrashFix();
            VersionTracking.Track();

            InitializeComponent();
            MainPage = new AppShell();
        }

        protected override async void OnStart()
        {
            await InitAppCenterLogging();
        }

        private static async Task InitAppCenterLogging()
        {
            bool showCrashLog = true;
            string key = Keys.MicrosoftAppCenterDebugKey;
#if RELEASE
            showCrashLog = SettingsRepository.Settings.IsDebugMode;
            if (DeviceInfo.DeviceType != DeviceType.Virtual)
            {
                key = Keys.MicrosoftAppCenterKey;
            }
#endif
            AppCenter.Start(key, typeof(Analytics), typeof(Crashes));

            // Log currect timetable view mode
            Analytics.TrackEvent("Timetable view mode", new Dictionary<string, string>
            {
                { nameof(SettingsRepository.Settings.TimetableViewMode), SettingsRepository.Settings.TimetableViewMode.ToString() }
            });

            // Display crash information
            if (showCrashLog && await Crashes.HasCrashedInLastSessionAsync())
            {
                var report = await Crashes.GetLastSessionCrashReportAsync();
                await Shell.Current.DisplayAlert(LN.ErrorDetails, report.StackTrace, LN.Ok);
            }
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
