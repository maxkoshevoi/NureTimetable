using Newtonsoft.Json;
using System.Collections.Generic;
using static NureTimetable.DAL.Helpers.Serialisation;

namespace NureTimetable.DAL.Models.Cist
{
    class Room
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("short_name")]
        public string ShortName { get; set; }

        [JsonProperty("floor")]
        public int? Floor { get; set; }

        [JsonProperty("is_have_power")]
        [JsonConverter(typeof(StringBoolConverter))]
        public bool? IsHavePower { get; set; }

        [JsonProperty("auditory_types")]
        public List<RoomType> RoomTypes { get; set; } = new List<RoomType>();
    }
}
