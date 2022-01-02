using Microsoft.Maui.Controls;
using NureTimetable.DAL.Settings;
using NureTimetable.UI.ViewModels;

namespace NureTimetable.UI.Views;

public partial class ManageLessonsPage : ContentPage
{
    public ManageLessonsPage()
    {
        InitializeComponent();

        // Remove SyncDl button if no DL user
        if (SettingsRepository.Settings.DlNureUser == null)
        {
            ToolbarItems.RemoveAt(0);
        }
    }

    private void Handle_ItemTapped(object sender, ItemTappedEventArgs e)
    {
        ((ListView)sender).SelectedItem = null;
    }

    protected override bool OnBackButtonPressed()
    {
        (BindingContext as ManageLessonsViewModel)!.BackButtonPressedCommand.Execute(null);
        return true;
    }
}
