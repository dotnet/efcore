// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     Represents a lookup based on <see cref="StoreObjectPair" /> keys.
/// </summary>
/// <typeparam name="T">The values type.</typeparam>
public class StoreObjectPairDictionary<T> : IReadOnlyStoreObjectPairDictionary<T>
    where T : class
{
    private readonly Dictionary<StoreObjectPair, T> _dictionary = [];

    /// <inheritdoc />
    public virtual T? Find(in StoreObjectPair storeObjects)
        => _dictionary.GetValueOrDefault(storeObjects);

    /// <inheritdoc />
    public virtual IEnumerable<T> GetValues()
        => _dictionary
            .OrderBy(pair => pair.Key.DependentStoreObject)
            .ThenBy(pair => pair.Key.PrincipalStoreObject)
            .Select(pair => pair.Value);

    /// <summary>
    ///     Adds the specified key and value to the dictionary.
    /// </summary>
    /// <param name="storeObjects">The store object pair.</param>
    /// <param name="value">The value to store.</param>
    public virtual void Add(in StoreObjectPair storeObjects, T value)
        => _dictionary.Add(storeObjects, value);

    /// <summary>
    ///     Removes the value with the specified key from the collection
    ///     and returns it if successful.
    /// </summary>
    /// <param name="storeObjects">The key of the element to remove.</param>
    /// <returns>The removed value.</returns>
    public virtual T? Remove(in StoreObjectPair storeObjects)
        => _dictionary.Remove(storeObjects, out var value)
            ? value
            : null;
}
