using System.Net;

namespace NureTimetable.Core.Extensions
{
    public static class HttpClientExtensions
    {
        public static async Task<string> GetStringOrWebExceptionAsync(this HttpClient httpClient, Uri requestUri)
        {
            _ = httpClient ?? throw new ArgumentNullException(nameof(httpClient));

            try
            {
                return await httpClient.GetStringAsync(requestUri);
            }
            catch (Exception ex)
            {
                throw new WebException(ex.Message, ex);
            }
        }
    }
}
