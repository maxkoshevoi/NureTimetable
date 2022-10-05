using Microsoft.AppCenter.Analytics;
using NureTimetable.BL;
using NureTimetable.Core.BL;
using NureTimetable.Core.Extensions;
using NureTimetable.Core.Localization;
using NureTimetable.DAL.Settings;
using NureTimetable.Migrations;
using NureTimetable.Models.Consts;
using Xamarin.Essentials;
using Xamarin.Forms;
using AppTheme = NureTimetable.DAL.Settings.Models.AppTheme;

namespace NureTimetable;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        InitTheme();
        InitSettingsAnalytics();

        ExceptionService.ExceptionLogged += ex =>
        {
            if (SettingsRepository.Settings.IsDebugMode)
            {
                MainThread.BeginInvokeOnMainThread(() => Shell.Current.DisplayAlert(LN.ErrorDetails, ex.ToString(), LN.Ok));
            }
        };
    }

    private static void InitSettingsAnalytics()
    {
        string[] excludeFromLogging =
        {
            nameof(AppSettings.LastCistAllEntitiesUpdate)
        };

        SettingsRepository.Settings.PropertyChanged += (_, e) =>
        {
            if (excludeFromLogging.Contains(e.PropertyName))
            {
                return;
            }

            Analytics.TrackEvent($"Setting changed: {e.PropertyName}", new Dictionary<string, string?>
            {
                { "New Value", SettingsRepository.Settings
                    .GetType()
                    .GetProperty(e.PropertyName)?
                    .GetValue(SettingsRepository.Settings)?
                    .ToString() }
            });
        };
    }

    private static void InitTheme()
    {
        ThemeService.SetAppTheme();
        SettingsRepository.Settings.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(SettingsRepository.Settings.Theme))
                ThemeService.SetAppTheme();
        };
        App.Current.RequestedThemeChanged += (_, e) =>
        {
            if (SettingsRepository.Settings.Theme == AppTheme.FollowSystem)
                ThemeService.SetAppTheme();
        };
    }

    protected override bool OnBackButtonPressed()
    {
        if (Shell.Current.CurrentItem.CurrentItem.Stack.Count == 1 &&
            Shell.Current.CurrentState.Location.OriginalString != Route.EventsTab)
        {
            Shell.Current.GoToAsync(Route.EventsTab, true);
            return true;
        }

        return base.OnBackButtonPressed();
    }

    public static async Task PerformMigrations()
    {
        if (!VersionTracking.IsFirstLaunchForCurrentBuild)
            return;

        List<BaseMigration> migrationsToApply = await BaseMigration.Migrations.Where(m => m.IsNeedsToBeApplied()).ToListAsync();
        if (migrationsToApply.Any())
        {
            // Not Shell.Current.DisplayAlert cause Shell.Current is null here
            await App.Current.MainPage.DisplayAlert(LN.FinishingUpdateTitle, LN.FinishingUpdateDescription, LN.Ok);
            bool isSuccess = true;
            foreach (var migration in migrationsToApply)
            {
                if (!await migration.Apply())
                {
                    isSuccess = false;
                }
            }
            if (isSuccess)
            {
                App.Current.MainPage = new AppShell();
            }
            else
            {
                await App.Current.MainPage.DisplayAlert(LN.FinishingUpdateTitle, LN.SomethingWentWrong, LN.Ok);
            }
        }
    }
}