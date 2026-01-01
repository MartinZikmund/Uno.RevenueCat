using Foundation;
using System.Text.Json;

namespace Uno.RevenueCat.Platforms.iOS.Extensions;

internal static class NsDictionaryExtensions
{
    internal static NSDictionary<NSString, NSString> ToNSDictionary(this IReadOnlyDictionary<string, string> dictionary)
    {
        if (dictionary is null || !dictionary.Any())
        {
            return new NSDictionary<NSString, NSString>();
        }

        var nsDictionary = new NSMutableDictionary<NSString, NSString>();
        foreach (var kvp in dictionary)
        {
            nsDictionary.Add(new NSString(kvp.Key), new NSString(kvp.Value));
        }

        return NSDictionary<NSString, NSString>.FromObjectsAndKeys(
            nsDictionary.Values.ToArray(),
            nsDictionary.Keys.ToArray(),
            (nint)nsDictionary.Count
        );
    }

    internal static string? ToJson(this NSDictionary<NSString, NSObject>? dictionary)
    {
        if (dictionary is null || dictionary.Count == 0)
        {
            return null;
        }

        var dict = new Dictionary<string, object?>();
        for (nuint i = 0; i < dictionary.Count; i++)
        {
            var key = dictionary.Keys[i];
            var value = dictionary.Values[i];
            dict[key.ToString()] = value?.ToString();
        }

        return JsonSerializer.Serialize(dict);
    }
}
