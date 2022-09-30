﻿using Android.OS;
using Android.Views;
using Microsoft.Maui.Platform;
using System.Runtime.Versioning;
using Activity = Android.App.Activity;
using Window = Android.Views.Window;

namespace CommunityToolkit.Maui.Core.Platform;

/// <summary>
/// Enum that represents the possible style values for the device
/// </summary>
public enum NavigationBarStyle
{
    /// <summary>
    /// The default style value
    /// </summary>
    Default = 0,
    /// <summary>
    /// Light style value
    /// </summary>
    LightContent = 1,
    /// <summary>
    /// Dark style value
    /// </summary>
    DarkContent = 2
}

/// <summary>
/// Class that hold the method to customize the NavigationBar
/// </summary>
public static partial class NavigationBar
{
    /// <summary>
    /// Method to change the color of the navigation bar.
    /// </summary>
    /// <param name="color">The <see cref="Color"/> that will be set to the navigation bar.</param>
    public static void SetColor(Color? color) =>
        PlatformSetColor(color ?? Colors.Transparent);

    /// <summary>
    /// Method to change the style of the navigation bar.
    /// </summary>
    /// <param name="navigationBarStyle"> The <see cref="NavigationBarStyle"/> that will used by navigation bar.</param>
    public static void SetStyle(NavigationBarStyle navigationBarStyle) =>
        PlatformSetStyle(navigationBarStyle);
}

static partial class NavigationBar
{
    static Activity Activity => Microsoft.Maui.ApplicationModel.Platform.CurrentActivity ?? throw new InvalidOperationException("Android Activity can't be null.");

    static void PlatformSetColor(Color color)
    {
        if (IsSupported())
        {
            Activity.Window?.SetNavigationBarColor(color.ToPlatform());
        }
    }

    static void PlatformSetStyle(NavigationBarStyle style)
    {
        if (!IsSupported())
        {
            return;
        }

        switch (style)
        {
            case NavigationBarStyle.DarkContent:
                AddBarAppearanceFlag(Activity, (StatusBarVisibility)SystemUiFlags.LightNavigationBar);
                break;

            case NavigationBarStyle.Default:
            case NavigationBarStyle.LightContent:
                RemoveBarAppearanceFlag(Activity, (StatusBarVisibility)SystemUiFlags.LightNavigationBar);
                break;

            default:
                throw new NotSupportedException($"{nameof(NavigationBarStyle)} {style} is not yet supported on Android");
        }
    }

    static void AddBarAppearanceFlag(Activity activity, StatusBarVisibility flag) =>
        SetBarAppearance(activity, barAppearance => barAppearance |= flag);


    static void RemoveBarAppearanceFlag(Activity activity, StatusBarVisibility flag) =>
        SetBarAppearance(activity, barAppearance => barAppearance &= ~flag);


    static void SetBarAppearance(Activity activity, Func<StatusBarVisibility, StatusBarVisibility> updateAppearance)
    {
        var window = GetCurrentWindow(activity);
        var appearance = window.DecorView.SystemUiVisibility;
        appearance = updateAppearance(appearance);
        window.DecorView.SystemUiVisibility = appearance;

        static Window GetCurrentWindow(Activity activity)
        {
            var window = activity.Window ?? throw new InvalidOperationException($"{nameof(activity.Window)} cannot be null");
            window.ClearFlags(WindowManagerFlags.TranslucentStatus);
            window.AddFlags(WindowManagerFlags.DrawsSystemBarBackgrounds);
            return window;
        }
    }

    static bool IsSupported()
    {
        if (OperatingSystem.IsAndroidVersionAtLeast((int)BuildVersionCodes.M))
        {
            return true;
        }

        System.Diagnostics.Debug.WriteLine($"This functionality is not available. Minimum supported API is {(int)BuildVersionCodes.M}");
        return false;
    }
}