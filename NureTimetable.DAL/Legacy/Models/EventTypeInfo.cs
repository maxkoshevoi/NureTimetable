namespace NureTimetable.DAL.Legacy.Models
{
    class EventTypeInfo
    {
        public string? Name { get; set; }

        public List<string> Teachers { get; } = new();

        public override string ToString() => $"{Name} - {string.Join(", ", Teachers)}";
    }
}
