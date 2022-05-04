namespace NureTimetable.DAL.Cist.Legacy.Models
{
    [Obsolete("", true)]
    class LessonInfo
    {
        #region Details
        public string? ShortName { get; set; }
        public string? LongName { get; set; }
        public List<EventTypeInfo> EventTypesInfo { get; set; } = new();
        public DateTime? LastUpdated { get; set; }
        #endregion

        public override string ToString() => $"{ShortName} - {string.Join(", ", EventTypesInfo.Select(et => et.Name))}";
    }
}
