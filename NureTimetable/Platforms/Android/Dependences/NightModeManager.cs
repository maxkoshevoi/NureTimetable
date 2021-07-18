using AndroidX.AppCompat.App;
using NureTimetable.Core.Models.InterplatformCommunication;
using NureTimetable.Platforms.Android.Dependences;

[assembly: Xamarin.Forms.Dependency(typeof(NightModeManager))]
namespace NureTimetable.Platforms.Android.Dependences
{
    public class NightModeManager : INightModeManager
    {
        public NightModeStyle DefaultNightMode
        {
            get => (NightModeStyle)AppCompatDelegate.DefaultNightMode;
            set => AppCompatDelegate.DefaultNightMode = (int)value;
        }
    }
}
