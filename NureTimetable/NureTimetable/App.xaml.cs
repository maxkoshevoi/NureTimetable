using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using NureTimetable.Core.Localization;
using NureTimetable.Core.Models.Consts;
using NureTimetable.DAL.Settings;
using NureTimetable.DAL.Settings.Models;
using Syncfusion.Licensing;
using System.Globalization;
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
            SyncfusionLicenseProvider.RegisterLicense(Keys.SyncfusionLicenseKey);
            InitLanguage();
            VersionTracking.Track();
#if DEBUG
            SettingsRepository.Settings.IsDebugMode = true;
#endif

            InitializeComponent();
            MainPage = new AppShell();
        }

        private static void InitLanguage()
        {
            LocalizationResourceManager.Current.PropertyChanged += (_, _) => LN.Culture = LocalizationResourceManager.Current.CurrentCulture;
            LocalizationResourceManager.Current.Init(LN.ResourceManager, GetCurrentCulture());
            SettingsRepository.Settings.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(SettingsRepository.Settings.Language))
                {
                    LocalizationResourceManager.Current.CurrentCulture = GetCurrentCulture();
                }
            };

            static CultureInfo GetCurrentCulture()
            {
                if (SettingsRepository.Settings.Language != AppLanguage.FollowSystem)
                {
                    return new CultureInfo((int)SettingsRepository.Settings.Language);
                }
                else if (CultureInfo.CurrentCulture.TwoLetterISOLanguageName == "ru")
                {
                    return new CultureInfo((int)AppLanguage.Ukrainian);
                }

                return CultureInfo.CurrentCulture;
            }
        }

        protected override async void OnStart()
        {
            await InitAppCenterLogging();
        }

        private static async Task InitAppCenterLogging()
        {
            bool showCrashLog = true;
            bool isEnabled = false;
#if RELEASE
            showCrashLog = SettingsRepository.Settings.IsDebugMode;
            if (DeviceInfo.DeviceType != DeviceType.Virtual)
            {
                isEnabled = true;
            }
#endif
            AppCenter.IsNetworkRequestsAllowed = isEnabled;
            AppCenter.Start(Keys.MicrosoftAppCenterKey, typeof(Analytics), typeof(Crashes));

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
