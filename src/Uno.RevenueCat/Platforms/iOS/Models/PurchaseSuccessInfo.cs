using Maui.RevenueCat.iOS;

namespace Uno.RevenueCat.Platforms.iOS.Models;

/// <summary>
/// Contains information about a successful purchase.
/// </summary>
public sealed class PurchaseSuccessInfo
{
    /// <summary>
    /// Gets the store transaction.
    /// </summary>
    public RCStoreTransaction StoreTransaction { get; }

    /// <summary>
    /// Gets the customer info after the purchase.
    /// </summary>
    public RCCustomerInfo CustomerInfo { get; }

    /// <summary>
    /// Creates a new instance.
    /// </summary>
    public PurchaseSuccessInfo(RCStoreTransaction transaction, RCCustomerInfo customerInfo)
    {
        StoreTransaction = transaction;
        CustomerInfo = customerInfo;
    }
}
