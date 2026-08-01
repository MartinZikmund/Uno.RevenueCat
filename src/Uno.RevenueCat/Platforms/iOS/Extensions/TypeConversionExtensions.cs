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

    // Never throws: this runs inside customer-info mapping, and an unmapped value there
    // would lock a paying user out of their entitlements (or escape a completed purchase).
    internal static PeriodType ToPeriodType(this RCPeriodType periodType) => periodType switch
    {
        RCPeriodType.Intro => PeriodType.Intro,
        RCPeriodType.Trial => PeriodType.Trial,
        RCPeriodType.Normal => PeriodType.Normal,
        RCPeriodType.Prepaid => PeriodType.Prepaid,
        _ => PeriodType.Normal,
    };

    internal static StoreType ToStoreType(this RCStore store) => store switch
    {
        RCStore.AppStore => StoreType.AppStore,
        RCStore.MacAppStore => StoreType.MacAppStore,
        RCStore.PlayStore => StoreType.PlayStore,
        RCStore.Amazon => StoreType.Amazon,
        RCStore.Promotional => StoreType.Promotional,
        RCStore.Stripe => StoreType.Stripe,
        RCStore.Billing => StoreType.RcBilling,
        RCStore.External => StoreType.External,
        RCStore.Paddle => StoreType.Paddle,
        RCStore.TestStore => StoreType.TestStore,
        RCStore.Galaxy => StoreType.Galaxy,
        _ => StoreType.UnknownStore,
    };

    internal static SubscriptionUnit ToSubscriptionUnit(this RCSubscriptionPeriodUnit unit) => unit switch
    {
        RCSubscriptionPeriodUnit.Day => SubscriptionUnit.Day,
        RCSubscriptionPeriodUnit.Week => SubscriptionUnit.Week,
        RCSubscriptionPeriodUnit.Month => SubscriptionUnit.Month,
        RCSubscriptionPeriodUnit.Year => SubscriptionUnit.Year,
        _ => SubscriptionUnit.Unknown,
    };

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
