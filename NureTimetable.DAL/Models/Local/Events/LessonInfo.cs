namespace NureTimetable.DAL.Models.Local
{
    public class LessonInfo
    {
        public LessonInfo(Lesson lesson)
        {
            Lesson = lesson;
        }

        public Lesson Lesson { get; }

        public string? Notes { get; set; }

        public LessonSettings Settings { get; set; } = new();
    }
}
