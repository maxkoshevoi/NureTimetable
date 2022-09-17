using CommunityToolkit.Mvvm.ComponentModel;

namespace NureTimetable.DAL.Models;

public partial class SavedEntity : ObservableObject
{
    public SavedEntity(Entity entity)
    {
        Entity = entity ?? throw new ArgumentNullException(nameof(entity));
    }

    public Entity Entity { get; }

    [ObservableProperty]
    private DateTime? lastUpdated;

    [ObservableProperty]
    private bool isSelected;

    #region Equals
    public static implicit operator Entity(SavedEntity savedEntity) =>
        savedEntity.Entity;

    public static bool operator ==(SavedEntity? obj1, SavedEntity? obj2) =>
        ReferenceEquals(obj1, obj2) || obj1?.Equals(obj2) == true;

    public static bool operator !=(SavedEntity? obj1, SavedEntity? obj2) =>
        !(obj1 == obj2);

    public override bool Equals(object? obj)
    {
        if (obj is SavedEntity savedEntity)
        {
            return Entity == savedEntity.Entity;
        }
        return false;
    }

    public override int GetHashCode() => Entity.GetHashCode();
    #endregion
}