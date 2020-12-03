using System;

namespace NureTimetable.DAL.Models.Local
{
    public class SavedEntity : Entity
    {
        protected SavedEntity() 
        {
        }

        public SavedEntity(Group group) : base(group) 
        { 
        }

        public SavedEntity(Teacher teacher) : base(teacher) 
        { 
        }

        public SavedEntity(Room room) : base(room) 
        { 
        }

        private DateTime? lastUpdated;
        public DateTime? LastUpdated { get => lastUpdated; set => SetProperty(ref lastUpdated, value); }

        private bool isSelected;
        public bool IsSelected { get => isSelected; set => SetProperty(ref isSelected, value); }
    }
}
