using System.Collections.Generic;

namespace NureTimetable.DAL.Legacy.Models
{
    class EventTypeInfo
    {
        public string? Name { get; set; }

        public List<string> Teachers { get; } = new();

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
