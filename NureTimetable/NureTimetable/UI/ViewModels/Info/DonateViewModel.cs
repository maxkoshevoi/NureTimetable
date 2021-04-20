﻿using NureTimetable.BL;
using NureTimetable.Core.Extensions;
using NureTimetable.Core.Localization;
using Plugin.InAppBilling;
using System.Threading.Tasks;
using Xamarin.CommunityToolkit.Extensions;
using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.Forms;

namespace NureTimetable.UI.ViewModels
{
    public class DonateViewModel : BaseViewModel
    {
        public IAsyncCommand<string> BuyProductCommand { get; }

        public DonateViewModel()
        {
            BuyProductCommand = CommandFactory.Create<string>(BuyProduct, allowsMultipleExecutions: false);
        }
        
        public static async Task BuyProduct(string productId)
        {
            InAppBillingPurchase purchase = await InAppPurchase.Buy(productId, true);
            string message = purchase == null ? LN.PurchaseFailed : LN.ThanksForYourSupport;
            try
            {
                Shell.Current.CurrentPage.DisplayToastAsync(message).Forget(false);
            }
            catch { } // TODO: Remove when https://github.com/xamarin/XamarinCommunityToolkit/issues/959 is fixed
        }
    }
}