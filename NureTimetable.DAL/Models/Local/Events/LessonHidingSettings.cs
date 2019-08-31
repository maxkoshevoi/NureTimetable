using System.Collections.Generic;
using Xamarin.Forms;

namespace NureTimetable.DAL.Models.Local
{
    // BindableObject needed for nullable types to bind properly
    public class LessonHidingSettings : BindableObject
    {
        /// <summary>
        /// if not null, other hidding settings are ignorred
        /// </summary>
        public bool? ShowLesson { get; set; } = true;
        
        public List<long> EventTypesToHide { get; } = new List<long>();
        
        public List<long> TeachersToHide { get; } = new List<long>();
    }
}