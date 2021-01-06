using Newtonsoft.Json;
using System.Collections.Generic;

namespace NureTimetable.DAL.Models.Cist
{
    public class University
    {
        [JsonProperty("short_name")]
        public string ShortName { get; set; }

        [JsonProperty("full_name")]
        public string FullName { get; set; }

        // Used when getting Teachers or Groups
        [JsonProperty("faculties", NullValueHandling = NullValueHandling.Ignore)]
        public List<Faculty> Faculties { get; set; } = new();

        // Used when getting Rooms
        [JsonProperty("buildings", NullValueHandling = NullValueHandling.Ignore)]
        public List<Building> Buildings { get; set; } = new();
    }
}
