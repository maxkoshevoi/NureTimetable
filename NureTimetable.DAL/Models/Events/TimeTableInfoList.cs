namespace NureTimetable.DAL.Models;

public class TimetableInfoList : TimetableStatistics
{
    public IReadOnlyList<TimetableInfo> Timetables { get; }

    public IEnumerable<Entity> Entities => Timetables.Select(t => t.Entity);

    public IReadOnlyList<Event> Events
    {
        get => events;
        init => events = value.ToList();
    }

    private TimetableInfoList(IReadOnlyList<TimetableInfo> timetables, IReadOnlyList<Event> events) =>
        (Timetables, Events) = (timetables, events);

    public static TimetableInfoList Empty { get; } = Build(null, false);

    public static TimetableInfoList Build(List<TimetableInfo>? timetableInfos, bool applyHiddingSettings)
    {
        timetableInfos ??= new();

        if (applyHiddingSettings)
        {
            timetableInfos.ForEach(tt => tt.ApplyLessonSettings());
        }
        TimetableInfoList timetableInfoList = new
        (
            timetables: timetableInfos.AsReadOnly(),
            events: timetableInfos.SelectMany(tt => tt.Events)
                .GroupBy(e => (e.Type, e.Lesson, e.Start, e.RoomName))
                .Select(group =>
                {
                    Event? combinedEvent = null;
                    foreach (var e in group)
                    {
                        if (combinedEvent == null)
                        {
                            combinedEvent = e;
                            continue;
                        }

                        combinedEvent.Groups.AddRange(e.Groups.Except(combinedEvent.Groups));
                        combinedEvent.Teachers.AddRange(e.Teachers.Except(combinedEvent.Teachers));
                    }
                    return combinedEvent!;
                })
                .ToList()
                .AsReadOnly()
        );
        return timetableInfoList;
    }
}
