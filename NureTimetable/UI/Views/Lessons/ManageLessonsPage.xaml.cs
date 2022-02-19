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

    protected override void OnAppearing()
    {
        base.OnAppearing();

        _ = Task.Run(async () =>
        {
            await Task.Delay(1000);

            MainThread.BeginInvokeOnMainThread(() =>
            {
                var items = ToolbarItems.ToList();
                ToolbarItems.Clear();
                foreach (var item in items)
                {
                    ToolbarItems.Add(item);
                }
            });
        });
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
