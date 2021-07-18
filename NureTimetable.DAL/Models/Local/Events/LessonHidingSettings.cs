using Microsoft.Maui.Controls;
using System.Collections.Generic;

namespace NureTimetable.DAL.Models.Local
{
    // BindableObject needed for nullable types to bind properly
    public class LessonHidingSettings : BindableObject
    {
        /// <summary>
        /// if not null, other hiding settings are ignored
        /// </summary>
        public bool? ShowLesson { get; set; } = true;
        
        public List<long> EventTypesToHide { get; } = new();
        
        public List<long> TeachersToHide { get; } = new();
    }
}