using Foundation;
using Maui.RevenueCat.iOS;
using Uno.RevenueCat.Platforms.iOS.Exceptions;
using Uno.RevenueCat.Platforms.iOS.Models;

namespace Uno.RevenueCat.Platforms.iOS.Extensions;

internal static class PurchasesExtensions
{
    /// <summary>
    /// Bridges an Objective-C completion handler onto a <see cref="Task{TResult}"/>, disposing the
    /// cancellation registration once the operation settles. Without the dispose, a long-lived token
    /// reused across many calls accumulates registrations that root every completion source.
    /// </summary>
    private sealed class CompletionBridge<TResult>
    {
        // RunContinuationsAsynchronously: without it the awaiter's continuation (arbitrary app code)
        // runs inline on the StoreKit callback thread.
        private readonly TaskCompletionSource<TResult> _taskCompletionSource =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        private readonly CancellationTokenRegistration _registration;

        internal CompletionBridge(CancellationToken cancellationToken) =>
            _registration = cancellationToken.Register(
                static state => ((TaskCompletionSource<TResult>)state!).TrySetCanceled(),
                _taskCompletionSource);

        internal Task<TResult> Task => _taskCompletionSource.Task;

        internal void SetResult(TResult result)
        {
            Unregister();
            _taskCompletionSource.TrySetResult(result);
        }

        internal void SetException(Exception exception)
        {
            Unregister();
            _taskCompletionSource.TrySetException(exception);
        }

        // Unregister rather than Dispose: Dispose blocks until a concurrently-running cancellation
        // callback finishes, and we are on the native callback thread here.
        private void Unregister() => _registration.Unregister();
    }

    internal static Task<LoginResult> LoginAsync(this RCPurchases purchases, string appUserId,
        CancellationToken cancellationToken = default)
    {
        var bridge = new CompletionBridge<LoginResult>(cancellationToken);
        purchases.LogIn(appUserId, (customerInfo, created, error) =>
        {
            if (error != null)
            {
                bridge.SetException(new PurchasesErrorException(error, false));
            }
            else
            {
                bridge.SetResult(new LoginResult(customerInfo, created));
            }
        });
        return bridge.Task;
    }

    internal static Task<RCCustomerInfo> LogOutAsync(this RCPurchases purchases,
        CancellationToken cancellationToken = default)
    {
        var bridge = new CompletionBridge<RCCustomerInfo>(cancellationToken);
        purchases.LogOutWithCompletion((customerInfo, error) =>
        {
            if (error != null)
            {
                bridge.SetException(new PurchasesErrorException(error, false));
            }
            else
            {
                bridge.SetResult(customerInfo);
            }
        });
        return bridge.Task;
    }

    internal static Task<NSDictionary<NSString, RCIntroEligibility>> CheckTrialOrIntroDiscountEligibilityAsync(
        this RCPurchases purchases,
        IEnumerable<string> identifiers,
        CancellationToken cancellationToken = default)
    {
        var bridge = new CompletionBridge<NSDictionary<NSString, RCIntroEligibility>>(cancellationToken);
        purchases.CheckTrialOrIntroDiscountEligibility(
            identifiers.ToArray(),
            (NSDictionary<NSString, RCIntroEligibility> eligibilities) => bridge.SetResult(eligibilities));
        return bridge.Task;
    }

    internal static Task<RCOfferings> GetOfferingsAsync(this RCPurchases purchases,
        CancellationToken cancellationToken = default)
    {
        var bridge = new CompletionBridge<RCOfferings>(cancellationToken);
        purchases.GetOfferingsWithCompletion((RCOfferings offerings, NSError error) =>
        {
            if (error != null)
            {
                bridge.SetException(new PurchasesErrorException(error, false));
            }
            else
            {
                bridge.SetResult(offerings);
            }
        });
        return bridge.Task;
    }

    internal static Task<PurchaseSuccessInfo> PurchasePackageAsync(this RCPurchases purchases,
        RCPackage packageToPurchase, CancellationToken cancellationToken = default)
    {
        var bridge = new CompletionBridge<PurchaseSuccessInfo>(cancellationToken);
        purchases.PurchasePackage(packageToPurchase,
            (RCStoreTransaction transaction, RCCustomerInfo customerInfo, NSError error, bool userCancelled) =>
            {
                if (error != null || userCancelled)
                {
                    bridge.SetException(new PurchasesErrorException(error, userCancelled));
                }
                else
                {
                    bridge.SetResult(new PurchaseSuccessInfo(transaction, customerInfo));
                }
            });
        return bridge.Task;
    }

    internal static Task<RCCustomerInfo> RestorePurchasesAsync(this RCPurchases purchases,
        CancellationToken cancellationToken = default)
    {
        var bridge = new CompletionBridge<RCCustomerInfo>(cancellationToken);
        purchases.RestorePurchasesWithCompletion((RCCustomerInfo customerInfo, NSError error) =>
        {
            if (error != null)
            {
                bridge.SetException(new PurchasesErrorException(error, false));
            }
            else
            {
                bridge.SetResult(customerInfo);
            }
        });
        return bridge.Task;
    }

    internal static Task<RCCustomerInfo> GetCustomerInfoAsync(this RCPurchases purchases,
        CancellationToken cancellationToken = default)
    {
        var bridge = new CompletionBridge<RCCustomerInfo>(cancellationToken);
        purchases.GetCustomerInfoWithCompletion((RCCustomerInfo customerInfo, NSError error) =>
        {
            if (error != null)
            {
                bridge.SetException(new PurchasesErrorException(error, false));
            }
            else
            {
                bridge.SetResult(customerInfo);
            }
        });
        return bridge.Task;
    }
}
