﻿using Newtonsoft.Json;
using System.Collections.Generic;

namespace NureTimetable.DAL.Models.Cist
{
    class Faculty
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("short_name")]
        public string ShortName { get; set; }

        [JsonProperty("full_name")]
        public string FullName { get; set; }

        // Used when getting Teachers
        [JsonProperty("departments", NullValueHandling = NullValueHandling.Ignore)]
        public List<Department> Departments { get; set; } = new List<Department>();

        // Used when getting Groups
        [JsonProperty("directions", NullValueHandling = NullValueHandling.Ignore)]
        public List<Direction> Directions { get; set; } = new List<Direction>();
    }
}
