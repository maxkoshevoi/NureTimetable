using NureTimetable.Core.Extensions;
using System;
using System.Linq;
using System.Text;

namespace NureTimetable.DAL.Helpers
{
    public static class JsonFixers
    {
        private const string stringStart = "\":\"";
        private static readonly string[] stringEnd =
        {
            "\",",
            "\"}",
            "\"]",
            "\",\""
        };
        private static readonly string[] nonEssentialCharacters =
        {
            "\r",
            "\n",
            " ",
            "\t"
        };
        private static readonly string[] noValue =
        {
            ":,",
            ":]",
            ":}"
        };

        /// <summary>
        /// 1. Escapes double quotes in json property values
        /// 2. Replaces ":," with ":null,"
        /// </summary>
        public static string TryFix(string invalidJsonStr)
        {
            const int notFound = -1;
            StringBuilder invalidJson = new(invalidJsonStr);

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
            newJson = nonEssentialCharacters.Aggregate(newJson, (res, ch) => res.Replace(ch, string.Empty));

            // Add null values instead of empty ones
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
    }
}
