namespace NureTimetable.DAL.Models.Local
{
    public class Teacher
    {
        public long ID { get; set; }
        public string Name { get; set; }
        public string ShortName { get; set; }
        public BaseEntity<long> Department { get; set; }

        #region Equals
        public static bool operator ==(Teacher obj1, Teacher obj2)
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

        public static bool operator !=(Teacher obj1, Teacher obj2)
        {
            return !(obj1 == obj2);
        }

        public override bool Equals(object obj)
        {
            if (obj is Teacher)
            {
                return this == (Teacher)obj;
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
