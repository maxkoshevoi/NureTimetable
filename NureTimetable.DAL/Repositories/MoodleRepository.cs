using Flurl;
using Flurl.Http;
using NureTimetable.DAL.Models.Consts;
using NureTimetable.DAL.Models.Moodle;
using NureTimetable.DAL.Models.Moodle.Auth;
using System;
using System.Threading.Tasks;

namespace NureTimetable.DAL.Repositories;

public class MoodleRepository
{
    private readonly Uri baseUrl;
    private Uri baseWebServiceUrl;

    public MoodleUser? User = null;

    public MoodleRepository()
    {
        this.baseUrl = Urls.DlNure.SetQueryParam("moodlewsrestformat", "json").ToUri();
        baseWebServiceUrl = this.baseUrl.AppendPathSegment("webservice/rest/server.php").ToUri();
    }

    public async Task AuthenticateAsync(string username, string password, ServiceType? service = null)
    {
        var query = baseUrl
            .AppendPathSegment("login/token.php")
            .SetQueryParams(new
            {
                username,
                password,
                service = service?.ToString()
            });

        var response = await query.GetJsonAsync<TokenResponse>();
        await UpdateUserInfoAsync(response.Token);

        async Task UpdateUserInfoAsync(string token)
        {
            baseWebServiceUrl = baseWebServiceUrl.SetQueryParam("wstoken", token).ToUri();

            User = new MoodleUser(username, password, token);
            var siteInfo = await GetSiteInfoAsync();
            User = User with
            {
                Id = siteInfo.UserId,
                FullName = siteInfo.FullName,
            };
        }
    }

    public async Task<SiteInfo> GetSiteInfoAsync()
    {
        var url = baseWebServiceUrl.SetQueryParam("wsfunction", "core_webservice_get_site_info");
        var response = await url.GetJsonAsync<SiteInfo>();
        return response;
    }
}
