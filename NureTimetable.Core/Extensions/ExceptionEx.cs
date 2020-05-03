using System;
using System.Linq;
using System.Net;

namespace NureTimetable.Core.Extensions
{
    public static class ExceptionEx
    {
        private static WebExceptionStatus[] noInternetStatuses =
        {
            WebExceptionStatus.NameResolutionFailure, 
            WebExceptionStatus.ConnectFailure
        };

        public static bool IsNoInternet(this Exception ex)
        {
            _ = ex ?? throw new ArgumentNullException(nameof(ex));

            return ex is WebException webEx && noInternetStatuses.Contains(webEx.Status);
        }
    }
}
