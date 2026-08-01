namespace Uno.RevenueCat.Enums;

/// <summary>
/// The store a purchase or entitlement originated from.
/// </summary>
public enum StoreType
{
    /// <summary>Amazon Appstore.</summary>
    Amazon = 0,

    /// <summary>Apple App Store (iOS).</summary>
    AppStore = 1,

    /// <summary>Apple Mac App Store.</summary>
    MacAppStore = 2,

    /// <summary>Google Play Store.</summary>
    PlayStore = 3,

    /// <summary>Access granted for free by RevenueCat, not tied to a store purchase.</summary>
    Promotional = 4,

    /// <summary>Stripe, used for RevenueCat's web billing.</summary>
    Stripe = 5,

    /// <summary>The store could not be determined.</summary>
    UnknownStore = 6,

    /// <summary>RevenueCat Billing, RevenueCat's own merchant-of-record web store.</summary>
    RcBilling = 7,

    /// <summary>A purchase recorded through RevenueCat's external purchases API rather than a native store.</summary>
    External = 8,

    /// <summary>Paddle.</summary>
    Paddle = 9,

    /// <summary>RevenueCat's test store, used for development and testing without a real store.</summary>
    TestStore = 10,

    /// <summary>Samsung Galaxy Store.</summary>
    Galaxy = 11
}
