using CommunityToolkit.Maui.Core.Platform;
using NureTimetable.Platforms.Android.Models;
using NureTimetable.Platforms.Android.Services;
using NureTimetable.UI.Models.Consts;
using NureTimetable.UI.Themes;

namespace NureTimetable.BL;

public static class ThemeService
{
    public static bool SetAppTheme()
    {
        AppTheme selectedTheme = SettingsRepository.Settings.Theme;

        UpdateNativeStyle(selectedTheme);
        UpdateAppStyle(selectedTheme);
        UpdateBarStyle().Forget();

        MessagingCenter.Send(Application.Current, MessageTypes.ThemeChanged, selectedTheme);
        return true;
    }

    private static void UpdateNativeStyle(AppTheme selectedTheme)
    {
        NightModeStyle style = selectedTheme switch
        {
            AppTheme.Dark => NightModeStyle.Yes,
            AppTheme.Light => NightModeStyle.No,
            AppTheme.FollowSystem => NightModeStyle.FollowSystem,
            _ => throw new InvalidOperationException("Unsupported theme"),
        };

        NightModeService.DefaultNightMode = style;
    }

    private static bool UpdateAppStyle(AppTheme selectedTheme)
    {
        if (selectedTheme == AppTheme.FollowSystem)
        {
            selectedTheme = (AppTheme)App.Current!.RequestedTheme;
        }

        ResourceDictionary theme = selectedTheme switch
        {
            AppTheme.Dark => new DarkTheme(),
            AppTheme.Light => new LightTheme(),
            _ => throw new InvalidOperationException("Unsupported theme"),
        };

        ICollection<ResourceDictionary> resources = App.Current!.Resources.MergedDictionaries;
        if (resources.FirstOrDefault()?.GetType() == theme.GetType())
        {
            return false;
        }

        try
        {
            resources.Clear();
            resources.Add(theme);
        }
        catch (Exception ex)
        {
            ExceptionService.LogException(ex);
        }

        return true;
    }

    private static async Task UpdateBarStyle()
    {
        if (OperatingSystem.IsAndroidVersionAtLeast(23))
        {
            if (Platform.CurrentActivity == null)
            {
                await Task.Yield();
            }

            StatusBar.SetColor(ResourceManager.StatusBarColor);
            StatusBar.SetStyle(ResourceManager.StatusBarStyle);

            NavigationBar.SetColor(ResourceManager.NavigationBarColor);
            NavigationBar.SetStyle(ResourceManager.NavigationBarStyle);
        }
    }
}