using System.Net;

namespace NureTimetable.Core.Extensions;

public static class ExceptionExtensions
{
    private static readonly WebExceptionStatus[] noInternetStatuses =
    {
        WebExceptionStatus.NameResolutionFailure,
        WebExceptionStatus.ConnectFailure
    };

    public static bool IsNoInternet(this WebException ex)
    {
        ArgumentNullException.ThrowIfNull(ex);

        return noInternetStatuses.Contains(ex.Status);
    }
}
