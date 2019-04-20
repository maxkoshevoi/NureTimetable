namespace NureTimetable.DAL.Models.Local
{
    public class EventType
    {
        public long ID { get; set; }
        public string ShortName { get; set; }
        public string FullName { get; set; }
        public long BaseTypeId { get; set; }
        public string EnglishBaseName { get; set; }

        public static bool operator ==(EventType obj1, EventType obj2)
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

        public static bool operator !=(EventType obj1, EventType obj2)
        {
            return !(obj1 == obj2);
        }
    }
}
