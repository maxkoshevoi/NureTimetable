using System;

namespace NureTimetable.Core.Models.Consts
{
    public static class Config
    {
        public static DateTime TimetableFromDate => 
            DateTime.Now.AddMonths(-6);

        public static DateTime TimetableToDate => 
            DateTime.Now.AddMonths(6);

        public static TimeSpan CistDailyTimetableUpdateTime { get; } =
            TimeSpan.FromHours(7);

        public static TimeSpan CistAllEntitiesUpdateMinInterval { get; } =
            TimeSpan.FromDays(1);

        public static TimeSpan CistLessonsInfoUpdateMinInterval { get; } =
            TimeSpan.FromDays(7);

        public static TimeZoneInfo UkraineTimezone { get; } =
            TimeZoneInfo.FindSystemTimeZoneById("FLE Standard Time");
    }
}
