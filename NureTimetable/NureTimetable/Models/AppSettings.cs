using System;
using System.Collections.Generic;
using System.Text;

namespace NureTimetable.Models
{
    public class AppSettings
    {
        public TimetableViewMode TimetableViewMode { get; set; } = TimetableViewMode.Week;
    }

    public enum TimetableViewMode { Day, Week, Month }
}
