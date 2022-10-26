using System.Net;

namespace NureTimetable.Core.Extensions;

public static class UriExtensions
{
    private static readonly Lazy<HttpClient> httpClient = new();

    public static async Task<string> GetStringOrWebExceptionAsync(this Uri requestUri)
    {
        ArgumentNullException.ThrowIfNull(requestUri);

        try
        {
            return await httpClient.Value.GetStringAsync(requestUri);
        }
        catch (Exception ex)
        {
            throw new WebException(ex.Message, ex);
        }
    }
}
