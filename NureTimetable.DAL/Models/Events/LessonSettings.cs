namespace NureTimetable.DAL.Models
{
    public class LessonSettings
    {
        public bool IsSomeSettingsApplied => Hiding.ShowLesson != true;

        public LessonHidingSettings Hiding { get; } = new();
    }
}
