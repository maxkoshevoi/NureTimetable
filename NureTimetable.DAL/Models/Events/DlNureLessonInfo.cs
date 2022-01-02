using System;

namespace NureTimetable.DAL.Models;

public class DlNureLessonInfo
{
    public int? LessonId { get; set; }

    public bool ShowAttendance { get; set; } = true;

    public Uri? AttendanceUrl { get; set; }
}