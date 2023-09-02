using Microsoft.AppCenter.Crashes;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Nito.AsyncEx;
using NureTimetable.Core.BL;
using NureTimetable.Core.Extensions;
using NureTimetable.DAL.Cist;

namespace NureTimetable.DAL;

public static class Serialization
{
    private static readonly AsyncReaderWriterLock fileLock = new();

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

    /// <returns>default(T) if file doesn't exist</returns>
    public static async Task<T?> FromJsonFile<T>(string filePath)
    {
        if (!File.Exists(filePath))
            return default;

        try
        {
            string fileContent;
            using (var readLock = await fileLock.ReaderLockAsync())
            {
                // ReadAllTextAsync may not read all data from the file 
                fileContent = File.ReadAllText(filePath);
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
                string badJson = json;
                json = JsonFixers.TryFix(json);
                instance = JsonConvert.DeserializeObject<T>(json);

                InvalidOperationException ex = new("Fixed invalid Json");
                EnrichException(badJson, ex);
                ExceptionService.LogException(ex);
            }

            // If T is nullable, instance can be null
            return instance!;
        }
        catch (Exception ex)
        {
            EnrichException(json, ex);
            throw;
        }

        static void EnrichException(string json, Exception ex)
        {
            ex.Data.TryAdd("Type", typeof(T).FullName);
            ex.Data.TryAdd("Json", ErrorAttachmentLog.AttachmentWithText(json, "Json.json"));
        }
    }

    private static bool IsJson(string json)
    {
        json = json.Trim(' ', '\t', '\r', '\n');
        return json == "null"
            || ((json.StartsWith("{") || json.StartsWith("["))
               && (json.EndsWith("}") || json.EndsWith("]")));
    }

    #region Converters
    internal class UnixDateTimeConverter : DateTimeConverterBase
    {
        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                return null;
            }
            return DateTime.UnixEpoch.AddSeconds((long)reader.Value!);
        }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }

    internal class StringBoolConverter : JsonConverter
    {
        private readonly Dictionary<string, bool> replacementValues = new() { { "1", true }, { "0", false } };

        public override bool CanConvert(Type t) => t == typeof(bool?) || t == typeof(bool);

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            string? key = (string?)reader.Value;
            if (key == null || !replacementValues.ContainsKey(key))
            {
                return null;
            }
            return replacementValues[key];
        }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            string? newValue = replacementValues.FirstOrDefault(x => x.Value.Equals(value)).Key;
            serializer.Serialize(writer, newValue);
        }
    }

    internal class SecondTimeSpanConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType) => objectType == typeof(TimeSpan) || objectType == typeof(TimeSpan?);

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                return null;
            }
            return TimeSpan.FromSeconds((long)reader.Value!);
        }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            writer.WriteRawValue(((TimeSpan)value!).TotalSeconds.ToString());
        }
    }
    #endregion
}
