using Newtonsoft.Json;

namespace NureTimetable.DAL.Cist.Models
{
    public class Department
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("short_name")]
        public string ShortName { get; set; } = string.Empty;

        [JsonProperty("full_name")]
        public string FullName { get; set; } = string.Empty;

        [JsonProperty("teachers")]
        public List<Teacher> Teachers { get; set; } = new();
    }
}
