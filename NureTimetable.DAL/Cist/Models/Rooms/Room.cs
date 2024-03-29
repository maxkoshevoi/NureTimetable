﻿using Newtonsoft.Json;
using static NureTimetable.DAL.Serialization;

namespace NureTimetable.DAL.Cist.Models;

public class Room
{
    [JsonProperty("id")]
    public long Id { get; set; }

    [JsonProperty("short_name")]
    public string ShortName { get; set; } = string.Empty;

    [JsonProperty("floor")]
    public int? Floor { get; set; }

    [JsonProperty("is_have_power")]
    [JsonConverter(typeof(StringBoolConverter))]
    public bool? IsHavePower { get; set; }

    [JsonProperty("auditory_types")]
    public List<RoomType> RoomTypes { get; set; } = new();
}
