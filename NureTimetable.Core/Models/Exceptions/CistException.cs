﻿using System.Collections;

namespace NureTimetable.Core.Models.Exceptions;

public enum CistExceptionStatus
{
    UnknownError = 0,
    OutOfMemory = 04030,
    ShutdownInProgress = 01089,
    UnableToExtendTempSegment = 01652,
    ObjectNoLongerExists = 08103
}

public class CistException : Exception
{
    public CistExceptionStatus Status { get; }

    public CistException(string message, CistExceptionStatus status, Exception innerException) : base(message, innerException)
    {
        Status = status;
        foreach (DictionaryEntry de in innerException.Data)
        {
            Data.Add(de.Key, de.Value);
        }
    }
}
