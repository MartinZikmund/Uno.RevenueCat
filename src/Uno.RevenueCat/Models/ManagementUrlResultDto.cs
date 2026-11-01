using Types.Result;
using Uno.RevenueCat.Enums;

namespace Uno.RevenueCat.Models;

/// <summary>
/// Outcome of resolving the store's subscription-management URL. A success with a null <see cref="DataResult{TValue, TError}.Value"/> means the user has no store-managed subscription, which is distinct from a failure.
/// </summary>
public class ManagementUrlResultDto : DataResult<string, PurchaseErrorStatus>
{
}
