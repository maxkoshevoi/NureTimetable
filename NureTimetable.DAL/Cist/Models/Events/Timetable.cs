using Newtonsoft.Json;

namespace NureTimetable.DAL.Cist.Models
{
    public class Timetable
    {
        [JsonProperty("time-zone")]
        public string TimeZone { get; set; } = string.Empty;

        [JsonProperty("events")]
        public List<Event> Events { get; set; } = new();

        [JsonProperty("groups")]
        public List<Group> Groups { get; set; } = new();

        [JsonProperty("teachers")]
        public List<Teacher> Teachers { get; set; } = new();

        [JsonProperty("subjects")]
        public List<Lesson> Lessons { get; set; } = new();

        [JsonProperty("types")]
        public List<EventType> EventTypes { get; set; } = new();
    }
}
