namespace NureTimetable.UI.Views;

public partial class LessonSettingsPage : ContentPage
{
    public LessonSettingsPage()
    {
        InitializeComponent();
    }

    protected override bool OnBackButtonPressed()
    {
        (BindingContext as LessonSettingsViewModel)!.BackButtonPressedCommand.Execute(null);
        return true;
    }
}
