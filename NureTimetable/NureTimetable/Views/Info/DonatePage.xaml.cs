using NureTimetable.Helpers;
using NureTimetable.Models.Consts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                await DisplayAlert("Покупка", "Спасибо за поддержку проекта!", "Ок");
            }
            else
            {
                await DisplayAlert("Покупка", "Что-то пошло не так, покупка не удалась.", "Ок");
            }
        }

        private async void DonateMedium_Clicked(object sender, EventArgs e)
        {
            if (await InAppPurchase.Buy(InAppProducts.DonateMedium, true) != null)
            {
                await DisplayAlert("Покупка", "Спасибо за поддержку проекта!", "Ок");
            }
            else
            {
                await DisplayAlert("Покупка", "Что-то пошло не так, покупка не удалась.", "Ок");
            }
        }

        private async void DonateHigh_Clicked(object sender, EventArgs e)
        {
            if (await InAppPurchase.Buy(InAppProducts.DonateHigh, true) != null)
            {
                await DisplayAlert("Покупка", "Спасибо за поддержку проекта!", "Ок");
            }
            else
            {
                await DisplayAlert("Покупка", "Что-то пошло не так, покупка не удалась.", "Ок");
            }
        }
    }
}