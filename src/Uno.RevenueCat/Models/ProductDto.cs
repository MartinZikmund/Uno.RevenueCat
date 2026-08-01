namespace Uno.RevenueCat.Models;

/// <summary>
/// The store product (SKU) that a <see cref="PackageDto"/> maps to.
/// </summary>
public sealed record ProductDto
{
    /// <summary>The product's price.</summary>
    public PricingDto Pricing { get; init; } = new();

    /// <summary>The store's product identifier for this product.</summary>
    public string Sku { get; init; } = string.Empty;

    /// <summary>
    /// Billing period of a subscription product, or <c>null</c> for non-subscription
    /// products (consumables, lifetime).
    /// </summary>
    public SubscriptionPeriodDto? SubscriptionPeriod { get; init; }
}
