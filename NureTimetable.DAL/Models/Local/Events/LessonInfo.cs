using System;

namespace NureTimetable.DAL.Models.Local
{
    public class LessonInfo
    {
        public Lesson Lesson { get; set; }
        
        public string Notes { get; set; }
        
        private LessonSettings settings = new LessonSettings();
        public LessonSettings Settings
        {
            get => settings;
            set => settings = value ?? throw new NullReferenceException($"Attemt to set {Settings} to null");
        }
    }
}
