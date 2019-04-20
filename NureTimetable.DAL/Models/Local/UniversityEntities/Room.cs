using System.Collections.Generic;

namespace NureTimetable.DAL.Models.Local
{
    public class Room
    {
        public long ID { get; set; }
        public string ShortName { get; set; }
        public int Floor { get; set; }
        public bool? IsHavePower { get; set; }
        public List<RoomType> RoomTypes { get; set; } = new List<RoomType>();
        public BaseEntity<string> Building { get; set; }
    }
}
