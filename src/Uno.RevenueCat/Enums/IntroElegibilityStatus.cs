namespace Uno.RevenueCat.Enums;

/// <summary>
/// Whether a user can redeem a trial or introductory offer for a product. Only meaningful on
/// iOS/StoreKit; RevenueCat's Android SDK has no equivalent eligibility check.
/// </summary>
public enum IntroElegibilityStatus
{
    /// <summary>The user has not used a trial or intro offer for this product and can redeem one.</summary>
    Eligible,

    /// <summary>The user already used a trial or intro offer, or otherwise does not qualify.</summary>
    Ineligible,

    /// <summary>The product has no introductory offer configured in App Store Connect.</summary>
    NoIntroOfferExists,

    /// <summary>Eligibility could not be determined, e.g. StoreKit could not reach Apple's servers.</summary>
    Unknown
}
