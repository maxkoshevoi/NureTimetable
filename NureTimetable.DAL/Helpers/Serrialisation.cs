using Microsoft.AppCenter.Crashes;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using NureTimetable.Core.Extensions;
using NureTimetable.Core.Models.Consts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace NureTimetable.DAL.Helpers
{
    public static class Serialisation
    {
        public static void ToJsonFile<T>(T instance, string filePath)
        {
            try
            {
                string json = ToJson(instance);
                File.WriteAllText(filePath, json);
            }
            catch (Exception ex)
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    MessagingCenter.Send(Application.Current, MessageTypes.ExceptionOccurred, ex);
                });
            }
        }

        public static T FromJsonFile<T>(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    string fileContent = File.ReadAllText(filePath);
                    T instance = FromJson<T>(fileContent);
                    return instance;
                }
            }
            catch (Exception ex)
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    MessagingCenter.Send(Application.Current, MessageTypes.ExceptionOccurred, ex);
                });
            }
            return default;
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
                if (typeof(T).IsPrimitive || typeof(T) == typeof(string) || typeof(T) == typeof(DateTime))
                {
                    json = json?.Trim('\"');
                    return (T)Convert.ChangeType(json, typeof(T));
                }

                if (IsJson(json))
                {
                    throw new ArgumentException($"Argument is not recognized as a valid json string");
                }

                T instance;
                try
                {
                    instance = JsonConvert.DeserializeObject<T>(json);
                }
                catch (JsonReaderException)
                {
                    json = TryToFixJson(json);
                    instance = JsonConvert.DeserializeObject<T>(json);
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
            json = json.TrimStart(' ', '\t', '\r', '\n');
            return !json.StartsWith("{") && !json.StartsWith("[");
        }

        #region Converters
#pragma warning disable CA1812
        internal class SecondEpochConverter : DateTimeConverterBase
        {
            private static readonly DateTime _epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                if (reader.TokenType == JsonToken.Null)
                {
                    return null;
                }
                return _epoch.AddSeconds((long)reader.Value);
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                writer.WriteRawValue(((DateTime)value - _epoch).TotalSeconds.ToString());
            }
        }

        internal class StringBoolConverter: JsonConverter
        {
            private readonly Dictionary<string, bool> replacementValues = new Dictionary<string, bool> { { "1", true }, { "0", false } };

            public override bool CanConvert(Type t) => t == typeof(bool?) || t == typeof(bool);

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                string key = (string)reader.Value;
                if (key == null || !replacementValues.ContainsKey(key))
                {
                    return null;
                }
                return replacementValues[key];
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                string newValue = replacementValues.FirstOrDefault(x => x.Value.Equals(value)).Key;
                serializer.Serialize(writer, newValue);
            }
        }
#pragma warning restore CA1812
        #endregion

        #region JsonFixers
        /// <summary>
        /// 1. Escapes double quotes in json property values
        /// 2. Replaces ":," with ":null,"
        /// </summary>
        public static string TryToFixJson(string invalidJsonStr)
        {
            var invalidJson = new StringBuilder(invalidJsonStr);

            string[] stringStart =
            {
                "\":\"",
                "\": \""
            };
            string[] stringEnd =
            {
                "\",",
                "\"}",
                "\"]",
                "\",\""
            };

            foreach (string start in stringStart)
            {
                int lastStartIndex = -1,
                    startIndex = invalidJson.IndexOf(start);

                // Fix non-string Json
                if (startIndex > -1)
                {
                    string newJson = invalidJson.ToString(0, startIndex);
                    newJson = FixNonStringJson(newJson);
                    ReplaceStringPart(invalidJson, 0, startIndex, newJson);
                }

                while (startIndex != -1)
                {
                    int endIndex = stringEnd
                        .Select(end => invalidJson.IndexOf(end, startIndex + start.Length + 1))
                        .Where(index => index != -1)
                        .DefaultIfEmpty(-1)
                        .Min();
                    if (endIndex == -1)
                    {
                        break;
                    }

                    // Fix string
                    int innerStringStart = startIndex + start.Length, innerStringLength = endIndex - innerStringStart;
                    string newString = invalidJson.ToString(innerStringStart, innerStringLength);
                    if (newString.IndexOf('\"') != -1)
                    {
                        newString = FixJsonString(newString);
                        ReplaceStringPart(invalidJson, innerStringStart, innerStringLength, newString);
                    }

                    lastStartIndex = startIndex;
                    startIndex = invalidJson.IndexOf(start, lastStartIndex + 1);

                    // Fix non-string Json
                    if (startIndex > -1)
                    {
                        int nonStringLength = startIndex - endIndex;
                        string newJson = invalidJson.ToString(endIndex, nonStringLength);
                        newJson = FixNonStringJson(newJson);
                        ReplaceStringPart(invalidJson, endIndex, nonStringLength, newJson);

                        startIndex = invalidJson.IndexOf(start, lastStartIndex + 1);
                    }
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
            newJson = nonEssentialCharacters.Aggregate(newJson, (res, ch) => res.Replace(ch, ""));

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
            newString = newString.Replace("\"", "\\\"");
            return newString;
        }
        #endregion
    }
}
