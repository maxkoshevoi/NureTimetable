namespace NureTimetable.DAL.Models.Local
{
    public class Group
    {
        public long ID { get; set; }
        public string Name { get; set; }
        public BaseEntity<long> Faculty { get; set; }
        public BaseEntity<long> Direction { get; set; }
        public BaseEntity<long>? Speciality { get; set; }

        #region Equals
        public static bool operator ==(Group obj1, Group obj2)
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

        public static bool operator !=(Group obj1, Group obj2)
        {
            return !(obj1 == obj2);
        }

        public override bool Equals(object obj)
        {
            if (obj is Group)
            {
                return this == (Group)obj;
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
