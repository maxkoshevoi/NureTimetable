using Flurl;
using Flurl.Http;
using NureTimetable.DAL.Models.Consts;
using NureTimetable.DAL.Models.Moodle;
using NureTimetable.DAL.Models.Moodle.Auth;
using NureTimetable.DAL.Models.Moodle.Calendar;
using NureTimetable.DAL.Models.Moodle.Courses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NureTimetable.DAL.Repositories;

public class MoodleRepository
{
    private readonly Uri baseUrl;
    private Uri baseWebServiceUrl;

    public MoodleUser? User = null;

    public MoodleRepository()
    {
        baseUrl = Urls.DlNure.SetQueryParam("moodlewsrestformat", "json").ToUri();
        baseWebServiceUrl = baseUrl.AppendPathSegment("webservice/rest/server.php").ToUri();
    }

    public async Task AuthenticateAsync(string username, string password, ServiceType? service = null)
    {
        var response = await baseUrl
            .AppendPathSegment("login/token.php")
            .SetQueryParams(new
            {
                username,
                password,
                service = service?.ToString()
            })
            .GetJsonAsync<TokenResponse>();

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

    /// <summary>
    /// Return some site info / user info / list web service functions.
    /// </summary>
    public async Task<SiteInfo> GetSiteInfoAsync()
    {
        var response = await SetFunction("core_webservice_get_site_info").GetJsonAsync<SiteInfo>();
        return response;
    }

    /// <summary>
    /// Fetch the upcoming view data for a calendar.
    /// </summary>
    public async Task<List<Event>> GetUpcommingEvents(int? courseId = null)
    {
        var response = await SetFunction("core_calendar_get_calendar_upcoming_view")
            .SetQueryParams(new { courseId })
            .GetJsonAsync<GetUpCommingEventsResponse>();
        return response.Events;
    }

    /// <summary>
    /// Get the list of courses where a user is enrolled in.
    /// </summary>
    /// <param name="userId">null = current user.</param>
    /// <param name="returnUserCount"> Include count of enrolled users for each course? This can add several seconds to the response time if a user is on several large courses, so set this to false if the value will not be used to improve performance.</param>
    public async Task<List<FullCourse>> GetEnrolledCourses(int? userId = null, bool returnUserCount = false)
    {
        var response = await SetFunction("core_enrol_get_users_courses")
            .SetQueryParams(new 
            { 
                userid = userId ?? User!.Id,
                returnusercount = returnUserCount ? 1 : 0
            })
            .GetJsonAsync<List<FullCourse>>();
        return response;
    }

    public async Task<List<CourseSection>> GetCourseContents(int courseId, Dictionary<GetCourseContentsOption, object>? Options = null)
    {
        Options ??= new();

        var arguments = Options
            .SelectMany((item, index) => new KeyValuePair<string, object>[]
            {
                new($"options[{index}][name]", item.Key.ToString().ToLowerInvariant()),
                new($"options[{index}][value]", item.Value.ToString())
            })
            .ToList();
        arguments.Add(new("courseid", courseId));

        var response = await SetFunction("core_course_get_contents")
            .SetQueryParams(arguments)
            .GetJsonAsync<List<CourseSection>>();
        return response;
    }

    private Url SetFunction(string name) => baseWebServiceUrl.SetQueryParam("wsfunction", name);
}
