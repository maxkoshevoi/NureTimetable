using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NureTimetable.Core.Extensions
{
    public static class IEnumerableExtensions
    {
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

        public static IEnumerable<string> GroupBasedOnLastPart(this IEnumerable<string> collection, string sepparator = "-")
        {
            List<string[]> nameParts = collection
                .OrderBy(n => n)
                .Select(n => n.Split(sepparator))
                .ToList();

            if (nameParts.Count == 0)
            {
                yield break;
            }

            List<string> currentGrouping = new();
            for (int i = 0; i < nameParts.Count; i++)
            {
                if (currentGrouping.Any() && !AllPartsEqualExceptLast(nameParts[i - 1], nameParts[i]))
                {
                    yield return ProcessGrouping(nameParts[i - 1]);
                    currentGrouping.Clear();
                }

                currentGrouping.Add(nameParts[i].Last());
            }

            if (currentGrouping.Any())
            {
                yield return ProcessGrouping(nameParts.Last());
            }

            static bool AllPartsEqualExceptLast(string[] first, string[] second) =>
                first.Length == second.Length &&
                first.SkipLast(1).SequenceEqual(second.SkipLast(1));

            string ProcessGrouping(string[] lastGroup)
            {
                if (currentGrouping.Count == 1)
                {
                    return string.Join(sepparator, lastGroup);
                }

                if (currentGrouping.SelectMany(g => g).All(char.IsDigit))
                {
                    currentGrouping = currentGrouping.OrderBy(g => int.Parse(g)).ToList();
                }
                return $"{string.Join(sepparator, lastGroup.SkipLast(1))}{sepparator}({string.Join(',', currentGrouping)})";
            }
        }
    }
}
