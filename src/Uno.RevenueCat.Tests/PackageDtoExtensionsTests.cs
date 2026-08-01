using System.Globalization;
using Uno.RevenueCat.Enums;
using Uno.RevenueCat.Extensions;
using Uno.RevenueCat.Models;

namespace Uno.RevenueCat.Tests;

[TestClass]
public sealed class PackageDtoExtensionsTests
{
    private static PackageDto Package(string identifier, decimal price, string currency = "USD") =>
        new()
        {
            Identifier = identifier,
            Product = new ProductDto
            {
                Pricing = new PricingDto { Price = price, CurrencyCode = currency },
            },
        };

    // A 12.00 annual plan is 1.00/month, 0.24/week (rounded up to 2 decimals), 0.04/day.
    [TestMethod]
    [DataRow(DefaultPackageIdentifier.Annually, 12.00, PriceDuration.Monthly, 1.00)]
    [DataRow(DefaultPackageIdentifier.Annually, 12.00, PriceDuration.Yearly, 12.00)]
    [DataRow(DefaultPackageIdentifier.Monthly, 5.00, PriceDuration.Monthly, 5.00)]
    [DataRow(DefaultPackageIdentifier.Monthly, 30.00, PriceDuration.Daily, 1.00)]
    [DataRow(DefaultPackageIdentifier.Monthly, 30.00, PriceDuration.Weekly, 7.00)]
    [DataRow(DefaultPackageIdentifier.Quarterly, 30.00, PriceDuration.Monthly, 10.00)]
    [DataRow(DefaultPackageIdentifier.SemiAnnually, 60.00, PriceDuration.Monthly, 10.00)]
    [DataRow(DefaultPackageIdentifier.BiMonthly, 20.00, PriceDuration.Monthly, 10.00)]
    [DataRow(DefaultPackageIdentifier.Weekly, 7.00, PriceDuration.Monthly, 30.00)]
    public void GetPriceFor_NormalizesAcrossDurations(
        string identifier, double price, PriceDuration duration, double expected) =>
        Assert.AreEqual((decimal)expected, Package(identifier, (decimal)price).GetPriceFor(duration));

    [TestMethod]
    public void GetPriceFor_RoundsUpToTwoDecimalsByDefault()
    {
        // 10.00 annual => 0.8333.../month => rounds up to 0.84
        Assert.AreEqual(0.84m, Package(DefaultPackageIdentifier.Annually, 10.00m).GetPriceFor(PriceDuration.Monthly));
    }

    [TestMethod]
    public void GetPriceFor_SkipsRounding_WhenDecimalRoundUpToIsNull()
    {
        var actual = Package(DefaultPackageIdentifier.Annually, 10.00m)
            .GetPriceFor(PriceDuration.Monthly, decimalRoundUpTo: null);

        Assert.AreNotEqual(0.84m, actual);
        Assert.IsTrue(actual > 0.83m && actual < 0.84m, $"expected an unrounded value, got {actual}");
    }

    [TestMethod]
    public void GetPriceFor_ReturnsZero_ForUnknownIdentifier_WhenIgnoringExceptions() =>
        Assert.AreEqual(0m, Package("$rc_lifetime", 99m).GetPriceFor(PriceDuration.Monthly));

    [TestMethod]
    public void GetPriceFor_Throws_ForUnknownIdentifier_WhenNotIgnoringExceptions() =>
        Assert.ThrowsExactly<NotImplementedException>(() =>
            Package("$rc_lifetime", 99m).GetPriceFor(PriceDuration.Monthly, ignoreExceptions: false));

    [TestMethod]
    public void GetPriceWithCurrencyFor_FormatsZero_ForUnknownIdentifier_WhenIgnoringExceptions()
    {
        using var _ = new CultureScope("en-US");

        // GetPriceFor swallows the unsupported identifier and yields 0, so this formats a zero
        // price rather than hitting the "$0.00" catch-block fallback.
        Assert.AreEqual("$0", Package("$rc_lifetime", 99m).GetPriceWithCurrencyFor(PriceDuration.Monthly));
    }

    [TestMethod]
    public void GetPriceWithCurrencyFor_Throws_ForUnknownIdentifier_WhenNotIgnoringExceptions() =>
        Assert.ThrowsExactly<NotImplementedException>(() =>
            Package("$rc_lifetime", 99m).GetPriceWithCurrencyFor(PriceDuration.Monthly, ignoreExceptions: false));

    [TestMethod]
    public void GetLocalizedPrice_IsDeterministic_AcrossRepeatedCalls()
    {
        // The culture scan must not depend on ICU enumeration order.
        var first = PackageDtoExtensions.GetLocalizedPrice("USD", 9.99m);

        for (var i = 0; i < 50; i++)
        {
            Assert.AreEqual(first, PackageDtoExtensions.GetLocalizedPrice("USD", 9.99m));
        }
    }

    [TestMethod]
    [DataRow("en-US")]
    [DataRow("cs-CZ")]
    [DataRow("de-DE")]
    public void GetLocalizedPrice_DropsFractionalPart_ForWholePrices(string culture)
    {
        using var _ = new CultureScope(culture);

        var localized = PackageDtoExtensions.GetLocalizedPrice("CZK", 199m);

        // "199 Kč", never "199,00 Kč".
        StringAssert.Contains(localized, "199");
        Assert.IsFalse(localized.Contains("199.00") || localized.Contains("199,00"),
            $"whole price should not carry decimals, got '{localized}'");
    }

    [TestMethod]
    public void GetLocalizedPrice_KeepsFractionalPart_ForNonWholePrices()
    {
        using var _ = new CultureScope("en-US");

        StringAssert.Contains(PackageDtoExtensions.GetLocalizedPrice("USD", 9.99m), "9.99");
    }

    [TestMethod]
    public void GetLocalizedPrice_UsesCurrentCultureNumberConventions()
    {
        // Separators follow CurrentCulture; only the symbol/decimal-digits come from the currency.
        using var _ = new CultureScope("cs-CZ");

        var localized = PackageDtoExtensions.GetLocalizedPrice("USD", 1234.56m);

        StringAssert.Contains(localized, ",");
    }

    [TestMethod]
    [DataRow("en-US", "CZK", 199, "199 Kč")]
    [DataRow("en-US", "SEK", 120, "120 kr")]
    [DataRow("en-US", "USD", 25, "$25")]
    [DataRow("cs-CZ", "USD", 25, "$25")]
    [DataRow("de-DE", "USD", 25, "$25")]
    public void GetLocalizedPrice_PlacesSymbolUsingTheCurrencysOwnPattern(
        string culture, string iso, int price, string expected)
    {
        // The symbol must be placed by the CURRENCY's pattern, not CurrentCulture's. Taking only the
        // symbol from the currency while keeping the current culture's pattern yields "Kc199".
        using var _ = new CultureScope(culture);

        Assert.AreEqual(expected, PackageDtoExtensions.GetLocalizedPrice(iso, price));
    }

    [TestMethod]
    [DataRow(1.99)]
    [DataRow(4.99)]
    [DataRow(2.99)]
    [DataRow(0.99)]
    public void GetPriceFor_WeeklyPackage_RoundTripsToItsOwnPrice(double price)
    {
        // Normalizing a weekly package to a weekly price goes price -> monthly -> weekly. Decimal
        // division leaves a ~1e-28 residue, and RoundUp (Math.Ceiling) would turn that into a whole
        // extra cent: $1.99 would be reported as $2.00.
        var actual = Package(DefaultPackageIdentifier.Weekly, (decimal)price)
            .GetPriceFor(PriceDuration.Weekly);

        Assert.AreEqual((decimal)price, actual);
    }

    [TestMethod]
    public void GetPriceFor_StillRoundsUpGenuineFractions()
    {
        // Killing the FP residue must not defeat the intended round-up: 10.00/12 = 0.8333... -> 0.84
        Assert.AreEqual(0.84m, Package(DefaultPackageIdentifier.Annually, 10.00m).GetPriceFor(PriceDuration.Monthly));
    }

    [TestMethod]
    public void GetLocalizedPrice_FallsBackToIsoCode_ForUnknownCurrency()
    {
        using var _ = new CultureScope("en-US");

        StringAssert.Contains(PackageDtoExtensions.GetLocalizedPrice("XYZ", 1.50m), "XYZ");
    }

    [TestMethod]
    public void GetLocalizedPrice_DoesNotThrow_ForAnyKnownCurrency()
    {
        foreach (var code in new[] { "USD", "EUR", "CZK", "GBP", "JPY", "INR", "BRL", "XYZ", "" })
        {
            _ = PackageDtoExtensions.GetLocalizedPrice(code, 12.34m);
        }
    }

    private sealed class CultureScope : IDisposable
    {
        private readonly CultureInfo _original = CultureInfo.CurrentCulture;

        internal CultureScope(string name) => CultureInfo.CurrentCulture = new CultureInfo(name);

        public void Dispose() => CultureInfo.CurrentCulture = _original;
    }
}
