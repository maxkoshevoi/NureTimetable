using System.Collections.Generic;
using System.Linq;

namespace NureTimetable.DAL.Models.Local
{
    public class TimetableInfoList : TimetableStatistics
    {
        public IReadOnlyList<TimetableInfo> Timetables { get; private set; }

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

        public static TimetableInfoList Build(List<TimetableInfo> timetableInfos, bool applyHiddingSettings)
        {
            timetableInfos ??= new List<TimetableInfo>();

            if (applyHiddingSettings)
            {
                timetableInfos.ForEach(tt => tt.ApplyLessonSettings());
            }
            var timetableInfoList = new TimetableInfoList
            {
                Timetables = timetableInfos.AsReadOnly(),
                Events = timetableInfos.SelectMany(tt => tt.Events)
                    .GroupBy(e => new { e.Type, e.Lesson, e.Start, e.RoomName })
                    .Select(group =>
                    {
                        Event combinedEvent = null;
                        foreach (Event e in group)
                        {
                            if (combinedEvent == null)
                            {
                                combinedEvent = e;
                            }
                            else
                            {
                                combinedEvent.Groups.AddRange(e.Groups.Except(combinedEvent.Groups));
                                combinedEvent.Teachers.AddRange(e.Teachers.Except(combinedEvent.Teachers));
                            }
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
