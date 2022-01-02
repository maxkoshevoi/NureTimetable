using Microsoft.Maui.Controls;
using NureTimetable.UI.ViewModels;

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
