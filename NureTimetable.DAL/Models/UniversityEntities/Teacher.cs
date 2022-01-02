namespace NureTimetable.DAL.Models
{
    public class Teacher
    {
        public long ID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ShortName { get; set; } = string.Empty;
        public BaseEntity<long> Department { get; set; }

        #region Equals
        public static bool operator ==(Teacher? obj1, Teacher? obj2) =>
            ReferenceEquals(obj1, obj2) || obj1?.Equals(obj2) == true;

        public static bool operator !=(Teacher? obj1, Teacher? obj2) =>
            !(obj1 == obj2);

        public override bool Equals(object? obj)
        {
            if (obj is Teacher teacher)
            {
                return ID == teacher.ID;
            }
            return false;
        }

        public override int GetHashCode() => ID.GetHashCode();
        #endregion
    }
}
