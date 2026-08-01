namespace Uno.RevenueCat.Models;

//https://github.com/RevenueCat/purchases-ios/blob/main/Sources/Purchasing/PackageType.swift#L66
/// <summary>
/// The <c>$rc_*</c> identifier constants RevenueCat assigns to its standard package durations.
/// A <see cref="PackageDto.Identifier"/> equals one of these when the package maps to a
/// duration RevenueCat recognizes, rather than a custom, dashboard-defined identifier.
/// </summary>
public static class DefaultPackageIdentifier
{
    /// <summary>Identifier for a weekly package.</summary>
    public const string Weekly = "$rc_weekly";

    /// <summary>Identifier for a monthly package.</summary>
    public const string Monthly = "$rc_monthly";

    /// <summary>Identifier for a bi-monthly (every two months) package.</summary>
    public const string BiMonthly = "$rc_two_month";

    /// <summary>Identifier for a quarterly (every three months) package.</summary>
    public const string Quarterly = "$rc_three_month";

    /// <summary>Identifier for a semi-annual (every six months) package.</summary>
    public const string SemiAnnually = "$rc_six_month";

    /// <summary>Identifier for an annual package.</summary>
    public const string Annually = "$rc_annual";

    /// <summary>Identifier for a one-time, non-expiring lifetime package.</summary>
    public const string Lifetime = "$rc_lifetime";
}
