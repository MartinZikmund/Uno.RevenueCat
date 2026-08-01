using System.Collections.Immutable;
using System.Text.Json;

namespace Uno.RevenueCat.Models;

/// <summary>
/// An offering: a set of packages configured in the RevenueCat dashboard and presented
/// together on a paywall.
/// </summary>
public sealed record OfferingDto
{
    /// <summary>The offering's identifier, as configured in the RevenueCat dashboard.</summary>
    public string Identifier { get; init; } = string.Empty;

    /// <summary>The packages presented on the paywall for this offering.</summary>
    public List<PackageDto> AvailablePackages { get; init; } = new();

    /// <summary>Whether this is the offering RevenueCat currently designates as the default one.</summary>
    public bool IsCurrent { get; init; }

    /// <summary>
    /// Offering metadata configured in the RevenueCat dashboard. Empty when none is set.
    /// </summary>
    public IReadOnlyDictionary<string, JsonElement> Metadata { get; init; } =
        ImmutableDictionary<string, JsonElement>.Empty;
}
