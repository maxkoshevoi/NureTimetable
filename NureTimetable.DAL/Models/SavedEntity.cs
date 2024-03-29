﻿using Xamarin.CommunityToolkit.ObjectModel;

namespace NureTimetable.DAL.Models;

public class SavedEntity(Entity entity) : ObservableObject
{
    public Entity Entity { get; } = entity ?? throw new ArgumentNullException(nameof(entity));

    private DateTime? lastUpdated;
    public DateTime? LastUpdated { get => lastUpdated; set => SetProperty(ref lastUpdated, value); }

    private bool isSelected;
    public bool IsSelected { get => isSelected; set => SetProperty(ref isSelected, value); }

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
