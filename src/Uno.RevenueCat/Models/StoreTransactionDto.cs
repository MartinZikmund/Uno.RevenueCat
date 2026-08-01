namespace Uno.RevenueCat.Models;

/// <summary>
/// A single completed store transaction.
/// </summary>
public sealed record StoreTransactionDto
{
    /// <summary>The store product identifier that was purchased.</summary>
    public required string ProductIdentifier { get; init; }

    /// <summary>When the transaction was completed.</summary>
    public required DateTime PurchaseDate { get; init; }

    /// <summary>The store's unique identifier for this transaction.</summary>
    public required string TransactionIdentifier { get; init; }

    /// <summary>Number of units purchased in this transaction; usually <c>1</c>, but can be greater for consumables.</summary>
    public required long Quantity { get; init; }
}
