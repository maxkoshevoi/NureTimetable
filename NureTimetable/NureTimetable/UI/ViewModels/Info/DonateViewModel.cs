using NureTimetable.Core.Localization;
using NureTimetable.UI.Helpers;
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
            if (await InAppPurchase.Buy(productId, true) != null)
            {
                await Shell.Current.DisplayAlert(LN.Purchase, LN.ThanksForYourSupport, LN.Ok);
            }
            else
            {
                await Shell.Current.DisplayAlert(LN.Purchase, LN.PurchaseFailed, LN.Ok);
            }
        }
    }
}