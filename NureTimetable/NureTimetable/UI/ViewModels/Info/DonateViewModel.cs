using NureTimetable.Core.Localization;
using NureTimetable.UI.Helpers;
using Plugin.InAppBilling.Abstractions;
using System.Threading.Tasks;
using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.Forms;

namespace NureTimetable.UI.ViewModels.Info
{
    public class DonateViewModel : BaseViewModel
    {
        public IAsyncCommand<string> BuyProductCommand { get; }

        public DonateViewModel()
        {
            BuyProductCommand = CommandHelper.Create<string>(BuyProduct);
        }
        
        public static async Task BuyProduct(string productId)
        {
            InAppBillingPurchase purchaseResult = await InAppPurchase.Buy(productId, true);
            await Shell.Current.DisplayAlert(LN.Purchase, purchaseResult is null ? LN.PurchaseFailed : LN.ThanksForYourSupport, LN.Ok);
        }
    }
}