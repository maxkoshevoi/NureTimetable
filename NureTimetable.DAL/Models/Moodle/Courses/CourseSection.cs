using Newtonsoft.Json;
using System.Collections.Generic;

namespace NureTimetable.DAL.Models.Moodle.Courses;

public record CourseSection
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    [JsonProperty("visible")]
    public bool IsVisible { get; set; }

    public string Summary { get; set; } = string.Empty;

    public int SummaryFormat { get; set; }

    public int Section { get; set; }

    public int HiddenByNumSections { get; set; }

    public bool UserVisible { get; set; }

    public List<CourseModule> Modules { get; set; } = new();
}
