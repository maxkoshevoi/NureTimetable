using NureTimetable.Core.Models.Consts;
using Plugin.DeviceInfo;
using Plugin.InAppBilling;
using Plugin.InAppBilling.Abstractions;
using System;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace NureTimetable.Services.Helpers
{
    public static class InAppPurchase
    {
        public static async Task<InAppBillingPurchase> Buy(string productId, bool consume)
        {
            try
            {
                if (!await CrossInAppBilling.Current.ConnectAsync())
                {
                    //Couldn't connect to billing, could be offline
                    return null;
                }

                //try to purchase item
                InAppBillingPurchase purchase = await CrossInAppBilling.Current.PurchaseAsync(productId, ItemType.InAppPurchase, "apppayload");
                if (purchase != null && purchase.State == PurchaseState.Purchased)
                {
                    //Purchased
                    if (consume)
                    {
                        await Consume(purchase);
                    }
                    return purchase;
                }
            }
            catch (InAppBillingPurchaseException billingEx)
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    MessagingCenter.Send(Application.Current, MessageTypes.ExceptionOccurred, billingEx);
                });
            }
            catch (Exception ex)
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    MessagingCenter.Send(Application.Current, MessageTypes.ExceptionOccurred, ex);
                });
            }
            finally
            {
                //Disconnect, it is okay if we never connected, this will never throw an exception
                await CrossInAppBilling.Current.DisconnectAsync();
            }
            return null;
        }

        private static async Task<bool> Consume(InAppBillingPurchase purchase)
        {
            //Called after we have a successful purchase or later on
            if (CrossDeviceInfo.Current.Platform != Plugin.DeviceInfo.Abstractions.Platform.Android)
            {
                return true;
            }

            try
            {
                if (!await CrossInAppBilling.Current.ConnectAsync())
                {
                    return false;
                }

                InAppBillingPurchase consumedItem = await CrossInAppBilling.Current.ConsumePurchaseAsync(purchase.ProductId, purchase.PurchaseToken);
                if (consumedItem != null)
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    MessagingCenter.Send(Application.Current, MessageTypes.ExceptionOccurred, ex);
                });
            }
            finally
            {
                //Disconnect, it is okay if we never connected, this will never throw an exception
                await CrossInAppBilling.Current.DisconnectAsync();
            }
            return false;
        }
    }
}
