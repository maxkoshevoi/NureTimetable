using NureTimetable.Core.Models;

namespace NureTimetable.DAL.Models.Local
{
    public class Entity : NotifyPropertyChangedBase
    {
        protected Entity()
        { 
        }

        public Entity(Group group)
        {
            ID = group.ID;
            Name = group.Name;
            Type = TimetableEntityType.Group;
        }

        public Entity(Teacher teacher)
        {
            ID = teacher.ID;
            Name = teacher.ShortName;
            Type = TimetableEntityType.Teacher;
        }

        public Entity(Room room)
        {
            ID = room.ID;
            Name = room.Name;
            Type = TimetableEntityType.Room;
        }

        // public setters needed for deserialization
        public long ID { get; set; }
        public string Name { get; set; }
        public TimetableEntityType Type { get; set; }

        #region Equals
        public static bool operator ==(Entity obj1, Entity obj2)
        {
            if (ReferenceEquals(obj1, obj2))
            {
                return true;
            }
            if (obj1 is null || obj2 is null)
            {
                return false;
            }
            return obj1.Type == obj2.Type && obj1.ID == obj2.ID;
        }
        
        public static bool operator !=(Entity obj1, Entity obj2)
        {
            return !(obj1 == obj2);
        }

        public override bool Equals(object obj)
        {
            if (obj is Entity entity)
            {
                return this == entity;
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
