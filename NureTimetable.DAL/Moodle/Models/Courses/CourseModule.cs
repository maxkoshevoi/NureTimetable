using static NureTimetable.DAL.Serialisation;

namespace NureTimetable.DAL.Moodle.Models.Courses;

public record CourseModule
{
    public int Id { get; set; }

    public string Url { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public int Instance { get; set; }

    public int ContextId { get; set; }

    [JsonProperty("visible")]
    public bool IsVisible { get; set; }

    public bool UserVisible { get; set; }

    public int VisibleOnCoursePage { get; set; }

    public string ModIcon { get; set; } = string.Empty;

    public string ModName { get; set; } = string.Empty;

    public string ModPlural { get; set; } = string.Empty;

    public int Indent { get; set; }

    public string OnClick { get; set; } = string.Empty;

    public string? AfterLink { get; set; }

    public string CustomData { get; set; } = string.Empty;

    public bool NoViewLink { get; set; }

    public int Completion { get; set; }

    public string? Description { get; set; }

    public List<Date> Dates { get; set; } = new();

    public List<CourseSection> Contents { get; set; } = new();

    public ModuleContentsInfo? ContentsInfo { get; set; }

    public ModuleCompletionData? CompletionData { get; set; }

    public record Date(
        string Label,
        [JsonProperty("timestamp")]
        [JsonConverter(typeof(UnixDateTimeConverter))]
        DateTime TimeStampUtc);

    public record ModuleContentsInfo
    {
        public int FilesCount { get; set; }

        public int FilesSize { get; set; }

        [JsonConverter(typeof(UnixDateTimeConverter))]
        public DateTime LastModified { get; set; }

        public List<string> MimeTypes { get; set; } = new();

        public string RepositoryType { get; set; } = string.Empty;
    }

    public class ModuleCompletionData
    {
        public int State { get; set; }

        [JsonConverter(typeof(UnixDateTimeConverter))]
        public DateTime TimeCompleted { get; set; }

        public string? OverrideBy { get; set; }

        public bool ValueUsed { get; set; }

        public bool HasCompletion { get; set; }

        public bool IsAutomatic { get; set; }

        public bool IsTrackedUser { get; set; }

        public bool UserVisible { get; set; }

        public List<Detail> Details { get; set; } = new();

        public record Detail(string RuleName, RuleValue RuleValue);

        public record RuleValue(int Status, string Description);
    }
}
