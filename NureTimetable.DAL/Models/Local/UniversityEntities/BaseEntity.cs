using System;

namespace NureTimetable.DAL.Models.Local
{
    public struct BaseEntity<T> : IEquatable<BaseEntity<T>> where T: IComparable<T>
    {
        public T ID { get; set; }
        public string ShortName { get; set; }
        public string FullName { get; set; }

        #region Equals
        public static bool operator ==(BaseEntity<T> obj1, BaseEntity<T> obj2)
        {
            return obj1.ID.CompareTo(obj2.ID) == 0;
        }

        public static bool operator !=(BaseEntity<T> obj1, BaseEntity<T> obj2)
        {
            return !(obj1 == obj2);
        }

        public override bool Equals(object obj)
        {
            if (obj is BaseEntity<T> castObj)
            {
                return Equals(castObj);
            }
            return base.Equals(obj);
        }

        public bool Equals(BaseEntity<T> other)
        {
            return this == other;
        }

        public override int GetHashCode()
        {
            return ID.GetHashCode();
        }
        #endregion
    }
}
