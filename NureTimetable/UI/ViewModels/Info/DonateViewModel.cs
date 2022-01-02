using Microsoft.Maui.Controls;
using Xamarin.CommunityToolkit.Extensions;
using Xamarin.CommunityToolkit.ObjectModel;

namespace NureTimetable.UI.ViewModels;

public class DonateViewModel : BaseViewModel
{
    public IAsyncCommand<string> BuyProductCommand { get; }

    public DonateViewModel()
    {
        BuyProductCommand = CommandFactory.Create<string>(p => BuyProduct(p!), allowsMultipleExecutions: false);
    }

    public static async Task BuyProduct(string productId)
    {
        await Shell.Current.CurrentPage.DisplayToastAsync("Not supported in MAUI yet.");
    }
}
