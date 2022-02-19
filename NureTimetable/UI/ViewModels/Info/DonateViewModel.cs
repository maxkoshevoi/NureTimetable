using NureTimetable.BL;
using NureTimetable.Core.Localization;
using Plugin.InAppBilling;
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
        InAppBillingPurchase? purchase = await InAppPurchase.Buy(productId, true);
        string message = purchase == null ? LN.PurchaseFailed : LN.ThanksForYourSupport;
        Shell.Current.CurrentPage.DisplayToastAsync(message).Forget();
    }
}
