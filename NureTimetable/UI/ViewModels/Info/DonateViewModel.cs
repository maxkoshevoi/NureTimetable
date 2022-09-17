using Plugin.InAppBilling;

namespace NureTimetable.UI.ViewModels;

public class DonateViewModel : BaseViewModel
{
    public IRelayCommand<string> BuyProductCommand { get; }

    public DonateViewModel()
    {
        BuyProductCommand = CommandFactory.Create<string>(p => BuyProduct(p!));
    }

    public static async Task BuyProduct(string productId)
    {
        InAppBillingPurchase? purchase = await InAppPurchase.Buy(productId, true);
        Toast.Make(purchase == null ? LN.PurchaseFailed : LN.ThanksForYourSupport).Show().Forget();
    }
}
