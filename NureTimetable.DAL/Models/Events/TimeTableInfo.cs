﻿namespace NureTimetable.DAL.Models;

public class TimetableInfo(Entity entity) : TimetableStatistics
{
    public Entity Entity { get; } = entity ?? throw new ArgumentNullException(nameof(entity));

    public List<Event> Events
    {
        get => events;
        set => events = value;
    }

    /// <summary>
    /// Gets all available lesson infos (some lessons might not have one).
    /// </summary>
    public List<LessonInfo> LessonsInfo { get; set; } = new();

    public LessonInfo GetAndAddLessonsInfo(Lesson lesson)
    {
        var lessonInfo = LessonsInfo.SingleOrDefault(i => i.Lesson == lesson);
        if (lessonInfo == null)
        {
            lessonInfo = new(lesson);
            LessonsInfo.Add(lessonInfo);
        }

        return lessonInfo;
    }

    public void ApplyLessonSettings()
    {
        foreach (var lInfo in LessonsInfo.Where(ls => ls.Settings.IsSomeSettingsApplied))
        {
            // Hiding settings
            if (lInfo.Settings.Hiding.ShowLesson == false)
            {
                Events.RemoveAll(ev => ev.Lesson == lInfo.Lesson);
            }
            else if (lInfo.Settings.Hiding.ShowLesson == null)
            {
                Events.RemoveAll(ev => ev.Lesson == lInfo.Lesson &&
                    (lInfo.Settings.Hiding.EventTypesToHide.Contains(ev.Type.ID) ||
                    lInfo.Settings.Hiding.TeachersToHide.Intersect(ev.Teachers.Select(t => t.ID)).Any())
                );
            }
        }
    }
}
