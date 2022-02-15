﻿namespace NureTimetable.Core.Models.Exceptions
{
    public static class MoodleErrorCodes
    {
        public const string InvalidLogin = "invalidlogin";

        public const string InvalidToken = "invalidtoken";

        public const string SiteMaintenance = "sitemaintenance";
    }

    public class MoodleException : Exception
    {
        public MoodleException(string message, string errorCode) : base(message)
        {
            ErrorCode = errorCode;
        }

        public string ErrorCode { get; }
    }
}