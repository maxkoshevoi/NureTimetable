using Newtonsoft.Json;
using System.Collections.Generic;

namespace NureTimetable.DAL.Models.Cist
{
    public class Direction
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("short_name")]
        public string ShortName { get; set; }

        [JsonProperty("full_name")]
        public string FullName { get; set; }

        [JsonProperty("groups", NullValueHandling = NullValueHandling.Ignore)]
        public List<Group> Groups { get; private set; } = new();

        [JsonProperty("specialities")]
        public List<Speciality> Specialities { get; private set; } = new();
    }
}
