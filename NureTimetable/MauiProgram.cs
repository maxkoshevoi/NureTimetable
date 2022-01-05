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
            .ConfigureSyncfusionCore()
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("materialdesignicons-webfont.ttf", "MaterialDesignIcons");
            })
            .ConfigureLifecycleEvents(lifecycle =>
            {
#if ANDROID
                lifecycle.AddAndroid(d => d.OnBackPressed(activity => Popup.SendBackPressed(activity.OnBackPressed)));
#endif
            })
            .ConfigureEffects(effects =>
            {
                effects.Add<StatusBarEffect, PlatformStatusBarEffect>();
            });

        return builder.Build();
    }
}
