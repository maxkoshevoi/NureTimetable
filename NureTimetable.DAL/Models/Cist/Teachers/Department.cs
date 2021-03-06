﻿using Newtonsoft.Json;
using System.Collections.Generic;

namespace NureTimetable.DAL.Models.Cist
{
    public class Department
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("short_name")]
        public string ShortName { get; set; } = string.Empty;

        [JsonProperty("full_name")]
        public string FullName { get; set; } = string.Empty;

        [JsonProperty("teachers")]
        public List<Teacher> Teachers { get; set; } = new();
    }
}
