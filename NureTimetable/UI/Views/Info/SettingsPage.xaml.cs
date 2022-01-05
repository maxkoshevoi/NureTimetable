using NureTimetable.UI.ViewModels;

namespace NureTimetable.UI.Views;

public partial class SettingsPage : ContentPage
{
    public SettingsPage()
    {
        InitializeComponent();
        BindingContext = new SettingsViewModel();
    }
}
