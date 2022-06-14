using Microsoft.AppCenter;
using System.Text;

namespace NureTimetable;

public partial class App : Application
{
    public App()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        InitLanguage();
#if DEBUG
        SettingsRepository.Settings.IsDebugMode = true;
#endif

        InitializeComponent();
        MainPage = new AppShell();
    }

    private static void InitLanguage()
    {
        CultureInfo culture = CultureInfo.CurrentCulture;
        if (SettingsRepository.Settings.Language != AppLanguage.FollowSystem)
        {
            culture = new CultureInfo((int)SettingsRepository.Settings.Language);
        }
        LocalizationResourceManager.Current.PropertyChanged += (_, _) => LN.Culture = LocalizationResourceManager.Current.CurrentCulture;
        LocalizationResourceManager.Current.Init(LN.ResourceManager, culture);
        SettingsRepository.Settings.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(SettingsRepository.Settings.Language))
            {
                CultureInfo newLanguage = CultureInfo.CurrentCulture;
                if (SettingsRepository.Settings.Language != AppLanguage.FollowSystem)
                {
                    newLanguage = new CultureInfo((int)SettingsRepository.Settings.Language);
                }
                LocalizationResourceManager.Current.CurrentCulture = newLanguage;
            }
        };
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
}
