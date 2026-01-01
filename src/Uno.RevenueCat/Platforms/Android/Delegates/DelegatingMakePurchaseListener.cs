using Com.Revenuecat.Purchases;
using Com.Revenuecat.Purchases.Interfaces;
using Com.Revenuecat.Purchases.Models;
using Uno.RevenueCat.Platforms.Android.Exceptions;
using Uno.RevenueCat.Platforms.Android.Models;

namespace Uno.RevenueCat.Platforms.Android.Delegates;

internal sealed class DelegatingMakePurchaseListener : DelegatingListenerBase<PurchaseSuccessInfo>, IPurchaseCallback
{
    public DelegatingMakePurchaseListener(CancellationToken cancellationToken) : base(cancellationToken)
    {
    }

    public void OnCompleted(StoreTransaction storeTransaction, CustomerInfo customerInfo)
    {
        ReportSuccess(new PurchaseSuccessInfo(storeTransaction, customerInfo));
    }

    public void OnError(PurchasesError purchasesError, bool userCancelled)
    {
        ReportException(new PurchasesErrorException(purchasesError, userCancelled));
    }
}
