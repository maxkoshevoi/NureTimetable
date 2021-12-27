using Flurl;
using NureTimetable.Core.BL;
using NureTimetable.Core.Extensions;
using NureTimetable.Core.Localization;
using NureTimetable.DAL.Cist;
using NureTimetable.DAL.Models;
using NureTimetable.DAL.Moodle;
using NureTimetable.DAL.Moodle.Models.Courses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace NureTimetable.BL;

public static class DlNureService
{
    /// <param name="timetable">Used to save results</param>
    public static async ValueTask<Url?> GetAttendanceUrlAsync(Lesson lesson, TimetableInfo? timetable = null)
    {
        try
        {
            var lessonInfo = timetable?.GetAndAddLessonsInfo(lesson);
            if (lessonInfo?.DlNureInfo.AttendanceUrl != null)
            {
                return lessonInfo.DlNureInfo.AttendanceUrl;
            }

            int? lessonId = await GetLessonIdAsync(lesson, timetable);
            if (lessonId == null)
            {
                await Shell.Current.DisplayAlert(LN.SomethingWentWrong, "Unable to find lesson", LN.Ok);
                return null;
            }

            CourseModule? attendance = (await new MoodleRepository()
                .GetCourseContents(lessonId.Value, new() { { GetCourseContentsOption.ModName, "attendance" } }))
                .FirstOrDefault()?
                .Modules
                .FirstOrDefault();
            if (attendance == null)
            {
                await Shell.Current.DisplayAlert(LN.SomethingWentWrong, "Lesson doesn't have an attendance module", LN.Ok);
                return null;
            }

            Uri attendanceUrl = new(attendance.Url);
            if (timetable != null)
            {
                lessonInfo!.DlNureInfo.AttendanceUrl = attendanceUrl;
                await EventsRepository.UpdateLessonsInfo(timetable);
            }

            return attendanceUrl;
        }
        catch (Exception ex)
        {
            ex.Data.Add("Lesson", $"{lesson.FullName} ({lesson.ShortName} - {lesson.ID})");
            if (timetable != null)
            {
                ex.Data.Add("Entity", $"{timetable.Entity.Type} {timetable.Entity.Name} ({timetable.Entity.ID})");
            }
            ExceptionService.LogException(ex);
            return null;
        }
    }

    /// <param name="timetable">Used to save results</param>
    public static async ValueTask<int?> GetLessonIdAsync(Lesson lesson, TimetableInfo? timetable = null)
    {
        try
        {
            if (timetable == null)
            {
                List<FullCourse> courses = await new MoodleRepository().GetEnrolledCourses();
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
            ex.Data.Add("Lesson", $"{lesson.FullName} ({lesson.ShortName} - {lesson.ID})");
            if (timetable != null)
            {
                ex.Data.Add("Entity", $"{timetable.Entity.Type} {timetable.Entity.Name} ({timetable.Entity.ID})");
            }
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

            List<FullCourse> courses = await new MoodleRepository().GetEnrolledCourses();
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
            ex.Data.Add("Entity", $"{timetable.Entity.Type} {timetable.Entity.Name} ({timetable.Entity.ID})");
            ExceptionService.LogException(ex);
            return timetable.LessonsInfo;
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
