namespace NureTimetable.Core.Models.InterplatformCommunication;

public interface INightModeManager
{
    NightModeStyle DefaultNightMode { get; set; }
}

public enum NightModeStyle
{
    /// <summary>
    /// An unspecified mode for night mode.
    /// </summary>
    Unspecified = -100,

    /// <summary>
    /// Mode which uses the system's night mode setting to determine if it is night or not.
    /// </summary>
    FollowSystem = -1,

    /// <summary>
    /// Night mode which uses always uses a light mode, enabling non-night qualified resources regardless of the time.
    /// </summary>
    No = 1,

    /// <summary>
    /// Night mode which uses always uses a dark mode, enabling night qualified resources regardless of the time.
    /// </summary>
    Yes = 2,

    /// <summary>
    /// Night mode which uses a dark mode when the system's 'Battery Saver' feature is enabled, otherwise it uses a 'light mode'.
    /// </summary>
    AutoBattery = 3
}
