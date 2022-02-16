using Microsoft.Maui.LifecycleEvents;
using Rg.Plugins.Popup;
using Syncfusion.Maui.Core.Hosting;
using Xamarin.CommunityToolkit.Android.Effects;
using Xamarin.CommunityToolkit.Effects;

namespace NureTimetable;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureEssentials(config => config.UseVersionTracking())
            .ConfigureFonts(fonts => fonts.AddFont("materialdesignicons-webfont.ttf", "MaterialDesignIcons"))
            .ConfigureLifecycleEvents(lifecycle =>
            {
                lifecycle.AddAndroid(d => d.OnBackPressed(activity => Popup.SendBackPressed(activity.OnBackPressed)));
            })
            .ConfigureEffects(effects =>
            {
                effects.Add<StatusBarEffect, PlatformStatusBarEffect>();
                effects.Add<TouchEffect, PlatformTouchEffect>();
            })
            .ConfigureSyncfusionCore();

        return builder.Build();
    }
}
