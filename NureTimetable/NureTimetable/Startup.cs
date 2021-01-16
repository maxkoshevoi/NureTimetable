using Microsoft.Extensions.DependencyInjection;
using Shiny;
using Shiny.Notifications;

namespace NureTimetable
{
    public class Startup : Shiny.ShinyStartup
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.UseNotifications(androidConfig: new AndroidOptions 
            { 
                SmallIconResourceName = "notification_icon" 
            });
        }
    }
}
