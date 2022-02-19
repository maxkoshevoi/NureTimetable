using System.Collections;

namespace NureTimetable.Core.Extensions;

public static class EnumerableExtensions
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

    /// <summary>
    /// Determines whether a sequence is empty.
    /// </summary>
    public static bool None<T>(this IEnumerable<T> source) => !source.Any();

    /// <summary>
    /// Determines whether no element of a sequence satisfies a condition.
    /// </summary>
    public static bool None<T>(this IEnumerable<T> source, Func<T, bool> predicate) => !source.Any(predicate);

    public static bool TryAdd(this IDictionary dictionary, object key, object? value)
    {
        if (dictionary.Contains(key))
        {
            return false;
        }

        dictionary.Add(key, value);
        return true;
    }

    public static IEnumerable<string> GroupBasedOnLastPart(this IEnumerable<string> collection, string sepparator = "-")
    {
        List<string[]> nameParts = collection
            .OrderBy(n => n)
            .Select(n => n.Split(sepparator))
            .ToList();

        if (nameParts.None())
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
