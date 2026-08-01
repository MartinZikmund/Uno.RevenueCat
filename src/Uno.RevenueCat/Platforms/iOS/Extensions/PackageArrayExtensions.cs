using Maui.RevenueCat.iOS;
using Uno.RevenueCat.Extensions;
using Uno.RevenueCat.Models;

namespace Uno.RevenueCat.Platforms.iOS.Extensions;

internal static class PackageArrayExtensions
{
    internal static List<PackageDto> ToPackageDtoList(this RCPackage[] packages)
    {
        var packageDtos = new List<PackageDto>();

        foreach (var package in packages)
        {
            var currencyCode = package.StoreProduct.CurrencyCode ?? string.Empty;
            var price = Convert.ToDecimal(package.StoreProduct.Price.DoubleValue);

            var packageDto = new PackageDto()
            {
                OfferingIdentifier = package.OfferingIdentifier,
                Identifier = package.Identifier,
                Product = new ProductDto()
                {
                    Pricing = new PricingDto
                    {
                        CurrencyCode = currencyCode,
                        Price = price,
                        // Round rather than truncate: a binary double often lands just below the
                        // intended integer (2.01 * 1e6 == 2009999.9999999998), and Android reports
                        // the canonical AmountMicros for the same product.
                        PriceMicros = (long)decimal.Round(price * 1_000_000m, MidpointRounding.AwayFromZero),
                        PriceLocalized = PackageDtoExtensions.GetLocalizedPrice(currencyCode, price)
                    },
                    Sku = package.StoreProduct.ProductIdentifier,
                    SubscriptionPeriod = package.StoreProduct.SubscriptionPeriod.ToSubscriptionPeriodDto(),
                }
            };

            packageDtos.Add(packageDto);
        }

        return packageDtos;
    }
}
