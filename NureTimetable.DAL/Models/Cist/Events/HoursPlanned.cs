using Newtonsoft.Json;
using System.Collections.Generic;

namespace NureTimetable.DAL.Models.Cist
{
    public class HoursPlanned
    {
        [JsonProperty("type")]
        public long? EventTypeId { get; set; }

        [JsonProperty("val")]
        public long Hours { get; set; }

        [JsonProperty("teachers")]
        public List<long> TeacherIds { get; set; }
    }
}
