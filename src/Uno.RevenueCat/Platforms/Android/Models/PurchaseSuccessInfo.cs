using Com.Revenuecat.Purchases;
using Com.Revenuecat.Purchases.Models;

namespace Uno.RevenueCat.Platforms.Android.Models;

/// <summary>
/// Contains information about a successful purchase.
/// </summary>
public class PurchaseSuccessInfo
{
    /// <summary>
    /// Gets the store transaction.
    /// </summary>
    public StoreTransaction StoreTransaction { get; }

    /// <summary>
    /// Gets the customer info after the purchase.
    /// </summary>
    public CustomerInfo CustomerInfo { get; }

    /// <summary>
    /// Creates a new instance.
    /// </summary>
    public PurchaseSuccessInfo(StoreTransaction storeTransaction, CustomerInfo customerInfo)
    {
        StoreTransaction = storeTransaction;
        CustomerInfo = customerInfo;
    }
}
