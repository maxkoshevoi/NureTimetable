using Newtonsoft.Json;
using System.Collections.Generic;

namespace NureTimetable.DAL.Models.Cist
{
    public class Timetable
    {
        [JsonProperty("time-zone")]
        public string TimeZone { get; set; }

        [JsonProperty("events")]
        public List<Event> Events { get; set; }

        [JsonProperty("groups")]
        public List<Group> Groups { get; set; }

        [JsonProperty("teachers")]
        public List<Teacher> Teachers { get; set; }

        [JsonProperty("subjects")]
        public List<Lesson> Lessons { get; set; }

        [JsonProperty("types")]
        public List<EventType> EventTypes { get; set; }
    }
}
