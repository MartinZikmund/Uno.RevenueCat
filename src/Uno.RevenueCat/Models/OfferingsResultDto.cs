using Types.Result;
using Uno.RevenueCat.Enums;

namespace Uno.RevenueCat.Models;

/// <summary>
/// Outcome of fetching the configured offerings and their packages.
/// </summary>
public class OfferingsResultDto : DataResult<IReadOnlyList<OfferingDto>, PurchaseErrorStatus>
{
}
