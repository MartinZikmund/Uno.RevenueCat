namespace Uno.RevenueCat.Enums;

/// <summary>
/// Which billing phase an entitlement's current period belongs to.
/// </summary>
public enum PeriodType
{
    /// <summary>Currently in a discounted introductory pricing period.</summary>
    Intro,

    /// <summary>Regular, full-price billing period (not trial, intro, or prepaid).</summary>
    Normal,

    /// <summary>Currently in a free trial period.</summary>
    Trial,

    /// <summary>Google Play prepaid base plan.</summary>
    Prepaid
}
