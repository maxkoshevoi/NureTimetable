using Flurl;
using Flurl.Http;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Newtonsoft.Json;
using NureTimetable.Core.Extensions;
using NureTimetable.Core.Models.Exceptions;
using NureTimetable.DAL.Moodle.Consts;
using NureTimetable.DAL.Moodle.Models;
using NureTimetable.DAL.Moodle.Models.Auth;
using NureTimetable.DAL.Moodle.Models.Calendar;
using NureTimetable.DAL.Moodle.Models.Courses;
using NureTimetable.DAL.Moodle.Models.WebService;
using NureTimetable.DAL.Settings;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace NureTimetable.DAL.Moodle;

public class MoodleRepository
{
    private const string wstoken = nameof(wstoken);

    private readonly Uri baseUrl;
    private Uri baseWebServiceUrl;

    private MoodleUser? _user;
    public MoodleUser? User
    {
        get => _user;
        set
        {
            _user = value;
            baseWebServiceUrl = baseWebServiceUrl.SetQueryParam(wstoken, User?.Token).ToUri();
        }
    }

    public MoodleRepository()
    {
        baseUrl = Urls.DlNure.SetQueryParam("moodlewsrestformat", "json").ToUri();
        baseWebServiceUrl = baseUrl.AppendPathSegment("webservice/rest/server.php").ToUri();

        User = SettingsRepository.Settings.DlNureUser;
    }

    public Url GetAuthenticateUrl(string username, string password, ServiceType? service = ServiceType.moodle_mobile_app) => baseUrl
        .AppendPathSegment("login/token.php")
        .SetQueryParams(new
        {
            username,
            password,
            service = service?.ToString()
        });

#pragma warning disable CS8774 // Member must have a non-null value when exiting. It'll be set, trust me
    [MemberNotNull(nameof(User))]
    public async Task AuthenticateAsync(string username, string password, ServiceType? service = ServiceType.moodle_mobile_app)
    {
        var url = baseUrl
            .AppendPathSegment("login/token.php")
            .SetQueryParams(new
            {
                username,
                password,
                service = service?.ToString()
            });
        var response = await ExecuteActionAsync<TokenResponse>(url, true, false);
        await UpdateUserInfoAsync(response.Token);

        SettingsRepository.Settings.DlNureUser = User;

        async Task UpdateUserInfoAsync(string token)
        {
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

    [MemberNotNull(nameof(User))]
    public async Task UpdateTokenAsync()
    {
        if (User == null)
        {
            throw new InvalidOperationException($"{nameof(User)} cannot be null");
        }

        var url = GetAuthenticateUrl(User.Login, User.Password);
        var response = await ExecuteActionAsync<TokenResponse>(url, true, false);
        User = User with { Token = response.Token };

        SettingsRepository.Settings.DlNureUser = User;
    }

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
    public async Task<List<Event>> GetUpcommingEventsAsync(int? courseId = null)
    {
        var query = SetFunction("core_calendar_get_calendar_upcoming_view").SetQueryParams(new { courseId });
        return (await ExecuteActionAsync<GetUpCommingEventsResponse>(query)).Events;
    }

    /// <summary>
    /// Get the list of courses where a user is enrolled in.
    /// </summary>
    /// <param name="userId">null = current user.</param>
    /// <param name="returnUserCount">Include count of enrolled users for each course? This can add several seconds to the response time if a user is on several large courses, so set this to false if the value will not be used to improve performance.</param>
    public async Task<List<FullCourse>> GetEnrolledCoursesAsync(int? userId = null, bool returnUserCount = false)
    {
        var query = SetFunction("core_enrol_get_users_courses")
            .SetQueryParams(new
            {
                userid = userId ?? User!.Id,
                returnusercount = returnUserCount ? 1 : 0
            });
        return await ExecuteActionAsync<List<FullCourse>>(query);
    }

    public async Task<List<CourseSection>> GetCourseContentsAsync(int courseId, Dictionary<GetCourseContentsOption, object>? Options = null)
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
    private async Task<T> ExecuteActionAsync<T>(Url url, bool allowAnonymous = false, bool tryRelogin = true, [CallerMemberName] string method = "")
    {
        try
        {
            Analytics.TrackEvent("Moodle request", new Dictionary<string, string>
            {
                { "Type", method }
            });

            if (!allowAnonymous && !url.QueryParams.Contains(wstoken))
            {
                throw new InvalidOperationException($"Call {nameof(AuthenticateAsync)} or set {nameof(User)} before making this request.");
            }

            string result = await url.ToUri().GetStringOrWebExceptionAsync();

            try
            {
                var errorResult = Serialisation.FromJson<ErrorResult>(result);
                if (errorResult.ErrorCode != null && errorResult.ErrorMessage != null)
                {
                    bool shouldTryRelogin = tryRelogin && errorResult.ErrorCode == MoodleErrorCodes.InvalidToken;
                    if (shouldTryRelogin)
                    {
                        return await ReloginAndReexecuteAsync();
                    }

                    throw new MoodleException(errorResult.ErrorMessage.Replace("<br />", Environment.NewLine), errorResult.ErrorCode);
                }
            }
            catch (JsonSerializationException) { }

            return Serialisation.FromJson<T>(result);
        }
        catch (Exception ex)
        {
            ex.Data.TryAdd("Url", ErrorAttachmentLog.AttachmentWithText(url.ToString(), "Url.txt"));
            throw;
        }

        async Task<T> ReloginAndReexecuteAsync()
        {
            await UpdateTokenAsync();
            url.SetQueryParam(wstoken, User.Token);
            return await ExecuteActionAsync<T>(url, allowAnonymous, false, method);
        }
    }
}