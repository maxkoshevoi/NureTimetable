using NureTimetable.Core.Extensions;
using System;
using System.Globalization;
using Xamarin.Essentials;

namespace NureTimetable.Core.Models.Settings
{
    public class AppSettings
    {
        public static AppSettings Instance { get; } = new();

        private AppSettings()
        {
        }

        public TimetableViewMode TimetableViewMode 
        {
            get => (TimetableViewMode)Preferences.Get(nameof(TimetableViewMode), (int)TimetableViewMode.Week);
            set => Preferences.Set(nameof(TimetableViewMode), (int)value);
        }

        public AppTheme Theme
        {
            get => (AppTheme)Preferences.Get(nameof(Theme), (int)AppTheme.FollowSystem);
            set => Preferences.Set(nameof(Theme), (int)value);
        }

        public AppLanguage Language
        {
            get => (AppLanguage)Preferences.Get(nameof(Language), (int)AppLanguage.FollowSystem);
            set => Preferences.Set(nameof(Language), (int)value);
        }

        public string DefaultCalendarId
        {
            get => Preferences.Get(nameof(DefaultCalendarId), string.Empty);
            set => Preferences.Set(nameof(DefaultCalendarId), value);
        }

        public DateTime? LastCistAllEntitiesUpdate
        {
            get 
            {
                string storedValue = Preferences.Get(nameof(LastCistAllEntitiesUpdate), null);
                return storedValue == null ? null : DateTime.Parse(storedValue, CultureInfo.InvariantCulture);
            }
            set => Preferences.Set(nameof(LastCistAllEntitiesUpdate), value?.ToString(CultureInfo.InvariantCulture));
        }

        public bool IsDebugMode
        {
            get => Preferences.Get(nameof(IsDebugMode), false);
            set => Preferences.Set(nameof(IsDebugMode), value);
        }
    }
}
