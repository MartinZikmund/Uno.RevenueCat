namespace Uno.RevenueCat.Models;

/// <summary>
/// A package: a purchasable duration (e.g. weekly, monthly, annual) within an offering,
/// wrapping the underlying store product a user actually buys.
/// </summary>
public sealed record PackageDto
{
    /// <summary>Identifier of the <see cref="OfferingDto"/> this package belongs to.</summary>
    public string OfferingIdentifier { get; init; } = string.Empty;

    /// <summary>
    /// The package's identifier, e.g. one of the <see cref="DefaultPackageIdentifier"/>
    /// constants, or a custom identifier defined in the dashboard.
    /// </summary>
    public string Identifier { get; init; } = string.Empty;

    /// <summary>The store product this package maps to.</summary>
    public ProductDto Product { get; init; } = new();
}
