using System.Collections.Immutable;
using System.Text.Json;

namespace Uno.RevenueCat.Extensions;

internal static class MetadataExtensions
{
    /// <summary>
    /// Parses the raw offering metadata JSON into a typed dictionary. Offering metadata is authored
    /// in the RevenueCat dashboard, so malformed or non-object JSON is treated as "no metadata"
    /// rather than being allowed to throw out of <c>GetOfferingsAsync</c>.
    /// </summary>
    internal static IReadOnlyDictionary<string, JsonElement> ToMetadataDictionary(this string? json)
    {
        if (string.IsNullOrEmpty(json))
        {
            return ImmutableDictionary<string, JsonElement>.Empty;
        }

        try
        {
            using var document = JsonDocument.Parse(json);

            if (document.RootElement.ValueKind != JsonValueKind.Object)
            {
                return ImmutableDictionary<string, JsonElement>.Empty;
            }

            var metadata = new Dictionary<string, JsonElement>();

            foreach (var property in document.RootElement.EnumerateObject())
            {
                // Indexer, not ToDictionary: a duplicate key would make ToDictionary throw.
                // Clone, because elements do not outlive the JsonDocument they came from.
                metadata[property.Name] = property.Value.Clone();
            }

            return metadata;
        }
        catch (Exception exception) when (exception is JsonException or ArgumentException)
        {
            return ImmutableDictionary<string, JsonElement>.Empty;
        }
    }
}
