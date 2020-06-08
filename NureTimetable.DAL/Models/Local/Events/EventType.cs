namespace NureTimetable.DAL.Models.Local
{
    public class EventType
    {
        public long ID { get; set; }
        public string ShortName { get; set; }
        public string FullName { get; set; }
        public long BaseTypeId { get; set; }
        public string EnglishBaseName { get; set; }

        #region Equals
        public static bool operator ==(EventType obj1, EventType obj2)
        {
            if (ReferenceEquals(obj1, obj2))
            {
                return true;
            }
            if (obj1 is null || obj2 is null)
            {
                return false;
            }
            return obj1.ID == obj2.ID;
        }

        public static bool operator !=(EventType obj1, EventType obj2)
        {
            return !(obj1 == obj2);
        }

        public override bool Equals(object obj)
        {
            if (obj is EventType eventType)
            {
                return this == eventType;
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
