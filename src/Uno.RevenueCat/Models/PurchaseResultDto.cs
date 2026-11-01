using Types.Result;
using Uno.RevenueCat.Enums;

namespace Uno.RevenueCat.Models;

/// <summary>
/// Outcome of a purchase: the refreshed customer info in <see cref="DataResult{TValue, TError}.Value"/>,
/// plus the <see cref="Transaction"/> that only a purchase produces.
/// </summary>
/// <remarks>
/// A sibling of <see cref="CustomerInfoResultDto"/> rather than a subclass - a purchase is not a kind
/// of customer-info read. A pending purchase is an error (<see cref="PurchaseErrorStatus.PaymentPendingError"/>)
/// that still carries the transaction and customer info.
/// </remarks>
public class PurchaseResultDto : DataResult<CustomerInfoDto, PurchaseErrorStatus>
{
    /// <summary>The store transaction the purchase produced, when the store returned one.</summary>
    public StoreTransactionDto? Transaction { get; init; }
}
