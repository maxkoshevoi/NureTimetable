using System;
using System.Collections.Generic;

namespace NureTimetable.DAL.Models.Local
{
    public class Event
    {
        public EventType Type { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public string RoomName { get; set; }
        public Lesson Lesson { get; set; }
        public int PairNumber { get; set; }
        public List<Teacher> Teachers { get; set; }
        public List<Group> Groups { get; set; }
    }
}
