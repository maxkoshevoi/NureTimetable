using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;

namespace NureTimetable.DAL.Cist.Models
{
    public class Event
    {
        [JsonProperty("subject_id")]
        public long LessonId { get; set; }

        [JsonProperty("start_time")]
        [JsonConverter(typeof(UnixDateTimeConverter))]
        public DateTime StartTime { get; set; }

        [JsonProperty("end_time")]
        [JsonConverter(typeof(UnixDateTimeConverter))]
        public DateTime EndTime { get; set; }

        [JsonProperty("type")]
        public long? TypeId { get; set; }

        [JsonProperty("number_pair")]
        public int PairNumber { get; set; }

        [JsonProperty("auditory")]
        public string Room { get; set; } = string.Empty;

        [JsonProperty("teachers")]
        public List<long> TeacherIds { get; set; } = new();

        [JsonProperty("groups")]
        public List<long> GroupIds { get; set; } = new();
    }
}
