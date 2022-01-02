using Newtonsoft.Json;

namespace NureTimetable.DAL.Cist.Models
{
    public class Building
    {
        [JsonProperty("id")]
        public string Id { get; set; } = string.Empty;

        [JsonProperty("short_name")]
        public string ShortName { get; set; } = string.Empty;

        [JsonProperty("full_name")]
        public string FullName { get; set; } = string.Empty;

        [JsonProperty("auditories")]
        public List<Room> Rooms { get; set; } = new();
    }
}
