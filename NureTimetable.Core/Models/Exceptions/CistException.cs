using System;
using System.Collections;

namespace NureTimetable.Core.Models.Exceptions
{
    public enum CistExceptionStatus
    {
        UnknownError = 0,
        OutOfMemory = 04030,
        ShutdownInProgress = 01089,
        UnableToExtendTempSegment = 01652
    }

    public class CistException : Exception
    {
        public CistExceptionStatus Status { get; }

        public CistException()
        {
        }

        public CistException(string message) : base(message)
        {
        }

        public CistException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public CistException(string message, CistExceptionStatus status) : base(message)
        {
            Status = status;
        }

        public CistException(string message, CistExceptionStatus status, Exception innerException) : base(message, innerException)
        {
            Status = status;
            foreach (DictionaryEntry de in innerException.Data)
            {
                Data.Add(de.Key, de.Value);
            }
        }
    }
}
