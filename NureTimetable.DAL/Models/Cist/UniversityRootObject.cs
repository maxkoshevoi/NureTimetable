using Newtonsoft.Json;

namespace NureTimetable.DAL.Models.Cist
{
    public class UniversityRootObject
    {
        [JsonProperty("university")]
        public University University { get; set; }
    }
}
