using Newtonsoft.Json;

namespace NureTimetable.DAL.Cist.Models
{
    public class Speciality
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("short_name")]
        public string ShortName { get; set; } = string.Empty;

        [JsonProperty("full_name")]
        public string FullName { get; set; } = string.Empty;

        [JsonProperty("groups")]
        public List<Group> Groups { get; set; } = new();
    }
}
