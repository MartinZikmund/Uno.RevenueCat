using Android.App;
using Android.Content;
using Com.Revenuecat.Purchases;
using Uno.RevenueCat.Enums;
using Uno.RevenueCat.Platforms.Android.Delegates;
using Uno.RevenueCat.Platforms.Android.Models;
using Package = Com.Revenuecat.Purchases.Package;

namespace Uno.RevenueCat.Platforms.Android.Extensions;

internal static class PurchasesExtensions
{
    internal static async Task<bool> CanMakePaymentsAsync(Context context,
        CancellationToken cancellationToken = default)
    {
        // Not disposed: Play Billing holds the callback across an async startConnection and invokes
        // it later. Disposing the JNI peer here (e.g. when the token cancels first) would destroy it
        // mid-flight, and the SDK's eventual upcall would then fail to activate it and abort.
        var callback = new DelegatingCallback<Java.Lang.Boolean>(cancellationToken);
        Purchases.CanMakePayments(context, callback);
        var result = await callback.Task;
        return result.BooleanValue();
    }

    internal static Task<string> GetStorefrontCountryCodeAsync(this Purchases purchases,
        CancellationToken cancellationToken = default)
    {
        var callback = new DelegatingGetStorefrontCountryCodeCallback(cancellationToken);
        purchases.GetStorefrontCountryCode(callback);
        return callback.Task;
    }

    internal static Task<CustomerInfo> GetCustomerInfoAsync(this Purchases purchases,
        CancellationToken cancellationToken = default,
        CustomerInfoFetchPolicy fetchPolicy = CustomerInfoFetchPolicy.CachedOrFetched)
    {
        var listener = new DelegatingReceiveCustomerInfoCallback(cancellationToken);
        purchases.GetCustomerInfo(fetchPolicy.ToRCCacheFetchPolicy(), listener);
        return listener.Task;
    }

    internal static Task<CustomerInfo> LogInAsync(this Purchases purchases, string newAppUserId,
        CancellationToken cancellationToken = default)
    {
        var listener = new DelegatingLogInCallback(cancellationToken);
        purchases.LogIn(newAppUserId, listener);
        return listener.Task;
    }

    internal static Task<CustomerInfo> LogOutAsync(this Purchases purchases,
        CancellationToken cancellationToken = default)
    {
        var listener = new DelegatingReceiveCustomerInfoCallback(cancellationToken);
        purchases.LogOut(listener);
        return listener.Task;
    }

    internal static Task<Offerings> GetOfferingsAsync(this Purchases purchases,
        CancellationToken cancellationToken = default)
    {
        var listener = new DelegatingReceiveOfferingsCallback(cancellationToken);
        purchases.GetOfferings(listener);
        return listener.Task;
    }

    internal static Task<PurchaseSuccessInfo> PurchaseAsync(this Purchases purchases, Activity activity,
        Package packageToPurchase, CancellationToken cancellationToken = default)
    {
        var listener = new DelegatingMakePurchaseListener(cancellationToken);
        var purchaseParams = new PurchaseParams(new PurchaseParams.Builder(activity, packageToPurchase));

        // Play Billing requires launchBillingFlow on the main thread, and the Test Store shows its
        // dialog synchronously (Looper.prepare() crash off it). A throw inside the posted lambda
        // would crash the looper and leave the task pending, so route it into the task instead.
        activity.RunOnUiThread(() =>
        {
            try
            {
                purchases.Purchase(purchaseParams, listener);
            }
            catch (Exception exception)
            {
                listener.ReportException(exception);
            }
        });

        return listener.Task;
    }

    internal static Task<CustomerInfo> RestorePurchasesAsync(this Purchases purchases,
        CancellationToken cancellationToken = default)
    {
        var listener = new DelegatingReceiveCustomerInfoCallback(cancellationToken);
        purchases.RestorePurchases(listener);
        return listener.Task;
    }
}
