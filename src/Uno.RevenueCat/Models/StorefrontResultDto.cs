using Types.Result;
using Uno.RevenueCat.Enums;

namespace Uno.RevenueCat.Models;

/// <summary>
/// Outcome of resolving the user's store storefront country code (ISO 3166-1 alpha-2). An empty string means the platform has not observed the storefront yet.
/// </summary>
public class StorefrontResultDto : DataResult<string, PurchaseErrorStatus>
{
}
