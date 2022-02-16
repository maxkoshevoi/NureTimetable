using AndroidX.AppCompat.App;
using NureTimetable.Platforms.Android.Models;

namespace NureTimetable.Platforms.Android.Services;

public static class NightModeService
{
    public static NightModeStyle DefaultNightMode
    {
        get => (NightModeStyle)AppCompatDelegate.DefaultNightMode;
        set => AppCompatDelegate.DefaultNightMode = (int)value;
    }
}
