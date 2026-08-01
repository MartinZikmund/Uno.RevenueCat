using System.Collections.Concurrent;
using System.Globalization;
using Uno.RevenueCat.Enums;
using Uno.RevenueCat.Models;

namespace Uno.RevenueCat.Extensions;

/// <summary>
/// Display helpers for <see cref="PackageDto"/>: normalizes a package's price to any period and
/// formats it as a localized currency string.
/// </summary>
public static partial class PackageDtoExtensions
{
    private static readonly decimal _daysInWeek = 7m;
    private static readonly decimal _daysInMonth = 30m;
    private static readonly decimal _monthsInBiMonthly = 2m;
    private static readonly decimal _monthsInQuartal = 3m;
    private static readonly decimal _monthsInHalfYear = 6m;
    private static readonly decimal _monthsInYear = 12m;

    // GetLocalizedPrice runs once per package and the culture scan is O(all specific cultures),
    // so the resolved culture is cached per ISO currency code.
    private static readonly ConcurrentDictionary<string, CultureInfo?> _currencyCultureCache =
        new(StringComparer.Ordinal);

    /// <summary>
    /// Returns the package's price normalized to <paramref name="duration"/>.
    /// </summary>
    /// <param name="packageDto">The package.</param>
    /// <param name="duration">Period to normalize the price to.</param>
    /// <param name="ignoreExceptions">Return <c>0</c> instead of throwing for unsupported package identifiers.</param>
    /// <param name="decimalRoundUpTo">Decimal places to round up to, or <c>null</c> to skip rounding.</param>
    public static decimal GetPriceFor(
        this PackageDto packageDto,
        PriceDuration duration,
        bool ignoreExceptions = true,
        int? decimalRoundUpTo = 2)
    {
        var monthlyPrice = NormalizeToMonthly(packageDto, ignoreExceptions);

        var result = duration switch
        {
            PriceDuration.Daily => monthlyPrice / _daysInMonth,
            PriceDuration.Weekly => monthlyPrice / _daysInMonth * _daysInWeek,
            PriceDuration.Monthly => monthlyPrice,
            PriceDuration.Yearly => monthlyPrice * _monthsInYear,
            _ => throw new ArgumentOutOfRangeException(nameof(duration), duration, "Unknown price duration."),
        };

        if (decimalRoundUpTo is null)
        {
            return result;
        }

        // Normalizing through months leaves a ~1e-28 decimal-division residue, and RoundUp is a
        // ceiling: without collapsing the residue first, a $1.99 weekly package would round up to
        // $2.00. Rounding to 10 places kills the noise while leaving genuine fractions intact.
        return decimal.Round(result, 10).RoundUp(decimalRoundUpTo.Value);
    }

    /// <summary>
    /// As <see cref="GetPriceFor"/>, but formatted as a localized currency string.
    /// Returns <c>"$0.00"</c> on failure when <paramref name="ignoreExceptions"/> is <c>true</c>.
    /// </summary>
    public static string GetPriceWithCurrencyFor(
        this PackageDto packageDto,
        PriceDuration duration,
        bool ignoreExceptions = true,
        int? decimalRoundUpTo = 2)
    {
        try
        {
            var price = packageDto.GetPriceFor(duration, ignoreExceptions, decimalRoundUpTo);

            return GetLocalizedPrice(packageDto.Product.Pricing.CurrencyCode, price);
        }
        catch (Exception)
        {
            if (ignoreExceptions)
            {
                return "$0.00";
            }

            throw;
        }
    }

    /// <summary>
    /// Formats <paramref name="price"/> as a localized currency string. Number conventions
    /// (separators, grouping) come from <see cref="CultureInfo.CurrentCulture"/>; the currency symbol
    /// and decimal-digit count come from the currency itself. Whole prices drop the fractional part
    /// (<c>199 Kč</c>, not <c>199,00 Kč</c>). Falls back to the ISO code as the symbol when the
    /// currency is unknown — which is also the case under <c>InvariantGlobalization</c>.
    /// </summary>
    public static string GetLocalizedPrice(string priceIsoCurrencyCode, decimal price)
    {
        var format = (NumberFormatInfo)CultureInfo.CurrentCulture.NumberFormat.Clone();
        var currencyCulture = _currencyCultureCache.GetOrAdd(priceIsoCurrencyCode, FindCurrencyCulture);

        if (currencyCulture is not null)
        {
            var currencyFormat = currencyCulture.NumberFormat;

            format.CurrencyDecimalDigits = currencyFormat.CurrencyDecimalDigits;
            format.CurrencySymbol = currencyFormat.CurrencySymbol;

            // The symbol's placement must come from the currency too. Keeping CurrentCulture's
            // pattern while swapping in a foreign symbol yields "Kč199" instead of "199 Kč".
            format.CurrencyPositivePattern = currencyFormat.CurrencyPositivePattern;
            format.CurrencyNegativePattern = currencyFormat.CurrencyNegativePattern;
        }
        else
        {
            format.CurrencySymbol = priceIsoCurrencyCode;
        }

        return price == Math.Floor(price)
            ? price.ToString("C0", format)
            : price.ToString("C", format);
    }

    // Single source of truth for every per-period conversion.
    private static decimal NormalizeToMonthly(PackageDto packageDto, bool ignoreExceptions) =>
        packageDto.Identifier switch
        {
            DefaultPackageIdentifier.Weekly => packageDto.Product.Pricing.Price / _daysInWeek * _daysInMonth,
            DefaultPackageIdentifier.Monthly => packageDto.Product.Pricing.Price,
            DefaultPackageIdentifier.BiMonthly => packageDto.Product.Pricing.Price / _monthsInBiMonthly,
            DefaultPackageIdentifier.Quarterly => packageDto.Product.Pricing.Price / _monthsInQuartal,
            DefaultPackageIdentifier.SemiAnnually => packageDto.Product.Pricing.Price / _monthsInHalfYear,
            DefaultPackageIdentifier.Annually => packageDto.Product.Pricing.Price / _monthsInYear,
            _ => ignoreExceptions
                ? 0m
                : throw new NotImplementedException("Specified offering identifier is not supported."),
        };

    // Ordinal sort makes the result deterministic across devices and ICU versions; a culture whose
    // name RegionInfo rejects is skipped rather than taking out the whole scan.
    private static CultureInfo? FindCurrencyCulture(string isoCurrencyCode) =>
        CultureInfo.GetCultures(CultureTypes.SpecificCultures)
            .OrderBy(static culture => culture.Name, StringComparer.Ordinal)
            .FirstOrDefault(culture =>
            {
                try
                {
                    return new RegionInfo(culture.Name).ISOCurrencySymbol == isoCurrencyCode;
                }
                catch
                {
                    return false;
                }
            });
}
