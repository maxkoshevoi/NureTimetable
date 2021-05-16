using AndroidX.AppCompat.App;
using NureTimetable.Core.Models.InterplatformCommunication;
using NureTimetable.Droid.Dependences;

[assembly: Xamarin.Forms.Dependency(typeof(NightModeManager))]
namespace NureTimetable.Droid.Dependences
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