namespace NureTimetable.UI.Helpers;

public static class CommonCommands
{
    public static IAsyncCommand<string> NavigateUriCommand { get; } =
        CommandFactory.Create<string>(async url => await Browser.OpenAsync(url));
}
