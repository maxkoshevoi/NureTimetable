using System;

namespace NureTimetable.DAL.Helpers
{
    /// <summary>
    /// Universal type converter. Supports conversion to Nullable<T> types.
    /// </summary>
    public static class TypeConverter
    {
        public static T Get<T>(this object value, bool DefaultIfNull = false)
        {
            Type type = typeof(T);
            Type underlyingType = Nullable.GetUnderlyingType(type);
            if ((DefaultIfNull || underlyingType != null) && IsNull(value))
            {
                return default;
            }
            if (underlyingType != null)
            {
                type = underlyingType;
            }
            return (T)Convert.ChangeType(value, type);
        }

        public static bool IsNull(object value)
        {
            return value == null || value == DBNull.Value;
        }
    }
}
