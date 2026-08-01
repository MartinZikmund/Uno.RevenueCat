using System.Collections;
using Org.Json;
using Uno.RevenueCat.Extensions;

namespace Uno.RevenueCat.Platforms.Android.Extensions;

internal static class DictionaryExtensions
{
    // No stringifying fallback: JSONObject preserves value types, and coercing everything to a
    // string would silently corrupt numbers, booleans and nested objects in offering metadata.
    internal static string? ToJson<T, U>(this IDictionary<T, U> dictionary) =>
        dictionary.IsNullOrEmpty()
            ? null
            : new JSONObject((IDictionary)dictionary).ToString();
}
