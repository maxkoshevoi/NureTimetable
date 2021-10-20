namespace NureTimetable.DAL.Models
{
    public class EventType
    {
        public long ID { get; set; }
        public string ShortName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public long BaseTypeId { get; set; }
        public string EnglishBaseName { get; set; } = string.Empty;

        public static EventType UnknownType => new()
        {
            ID = -1,
            BaseTypeId = -1,
            ShortName = "-",
            FullName = "-",
            EnglishBaseName = "Unknown"
        };

        #region Equals
        public static bool operator ==(EventType? obj1, EventType? obj2) =>
            ReferenceEquals(obj1, obj2) || obj1?.Equals(obj2) == true;

        public static bool operator !=(EventType? obj1, EventType? obj2) =>
            !(obj1 == obj2);

        public override bool Equals(object? obj)
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
