namespace NureTimetable.DAL.Models.Moodle.Auth;

public record MoodleUser(string Login, string Password, string Token)
{
    public int Id { get; init; }

    public string FullName { get; init; } = string.Empty;
}
