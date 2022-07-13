// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///    Represents a lookup based on <see cref="StoreObjectIdentifier" /> keys.
    /// </summary>
    /// <typeparam name="T">The values type.</typeparam>
    public interface IReadOnlyStoreObjectDictionary<out T>
        where T : class
    {
        /// <summary>
        ///     Gets the value associated with the specified key.
        /// </summary>
        /// <param name="storeObject">The key of the value to get.</param>
        /// <returns>
        ///     <see langword="true"/> if the collection contains an element with the specified key;
        ///     otherwise, <see langword="false"/>.
        /// </returns>
        T? Find(in StoreObjectIdentifier storeObject);
        
        /// <summary>
        ///    Gets a collection containing the values from this collection.
        /// </summary>
        /// <returns>A collection containing the values from this collection.</returns>
        IEnumerable<T> GetValues();
    }
}
