using System.Collections.Generic;
using System.Linq;

namespace NureTimetable.DAL.Models.Local
{
    public class TimetableInfoList : TimetableStatistics
    {
        public IReadOnlyList<TimetableInfo> Timetables { get; private set; }

        public IEnumerable<Entity> Entities => Timetables.Select(t => t.Entity);

        public IReadOnlyList<Event> Events
        {
            get => events;
            private set => events = value.ToList();
        }

        public List<LessonInfo> LessonsInfo
        {
            get
            {
                if (Timetables.Count != 1)
                {
                    return null;
                }
                return Timetables[0].LessonsInfo;
            }
        }

        private TimetableInfoList()
        { }

        public static TimetableInfoList Empty { get; } = Build(null, false);

        public static TimetableInfoList Build(List<TimetableInfo> timetableInfos, bool applyHiddingSettings)
        {
            timetableInfos ??= new();

            if (applyHiddingSettings)
            {
                timetableInfos.ForEach(tt => tt.ApplyLessonSettings());
            }
            TimetableInfoList timetableInfoList = new()
            {
                Timetables = timetableInfos.AsReadOnly(),
                Events = timetableInfos.SelectMany(tt => tt.Events)
                    .GroupBy(e => new { e.Type, e.Lesson, e.Start, e.RoomName })
                    .Select(group =>
                    {
                        Event combinedEvent = null;
                        foreach (Event e in group)
                        {
                            if (combinedEvent is null)
                            {
                                combinedEvent = e;
                                continue;
                            }

                            combinedEvent.Groups.AddRange(e.Groups.Except(combinedEvent.Groups));
                            combinedEvent.Teachers.AddRange(e.Teachers.Except(combinedEvent.Teachers));
                        }
                        return combinedEvent;
                    })
                    .ToList()
                    .AsReadOnly()
            };
            return timetableInfoList;
        }
    }
}
