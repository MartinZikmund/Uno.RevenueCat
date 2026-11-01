using Types.Result;
using Uno.RevenueCat.Enums;

namespace Uno.RevenueCat.Models;

/// <summary>
/// Outcome of a trial / introductory discount eligibility check, keyed by product identifier.
/// </summary>
public class IntroEligibilityResultDto : DataResult<IReadOnlyDictionary<string, IntroElegibilityStatus>, PurchaseErrorStatus>
{
}
