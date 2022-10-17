using CommunityToolkit.Maui;
using Microsoft.Maui.LifecycleEvents;
using Rg.Plugins.Popup;
using Syncfusion.Licensing;
using Syncfusion.Maui.Core.Hosting;

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
                lifecycle.AddAndroid(d => d.OnBackPressed(_ => Popup.SendBackPressed()));
            })
            .UseMauiCommunityToolkit()
            .ConfigureSyncfusion();

        return builder.Build();
    }

    public static MauiAppBuilder ConfigureSyncfusion(this MauiAppBuilder builder)
    {
        SyncfusionLicenseProvider.RegisterLicense(Keys.SyncfusionLicenseKey);
        return builder.ConfigureSyncfusionCore();
    }
}
