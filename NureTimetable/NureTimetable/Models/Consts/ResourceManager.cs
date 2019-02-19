using System.Linq;

namespace NureTimetable.Models.Consts
{
    public static class ResourceManager
    {
        public static string KeyForEventColor(string type)
        {
            string comparableType = type.ToLower();

            return KnownEventTypes.Values.Contains(comparableType)
                ? $"{comparableType}Color"
                : "defaultColor";
        }
    }
}
