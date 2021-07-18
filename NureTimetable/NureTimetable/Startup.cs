using Microsoft.Maui;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.LifecycleEvents;

namespace NureTimetable
{
	public class Startup : IStartup
	{
		public void Configure(IAppHostBuilder appBuilder)
		{
			appBuilder
				.UseMauiApp<App>()
				.ConfigureFonts(fonts => {
					fonts.AddFont("materialdesignicons-webfont.ttf", "Material Design Icons");
				})
				.ConfigureLifecycleEvents(lifecycle => {
#if ANDROID
					lifecycle.AddAndroid(d => {
						d.OnBackPressed(activity => {
							// bool isPopupStackEmpty = !Rg.Plugins.Popup.Popup.SendBackPressed(base.OnBackPressed);
						});
					});
#endif
				});
		}
	}
}
