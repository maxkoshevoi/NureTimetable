using NureTimetable.UI.ViewModels;

namespace NureTimetable.UI.Views;

public partial class ManageEntitiesPage : ContentPage
{
    public ManageEntitiesPage()
    {
        InitializeComponent();
        BindingContext = new ManageEntitiesViewModel();
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

    private void EntitiesList_OnItemTapped(object sender, ItemTappedEventArgs e)
    {
        ((ListView)sender).SelectedItem = null;
    }
}
