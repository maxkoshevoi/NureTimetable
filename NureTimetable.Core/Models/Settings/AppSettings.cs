using Xamarin.Essentials;

namespace NureTimetable.Core.Models.Settings
{
    public class AppSettings
    {
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
    }
}
