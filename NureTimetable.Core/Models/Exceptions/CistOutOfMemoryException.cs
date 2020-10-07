using System;

namespace NureTimetable.Core.Models.Exceptions
{
    public class CistOutOfMemoryException : Exception
    {
        public CistOutOfMemoryException() : base()
        {
        }

        public CistOutOfMemoryException(string message) : base(message)
        {
        }

        public CistOutOfMemoryException(string message, Exception innerException) : base(message, innerException)
        {
            foreach (object key in innerException.Data.Keys)
            {
                innerException.Data.Add(key, innerException.Data[key]);
            }
        }
    }
}
