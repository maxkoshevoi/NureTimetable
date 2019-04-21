using System.Collections.Generic;
using Xamarin.Forms;

namespace NureTimetable.DAL.Models.Local
{
    // BindableObject needed for nullable types to bind properly
    public class LessonHidingSettings : BindableObject
    {
        /// <summary>
        /// if null, take settings from HideOnlyThisEventTypes list
        /// </summary>
        public bool? ShowLesson { get; set; } = true;

        /// <summary>
        /// If null or empty, only HideLesson setting is taken into account
        /// </summary>
        public List<long> HideOnlyThisEventTypes { get; } = new List<long>();
    }
}