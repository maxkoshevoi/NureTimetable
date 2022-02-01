using Newtonsoft.Json;
using NureTimetable.DAL.Moodle.Models.Courses;
using static NureTimetable.DAL.Serialisation;

namespace NureTimetable.DAL.Moodle.Models.Calendar
{
    public record Event
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public int DescriptionFormat { get; set; }

        public string Location { get; set; } = string.Empty;

        public int? CategoryId { get; set; }

        public int? GroupId { get; set; }

        public string? GroupName { get; set; }

        public int UserId { get; set; }

        public int? Repeatid { get; set; }

        public int? EventCount { get; set; }

        public string Component { get; set; } = string.Empty;

        public string ModuleName { get; set; } = string.Empty;

        public int Instance { get; set; }

        public string EventType { get; set; } = string.Empty;

        [JsonProperty("timestart")]
        [JsonConverter(typeof(UnixDateTimeConverter))]
        public DateTime StartUtc { get; set; }

        [JsonProperty("timeduration")]
        [JsonConverter(typeof(SecondTimeSpanConverter))]
        public TimeSpan Duration { get; set; }

        [JsonProperty("timesort")]
        [JsonConverter(typeof(UnixDateTimeConverter))]
        public DateTime SortUtc { get; set; }

        public int TimeUserMidnight { get; set; }

        public int Visible { get; set; }

        [JsonProperty("timemodified")]
        [JsonConverter(typeof(UnixDateTimeConverter))]
        public DateTime ModifiedUtc { get; set; }

        public EventIcon Icon { get; set; } = null!;

        public Course Course { get; set; } = null!;

        public SubscriptionInfo Subscription { get; set; } = null!;

        public bool CanEdit { get; set; }

        public bool CandDlete { get; set; }

        public string DeleteUrl { get; set; } = string.Empty;

        public string EditUrl { get; set; } = string.Empty;

        public string ViewUrl { get; set; } = string.Empty;

        public string FormattedTime { get; set; } = string.Empty;

        public bool IsActionEvent { get; set; }

        public bool IsCourseEvent { get; set; }

        public bool IsCategoryEvent { get; set; }

        public string NormalisedEventType { get; set; } = string.Empty;

        public string NormaliseDeventTypeText { get; set; } = string.Empty;

        public string Url { get; set; } = string.Empty;

        public bool IsLastDay { get; set; }

        public string PopupName { get; set; } = string.Empty;

        public bool Draggable { get; set; }

        public DateTime Start => StartUtc.Add(TimeZoneInfo.Local.GetUtcOffset(StartUtc));

        public DateTime End => Start.Add(Duration);

        public record SubscriptionInfo(bool DisplayEventSource);

        public record EventIcon(string Key, string Component, string AltText);
    }
}