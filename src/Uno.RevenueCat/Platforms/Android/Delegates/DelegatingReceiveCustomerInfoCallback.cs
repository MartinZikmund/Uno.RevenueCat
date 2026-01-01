using Com.Revenuecat.Purchases;
using Com.Revenuecat.Purchases.Interfaces;
using Uno.RevenueCat.Platforms.Android.Exceptions;

namespace Uno.RevenueCat.Platforms.Android.Delegates;

internal sealed class DelegatingReceiveCustomerInfoCallback : DelegatingListenerBase<CustomerInfo>, IReceiveCustomerInfoCallback
{
    public DelegatingReceiveCustomerInfoCallback(CancellationToken cancellationToken) : base(cancellationToken)
    {
    }

    public void OnError(PurchasesError error)
    {
        ReportException(new PurchasesErrorException(error, false));
    }

    public void OnReceived(CustomerInfo customerInfo)
    {
        ReportSuccess(customerInfo);
    }
}
