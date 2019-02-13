using System;
using System.ComponentModel;

namespace NureTimetable.Models
{
    public class SavedGroup : Group, INotifyPropertyChanged
    {
        public SavedGroup()
        {

        }

        public SavedGroup(Group group)
        {
            if (group != null)
            {
                ID = group.ID;
                Name = group.Name;
            }
        }
        
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
    }
}
