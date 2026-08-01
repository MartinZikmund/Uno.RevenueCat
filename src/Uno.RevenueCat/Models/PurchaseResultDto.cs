using Uno.RevenueCat.Enums;

namespace Uno.RevenueCat.Models;

/// <summary>
/// Outcome of a <c>PurchaseProductAsync</c> attempt.
/// </summary>
public sealed record PurchaseResultDto
{
    /// <summary>Whether the purchase completed successfully.</summary>
    public bool IsSuccess { get; set; }

    /// <summary>Whether the purchase failed. <c>true</c> whenever <see cref="ErrorStatus"/> is set.</summary>
    public bool IsError => !(ErrorStatus is null);

    /// <summary>The reason the purchase failed, or <c>null</c> on success.</summary>
    public PurchaseErrorStatus? ErrorStatus { get; set; }

    /// <summary>The store transaction recorded for a successful purchase, or <c>null</c> on failure.</summary>
    public StoreTransactionDto? Transaction { get; set; }

    /// <summary>The subscriber's updated customer info after the purchase, or <c>null</c> on failure.</summary>
    public CustomerInfoDto? CustomerInfo { get; set; }
}
