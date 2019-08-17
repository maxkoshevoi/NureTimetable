using System.Collections.Generic;

namespace NureTimetable.DAL.Models.Local
{
    public class Room
    {
        public long ID { get; set; }
        public string Name { get; set; }
        public int Floor { get; set; }
        public bool? IsHavePower { get; set; }
        public List<RoomType> RoomTypes { get; set; } = new List<RoomType>();
        public BaseEntity<string> Building { get; set; }

        #region Equals
        public static bool operator ==(Room obj1, Room obj2)
        {
            if (ReferenceEquals(obj1, obj2))
            {
                return true;
            }
            if (ReferenceEquals(obj1, null) || ReferenceEquals(obj2, null))
            {
                return false;
            }
            return obj1.ID == obj2.ID;
        }

        public static bool operator !=(Room obj1, Room obj2)
        {
            return !(obj1 == obj2);
        }

        public override bool Equals(object obj)
        {
            if (obj is Room)
            {
                return this == (Room)obj;
            }
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return ID.GetHashCode();
        }
        #endregion
    }
}
