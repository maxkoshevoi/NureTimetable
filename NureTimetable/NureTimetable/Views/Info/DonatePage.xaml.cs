using NureTimetable.Core.Localization;
using NureTimetable.Core.Models.Consts;
using NureTimetable.Helpers;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace NureTimetable.Views.Info
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class DonatePage : ContentPage
	{
		public DonatePage ()
		{
			InitializeComponent ();
		}

        private async void DonateLow_Clicked(object sender, EventArgs e)
        {
            if (await InAppPurchase.Buy(InAppProducts.DonateLow, true) != null)
            {
                await DisplayAlert(LN.Purchase, LN.ThanksForYourSupport, LN.Ok);
            }
            else
            {
                await DisplayAlert(LN.Purchase, LN.PurchaseFailed, LN.Ok);
            }
        }

        private async void DonateMedium_Clicked(object sender, EventArgs e)
        {
            if (await InAppPurchase.Buy(InAppProducts.DonateMedium, true) != null)
            {
                await DisplayAlert(LN.Purchase, LN.ThanksForYourSupport, LN.Ok);
            }
            else
            {
                await DisplayAlert(LN.Purchase, LN.PurchaseFailed, LN.Ok);
            }
        }

        private async void DonateHigh_Clicked(object sender, EventArgs e)
        {
            if (await InAppPurchase.Buy(InAppProducts.DonateHigh, true) != null)
            {
                await DisplayAlert(LN.Purchase, LN.ThanksForYourSupport, LN.Ok);
            }
            else
            {
                await DisplayAlert(LN.Purchase, LN.PurchaseFailed, LN.Ok);
            }
        }
    }
}