using System.Net;

namespace NureTimetable.Core.Extensions;

public static class UriExtensions
{
    private static readonly Lazy<HttpClient> httpClient = new();

    public static async Task<string> GetStringOrWebExceptionAsync(this Uri requestUri)
    {
        _ = requestUri ?? throw new ArgumentNullException(nameof(requestUri));

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
