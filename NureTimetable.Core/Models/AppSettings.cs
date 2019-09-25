namespace NureTimetable.Core.Models
{
    public class AppSettings
    {
        public TimetableViewMode TimetableViewMode { get; set; } = TimetableViewMode.Week;
    }

    public enum TimetableViewMode 
    { 
        Day = 0, 
        Week = 1, 
        Month = 2, 
        Timeline = 3 
    }
}
