using Newtonsoft.Json;

namespace NureTimetable.DAL.Models.Cist
{
    class UniversityRootObject
    {
        [JsonProperty("university")]
        public University University { get; set; }
    }
}
