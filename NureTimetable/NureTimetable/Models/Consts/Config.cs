using System;

namespace NureTimetable.Models.Consts
{
    public static class Config
    {
        public static DateTime TimetableFromDate
            => DateTime.Now.AddMonths(-2);

        public static DateTime TimetableToDate
            => DateTime.Now.AddMonths(6);

        public static TimeSpan CistDailyTimetableUpdateTime
            => TimeSpan.FromHours(7);

        public static TimeSpan CistAllGroupsUpdateMinInterval
            => TimeSpan.FromHours(24);
    }
}
