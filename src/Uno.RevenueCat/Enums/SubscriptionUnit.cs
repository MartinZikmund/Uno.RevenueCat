namespace Uno.RevenueCat.Enums;

/// <summary>
/// Time-unit granularity of a subscription's billing period. Cross-platform equivalent of
/// iOS <c>RCSubscriptionPeriodUnit</c> and Android <c>Period.Unit</c>.
/// </summary>
public enum SubscriptionUnit
{
    /// <summary>The unit could not be determined.</summary>
    Unknown = 0,

    /// <summary>Daily billing period.</summary>
    Day,

    /// <summary>Weekly billing period.</summary>
    Week,

    /// <summary>Monthly billing period.</summary>
    Month,

    /// <summary>Yearly billing period.</summary>
    Year
}
