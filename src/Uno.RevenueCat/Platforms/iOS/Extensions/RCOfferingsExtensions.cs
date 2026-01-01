using Maui.RevenueCat.iOS;
using Uno.RevenueCat.Models;

namespace Uno.RevenueCat.Platforms.iOS.Extensions;

internal static class RCOfferingsExtensions
{
    internal static List<OfferingDto> ToOfferingDtoList(this RCOfferings offerings)
    {
        var offeringDtos = new List<OfferingDto>();

        foreach (var offer in offerings.All.Values)
        {
            var offeringDto = new OfferingDto()
            {
                Identifier = offer.Identifier,
                AvailablePackages = offer.AvailablePackages.ToPackageDtoList(),
                IsCurrent = offer.Identifier == offerings.Current?.Identifier,
                Metadata = offer.Metadata.ToJson()
            };
            offeringDtos.Add(offeringDto);
        }

        return offeringDtos;
    }
}
