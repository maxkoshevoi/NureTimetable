using Plugin.InAppBilling;

namespace NureTimetable.BL;

public static class InAppPurchase
{
    public static async Task<InAppBillingPurchase?> Buy(string productId, bool consume)
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
            if (billingEx.PurchaseError != PurchaseError.UserCancelled &&
                billingEx.PurchaseError != PurchaseError.ServiceUnavailable)
            {
                billingEx.Data.Add(nameof(billingEx.PurchaseError), billingEx.PurchaseError);
                billingEx.Data.Add("Product Id", productId);
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
    /// Consumes or Acknowledges the purchase based on consume parameter
    /// </summary>
    private static async Task ProcessPurchase(IInAppBilling billing, InAppBillingPurchase purchase, bool consume)
    {
        if (consume)
        {
            if (purchase.ConsumptionState == ConsumptionState.NoYetConsumed)
            {
                await billing.ConsumePurchaseAsync(purchase.ProductId, purchase.TransactionIdentifier);
            }
            return;
        }

        if (purchase.IsAcknowledged == false)
        {
            await billing.FinalizePurchaseAsync(purchase.TransactionIdentifier);
        }
    }
}
