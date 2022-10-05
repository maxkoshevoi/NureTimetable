using NureTimetable.Core.Extensions;

namespace NureTimetable.DAL.Models;

public class Event
{
    public EventType Type { get; set; } = EventType.UnknownType;
    public DateTime StartUtc { get; set; }
    public DateTime EndUtc { get; set; }
    public DateTime Start => StartUtc.Add(TimeZoneInfo.Local.GetUtcOffset(StartUtc));
    public DateTime End => EndUtc.Add(TimeZoneInfo.Local.GetUtcOffset(EndUtc));
    public string RoomName { get; set; } = string.Empty;
    public Lesson Lesson { get; set; } = null!;
    public int PairNumber { get; set; }
    public List<Teacher> Teachers { get; set; } = new();
    public List<Group> Groups { get; set; } = new();

    #region Equals
    public static bool operator ==(Event? obj1, Event? obj2) =>
        ReferenceEquals(obj1, obj2) || obj1?.Equals(obj2) == true;

    public static bool operator !=(Event? obj1, Event? obj2) =>
        !(obj1 == obj2);

    public override bool Equals(object? obj)
    {
        if (obj is Event e)
        {
            return Type.Equals(e.Type) &&
                Start == e.Start &&
                RoomName == e.RoomName &&
                Lesson.Equals(e.Lesson) &&
                Teachers.Count == e.Teachers.Count && Teachers.Except(e.Teachers).None() &&
                Groups.Count == e.Groups.Count && Groups.Except(e.Groups).None();
        }
        return false;
    }

    public override int GetHashCode() =>
        HashCode.Combine(
            Type,
            Start,
            RoomName,
            Lesson,
            Teachers,
            Groups);
    #endregion
}
