namespace Uno.RevenueCat.Models;

/// <summary>
/// Store pricing for a product, in both raw and display-ready forms.
/// </summary>
public sealed record PricingDto
{
    /// <summary>ISO 4217 currency code of <see cref="Price"/>, e.g. <c>"USD"</c>.</summary>
    public string CurrencyCode { get; init; } = string.Empty;

    /// <summary>Price in the currency's major unit (e.g. dollars), as reported by the store.</summary>
    public decimal Price { get; init; }

    /// <summary>Price in millionths of the currency's major unit — the store's canonical integer form (e.g. $1.00 is 1_000_000).</summary>
    public long PriceMicros { get; init; }

    /// <summary>
    /// Display string produced by <see cref="Uno.RevenueCat.Extensions.PackageDtoExtensions.GetLocalizedPrice"/>
    /// from <see cref="Price"/> and <see cref="CurrencyCode"/> using the current culture — not
    /// the localized string reported by the store itself.
    /// </summary>
    public string PriceLocalized { get; init; } = string.Empty;
}
