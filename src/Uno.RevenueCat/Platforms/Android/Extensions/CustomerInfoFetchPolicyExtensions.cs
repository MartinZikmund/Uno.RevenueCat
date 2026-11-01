using Com.Revenuecat.Purchases;
using Uno.RevenueCat.Enums;

namespace Uno.RevenueCat.Platforms.Android.Extensions;

internal static class CustomerInfoFetchPolicyExtensions
{
    internal static CacheFetchPolicy ToRCCacheFetchPolicy(this CustomerInfoFetchPolicy fetchPolicy)
    {
        var revenueCatFetchPolicy = fetchPolicy switch
        {
            CustomerInfoFetchPolicy.CachedOrFetched => CacheFetchPolicy.CachedOrFetched,
            CustomerInfoFetchPolicy.FetchCurrent => CacheFetchPolicy.FetchCurrent,
            CustomerInfoFetchPolicy.NotStaleCachedOrFetched => CacheFetchPolicy.NotStaleCachedOrCurrent,
            CustomerInfoFetchPolicy.FromCacheOnly => CacheFetchPolicy.CacheOnly,
            _ => throw new ArgumentOutOfRangeException(nameof(fetchPolicy), fetchPolicy, null),
        };

        // The binding exposes the Kotlin enum entries as nullable statics.
        return revenueCatFetchPolicy
            ?? throw new InvalidOperationException($"RevenueCat has no {nameof(CacheFetchPolicy)} for {fetchPolicy}.");
    }
}
