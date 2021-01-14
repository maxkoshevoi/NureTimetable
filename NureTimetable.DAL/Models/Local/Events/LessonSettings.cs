using Xamarin.CommunityToolkit.ObjectModel;

namespace NureTimetable.DAL.Models.Local
{
    public class LessonSettings : ObservableObject
    {
        public bool IsSomeSettingsApplied => Hiding.ShowLesson != true;

        public LessonHidingSettings Hiding { get; } = new();

        public void NotifyChanged() => OnPropertyChanged(nameof(Hiding));
    }
}
