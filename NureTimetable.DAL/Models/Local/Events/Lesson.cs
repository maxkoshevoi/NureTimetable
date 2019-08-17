namespace NureTimetable.DAL.Models.Local
{
    public class Lesson
    {
        public long ID { get; set; }
        public string ShortName { get; set; }
        public string FullName { get; set; }
        //public int HoursPlanned { get; set; }

        #region Equals
        public static bool operator ==(Lesson obj1, Lesson obj2)
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

        public static bool operator !=(Lesson obj1, Lesson obj2)
        {
            return !(obj1 == obj2);
        }

        public override bool Equals(object obj)
        {
            if (obj is Lesson)
            {
                return this == (Lesson)obj;
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
