using CommunityToolkit.Mvvm.ComponentModel;
using NureTimetable.DAL.Consts;
using NureTimetable.DAL.Moodle.Models.Auth;
using NureTimetable.DAL.Settings.Models;
using AppTheme = NureTimetable.DAL.Settings.Models.AppTheme;

namespace NureTimetable.DAL.Settings;

public class AppSettings : ObservableObject
{
    internal static AppSettings Instance { get; } = new();

    private AppSettings()
    {
    }

    public TimetableViewMode TimetableViewMode
    {
        get => (TimetableViewMode)Preferences.Get(nameof(TimetableViewMode), (int)TimetableViewMode.Week);
        set
        {
            if (TimetableViewMode != value)
            {
                Preferences.Set(nameof(TimetableViewMode), (int)value);
                OnPropertyChanged(nameof(TimetableViewMode));
            }
        }
    }

    public AppTheme Theme
    {
        get => (AppTheme)Preferences.Get(nameof(Theme), (int)AppTheme.FollowSystem);
        set
        {
            if (Theme != value)
            {
                Preferences.Set(nameof(Theme), (int)value);
                OnPropertyChanged(nameof(Theme));
            }
        }
    }

    public AppLanguage Language
    {
        get => (AppLanguage)Preferences.Get(nameof(Language), (int)AppLanguage.FollowSystem);
        set
        {
            if (Language != value)
            {
                Preferences.Set(nameof(Language), (int)value);
                OnPropertyChanged(nameof(Language));
            }
        }
    }

    #region Calendar settings
    public string DefaultCalendarId
    {
        get => Preferences.Get(nameof(DefaultCalendarId), string.Empty);
        set
        {
            if (DefaultCalendarId != value)
            {
                Preferences.Set(nameof(DefaultCalendarId), value);
                OnPropertyChanged(nameof(DefaultCalendarId));
            }
        }
    }

    public TimeSpan? TimeBeforeEventReminder
    {
        get
        {
            double currentValue = Preferences.Get(nameof(TimeBeforeEventReminder), TimeSpan.FromMinutes(30).TotalMinutes);
            return currentValue < 0 ? null : TimeSpan.FromMinutes(currentValue);
        }
        set
        {
            if (TimeBeforeEventReminder != value)
            {
                Preferences.Set(nameof(TimeBeforeEventReminder), value?.TotalMinutes ?? -1);
                OnPropertyChanged(nameof(TimeBeforeEventReminder));
            }
        }
    }
    #endregion

    public DateTime? LastCistAllEntitiesUpdate
    {
        get
        {
            string? storedValue = Preferences.Get(nameof(LastCistAllEntitiesUpdate), null);
            return storedValue == null ? null : DateTime.Parse(storedValue, CultureInfo.InvariantCulture);
        }
        set
        {
            if (LastCistAllEntitiesUpdate != value)
            {
                Preferences.Set(nameof(LastCistAllEntitiesUpdate), value?.ToString(CultureInfo.InvariantCulture));
                OnPropertyChanged(nameof(LastCistAllEntitiesUpdate));
            }
        }
    }

    public bool IsDebugMode
    {
        get => Preferences.Get(nameof(IsDebugMode), false);
        set
        {
            if (IsDebugMode != value)
            {
                Preferences.Set(nameof(IsDebugMode), value);
                OnPropertyChanged(nameof(IsDebugMode));
            }
        }
    }

    public bool Autoupdate
    {
        get => Preferences.Get(nameof(Autoupdate), true);
        set
        {
            if (Autoupdate != value)
            {
                Preferences.Set(nameof(Autoupdate), value);
                OnPropertyChanged(nameof(Autoupdate));
            }
        }
    }

    public MoodleUser? DlNureUser
    {
        get => Serialisation.FromJsonFile<MoodleUser?>(FilePath.MoodleUser).GetAwaiter().GetResult();
        set
        {
            if (DlNureUser != value)
            {
                Serialisation.ToJsonFile(value, FilePath.MoodleUser).GetAwaiter().GetResult();
                OnPropertyChanged(nameof(DlNureUser));
            }
        }
    }
}
