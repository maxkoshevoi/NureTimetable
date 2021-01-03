using NureTimetable.Core.Localization;
using NureTimetable.UI.Helpers;
using Plugin.InAppBilling;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace NureTimetable.UI.ViewModels.Info
{
    public class DonateViewModel : BaseViewModel
    {
        #region Properties
        public ICommand BuyProductCommand { get; }
        #endregion

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