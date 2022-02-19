namespace NureTimetable.DAL.Models;

public class Entity
{
    public Entity(Group group) : this(group.ID, group.Name, TimetableEntityType.Group)
    { }

    public Entity(Teacher teacher) : this(teacher.ID, teacher.Name, TimetableEntityType.Teacher)
    { }

    public Entity(Room room) : this(room.ID, room.Name, TimetableEntityType.Room)
    { }

    [JsonConstructor]
    public Entity(long id, string name, TimetableEntityType type) =>
        (ID, Name, Type) = (id, name, type);

    // public setters needed for deserialization
    public long ID { get; set; }
    public string Name { get; set; }
    public TimetableEntityType Type { get; set; }

    #region Equals
    public static bool operator ==(Entity? obj1, Entity? obj2) =>
        ReferenceEquals(obj1, obj2) || obj1?.Equals(obj2) == true;

    public static bool operator !=(Entity? obj1, Entity? obj2) =>
        !(obj1 == obj2);

    public override bool Equals(object? obj)
    {
        if (obj is Entity entity)
        {
            return Type == entity.Type && ID == entity.ID;
        }
        return false;
    }

    public override int GetHashCode() => HashCode.Combine(ID, Type);
    #endregion
}
