using Microsoft.AppCenter.Crashes;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Nito.AsyncEx;
using NureTimetable.Core.BL;
using NureTimetable.Core.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace NureTimetable.DAL.Helpers
{
    public static class Serialisation
    {
        private static AsyncReaderWriterLock fileLock = new();

        public static async Task ToJsonFile<T>(T instance, string filePath)
        {
            try
            {
                string json = ToJson(instance);
                using (var writeLock = await fileLock.WriterLockAsync())
                {
                    // WriteAllTextAsync may not write all data to the file 
                    File.WriteAllText(filePath, json);
                }
            }
            catch (Exception ex)
            {
                ExceptionService.LogException(ex);
            }
        }

        public static async Task<T?> FromJsonFile<T>(string filePath)
        {
            if (!File.Exists(filePath))
                return default;

            try
            {
                string fileContent;
                using (var readLock = await fileLock.ReaderLockAsync())
                {
                    fileContent = await File.ReadAllTextAsync(filePath);
                }
                T instance = FromJson<T>(fileContent);
                return instance;
            }
            catch (Exception ex)
            {
                ex.Data.Add("FilePath", filePath);
                ExceptionService.LogException(ex);

                File.Delete(filePath);
                return default;
            }
        }

        public static string ToJson<T>(T instance)
        {
            string json = JsonConvert.SerializeObject(instance);
            return json;
        }

        public static T FromJson<T>(string json)
        {
            try
            {
                if (!IsJson(json))
                {
                    throw new ArgumentException($"Argument is not a valid json string");
                }

                T? instance;
                try
                {
                    instance = JsonConvert.DeserializeObject<T>(json);
                }
                catch (JsonReaderException)
                {
                    json = TryToFixJson(json);
                    instance = JsonConvert.DeserializeObject<T>(json);
                }

                if (instance == null)
                {
                    throw new InvalidOperationException("Deserializer returned null");
                }
                return instance;
            }
            catch (Exception ex)
            {
                ex.Data.Add("Type", typeof(T).FullName);
                ex.Data.Add("Json", ErrorAttachmentLog.AttachmentWithText(json, "Json.json"));
                throw;
            }
        }

        private static bool IsJson(string json)
        {
            json = json.Trim(' ', '\t', '\r', '\n');
            return (json.StartsWith("{") || json.StartsWith("["))
                && (json.EndsWith("}") || json.EndsWith("]"));
        }

        #region Converters
        internal class SecondEpochConverter : DateTimeConverterBase
        {
            private static readonly DateTime _epoch = new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
            {
                if (reader.TokenType == JsonToken.Null)
                {
                    return null;
                }
                return _epoch.AddSeconds((long)reader.Value!);
            }

            public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
            {
                writer.WriteRawValue(((DateTime)value! - _epoch).TotalSeconds.ToString());
            }
        }

        internal class StringBoolConverter: JsonConverter
        {
            private readonly Dictionary<string, bool> replacementValues = new() { { "1", true }, { "0", false } };

            public override bool CanConvert(Type t) => t == typeof(bool?) || t == typeof(bool);

            public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
            {
                string key = (string)reader.Value!;
                if (!replacementValues.ContainsKey(key))
                {
                    return null;
                }
                return replacementValues[key];
            }

            public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
            {
                string newValue = replacementValues.FirstOrDefault(x => x.Value.Equals(value)).Key;
                serializer.Serialize(writer, newValue);
            }
        }
        #endregion

        #region JsonFixers
        /// <summary>
        /// 1. Escapes double quotes in json property values
        /// 2. Replaces ":," with ":null,"
        /// </summary>
        public static string TryToFixJson(string invalidJsonStr)
        {
            const int notFound = -1;
            StringBuilder invalidJson = new(invalidJsonStr);

            const string stringStart = "\":\"";
            string[] stringEnd =
            {
                "\",",
                "\"}",
                "\"]",
                "\",\""
            };

            // Unify string start
            invalidJson = invalidJson.Replace("\": \"", stringStart);

            int lastStartIndex = notFound,
                startIndex = invalidJson.IndexOf(stringStart);

            // Fix non-string Json
            if (startIndex != notFound)
            {
                string newJson = invalidJson.ToString(0, startIndex);
                newJson = FixNonStringJson(newJson);
                ReplaceStringPart(invalidJson, 0, startIndex, newJson);
            }

            while (startIndex != notFound)
            {
                int endIndex = stringEnd
                    .Select(end => invalidJson.IndexOf(end, startIndex + stringStart.Length + 1))
                    .Where(index => index != notFound)
                    .DefaultIfEmpty(notFound)
                    .Min();
                if (endIndex == notFound)
                {
                    break;
                }

                // Fix string
                int innerStringStart = startIndex + stringStart.Length, innerStringLength = endIndex - innerStringStart;
                string newString = invalidJson.ToString(innerStringStart, innerStringLength);
                if (newString.IndexOf('\"') != notFound)
                {
                    newString = FixJsonString(newString);
                    ReplaceStringPart(invalidJson, innerStringStart, innerStringLength, newString);
                }

                lastStartIndex = startIndex;
                startIndex = invalidJson.IndexOf(stringStart, lastStartIndex + 1);

                // Fix non-string Json
                if (startIndex != notFound)
                {
                    int nonStringLength = startIndex - endIndex;
                    string newJson = invalidJson.ToString(endIndex, nonStringLength);
                    newJson = FixNonStringJson(newJson);
                    ReplaceStringPart(invalidJson, endIndex, nonStringLength, newJson);

                    startIndex = invalidJson.IndexOf(stringStart, lastStartIndex + 1);
                }
            }

            return invalidJson.ToString();
        }

        private static void ReplaceStringPart(StringBuilder stringToModify, int partStart, int partLength, string newString)
        {
            stringToModify.Remove(partStart, partLength);
            stringToModify.Insert(partStart, newString);
        }

        private static string FixNonStringJson(string newJson)
        {
            // Remove non-essential data
            string[] nonEssentialCharacters =
            {
                "\r",
                "\n",
                " ",
                "\t"
            };
            newJson = nonEssentialCharacters.Aggregate(newJson, (res, ch) => res.Replace(ch, string.Empty));

            // Add null values instead of empty ones
            string[] noValue = 
            {
                ":,",
                ":]",
                ":}"
            };
            newJson = noValue.Aggregate(newJson, (res, nv) => res.Replace(nv, nv.Insert(1, "null")));

            return newJson;
        }

        private static string FixJsonString(string newString)
        {
            newString = newString
                .Replace("\\\"", "\"")
                .Replace("\"", "\\\"");
            return newString;
        }
        #endregion
    }
}
