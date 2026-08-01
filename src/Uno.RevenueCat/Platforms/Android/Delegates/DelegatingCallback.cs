using Com.Revenuecat.Purchases.Interfaces;

namespace Uno.RevenueCat.Platforms.Android.Delegates;

internal sealed class DelegatingCallback<TResult> : DelegatingListenerBase<TResult>, ICallback
{
    public DelegatingCallback(CancellationToken cancellationToken) : base(cancellationToken)
    {
    }

    public void OnReceived(Java.Lang.Object? resultObject)
    {
        if (resultObject is TResult result)
        {
            ReportSuccess(result);
        }
        else
        {
            ReportException(new InvalidCastException(
                $"Expected {typeof(TResult).Name} but the callback returned {resultObject?.GetType().Name ?? "null"}."));
        }
    }
}
