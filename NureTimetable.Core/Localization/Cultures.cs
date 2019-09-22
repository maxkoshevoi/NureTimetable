using System.Globalization;
using System.Resources;

[assembly:NeutralResourcesLanguage("en")]
namespace NureTimetable.Core.Localization
{
    public static class Cultures
    {
        public static readonly CultureInfo[] SupportedCultures =
        {
            new CultureInfo("en"),
            new CultureInfo("ru"),
        };
    }
}
