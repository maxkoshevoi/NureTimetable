using NureTimetable.Core.Models.InterplatformCommunication;
using NureTimetable.iOS.Dependences;
using Xamarin.Essentials;

[assembly: Xamarin.Forms.Dependency(typeof(ActivityManager))]
namespace NureTimetable.iOS.Dependences
{
    public class ActivityManager : IActivityManager
    {
        public void Recreate()
        {
            var controller = Platform.GetCurrentUIViewController();
            // controller.Recreate(); // Don't know how to recreate main activity in iOS
        }
    }
}