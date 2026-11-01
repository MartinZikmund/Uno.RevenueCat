using Maui.RevenueCat.iOS;
using Uno.RevenueCat.Enums;

namespace Uno.RevenueCat.Platforms.iOS.Extensions;

internal static class CustomerInfoFetchPolicyExtensions
{
    internal static RCCacheFetchPolicy ToRCCacheFetchPolicy(this CustomerInfoFetchPolicy fetchPolicy) => fetchPolicy switch
    {
        CustomerInfoFetchPolicy.CachedOrFetched => RCCacheFetchPolicy.CachedOrFetched,
        CustomerInfoFetchPolicy.FetchCurrent => RCCacheFetchPolicy.FetchCurrent,
        CustomerInfoFetchPolicy.NotStaleCachedOrFetched => RCCacheFetchPolicy.NotStaleCachedOrFetched,
        CustomerInfoFetchPolicy.FromCacheOnly => RCCacheFetchPolicy.FromCacheOnly,
        _ => throw new ArgumentOutOfRangeException(nameof(fetchPolicy), fetchPolicy, null),
    };
}
