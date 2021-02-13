using System;
using static System.TimeZoneInfo;

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
            CreateCustomTimeZone("Ukraine", TimeSpan.FromHours(2), string.Empty, string.Empty, string.Empty, new[]
            {
                AdjustmentRule.CreateAdjustmentRule(DateTime.MinValue.Date, DateTime.MaxValue.Date,
                    TimeSpan.FromHours(1),
                    TransitionTime.CreateFixedDateRule(DateTime.MinValue.AddHours(3), 3, 28),
                    TransitionTime.CreateFixedDateRule(DateTime.MinValue.AddHours(4), 10, 31)
                )
            });
    }
}
