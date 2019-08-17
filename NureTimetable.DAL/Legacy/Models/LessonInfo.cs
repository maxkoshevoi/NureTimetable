using System;
using System.Collections.Generic;
using System.Linq;

namespace NureTimetable.DAL.Legacy.Models
{
    class LessonInfo
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
