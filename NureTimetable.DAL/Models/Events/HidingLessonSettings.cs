using Microsoft.Maui.Controls;

namespace NureTimetable.DAL.Models;

// BindableObject needed for nullable types to bind properly
public class HidingLessonSettings : BindableObject
{
    /// <summary>
    /// if not null, other hiding settings are ignored
    /// </summary>
    public bool? ShowLesson { get; set; } = true;

    public List<long> EventTypesToHide { get; } = new();

    public List<long> TeachersToHide { get; } = new();
}
