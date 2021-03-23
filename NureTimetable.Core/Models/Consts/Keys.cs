using Newtonsoft.Json.Linq;
using System.IO;
using System.Reflection;

namespace NureTimetable.Core.Models.Consts
{
    public static class Keys
    {
        public static string SyncfusionLicenseKey => Configuration?.Value<string>(nameof(SyncfusionLicenseKey));
        public static string MicrosoftAppCenterKey => Configuration?.Value<string>(nameof(MicrosoftAppCenterKey));
        public static string MicrosoftAppCenterDebugKey { get; } = "android=b7dd5224-0d79-47e9-a367-8ecd6a36740d;"; // random guid so that logs are not sent to AppCenter
        public static string CistApiKey => Configuration?.Value<string>(nameof(CistApiKey));

        private static JObject Configuration { get; } = Configuration = GetConfiguration("secrets.json");

        private static JObject GetConfiguration(string filePath)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            string assemblyName = assembly.GetName().Name;
            using Stream fileStream = assembly.GetManifestResourceStream($"{assemblyName}.{filePath}");
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
