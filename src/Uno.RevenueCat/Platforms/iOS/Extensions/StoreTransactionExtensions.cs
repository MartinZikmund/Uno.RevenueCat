using Maui.RevenueCat.iOS;
using Uno.RevenueCat.Models;

namespace Uno.RevenueCat.Platforms.iOS.Extensions;

internal static class StoreTransactionExtensions
{
    internal static StoreTransactionDto ToStoreTransactionDto(this RCStoreTransaction storeTransaction)
    {
        return new StoreTransactionDto
        {
            ProductIdentifier = storeTransaction.ProductIdentifier,
            PurchaseDate = storeTransaction.PurchaseDate.ToDateTime() ?? DateTime.UtcNow,
            TransactionIdentifier = storeTransaction.TransactionIdentifier,
            Quantity = storeTransaction.Quantity
        };
    }
}
