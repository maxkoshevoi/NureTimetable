using System.Collections.Generic;

namespace NureTimetable.DAL.Models
{
    public class Room
    {
        public long ID { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Floor { get; set; }
        public bool? IsHavePower { get; set; }
        public List<RoomType> RoomTypes { get; set; } = new();
        public BaseEntity<string> Building { get; set; }

        #region Equals
        public static bool operator ==(Room? obj1, Room? obj2) =>
            ReferenceEquals(obj1, obj2) || obj1?.Equals(obj2) == true;

        public static bool operator !=(Room? obj1, Room? obj2) =>
            !(obj1 == obj2);

        public override bool Equals(object? obj)
        {
            if (obj is Room room)
            {
                return ID == room.ID;
            }
            return false;
        }

        public override int GetHashCode() => ID.GetHashCode();
        #endregion
    }
}
