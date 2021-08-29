using Newtonsoft.Json;

namespace NureTimetable.DAL.Models.Cist
{
    public class Faculty
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("short_name")]
        public string ShortName { get; set; } = string.Empty;

        [JsonProperty("full_name")]
        public string FullName { get; set; } = string.Empty;

        // Used when getting Teachers
        [JsonProperty("departments", NullValueHandling = NullValueHandling.Ignore)]
        public List<Department> Departments { get; set; } = new();

        // Used when getting Groups
        [JsonProperty("directions", NullValueHandling = NullValueHandling.Ignore)]
        public List<Direction> Directions { get; set; } = new();
    }
}
