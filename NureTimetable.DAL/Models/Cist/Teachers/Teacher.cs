using Newtonsoft.Json;

namespace NureTimetable.DAL.Models.Cist
{
    class Teacher
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("full_name")]
        public string FullName { get; set; }

        [JsonProperty("short_name")]
        public string ShortName { get; set; }
    }
}
