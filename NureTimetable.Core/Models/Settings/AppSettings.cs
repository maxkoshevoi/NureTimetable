using NureTimetable.Core.Extensions;
using System;
using System.Globalization;
using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.Essentials;

namespace NureTimetable.Core.Models.Settings
{
    public class AppSettings : ObservableObject
    {
        public static AppSettings Instance { get; } = new();

        private AppSettings()
        {
        }

        public TimetableViewMode TimetableViewMode 
        {
            get => (TimetableViewMode)Preferences.Get(nameof(TimetableViewMode), (int)TimetableViewMode.Week);
            set 
            { 
                var currentValue = TimetableViewMode; 
                SetProperty(ref currentValue, value, onChanged: () => Preferences.Set(nameof(TimetableViewMode), (int)value)); 
            }
        }

        public AppTheme Theme
        {
            get => (AppTheme)Preferences.Get(nameof(Theme), (int)AppTheme.FollowSystem);
            set
            {
                var currentValue = Theme;
                SetProperty(ref currentValue, value, onChanged: () => Preferences.Set(nameof(Theme), (int)value));
            }
        }

        public AppLanguage Language
        {
            get => (AppLanguage)Preferences.Get(nameof(Language), (int)AppLanguage.FollowSystem);
            set
            {
                var currentValue = Language;
                SetProperty(ref currentValue, value, onChanged: () => Preferences.Set(nameof(Language), (int)value));
            }
        }

        public string DefaultCalendarId
        {
            get => Preferences.Get(nameof(DefaultCalendarId), string.Empty);
            set
            {
                var currentValue = DefaultCalendarId;
                SetProperty(ref currentValue, value, onChanged: () => Preferences.Set(nameof(DefaultCalendarId), value));
            }
        }

        public DateTime? LastCistAllEntitiesUpdate
        {
            get 
            {
                string storedValue = Preferences.Get(nameof(LastCistAllEntitiesUpdate), null);
                return storedValue == null ? null : DateTime.Parse(storedValue, CultureInfo.InvariantCulture);
            }
            set
            {
                var currentValue = LastCistAllEntitiesUpdate;
                SetProperty(ref currentValue, value, onChanged: () => Preferences.Set(nameof(LastCistAllEntitiesUpdate), value?.ToString(CultureInfo.InvariantCulture)));
            }
        }

        public bool IsDebugMode
        {
            get => Preferences.Get(nameof(IsDebugMode), false);
            set
            {
                var currentValue = IsDebugMode;
                SetProperty(ref currentValue, value, onChanged: () => Preferences.Set(nameof(IsDebugMode), value));
            }
        }

        public bool Autoupdate
        {
            get => Preferences.Get(nameof(Autoupdate), true);
            set
            {
                var currentValue = Autoupdate;
                SetProperty(ref currentValue, value, onChanged: () => Preferences.Set(nameof(Autoupdate), value));
            }
        }
    }
}
