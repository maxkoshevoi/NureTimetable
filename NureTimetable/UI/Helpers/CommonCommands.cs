namespace NureTimetable.UI.Helpers;

public static class CommonCommands
{
    public static IRelayCommand<string> NavigateUriCommand { get; } =
        CommandFactory.Create<string>(url => Browser.OpenAsync(url!));
}
