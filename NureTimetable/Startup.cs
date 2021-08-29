using Microsoft.Maui;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.LifecycleEvents;
using Rg.Plugins.Popup;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]

namespace NureTimetable
{
    public class Startup : IStartup
    {
        public void Configure(IAppHostBuilder appBuilder)
        {
            appBuilder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("materialdesignicons-webfont.ttf", "Material Design Icons");
                })
                .ConfigureLifecycleEvents(lifecycle =>
                {
#if ANDROID
                    lifecycle.AddAndroid(d =>
                    {
                        d.OnBackPressed(activity =>
                        {
                            bool isPopupStackEmpty = !Popup.SendBackPressed(activity.OnBackPressed);
                        });
                    });
#endif
                });
        }
    }
}
