namespace NureTimetable.DAL.Models;

public struct BaseEntity<T> : IEquatable<BaseEntity<T>> where T : IComparable<T>
{
    public T ID { get; set; }
    public string ShortName { get; set; }
    public string FullName { get; set; }

    #region Equals
    public static bool operator ==(BaseEntity<T> obj1, BaseEntity<T> obj2) =>
        obj1.Equals(obj2) == true;

    public static bool operator !=(BaseEntity<T> obj1, BaseEntity<T> obj2) =>
        !(obj1 == obj2);

    public bool Equals(BaseEntity<T> other) =>
        this == other;

    public override bool Equals(object? obj)
    {
        if (obj is BaseEntity<T> baseEntity)
        {
            return ID.CompareTo(baseEntity.ID) == 0;
        }
        return false;
    }

    public override int GetHashCode() => ID.GetHashCode();
    #endregion
}
