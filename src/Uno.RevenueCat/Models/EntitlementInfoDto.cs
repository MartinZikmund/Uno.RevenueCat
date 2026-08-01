using Uno.RevenueCat.Enums;

namespace Uno.RevenueCat.Models;

/// <summary>
/// The unlocked access ("entitlement") a purchase grants, and the billing state behind it.
/// </summary>
public sealed record EntitlementInfoDto
{
    /// <summary>When RevenueCat first detected a billing problem for this entitlement, or <c>null</c> if there isn't one.</summary>
    public required DateTime? BillingIssueDetectedAt { get; init; }

    /// <summary>When the entitlement's current period expires, or <c>null</c> for lifetime/non-expiring access.</summary>
    public required DateTime? ExpirationDate { get; init; }

    /// <summary>The entitlement identifier configured in the RevenueCat dashboard.</summary>
    public required string Identifier { get; init; }

    /// <summary>Whether this entitlement currently grants access.</summary>
    public required bool IsActive { get; init; }

    /// <summary>Whether the underlying purchase was made in a sandbox/test environment.</summary>
    public required bool IsSandbox { get; init; }

    /// <summary>When the most recent purchase or renewal for this entitlement occurred.</summary>
    public required DateTime? LatestPurchaseDate { get; init; }

    /// <summary>When the entitlement was first purchased, before any renewals.</summary>
    public required DateTime? OriginalPurchaseDate { get; init; }

    /// <summary>How the user came to hold this entitlement (purchased directly or family-shared).</summary>
    public required OwnershipType OwnershipType { get; init; }

    /// <summary>Which billing phase (trial, intro, normal, prepaid) the entitlement is currently in.</summary>
    public required PeriodType PeriodType { get; init; }

    /// <summary>The store product identifier backing this entitlement.</summary>
    public required string ProductIdentifier { get; init; }

    /// <summary>The specific subscription plan or base-plan identifier (e.g. Google Play base plan) backing this entitlement.</summary>
    public required string ProductPlanIdentifier { get; init; }

    /// <summary>Which store the underlying purchase came from.</summary>
    public required StoreType Store { get; init; }

    /// <summary>When the user turned off auto-renew, or <c>null</c> if they haven't.</summary>
    public required DateTime? UnsubscribeDetectedAt { get; init; }

    /// <summary>Whether the subscription is set to renew at the end of the current period.</summary>
    public required bool WillRenew { get; init; }
}
