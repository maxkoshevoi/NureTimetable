namespace NureTimetable.Core.Models
{
    public class AppSettings
    {
        public TimetableViewMode TimetableViewMode { get; set; } = TimetableViewMode.Week;
    }

    public enum TimetableViewMode { Day, Week, Month }
}
