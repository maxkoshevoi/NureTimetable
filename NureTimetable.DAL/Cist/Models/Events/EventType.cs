namespace NureTimetable.DAL.Cist.Models;

public class EventType
{
    [JsonProperty("id")]
    public long Id { get; set; }

    [JsonProperty("short_name")]
    public string ShortName { get; set; } = string.Empty;

    [JsonProperty("full_name")]
    public string FullName { get; set; } = string.Empty;

    [JsonProperty("id_base")]
    public long BaseTypeId { get; set; }

    [JsonProperty("type")]
    public string EnglishBaseName { get; set; } = string.Empty;
}
