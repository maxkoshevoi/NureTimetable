namespace NureTimetable.UI.Views;

public partial class TimetablePage : ContentPage, ITimetablePageCommands
{
    public TimetablePage()
    {
        InitializeComponent();
        BindingContext = new TimetableViewModel(this);
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

    public void TimetableNavigateTo(DateTime date) => Timetable.DisplayDate = date;

    public Task ScaleTodayButtonTo(double scale) => BToday.ScaleTo(scale);

    public async Task DisplayToastAsync(string message, int durationMilliseconds = 3000)
    {
        VisualElement anchor = this;
        if (BToday.Scale > 0)
        {
            anchor = BToday;
        }

        await Snackbar.Make(message, duration: TimeSpan.FromMilliseconds(durationMilliseconds), anchor: anchor).Show();
    }
}
