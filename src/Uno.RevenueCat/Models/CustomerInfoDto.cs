namespace Uno.RevenueCat.Models;

/// <summary>
/// Snapshot of a subscriber's purchase history and entitlements as known to RevenueCat.
/// </summary>
public sealed record CustomerInfoDto
{
    /// <summary>Product identifiers of subscriptions currently active for this user.</summary>
    public required List<string> ActiveSubscriptions { get; init; }

    /// <summary>Product identifiers of everything this user has ever purchased, active or not.</summary>
    public required List<string> AllPurchasedIdentifiers { get; init; }

    /// <summary>Product identifiers of one-time (non-consumable) purchases this user owns.</summary>
    public required List<string> NonConsumablePurchases { get; init; }

    /// <summary>When RevenueCat first saw this user.</summary>
    public required DateTime? FirstSeen { get; init; }

    /// <summary>The latest expiration date across all of this user's subscriptions, or <c>null</c> if none.</summary>
    public required DateTime? LatestExpirationDate { get; init; }

    /// <summary>Deep link to the store's subscription management page for this user, when available.</summary>
    public required string? ManagementURL { get; init; }

    /// <summary>The entitlements (unlocked access) currently known for this user.</summary>
    public required List<EntitlementInfoDto> Entitlements { get; init; }
}
