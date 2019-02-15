using System.Collections.Generic;

namespace NureTimetable.Models
{
    public class LessonHidingSettings
    {
        /// <summary>
        /// if null, take settings from HideOnlyThisEventTypes list
        /// </summary>
        public bool? ShowLesson { get; set; } = true;

        /// <summary>
        /// If null or empty, only HideLesson setting is taken into account
        /// </summary>
        public List<string> HideOnlyThisEventTypes { get; } = new List<string>();
    }
}