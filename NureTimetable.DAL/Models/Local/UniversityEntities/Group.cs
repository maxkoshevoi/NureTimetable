namespace NureTimetable.DAL.Models.Local
{
    public class Group
    {
        public long ID { get; set; }
        public string Name { get; set; } = string.Empty;
        public BaseEntity<long> Faculty { get; set; }
        public BaseEntity<long> Direction { get; set; }
        public BaseEntity<long>? Speciality { get; set; }

        #region Equals
        public static bool operator ==(Group? obj1, Group? obj2) =>
            ReferenceEquals(obj1, obj2) || obj1?.Equals(obj2) == true;

        public static bool operator !=(Group? obj1, Group? obj2) =>
            !(obj1 == obj2);

        public override bool Equals(object? obj)
        {
            if (obj is Group g)
            {
                return ID == g.ID;
            }
            return false;
        }

        public override int GetHashCode() => ID.GetHashCode();
        #endregion
    }
}
