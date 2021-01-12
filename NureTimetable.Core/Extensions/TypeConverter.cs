using System;

namespace NureTimetable.Core.Extensions
{
    /// <summary>
    /// Universal type converter. Supports conversion to Nullable<T> types.
    /// </summary>
    public static class TypeConverter
    {
        public static T Get<T>(object value, bool DefaultIfNull = false)
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
