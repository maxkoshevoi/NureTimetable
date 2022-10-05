using Newtonsoft.Json;

namespace NureTimetable.DAL.Cist.Models;

public class UniversityRootObject
{
    [JsonProperty("university")]
    public University University { get; set; } = new();
}
