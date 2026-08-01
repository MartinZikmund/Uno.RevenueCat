using Foundation;

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

    // Must go through NSJsonSerialization: stringifying each value via ToString() turns numbers
    // into "5", booleans into "1", and nested containers into Objective-C description text.
    internal static string? ToJson(this NSDictionary<NSString, NSObject>? dictionary)
    {
        if (dictionary is null || dictionary.Count == 0)
        {
            return null;
        }

        var jsonData = NSJsonSerialization.Serialize(dictionary, 0, out var error);

        return error is null
            ? NSString.FromData(jsonData, NSStringEncoding.UTF8)
            : null;
    }
}
