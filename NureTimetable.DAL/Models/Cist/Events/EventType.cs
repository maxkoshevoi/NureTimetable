using Newtonsoft.Json;

namespace NureTimetable.DAL.Models.Cist
{
    public class EventType
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("short_name")]
        public string ShortName { get; set; }

        [JsonProperty("full_name")]
        public string FullName { get; set; }

        [JsonProperty("id_base")]
        public long BaseTypeId { get; set; }

        [JsonProperty("type")]
        public string EnglishBaseName { get; set; }
    }
}
