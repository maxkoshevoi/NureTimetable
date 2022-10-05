namespace NureTimetable.DAL.Cist.Legacy.Models;

[Obsolete("", true)]
class EventTypeInfo
{
    public string? Name { get; set; }

    public List<string> Teachers { get; } = new();

    public override string ToString() => $"{Name} - {string.Join(", ", Teachers)}";
}
