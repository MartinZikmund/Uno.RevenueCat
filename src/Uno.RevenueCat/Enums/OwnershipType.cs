namespace Uno.RevenueCat.Enums;

/// <summary>
/// How the current app user came to hold an entitlement: bought it directly, or received it
/// through a family sharing plan.
/// </summary>
public enum OwnershipType
{
    /// <summary>Access was granted through Family Sharing (Apple) or a shared subscription (Google).</summary>
    FamilyShared,

    /// <summary>The current app user purchased the entitlement themselves.</summary>
    Purchased,

    /// <summary>Ownership could not be determined.</summary>
    Unknown,
}
