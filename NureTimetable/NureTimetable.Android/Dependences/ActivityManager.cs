using NureTimetable.Core.Models.InterplatformCommunication;
using NureTimetable.Droid.Dependences;
using Xamarin.Essentials;

[assembly: Xamarin.Forms.Dependency(typeof(ActivityManager))]
namespace NureTimetable.Droid.Dependences
{
    public class ActivityManager : IActivityManager
    {
        public void Recreate()
        {
            var activity = Platform.CurrentActivity;
            activity.Recreate();
        }
    }
}