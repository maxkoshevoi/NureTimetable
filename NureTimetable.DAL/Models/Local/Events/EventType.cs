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
        public static bool operator ==(EventType obj1, EventType obj2) =>
            ReferenceEquals(obj1, obj2) || obj1?.Equals(obj2) == true;

        public static bool operator !=(EventType obj1, EventType obj2) =>
            !(obj1 == obj2);

        public override bool Equals(object obj)
        {
            if (obj is EventType eventType)
            {
                return ID == eventType.ID;
            }
            return false;
        }

        public override int GetHashCode() => ID.GetHashCode();
        #endregion
    }
}
