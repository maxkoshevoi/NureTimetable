using Newtonsoft.Json;

namespace NureTimetable.DAL.Models.Cist
{
    public class RoomType
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("short_name")]
        public string ShortName { get; set; } = string.Empty;
    }
}
