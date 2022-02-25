using System.Collections.Generic;

namespace Ray1Editor;

/// <summary>
/// Extension methods for <see cref="Dictionary{TKey,TValue}"/>
/// </summary>
public static class DictionaryExtensions
{
    /// <summary>
    /// Tries to get a value from the specified dictionary using the specified key.
    /// Returns the default value if the value can not be found.
    /// </summary>
    /// <typeparam name="TKey">The key type</typeparam>
    /// <typeparam name="TValue">The value type</typeparam>
    /// <param name="dictionary">The dictionary</param>
    /// <param name="key">The key</param>
    /// <returns>The value, or the default value if not found</returns>
    public static TValue TryGetValue<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
    {
        return dictionary.TryGetValue(key, out TValue output) ? output : default;
    }
}