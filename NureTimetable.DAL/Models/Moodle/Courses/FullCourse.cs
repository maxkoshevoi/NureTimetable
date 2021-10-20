﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using static NureTimetable.DAL.Helpers.Serialisation;

namespace NureTimetable.DAL.Models.Moodle.Courses;

public record FullCourse : Course
{
    public string DisplayName { get; set; } = string.Empty;

    public int EnrolledUserCount { get; set; }

    public string Format { get; set; } = string.Empty;

    public bool ShowGrades { get; set; }

    public string Lang { get; set; } = string.Empty;

    public bool EnableCompletion { get; set; }

    public bool CompletionHasCriteria { get; set; }

    public bool CompletionUserTracked { get; set; }

    public int Category { get; set; }

    public bool? Completed { get; set; }

    public int Marker { get; set; }

    [JsonConverter(typeof(SecondEpochConverter))]
    public DateTime? LastAccess { get; set; }

    public List<Overviewfile> Overviewfiles { get; set; } = new();

    public record Overviewfile
    {
        public string FileName { get; set; } = string.Empty;

        public string FilePath { get; set; } = string.Empty;

        public int FileSize { get; set; }

        public string FileUrl { get; set; } = string.Empty;

        [JsonConverter(typeof(SecondEpochConverter))]
        public DateTime TimeModified { get; set; }

        public string MimeType { get; set; } = string.Empty;
    }
}
