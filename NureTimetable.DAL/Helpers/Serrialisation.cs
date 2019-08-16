using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using NureTimetable.Core.Extensions;
using NureTimetable.Core.Models.Consts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
                Device.BeginInvokeOnMainThread(() =>
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
                Device.BeginInvokeOnMainThread(() =>
                {
                    MessagingCenter.Send(Application.Current, MessageTypes.ExceptionOccurred, ex);
                });
            }
            return default(T);
        }

        public static string ToJson<T>(T instance)
        {
            string json = JsonConvert.SerializeObject(instance);
            return json;
        }
        
        public static T FromJson<T>(string json)
        {
            if (typeof(T).IsPrimitive || typeof(T) == typeof(string))
            {
                return (T)Convert.ChangeType(json, typeof(T));
            }

            T instance;
            try
            {
                instance = JsonConvert.DeserializeObject<T>(json);
            }
            catch (JsonReaderException)
            {
                json = EscapeDoubleQuotesInJsonPropertyValues(json);
                instance = JsonConvert.DeserializeObject<T>(json);
            }
            return instance;
        }

        #region Converters
        public class SecondEpochConverter : DateTimeConverterBase
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

        public class StringBoolConverter: JsonConverter
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
        #endregion

        #region JsonFixers
        public static string EscapeDoubleQuotesInJsonPropertyValues(string invalidJsonStr)
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

                    int innerStringStart = startIndex + start.Length, innerStringLength = endIndex - innerStringStart;
                    string newString = invalidJson.ToString(innerStringStart, innerStringLength);
                    if (newString.IndexOf('\"') != -1)
                    {
                        invalidJson.Remove(innerStringStart, innerStringLength);
                        newString = newString.Replace("\"", "\\\"");
                        invalidJson.Insert(innerStringStart, newString);
                    }

                    lastStartIndex = startIndex;
                    startIndex = invalidJson.IndexOf(start, lastStartIndex + 1);
                }
            }

            return invalidJson.ToString();
        }
        #endregion
    }
}
