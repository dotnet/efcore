// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     Represents a lookup based on <see cref="StoreObjectPair" /> keys.
/// </summary>
/// <typeparam name="T">The values type.</typeparam>
public interface IReadOnlyStoreObjectPairDictionary<out T>
    where T : class
{
    /// <summary>
    ///     Gets the value associated with the specified key.
    /// </summary>
    /// <param name="storeObjects">The key of the value to get.</param>
    /// <returns>
    ///     The value associated with the specified key, or <see langword="null" /> if not found.
    /// </returns>
    T? Find(in StoreObjectPair storeObjects);

    /// <summary>
    ///     Gets a collection containing the values from this collection.
    /// </summary>
    /// <returns>A collection containing the values from this collection.</returns>
    IEnumerable<T> GetValues();
}
