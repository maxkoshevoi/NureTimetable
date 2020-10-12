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
        }
    }
}
