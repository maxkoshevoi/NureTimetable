using System.Collections.Generic;

namespace NureTimetable.Models
{
    public class EventTypeInfo
    {
        public string Name { get; set; }

        public List<string> Teachers { get; } = new List<string>();

        public override string ToString()
        {
            if (!string.IsNullOrEmpty(Name))
            {
                return $"{Name} - {string.Join(", ", Teachers)}";
            }
            return base.ToString();
        }
    }
}
