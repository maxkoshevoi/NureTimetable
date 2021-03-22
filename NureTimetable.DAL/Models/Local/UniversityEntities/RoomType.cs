namespace NureTimetable.DAL.Models.Local
{
    public class RoomType
    {
        public long ID { get; set; }
        public string ShortName { get; set; }

        #region Equals
        public static bool operator ==(RoomType obj1, RoomType obj2) =>
            ReferenceEquals(obj1, obj2) || obj1?.Equals(obj2) == true;

        public static bool operator !=(RoomType obj1, RoomType obj2) =>
            !(obj1 == obj2);

        public override bool Equals(object obj)
        {
            if (obj is RoomType t)
            {
                return ID == t.ID;
            }
            return false;
        }

        public override int GetHashCode() => ID.GetHashCode();
        #endregion
    }
}
