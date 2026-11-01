using Types.Result;
using Uno.RevenueCat.Enums;

namespace Uno.RevenueCat.Models;

/// <summary>
/// Outcome of an operation that resolves the current customer's info - login, logout, restore and customer info reads all share this shape.
/// </summary>
public class CustomerInfoResultDto : DataResult<CustomerInfoDto, PurchaseErrorStatus>
{
}
