using Newtonsoft.Json;

namespace NureTimetable.DAL.Models.Cist
{
    public class University
    {
        [JsonProperty("short_name")]
        public string ShortName { get; set; } = string.Empty;

        [JsonProperty("full_name")]
        public string FullName { get; set; } = string.Empty;

        // Used when getting Teachers or Groups
        [JsonProperty("faculties", NullValueHandling = NullValueHandling.Ignore)]
        public List<Faculty> Faculties { get; set; } = new();

        // Used when getting Rooms
        [JsonProperty("buildings", NullValueHandling = NullValueHandling.Ignore)]
        public List<Building> Buildings { get; set; } = new();
    }
}
