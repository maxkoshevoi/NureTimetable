using Newtonsoft.Json.Linq;
using System.Reflection;

namespace NureTimetable.Core.Models.Consts
{
    public static class Keys
    {
        public static string SyncfusionLicenseKey => Configuration?.Value<string>(nameof(SyncfusionLicenseKey)) ?? throw new NullReferenceException(nameof(SyncfusionLicenseKey));
        public static string MicrosoftAppCenterKey => Configuration?.Value<string>(nameof(MicrosoftAppCenterKey)) ?? throw new NullReferenceException(nameof(MicrosoftAppCenterKey));
        public static string CistApiKey => Configuration?.Value<string>(nameof(CistApiKey)) ?? throw new NullReferenceException(nameof(CistApiKey));

        private static JObject? Configuration { get; } = GetConfiguration("secrets.json");

        private static JObject? GetConfiguration(string filePath)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            string? assemblyName = assembly.GetName().Name;
            using Stream? fileStream = assembly.GetManifestResourceStream($"{assemblyName}.{filePath}");
            if (fileStream == null)
            {
                return null;
            }

            using StreamReader sr = new(fileStream);
            string fileContent = sr.ReadToEnd();
            return JObject.Parse(fileContent);
        }
    }
}
