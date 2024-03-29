﻿namespace NureTimetable.DAL.Models;

public class Lesson
{
    public long ID { get; set; }
    public string ShortName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    //public int HoursPlanned { get; set; }

    #region Equals
    public static bool operator ==(Lesson? obj1, Lesson? obj2) =>
        ReferenceEquals(obj1, obj2) || obj1?.Equals(obj2) == true;

    public static bool operator !=(Lesson? obj1, Lesson? obj2) =>
        !(obj1 == obj2);

    public override bool Equals(object? obj)
    {
        if (obj is Lesson lesson)
        {
            return ID == lesson.ID;
        }
        return false;
    }

    public override int GetHashCode() => ID.GetHashCode();
    #endregion
}
