using Xamarin.CommunityToolkit.ObjectModel;

namespace NureTimetable.DAL.Models.Local
{
    public class LessonSettings
    {
        public bool IsSomeSettingsApplied => Hiding.ShowLesson != true;

        public LessonHidingSettings Hiding { get; } = new();
    }
}
