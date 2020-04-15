// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Update;

namespace Microsoft.EntityFrameworkCore.ChangeTracking
{
    /// <summary>
    ///     <para>
    ///         Gets a factory for key values based on the primary/principal key values taken from various forms of entity data.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    /// <typeparam name="TKey"> The key type. </typeparam>
    public interface IPrincipalKeyValueFactory<TKey>
    {
        /// <summary>
        ///     Creates a key object from key values obtained in-order from the given array.
        /// </summary>
        /// <param name="keyValues"> The key values. </param>
        /// <returns> The key object, or null if any of the key values were null. </returns>
        object CreateFromKeyValues([NotNull] object[] keyValues);

        /// <summary>
        ///     Creates a key object from key values obtained from their indexed position in the given <see cref="ValueBuffer" />.
        /// </summary>
        /// <param name="valueBuffer"> The buffer containing key values. </param>
        /// <returns> The key object, or null if any of the key values were null. </returns>
        object CreateFromBuffer(ValueBuffer valueBuffer);

        /// <summary>
        ///     Finds the first null in the given in-order array of key values and returns the associated <see cref="IProperty" />.
        /// </summary>
        /// <param name="keyValues"> The key values. </param>
        /// <returns> The associated property. </returns>
        IProperty FindNullPropertyInKeyValues([NotNull] object[] keyValues);

        /// <summary>
        ///     Creates a key object from the key values in the given entry.
        /// </summary>
        /// <param name="entry"> The entry tracking an entity instance. </param>
        /// <returns> The key value. </returns>
        TKey CreateFromCurrentValues([NotNull] IUpdateEntry entry);

        /// <summary>
        ///     Finds the first null key value in the given entry and returns the associated <see cref="IProperty" />.
        /// </summary>
        /// <param name="entry"> The entry tracking an entity instance. </param>
        /// <returns> The associated property. </returns>
        IProperty FindNullPropertyInCurrentValues([NotNull] IUpdateEntry entry);

        /// <summary>
        ///     Creates a key object from the original key values in the given entry.
        /// </summary>
        /// <param name="entry"> The entry tracking an entity instance. </param>
        /// <returns> The key value. </returns>
        TKey CreateFromOriginalValues([NotNull] IUpdateEntry entry);

        /// <summary>
        ///     Creates a key object from the relationship snapshot key values in the given entry.
        /// </summary>
        /// <param name="entry"> The entry tracking an entity instance. </param>
        /// <returns> The key value. </returns>
        TKey CreateFromRelationshipSnapshot([NotNull] IUpdateEntry entry);

        /// <summary>
        ///     An <see cref="IEqualityComparer{T}" /> for comparing key objects.
        /// </summary>
        IEqualityComparer<TKey> EqualityComparer { get; }
    }
}
