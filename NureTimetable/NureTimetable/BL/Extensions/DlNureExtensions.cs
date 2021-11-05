using NureTimetable.Core.BL;
using NureTimetable.Core.Extensions;
using NureTimetable.DAL.Models;
using NureTimetable.DAL.Moodle.Models.Courses;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NureTimetable.BL.Extensions;

public static class DlNureExtensions
{
    public static List<FullCourse> Find(this IEnumerable<FullCourse> courses, Lesson lesson)
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
                        Data = { { "Lesson", $"{lesson.FullName} ({lesson.ShortName})" }}
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
