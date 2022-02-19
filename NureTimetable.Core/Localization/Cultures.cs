using System.Resources;

[assembly: NeutralResourcesLanguage("en")]
namespace NureTimetable.Core.Localization;

public static class Cultures
{
    public static readonly CultureInfo[] SupportedCultures =
    {
            new("en"), new("ru"), new("uk"),
        };
}
