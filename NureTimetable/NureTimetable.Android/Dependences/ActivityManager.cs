using NureTimetable.Core.Models.InterplatformCommunication;
using NureTimetable.Droid.Dependences;
using Plugin.CurrentActivity;

[assembly: Xamarin.Forms.Dependency(typeof(ActivityManager))]
namespace NureTimetable.Droid.Dependences
{
    public class ActivityManager : IActivityManager
    {
        public void Recreate()
        {
            var activity = CrossCurrentActivity.Current.Activity;
            activity.Recreate();
        }
    }
}