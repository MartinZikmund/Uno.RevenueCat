using Com.Revenuecat.Purchases;
using Uno.RevenueCat.Enums;
using OwnershipTypeNative = Com.Revenuecat.Purchases.OwnershipType;
using PeriodTypeNative = Com.Revenuecat.Purchases.PeriodType;

namespace Uno.RevenueCat.Platforms.Android.Extensions;

internal static class TypeConversionExtensions
{
    internal static Enums.OwnershipType ToDtoOwnershipType(this OwnershipTypeNative ownershipType)
    {
        if (ownershipType == OwnershipTypeNative.FamilyShared) return Enums.OwnershipType.FamilyShared;
        if (ownershipType == OwnershipTypeNative.Purchased) return Enums.OwnershipType.Purchased;
        return Enums.OwnershipType.Unknown;
    }

    internal static Enums.PeriodType ToDtoPeriodType(this PeriodTypeNative periodType)
    {
        if (periodType == PeriodTypeNative.Intro) return Enums.PeriodType.Intro;
        if (periodType == PeriodTypeNative.Trial) return Enums.PeriodType.Trial;
        if (periodType == PeriodTypeNative.Normal) return Enums.PeriodType.Normal;
        throw new ArgumentException($"Unknown period type: {periodType}");
    }

    internal static StoreType ToStoreType(this Store store)
    {
        if (store == Store.Promotional) return StoreType.Promotional;
        if (store == Store.PlayStore) return StoreType.PlayStore;
        if (store == Store.AppStore) return StoreType.AppStore;
        if (store == Store.Amazon) return StoreType.Amazon;
        if (store == Store.Stripe) return StoreType.Stripe;
        if (store == Store.MacAppStore) return StoreType.MacAppStore;
        return StoreType.UnknownStore;
    }

    internal static Com.Revenuecat.Purchases.LogLevel ToRCLogLevel(this Enums.LogLevel logLevel)
    {
        Com.Revenuecat.Purchases.LogLevel? revenueCatLogLevel;

        revenueCatLogLevel = logLevel switch
        {
            Enums.LogLevel.Verbose => Com.Revenuecat.Purchases.LogLevel.Verbose,
            Enums.LogLevel.Debug => Com.Revenuecat.Purchases.LogLevel.Debug,
            Enums.LogLevel.Information => Com.Revenuecat.Purchases.LogLevel.Info,
            Enums.LogLevel.Warning => Com.Revenuecat.Purchases.LogLevel.Warn,
            Enums.LogLevel.Error => Com.Revenuecat.Purchases.LogLevel.Error,
            _ => throw new ArgumentOutOfRangeException(nameof(logLevel), logLevel, null)
        };

        return revenueCatLogLevel
            ?? throw new Exception($"Could not convert LogLevel to RevenueCat LogLevel");
    }
}
