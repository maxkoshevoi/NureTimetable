using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using static NureTimetable.DAL.Helpers.Serialisation;

namespace NureTimetable.DAL.Models.Cist
{
    public class Event
    {
        [JsonProperty("subject_id")]
        public long LessonId { get; set; }

        [JsonProperty("start_time")]
        [JsonConverter(typeof(SecondEpochConverter))]
        public DateTime StartTime { get; set; }

        [JsonProperty("end_time")]
        [JsonConverter(typeof(SecondEpochConverter))]
        public DateTime EndTime { get; set; }

        [JsonProperty("type")]
        public long TypeId { get; set; }

        [JsonProperty("number_pair")]
        public int PairNumber { get; set; }

        [JsonProperty("auditory")]
        public string Room { get; set; }

        [JsonProperty("teachers")]
        public List<long> TeacherIds { get; set; }

        [JsonProperty("groups")]
        public List<long> GroupIds { get; set; }
    }
}
