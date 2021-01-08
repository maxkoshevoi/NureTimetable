using System;

namespace NureTimetable.Core.Models.Consts
{
    public static class Config
    {
        public static DateTime TimetableFromDate => 
            DateTime.Now.AddMonths(-6);

        public static DateTime TimetableToDate => 
            DateTime.Now.AddMonths(6);

        public static TimeSpan CistDailyTimetableUpdateTime => 
            TimeSpan.FromHours(7);

        public static TimeSpan CistAllEntitiesUpdateMinInterval => 
            TimeSpan.FromDays(1);

        public static TimeSpan CistLessonsInfoUpdateMinInterval => 
            TimeSpan.FromDays(7);
    }
}
