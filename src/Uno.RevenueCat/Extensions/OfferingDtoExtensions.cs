using Uno.RevenueCat.Models;

namespace Uno.RevenueCat.Extensions;

public static partial class OfferingDtoExtensions
{
    public static OfferingDto? GetCurrent(this List<OfferingDto> offerings)
    {
        return offerings.FirstOrDefault(x => x.IsCurrent);
    }
}
