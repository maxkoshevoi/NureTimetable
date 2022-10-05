using Newtonsoft.Json;

namespace NureTimetable.DAL.Cist.Models;

public class Group
{
    [JsonProperty("id")]
    public long Id { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;
}
