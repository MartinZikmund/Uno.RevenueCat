using Types.Result;
using Uno.RevenueCat.Enums;

namespace Uno.RevenueCat.Models;

/// <summary>
/// Outcome of asking the store whether the device is allowed to make payments.
/// </summary>
public class CanMakePaymentsResultDto : DataResult<bool, PurchaseErrorStatus>
{
}
