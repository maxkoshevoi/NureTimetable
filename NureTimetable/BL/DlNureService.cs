using Flurl;
using NureTimetable.Core.BL;
using NureTimetable.Core.Extensions;
using NureTimetable.Core.Localization;
using NureTimetable.DAL.Cist;
using NureTimetable.DAL.Models;
using NureTimetable.DAL.Moodle;
using NureTimetable.DAL.Moodle.Models.Courses;

namespace NureTimetable.BL;

public static class DlNureService
{
    /// <param name="timetable">Used to save results</param>
    public static async ValueTask<(Url? attendanceUrl, string? errorMessage)> GetAttendanceUrlAsync(Lesson lesson, TimetableInfo? timetable = null)
    {
        try
        {
            var lessonInfo = timetable?.GetAndAddLessonsInfo(lesson);
            if (lessonInfo?.DlNureInfo.AttendanceUrl != null)
            {
                return (lessonInfo.DlNureInfo.AttendanceUrl, null);
            }

            int? lessonId = await GetLessonIdAsync(lesson, timetable);
            if (lessonId == null)
            {
                return (null, LN.LessonNotFound);
            }

            CourseModule? attendance = (await new MoodleRepository()
                .GetCourseContentsAsync(lessonId.Value, new() { { GetCourseContentsOption.ModName, "attendance" } }))
                .FirstOrDefault()?
                .Modules
                .FirstOrDefault();
            if (attendance == null)
            {
                return (null, LN.NoAttendanceModule);
            }

            Uri attendanceUrl = new(attendance.Url);
            if (timetable != null)
            {
                lessonInfo!.DlNureInfo.AttendanceUrl = attendanceUrl;
                await EventsRepository.UpdateLessonsInfo(timetable);
            }

            return (attendanceUrl, null);
        }
        catch (Exception ex)
        {
            EnrichException(ex, timetable?.Entity, lesson);
            ExceptionService.LogException(ex);
            return (null, ex.Message);
        }
    }

    /// <param name="timetable">Used to save results</param>
    public static async ValueTask<int?> GetLessonIdAsync(Lesson lesson, TimetableInfo? timetable = null)
    {
        try
        {
            if (timetable == null)
            {
                List<FullCourse> courses = await new MoodleRepository().GetEnrolledCoursesAsync();
                FullCourse? course = courses.Find(lesson).FirstOrDefault();
                return course?.Id;
            }
            else
            {
                var lessonInfos = await UpdateLessonIdsAsync(timetable);
                return lessonInfos.FirstOrDefault(li => li.Lesson == lesson)?.DlNureInfo.LessonId;
            }
        }
        catch (Exception ex)
        {
            EnrichException(ex, timetable?.Entity, lesson);
            ExceptionService.LogException(ex);
            return null;
        }
    }

    public static async Task<List<LessonInfo>> UpdateLessonIdsAsync(TimetableInfo timetable)
    {
        try
        {
            List<Lesson> lessons = timetable.Lessons().ToList();
            bool areAllIdsPresent = lessons
                .Select(l => timetable.GetAndAddLessonsInfo(l))
                .All(li => li.DlNureInfo.LessonId != null);
            if (areAllIdsPresent)
            {
                return timetable.LessonsInfo;
            }

            List<FullCourse> courses = await new MoodleRepository().GetEnrolledCoursesAsync();
            foreach (var lesson in lessons)
            {
                var lessonInfo = timetable.GetAndAddLessonsInfo(lesson);
                if (lessonInfo.DlNureInfo.LessonId != null)
                {
                    continue;
                }

                lessonInfo.DlNureInfo.LessonId = courses.Find(lesson).FirstOrDefault()?.Id;
            }

            await EventsRepository.UpdateLessonsInfo(timetable);

            return timetable.LessonsInfo;
        }
        catch (Exception ex)
        {
            EnrichException(ex, timetable.Entity);
            ExceptionService.LogException(ex);
            return timetable.LessonsInfo;
        }
    }

    private static void EnrichException(Exception ex, Entity? entity = null, Lesson? lesson = null)
    {
        if (lesson != null)
        {
            ex.Data.Add("Lesson", $"{lesson.FullName} ({lesson.ShortName} - {lesson.ID})");
        }
        if (entity != null)
        {
            ex.Data.Add("Entity", $"{entity.Type} {entity.Name} ({entity.ID})");
        }
    }

    private static List<FullCourse> Find(this IEnumerable<FullCourse> courses, Lesson lesson)
    {
        string simplifiedFullName = lesson.FullName.Simplify();
        List<FullCourse> matchedCourses = courses
            .Where(c => c.ShortName.Contains($":{lesson.ShortName}:") || simplifiedFullName.StartsWith(c.FullName.Simplify()))
            .ToList();

        if (matchedCourses.Count > 1)
        {
            List<FullCourse> ongoingCourses = matchedCourses.Where(c => c.StartDateUtc <= DateTime.UtcNow && c.EndDateUtc > DateTime.UtcNow).ToList();
            if (ongoingCourses.Any())
            {
                matchedCourses = ongoingCourses;

                if (matchedCourses.Count > 1)
                {
                    InvalidOperationException ex = new("Found multiple moodle courses")
                    {
                        Data = { { "Lesson", $"{lesson.FullName} ({lesson.ShortName})" } }
                    };
                    for (int i = 0; i < matchedCourses.Count; i++)
                    {
                        ex.Data.Add($"Course {i}", $"{matchedCourses[i].FullName} ({matchedCourses[i].ShortName})");
                    }
                    ExceptionService.LogException(ex);
                }
            }
        }

        return matchedCourses;
    }
}
