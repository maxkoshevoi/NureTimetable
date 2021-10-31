using Flurl;
using Flurl.Http;
using NureTimetable.DAL.Moodle.Consts;
using NureTimetable.DAL.Moodle.Models;
using NureTimetable.DAL.Moodle.Models.Auth;
using NureTimetable.DAL.Moodle.Models.Calendar;
using NureTimetable.DAL.Moodle.Models.Courses;
using NureTimetable.DAL.Moodle.Models.WebService;
using NureTimetable.DAL.Settings;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace NureTimetable.DAL.Moodle;

public class MoodleRepository
{
    private readonly Uri baseUrl;
    private Uri baseWebServiceUrl;

    public MoodleUser? User { get; set; }

    public MoodleRepository()
    {
        baseUrl = Urls.DlNure.SetQueryParam("moodlewsrestformat", "json").ToUri();
        baseWebServiceUrl = baseUrl.AppendPathSegment("webservice/rest/server.php").ToUri();

        User = SettingsRepository.Settings.DlNureUser;
    }

    [MemberNotNull(nameof(User))]
#pragma warning disable CS8774 // Member must have a non-null value when exiting. It'll be set, trust me
    public async Task<MoodleUser> AuthenticateAsync(string username, string password, ServiceType? service = null)
    {
        var url = baseUrl
            .AppendPathSegment("login/token.php")
            .SetQueryParams(new
            {
                username,
                password,
                service = service?.ToString()
            });
        var response = await ExecuteActionAsync<TokenResponse>(url, true);

        await UpdateUserInfoAsync(response.Token);
        return User!;

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
#pragma warning restore CS8774 // Member must have a non-null value when exiting.

    /// <summary>
    /// Return some site info / user info / list web service functions.
    /// </summary>
    public async Task<SiteInfo> GetSiteInfoAsync()
    {
        var query = SetFunction("core_webservice_get_site_info");
        return await ExecuteActionAsync<SiteInfo>(query);
    }

    /// <summary>
    /// Fetch the upcoming view data for a calendar.
    /// </summary>
    public async Task<List<Event>> GetUpcommingEvents(int? courseId = null)
    {
        var query = SetFunction("core_calendar_get_calendar_upcoming_view").SetQueryParams(new { courseId });
        return (await ExecuteActionAsync<GetUpCommingEventsResponse>(query)).Events;
    }

    /// <summary>
    /// Get the list of courses where a user is enrolled in.
    /// </summary>
    /// <param name="userId">null = current user.</param>
    /// <param name="returnUserCount"> Include count of enrolled users for each course? This can add several seconds to the response time if a user is on several large courses, so set this to false if the value will not be used to improve performance.</param>
    public async Task<List<FullCourse>> GetEnrolledCourses(int? userId = null, bool returnUserCount = false)
    {
        var query = SetFunction("core_enrol_get_users_courses")
            .SetQueryParams(new
            {
                userid = userId ?? User!.Id,
                returnusercount = returnUserCount ? 1 : 0
            });
        return await ExecuteActionAsync<List<FullCourse>>(query);
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

        var query = SetFunction("core_course_get_contents").SetQueryParams(arguments);
        return await ExecuteActionAsync<List<CourseSection>>(query);
    }

    private Url SetFunction(string name) => baseWebServiceUrl.SetQueryParam("wsfunction", name);

    /// <exception cref="InvalidOperationException"></exception>
    private async Task<T> ExecuteActionAsync<T>(Url url, bool allowAnonymous = false)
    {
        if (!allowAnonymous && User == null)
        {
            throw new InvalidOperationException($"Call {nameof(AuthenticateAsync)} before making this request.");
        }

        string result = await url.GetStringAsync();

        var errorResult = Serialisation.FromJson<ErrorResult>(result);
        if (errorResult.Error != null && errorResult.ErrorCode != null)
        {
            throw new InvalidOperationException(errorResult.Error.Replace("<br />", Environment.NewLine));
        }

        return Serialisation.FromJson<T>(result);
    }
}
