using System;

namespace NureTimetable.Core.Models.Exceptions
{
    public class MoodleException : Exception
    {
        public MoodleException(string message, string errorCode) : base(message)
        {
            ErrorCode = errorCode;
        }

        public string ErrorCode { get; }
        
        public static class ErrorCodes
        {
            public const string InvalidLogin = "invalidlogin";

            public const string InvalidToken = "invalidtoken";
        }
    }
}
