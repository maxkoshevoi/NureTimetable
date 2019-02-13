using System;

namespace NureTimetable.Models.Consts
{
    public static class Config
    {
        public static DateTime TimetableFromDate
            => DateTime.Now.AddMonths(-1);

        public static DateTime TimetableToDate
            => DateTime.Now.AddMonths(3);

        public static TimeSpan TimetableManualUpdateMinInterval
            => TimeSpan.FromDays(1);
    }
}
