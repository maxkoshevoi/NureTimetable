using Microsoft.Maui;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.LifecycleEvents;

namespace NureTimetable
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                })
                .ConfigureLifecycleEvents(lifecycle =>
                {
#if ANDROID
                    lifecycle.AddAndroid(d =>
                    {
                        d.OnBackPressed(activity =>
                        {
                            //bool isPopupStackEmpty = !Popup.SendBackPressed(activity.OnBackPressed);
                        });
                    });
#endif
                });

            return builder.Build();
        }
    }
}
