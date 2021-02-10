using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NureTimetable.Core.Extensions
{
    public static class LinqEx
    {
        /// <summary>
        /// Returns all distinct elements of the given source, where "distinctness"
        /// is determined via a projection and the default equality comparer for the projected type.
        /// </summary>
        /// <remarks>
        /// This operator uses deferred execution and streams the results, although
        /// a set of already-seen keys is retained. If a key is seen multiple times,
        /// only the first element with that key is returned.
        /// </remarks>
        /// <typeparam name="TSource">Type of the source sequence</typeparam>
        /// <typeparam name="TKey">Type of the projected element</typeparam>
        /// <param name="source">Source sequence</param>
        /// <param name="keySelector">Projection for determining "distinctness"</param>
        /// <returns>A sequence consisting of distinct elements from the source sequence,
        /// comparing them by the specified key projection.</returns>
        public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            HashSet<TKey> seenKeys = new();
            foreach (var element in source)
            {
                if (seenKeys.Add(keySelector(element)))
                {
                    yield return element;
                }
            }
        }

        /// <summary>
        /// Generates a hash code based on elements of the sequence
        /// </summary>
        public static int GetTrueHashCode<T>(this IEnumerable<T> source)
        {
            int hash = 0;
            foreach (var item in source)
            {
                hash ^= item.GetHashCode();
            }
            return hash;
        }

        public static async IAsyncEnumerable<T> Where<T>(this IEnumerable<T> source, Func<T, Task<bool>> predicate)
        {
            foreach (var item in source)
            {
                if (await predicate(item))
                {
                    yield return item;
                }
            }
        }
    }
}
