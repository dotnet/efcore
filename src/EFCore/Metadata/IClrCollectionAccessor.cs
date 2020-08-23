// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     Represents operations backed by compiled delegates that allow manipulation of collections
    ///     on navigation properties.
    /// </summary>
    public interface IClrCollectionAccessor
    {
        /// <summary>
        ///     Adds a value to the navigation property collection, unless it is already contained in the collection.
        /// </summary>
        /// <param name="entity"> The entity instance. </param>
        /// <param name="value"> The value to add. </param>
        /// <param name="forMaterialization"> If true, then the value is being added as part of query materialization.</param>
        /// <returns> <see langword="true" /> if a value was added; <see langword="false" /> if it was already in the collection. </returns>
        bool Add([NotNull] object entity, [NotNull] object value, bool forMaterialization);

        /// <summary>
        ///     Checks whether the value is contained in the collection.
        /// </summary>
        /// <param name="entity"> The entity instance. </param>
        /// <param name="value"> The value to check. </param>
        /// <returns> <see langword="true" /> if the value is contained in the collection; <see langword="false" /> otherwise. </returns>
        bool Contains([NotNull] object entity, [NotNull] object value);

        /// <summary>
        ///     Removes a value from the collection.
        /// </summary>
        /// <param name="entity"> The entity instance. </param>
        /// <param name="value"> The value to check. </param>
        /// <returns> <see langword="true" /> if the value was contained in the collection; <see langword="false" /> otherwise. </returns>
        bool Remove([NotNull] object entity, [NotNull] object value);

        /// <summary>
        ///     Creates a new collection instance of the appropriate type for the navigation property.
        /// </summary>
        /// <returns> The collection instance. </returns>
        object Create();

        /// <summary>
        ///     Either returns the existing collection instance set on the navigation property, or if none
        ///     exists, then creates a new instance, sets it, and returns it.
        /// </summary>
        /// <param name="entity"> The entity instance. </param>
        /// <param name="forMaterialization"> If true, then this is happening as part of query materialization; <see langword="false" /> otherwise. </param>
        /// <returns> The existing or new collection. </returns>
        object GetOrCreate([NotNull] object entity, bool forMaterialization);

        /// <summary>
        ///     The collection type.
        /// </summary>
        Type CollectionType { get; }
    }
}
