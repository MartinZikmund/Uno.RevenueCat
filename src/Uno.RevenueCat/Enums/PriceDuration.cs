namespace Uno.RevenueCat.Enums;

/// <summary>
/// Target period a package price is normalized to when displaying a "price per X" figure,
/// e.g. <c>$1.99/month</c> for a yearly subscription.
/// </summary>
public enum PriceDuration
{
    /// <summary>Normalize to a per-day price.</summary>
    Daily,

    /// <summary>Normalize to a per-week price.</summary>
    Weekly,

    /// <summary>Normalize to a per-month price.</summary>
    Monthly,

    /// <summary>Normalize to a per-year price.</summary>
    Yearly
}
