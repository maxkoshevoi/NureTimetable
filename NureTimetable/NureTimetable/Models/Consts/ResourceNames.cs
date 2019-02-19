using System.Linq;

namespace NureTimetable.Models.Consts
{
    class ResourceNames
    {
        public static string EventColor(string type)
        {
            var comparableType = type.ToLower();

            return KnownEventTypes.Values.Contains(comparableType)
                ? $"{comparableType}Color"
                : "defaultColor";
        }
    }
}
