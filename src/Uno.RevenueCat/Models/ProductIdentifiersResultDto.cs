using Types.Result;
using Uno.RevenueCat.Enums;

namespace Uno.RevenueCat.Models;

/// <summary>
/// Outcome of an operation that returns product identifiers - active subscriptions and all purchased identifiers share this shape.
/// </summary>
public class ProductIdentifiersResultDto : DataResult<IReadOnlyList<string>, PurchaseErrorStatus>
{
}
