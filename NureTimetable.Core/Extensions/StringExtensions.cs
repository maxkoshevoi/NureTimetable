namespace NureTimetable.Core.Extensions;

public static class StringExtensions
{
    /// <summary>
    /// Unifies all characters that are used interchangeably.
    /// </summary>
    public static string Simplify(this string str) => str
        .ToLower()
        .Replace('і', 'и')
        .Replace('ї', 'и')
        .Replace('є', 'э')
        .Replace('`', '\'')
        .Replace('"', '\'');
}
