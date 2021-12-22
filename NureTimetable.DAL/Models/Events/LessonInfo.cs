using System;

namespace NureTimetable.DAL.Models
{
    public class LessonInfo
    {
        public LessonInfo(Lesson lesson)
        {
            Lesson = lesson;
        }

        public Lesson Lesson { get; }

        public string? Notes { get; set; }

        public DlNureLessonInfo DlNureInfo { get; set; } = new();

        public LessonSettings Settings { get; set; } = new();
    }
}
