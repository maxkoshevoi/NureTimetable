using Newtonsoft.Json;

namespace NureTimetable.DAL.Models.Cist
{
    public class Group
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }
}
