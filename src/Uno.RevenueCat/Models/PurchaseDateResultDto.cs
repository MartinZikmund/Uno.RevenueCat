using Types.Result;
using Uno.RevenueCat.Enums;

namespace Uno.RevenueCat.Models;

/// <summary>
/// Outcome of looking up when a product was purchased. A success with a null <see cref="DataResult{TValue, TError}.Value"/> means the product was never purchased, which is distinct from a failure.
/// </summary>
public class PurchaseDateResultDto : DataResult<DateTime?, PurchaseErrorStatus>
{
}
