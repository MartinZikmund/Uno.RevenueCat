using Uno.RevenueCat.Models;

namespace Uno.RevenueCat.Extensions;

/// <summary>
/// Query helpers for collections of <see cref="OfferingDto"/>.
/// </summary>
public static partial class OfferingDtoExtensions
{
    /// <summary>
    /// Returns the offering marked as current, or <c>null</c> if there is none.
    /// </summary>
    public static OfferingDto? GetCurrent(this IReadOnlyList<OfferingDto> offerings) =>
        offerings.FirstOrDefault(x => x.IsCurrent);
}
