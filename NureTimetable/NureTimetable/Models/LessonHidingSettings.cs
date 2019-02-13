using System.Collections.Generic;

namespace NureTimetable.Models
{
    public class LessonHidingSettings
    {
        /// <summary>
        /// if false, all LessonHidingSettings are ignored
        /// </summary>
        public bool HideLesson { get; set; } = false;

        /// <summary>
        /// If null or empty, only HideLesson setting is taken into account
        /// </summary>
        public List<string> HideOnlyThisEventTypes { get; set; }
    }
}