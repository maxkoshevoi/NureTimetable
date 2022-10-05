namespace NureTimetable.Core.Extensions;

public static class EnumExtensions
{
    public static T AddFlag<T>(this Enum value, T flag)
    {
        if (!value.GetType().IsEquivalentTo(typeof(T)))
        {
            throw new ArgumentException("Enum value and flags types don't match.");
        }

        return (T)Enum.ToObject(typeof(T), Convert.ToUInt64(value) | Convert.ToUInt64(flag));
    }

    public static T RemoveFlag<T>(this Enum value, T flag)
    {
        if (!value.GetType().IsEquivalentTo(typeof(T)))
        {
            throw new ArgumentException("Enum value and flags types don't match.");
        }

        return (T)Enum.ToObject(typeof(T), Convert.ToUInt64(value) & ~Convert.ToUInt64(flag));
    }

    public static int FlagCount(this Enum value)
    {
        string binary = Convert.ToString(int.Parse(value.ToString("D")), 2);
        return binary.Count(x => x == '1');
    }
}
