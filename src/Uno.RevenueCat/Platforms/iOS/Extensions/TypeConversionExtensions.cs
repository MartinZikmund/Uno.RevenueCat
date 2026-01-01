using Maui.RevenueCat.iOS;
using Uno.RevenueCat.Enums;

namespace Uno.RevenueCat.Platforms.iOS.Extensions;

internal static class TypeConversionExtensions
{
    internal static OwnershipType ToOwnershipType(this RCPurchaseOwnershipType ownershipType)
    {
        switch (ownershipType)
        {
            case RCPurchaseOwnershipType.FamilyShared:
                return OwnershipType.FamilyShared;
            case RCPurchaseOwnershipType.Purchased:
                return OwnershipType.Purchased;
            default:
                return OwnershipType.Unknown;
        }
    }

    internal static PeriodType ToPeriodType(this RCPeriodType periodType)
    {
        switch (periodType)
        {
            case RCPeriodType.Intro:
                return PeriodType.Intro;
            case RCPeriodType.Trial:
                return PeriodType.Trial;
            case RCPeriodType.Normal:
                return PeriodType.Normal;
            default:
                throw new ArgumentException($"Unknown period type: {periodType}");
        }
    }

    internal static StoreType ToStoreType(this RCStore store)
    {
        switch (store)
        {
            case RCStore.AppStore:
                return StoreType.AppStore;
            case RCStore.MacAppStore:
                return StoreType.MacAppStore;
            case RCStore.PlayStore:
                return StoreType.PlayStore;
            case RCStore.Amazon:
                return StoreType.Amazon;
            case RCStore.Promotional:
                return StoreType.Promotional;
            case RCStore.Stripe:
                return StoreType.Stripe;
            default:
                return StoreType.UnknownStore;
        }
    }

    internal static RCLogLevel ToRCLogLevel(this Enums.LogLevel logLevel)
    {
        return logLevel switch
        {
            Enums.LogLevel.Verbose => RCLogLevel.Verbose,
            Enums.LogLevel.Debug => RCLogLevel.Debug,
            Enums.LogLevel.Information => RCLogLevel.Info,
            Enums.LogLevel.Warning => RCLogLevel.Warn,
            Enums.LogLevel.Error => RCLogLevel.Error,
            _ => throw new ArgumentOutOfRangeException(nameof(logLevel), logLevel, null)
        };
    }

    internal static IntroElegibilityStatus Convert(this RCIntroEligibilityStatus eligibility)
    {
        switch (eligibility)
        {
            case RCIntroEligibilityStatus.Ineligible:
                return IntroElegibilityStatus.Ineligible;
            case RCIntroEligibilityStatus.Eligible:
                return IntroElegibilityStatus.Eligible;
            case RCIntroEligibilityStatus.NoIntroOfferExists:
                return IntroElegibilityStatus.NoIntroOfferExists;
            case RCIntroEligibilityStatus.Unknown:
            default:
                return IntroElegibilityStatus.Unknown;
        }
    }
}
