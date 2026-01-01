using System.Collections;
using System.Text.Json;
using Org.Json;

namespace Uno.RevenueCat.Platforms.Android.Extensions;

internal static class NsDictionaryExtensions
{
    internal static string? ToJson<T, U>(this IDictionary<T, U> dictionary)
    {
        if (dictionary is null || dictionary.Count == 0)
        {
            return null;
        }

        try
        {
            return new JSONObject((IDictionary)dictionary).ToString();
        }
        catch
        {
            // Fallback to JSON serialization
            var dict = new Dictionary<string, object?>();
            foreach (var item in dictionary)
            {
                dict[item.Key?.ToString() ?? string.Empty] = item.Value?.ToString();
            }
            return JsonSerializer.Serialize(dict);
        }
    }
}
