using System;
using System.Linq;
using System.Text;

namespace NureTimetable.Models.Consts
{
    public static class ResourceManager
    {
        public static string KeyForEventColor(Event e)
        {
            string compType = e.Type.ToLower();

            return KnownEventTypes.Values.Contains(compType)
                ? $"{compType}Color"
                : "defaultColor";
        }
    }
}
