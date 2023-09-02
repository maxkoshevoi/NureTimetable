namespace NureTimetable.DAL.Models;

public class LessonInfo(Lesson lesson)
{
    public Lesson Lesson { get; } = lesson;

    public string? Notes { get; set; }

    public DlNureLessonInfo DlNureInfo { get; set; } = new();

    public LessonSettings Settings { get; set; } = new();
}
