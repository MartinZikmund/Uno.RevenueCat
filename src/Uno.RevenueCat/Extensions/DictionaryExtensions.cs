namespace Uno.RevenueCat.Extensions;

internal static class DictionaryExtensions
{
    internal static bool IsNullOrEmpty<TKey, TValue>(this IDictionary<TKey, TValue> dictionary) =>
        dictionary is null || dictionary.Count == 0;
}
