using Flurl;
using NureTimetable.BL.Extensions;
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
    public static async ValueTask<Url?> GetAttendanceUrlAsync(Lesson lesson, TimetableInfo? timetable = null)
    {
        var lessonInfo = timetable?.GetAndAddLessonsInfo(lesson) ?? new(lesson);
        if (lessonInfo.DlNureInfo.AttendanceUrl != null)
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

        lessonInfo.DlNureInfo.AttendanceUrl = new Uri(attendance.Url);
        if (timetable != null)
        {
            await EventsRepository.UpdateLessonsInfo(timetable);
        }

        return lessonInfo.DlNureInfo.AttendanceUrl;
    }

    public static async ValueTask<int?> GetLessonIdAsync(Lesson lesson, TimetableInfo? timetable = null)
    {
        var lessonInfo = timetable?.GetAndAddLessonsInfo(lesson) ?? new(lesson);
        if (lessonInfo.DlNureInfo.LessonId != null)
        {
            return lessonInfo.DlNureInfo.LessonId;
        }

        List<FullCourse> courses = await new MoodleRepository().GetEnrolledCourses();
        FullCourse? course = courses.Find(lesson).FirstOrDefault();
        if (course == null)
        {
            return null;
        }

        lessonInfo.DlNureInfo.LessonId = course.Id;
        if (timetable != null)
        {
            await EventsRepository.UpdateLessonsInfo(timetable);
        }

        return lessonInfo.DlNureInfo.LessonId;
    }
}
