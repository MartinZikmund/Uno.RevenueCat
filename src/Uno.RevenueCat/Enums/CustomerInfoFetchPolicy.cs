namespace Uno.RevenueCat.Enums;

/// <summary>
/// How <see cref="Services.IRevenueCatBilling.GetCustomerInfoAsync"/> treats the customer info the
/// RevenueCat SDK caches on the device. The cache goes stale after 5 minutes in the foreground, but
/// only after 25 hours in the background (iOS sandbox builds keep 5 minutes).
/// https://www.revenuecat.com/docs/customers/customer-info
/// </summary>
public enum CustomerInfoFetchPolicy
{
    /// <summary>
    /// Default. Returns cached data when available (even if stale), otherwise fetches. Stale data
    /// triggers a background refresh whose result this call does not return.
    /// </summary>
    CachedOrFetched,

    /// <summary>
    /// Always fetches up-to-date data and fails if the fetch fails. Offline, the error is
    /// <see cref="PurchaseErrorStatus.NetworkError"/> on Android and
    /// <see cref="PurchaseErrorStatus.OfflineConnectionError"/> on iOS.
    /// </summary>
    FetchCurrent,

    /// <summary>
    /// Returns cached data only if it is not stale, otherwise fetches. If that fetch fails, returns
    /// the error rather than outdated data (same offline errors as <see cref="FetchCurrent"/>).
    /// </summary>
    NotStaleCachedOrFetched,

    /// <summary>
    /// Returns cached data, never fetching. Fails with <see cref="PurchaseErrorStatus.CustomerInfoError"/>
    /// when nothing is cached.
    /// </summary>
    FromCacheOnly,
}
