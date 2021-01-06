using Newtonsoft.Json;
using System.Collections.Generic;

namespace NureTimetable.DAL.Models.Cist
{
    public class Building
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("short_name")]
        public string ShortName { get; set; }

        [JsonProperty("full_name")]
        public string FullName { get; set; }

        [JsonProperty("auditories")]
        public List<Room> Rooms { get; set; } = new();
    }
}
