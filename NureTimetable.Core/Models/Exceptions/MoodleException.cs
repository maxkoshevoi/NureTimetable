namespace NureTimetable.Core.Models.Exceptions;

public static class MoodleErrorCodes
{
    public const string InvalidLogin = "invalidlogin";

    public const string InvalidToken = "invalidtoken";

    public const string SiteMaintenance = "sitemaintenance";
}

public class MoodleException(string message, string errorCode) : Exception(message)
{
    public string ErrorCode { get; } = errorCode;
}
