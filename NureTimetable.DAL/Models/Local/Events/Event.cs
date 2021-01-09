using NureTimetable.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NureTimetable.DAL.Models.Local
{
    public class Event
    {
        public EventType Type { get; set; }
        public DateTime StartUtc { get; set; }
        public DateTime EndUtc { get; set; }
        public DateTime Start => StartUtc.Add(TimeZoneInfo.Local.GetUtcOffset(StartUtc));
        public DateTime End => EndUtc.Add(TimeZoneInfo.Local.GetUtcOffset(EndUtc));
        public string RoomName { get; set; }
        public Lesson Lesson { get; set; }
        public int PairNumber { get; set; }
        public List<Teacher> Teachers { get; set; }
        public List<Group> Groups { get; set; }

        #region Equals
        public static bool operator ==(Event obj1, Event obj2)
        {
            if (ReferenceEquals(obj1, obj2))
            {
                return true;
            }
            if (obj1 is null || obj2 is null)
            {
                return false;
            }
            return obj1.Type == obj2.Type && 
                obj1.Start == obj2.Start && 
                obj1.RoomName == obj2.RoomName && 
                obj1.Lesson == obj2.Lesson &&
                obj1.Teachers.Count == obj2.Teachers.Count && !obj1.Teachers.Except(obj2.Teachers).Any() &&
                obj1.Groups.Count == obj2.Groups.Count && !obj1.Groups.Except(obj2.Groups).Any();
        }

        public static bool operator !=(Event obj1, Event obj2)
        {
            return !(obj1 == obj2);
        }

        public override bool Equals(object obj)
        {
            if (obj is Event e)
            {
                return this == e;
            }
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                // Choose large primes to avoid hashing collisions
                const int HashingBase = (int)2166136261;
                const int HashingMultiplier = 16777619;

                int hash = HashingBase;
                hash = (hash * HashingMultiplier) ^ Type.GetHashCode();
                hash = (hash * HashingMultiplier) ^ Start.GetHashCode();
                hash = (hash * HashingMultiplier) ^ RoomName?.GetHashCode() ?? 0;
                hash = (hash * HashingMultiplier) ^ Lesson?.GetHashCode() ?? 0;
                hash = (hash * HashingMultiplier) ^ Teachers?.GetTrueHashCode() ?? 0;
                hash = (hash * HashingMultiplier) ^ Groups?.GetTrueHashCode() ?? 0;
                return hash;
            }
        }
        #endregion
    }
}
