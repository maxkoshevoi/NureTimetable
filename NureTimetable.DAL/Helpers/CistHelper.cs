using NureTimetable.Core.Models.Exceptions;
using System;

namespace NureTimetable.DAL.Helpers
{
    public static class CistHelper
    {
        public static T FromJson<T>(string json)
        {
            try
            {
                T result = Serialisation.FromJson<T>(json);
                return result;
            }
            catch (ArgumentException ex) when (json.Contains("ORA-04030"))
            {
                throw new CistOutOfMemoryException("Cist returned out of memory exception", ex);
            }
        }
    }
}
