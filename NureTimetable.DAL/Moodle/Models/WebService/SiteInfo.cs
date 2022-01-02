namespace NureTimetable.DAL.Moodle.Models.WebService;

public record SiteInfo
{
    public string SiteName { get; init; } = string.Empty;

    public string UserName { get; init; } = string.Empty;

    public string FirstName { get; init; } = string.Empty;

    public string LastName { get; init; } = string.Empty;

    public string FullName { get; init; } = string.Empty;

    public string Lang { get; init; } = string.Empty;

    public int UserId { get; init; }

    public string SiteUrl { get; init; } = string.Empty;

    public string UserPictureUrl { get; init; } = string.Empty;

    public List<Function> Functions { get; init; } = new();

    public int DownloadFiles { get; init; }

    public int UploadFiles { get; init; }

    public string Release { get; init; } = string.Empty;

    public string Version { get; init; } = string.Empty;

    public string MobileCssUrl { get; init; } = string.Empty;

    public List<AdvancedFeature> AdvancedFeatures { get; init; } = new();

    public bool UserCanManageOwnFiles { get; init; }

    public int UserQuota { get; init; }

    public int UserMaxUploadFileSize { get; init; }

    public int UserHomePage { get; init; }

    public record Function(string Name, string Version);

    public record AdvancedFeature(string Name, string Value);
}
