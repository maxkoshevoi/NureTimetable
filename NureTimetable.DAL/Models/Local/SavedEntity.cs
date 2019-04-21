using System;
using System.ComponentModel;

namespace NureTimetable.DAL.Models.Local
{
    public class SavedEntity : INotifyPropertyChanged
    {
        public SavedEntity()
        { }

        public SavedEntity(Group group)
        {
            ID = group.ID;
            Name = group.Name;
            Type = TimetableEntityType.Group;
        }

        public long ID { get; set; }
        public string Name { get; set; }
        public TimetableEntityType Type { get; set; }

        private DateTime? _lastUpdated;
        public DateTime? LastUpdated
        {
            get => _lastUpdated;
            set 
            {
                _lastUpdated = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LastUpdated)));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        #region Equals
        public static bool operator ==(SavedEntity obj1, SavedEntity obj2)
        {
            if (ReferenceEquals(obj1, obj2))
            {
                return true;
            }
            if (ReferenceEquals(obj1, null) || ReferenceEquals(obj2, null))
            {
                return false;
            }
            return obj1.Type == obj2.Type && obj1.ID == obj2.ID;
        }
        
        public static bool operator !=(SavedEntity obj1, SavedEntity obj2)
        {
            return !(obj1 == obj2);
        }

        public override bool Equals(object obj)
        {
            if (obj is SavedEntity)
            {
                return this == (SavedEntity)obj;
            }
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return ID.GetHashCode() ^ Type.GetHashCode();
        }
        #endregion
    }
}
