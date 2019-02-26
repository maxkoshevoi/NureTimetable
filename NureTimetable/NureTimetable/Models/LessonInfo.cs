using System;
using System.Collections.Generic;
using System.Linq;

namespace NureTimetable.Models
{
    public class LessonInfo
    {
        #region Details
        public string ShortName { get; set; }
        public string LongName { get; set; }
        private List<EventTypeInfo> eventTypesInfo = new List<EventTypeInfo>();
        public List<EventTypeInfo> EventTypesInfo
        {
            get => eventTypesInfo;
            set => eventTypesInfo = value ?? throw new NullReferenceException($"Attemt to set {EventTypesInfo} to null");
        }
        public DateTime? LastUpdated { get; set; }
        #endregion

        public string Notes { get; set; }

        #region Settings
        private LessonSettings settings = new LessonSettings();
        public LessonSettings Settings
        {
            get => settings;
            set => settings = value ?? throw new NullReferenceException($"Attemt to set {Settings} to null");
        }
        #endregion

        public override string ToString()
        {
            if (!string.IsNullOrEmpty(ShortName))
            {
                return $"{ShortName} - {string.Join(", ", EventTypesInfo.Select(et => et.Name))}";
            }
            return base.ToString();
        }
    }
}
