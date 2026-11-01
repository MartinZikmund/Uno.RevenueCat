using System.Globalization;

namespace Uno.RevenueCat.Extensions;

internal static class CountryCodeExtensions
{
    private static readonly Lazy<Dictionary<string, string>> _alpha2ByAlpha3 = new(BuildAlpha2ByAlpha3Map);

    /// <summary>
    /// Normalizes an ISO 3166-1 alpha-3 country code (e.g. "USA", as StoreKit reports it) to
    /// alpha-2 ("US"). Codes that are already alpha-2, unknown, or empty are returned unchanged.
    /// </summary>
    internal static string ToIsoAlpha2CountryCode(this string countryCode)
    {
        if (countryCode.Length != 3)
        {
            return countryCode;
        }

        return _alpha2ByAlpha3.Value.TryGetValue(countryCode, out var alpha2CountryCode)
            ? alpha2CountryCode
            : countryCode;
    }

    private static Dictionary<string, string> BuildAlpha2ByAlpha3Map()
    {
        Dictionary<string, string> alpha2ByAlpha3 = new(StringComparer.OrdinalIgnoreCase);
        foreach (var specificCulture in CultureInfo.GetCultures(CultureTypes.SpecificCultures))
        {
            try
            {
                RegionInfo regionInfo = new(specificCulture.Name);
                alpha2ByAlpha3[regionInfo.ThreeLetterISORegionName] = regionInfo.TwoLetterISORegionName;
            }
            catch (ArgumentException)
            {
                // Neutral or synthetic cultures have no resolvable region.
            }
        }

        return alpha2ByAlpha3;
    }
}
