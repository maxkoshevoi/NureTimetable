using System.Collections.Generic;

namespace NureTimetable.Core.Extensions
{
    public static class ListEx
    {
        public static int GetTrueHashCode<T>(this List<T> list)
        {
            int hash = 0;
            foreach (T item in list)
            {
                hash ^= item.GetHashCode();
            }
            return hash;
        }
    }
}
