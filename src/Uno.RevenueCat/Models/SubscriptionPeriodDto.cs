using Uno.RevenueCat.Enums;

namespace Uno.RevenueCat.Models;

/// <summary>
/// Cross-platform billing period of a subscription product. A yearly plan is
/// <c>Value = 1, Unit = Year</c>.
/// </summary>
public sealed record SubscriptionPeriodDto
{
    /// <summary>The numeric length of the period, e.g. <c>1</c>.</summary>
    public int Value { get; init; }

    /// <summary>The time unit <see cref="Value"/> is expressed in.</summary>
    public SubscriptionUnit Unit { get; init; } = SubscriptionUnit.Unknown;
}
