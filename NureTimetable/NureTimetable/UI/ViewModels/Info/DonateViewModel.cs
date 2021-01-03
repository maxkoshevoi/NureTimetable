using NureTimetable.Core.Localization;
using NureTimetable.UI.Helpers;
using Plugin.InAppBilling;
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
            InAppBillingPurchase purchase = await InAppPurchase.Buy(productId, true);
            if (purchase is not null)
            {
                await Shell.Current.DisplayAlert(LN.Purchase, LN.ThanksForYourSupport, LN.Ok);
            }
        }
    }
}