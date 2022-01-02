using Newtonsoft.Json;
using static NureTimetable.DAL.Serialisation;

namespace NureTimetable.DAL.Moodle.Models.Courses;

public record Course
{
    public int Id { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string ShortName { get; set; } = string.Empty;

    public string IdNumber { get; set; } = string.Empty;

    public string Summary { get; set; } = string.Empty;

    public int SummaryFormat { get; set; }

    [JsonProperty("startdate")]
    [JsonConverter(typeof(UnixDateTimeConverter))]
    public DateTime StartDateUtc { get; set; }

    [JsonProperty("enddate")]
    [JsonConverter(typeof(UnixDateTimeConverter))]
    public DateTime EndDateUtc { get; set; }

    [JsonProperty("visible")]
    public bool IsVisible { get; set; }

    public bool ShowActivityDates { get; set; }

    public bool? ShowCompletionConditions { get; set; }

    public string FullNameDisplay { get; set; } = string.Empty;

    public string ViewUrl { get; set; } = string.Empty;

    public string CourseImage { get; set; } = string.Empty;

    /// <summary>
    /// 0-100
    /// </summary>
    public double? Progress { get; set; }

    public bool HasProgress { get; set; }

    public bool IsFavourite { get; set; }

    [JsonProperty("hidden")]
    public bool IsHidden { get; set; }

    public bool ShowShortName { get; set; }

    public string CourseCategory { get; set; } = string.Empty;
}
