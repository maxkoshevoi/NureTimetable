using Newtonsoft.Json;
using System.Collections.Generic;

namespace NureTimetable.DAL.Models.Cist
{
    public class Lesson
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("brief")]
        public string ShortName { get; set; } = string.Empty;

        [JsonProperty("title")]
        public string FullName { get; set; } = string.Empty;

        [JsonProperty("hours")]
        public List<HoursPlanned> Duration { get; set; } = new();
    }
}
