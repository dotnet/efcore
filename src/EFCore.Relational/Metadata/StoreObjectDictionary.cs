// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     Represents a lookup based on <see cref="StoreObjectIdentifier" /> keys.
/// </summary>
/// <typeparam name="T">The values type.</typeparam>
public class StoreObjectDictionary<T> : IReadOnlyStoreObjectDictionary<T>
    where T : class
{
    private readonly Dictionary<StoreObjectIdentifier, T> _dictionary = new();

    /// <inheritdoc />
    public virtual T? Find(in StoreObjectIdentifier storeObject)
        => _dictionary.TryGetValue(storeObject, out var value)
            ? value
            : null;

    /// <inheritdoc />
    public virtual IEnumerable<T> GetValues()
        => _dictionary.OrderBy(pair => pair.Key.Name, StringComparer.Ordinal).Select(pair => pair.Value);

    /// <summary>
    ///     Adds the specified key and value to the dictionary.
    /// </summary>
    /// <param name="storeObject">The store object.</param>
    /// <param name="value">The value to store.</param>
    public virtual void Add(in StoreObjectIdentifier storeObject, T value)
        => _dictionary.Add(storeObject, value);

    /// <summary>
    ///     Removes the value with the specified key from the collection
    ///     and returns it if successful.
    /// </summary>
    /// <param name="storeObject">The key of the element to remove.</param>
    /// <returns>The removed value.</returns>
    public virtual T? Remove(in StoreObjectIdentifier storeObject)
        => _dictionary.Remove(storeObject, out var value)
            ? value
            : null;
}
