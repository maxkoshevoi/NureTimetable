using NureTimetable.Core.BL;
using NureTimetable.Core.Models.Consts;
using Plugin.InAppBilling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace NureTimetable.UI.Helpers
{
    public static class InAppPurchase
    {
        public static async Task<InAppBillingPurchase> Buy(string productId, bool consume)
        {
            IInAppBilling billing = CrossInAppBilling.Current;
            try
            {
                if (!CrossInAppBilling.IsSupported || !await billing.ConnectAsync())
                {
                    return null;
                }

                List<InAppBillingPurchase> existingPurchases = (await billing.GetPurchasesAsync(ItemType.InAppPurchase))
                    .Where(p => p.ProductId == productId)
                    .ToList();
                foreach (var existingPurchase in existingPurchases)
                {
                    await ProcessPurchase(billing, existingPurchase, consume);
                }

                InAppBillingPurchase purchase = await billing.PurchaseAsync(productId, ItemType.InAppPurchase);
                if (purchase != null && purchase.State == PurchaseState.Purchased)
                {
                    await ProcessPurchase(billing, purchase, consume);
                    return purchase;
                }
            }
            catch (InAppBillingPurchaseException billingEx)
            {
                if (billingEx.PurchaseError != PurchaseError.UserCancelled)
                {
                    billingEx.Data.Add(nameof(billingEx.PurchaseError), billingEx.PurchaseError);
                    ExceptionService.LogException(billingEx);
                }
            }
            catch (TaskCanceledException) { }
            catch (Exception ex)
            {
                ExceptionService.LogException(ex);
            }
            finally
            {
                // Disconnect, it is okay if we never connected, this will never throw an exception
                await billing.DisconnectAsync();
            }
            return null;
        }

        /// <summary>
        /// Consumes or Acknowledges the purchse based on consume parameter
        /// </summary>
        private static async Task ProcessPurchase(IInAppBilling billing, InAppBillingPurchase purchase, bool consume)
        {
            if (consume)
            {
                if (purchase.ConsumptionState == ConsumptionState.NoYetConsumed)
                {
                    await billing.ConsumePurchaseAsync(purchase.ProductId, purchase.PurchaseToken);
                }
                return;
            }

            if (purchase.IsAcknowledged == false)
            {
                await billing.AcknowledgePurchaseAsync(purchase.PurchaseToken);
            }
        }
    }
}
