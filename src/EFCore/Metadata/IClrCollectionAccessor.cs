// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     <para>
///         Represents operations backed by compiled delegates that allow manipulation of collections
///         on navigation properties.
///     </para>
///     <para>
///         This type is typically used by database providers (and other extensions). It is generally
///         not used in application code.
///     </para>
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-providers">Implementation of database providers and extensions</see>
///     for more information and examples.
/// </remarks>
public interface IClrCollectionAccessor
{
    /// <summary>
    ///     Adds a value to the navigation property collection, unless it is already contained in the collection.
    /// </summary>
    /// <param name="entity">The entity instance.</param>
    /// <param name="value">The value to add.</param>
    /// <param name="forMaterialization">If true, then the value is being added as part of query materialization.</param>
    /// <returns><see langword="true" /> if a value was added; <see langword="false" /> if it was already in the collection.</returns>
    bool Add(object entity, object value, bool forMaterialization);

    /// <summary>
    ///     Checks whether the value is contained in the collection.
    /// </summary>
    /// <param name="entity">The entity instance.</param>
    /// <param name="value">The value to check.</param>
    /// <returns><see langword="true" /> if the value is contained in the collection; <see langword="false" /> otherwise.</returns>
    bool Contains(object entity, object value);

    /// <summary>
    ///     Removes a value from the collection.
    /// </summary>
    /// <param name="entity">The entity instance.</param>
    /// <param name="value">The value to check.</param>
    /// <returns><see langword="true" /> if the value was contained in the collection; <see langword="false" /> otherwise.</returns>
    bool Remove(object entity, object value);

    /// <summary>
    ///     Adds a value to the passed collection, unless it is already contained in the collection.
    /// </summary>
    /// <param name="collection">The collection.</param>
    /// <param name="value">The value to add.</param>
    /// <returns><see langword="true" /> if a value was added; <see langword="false" /> if it was already in the collection.</returns>
    bool AddStandalone(object collection, object value);

    /// <summary>
    ///     Checks whether the value is contained in the passed collection.
    /// </summary>
    /// <param name="collection">The collection.</param>
    /// <param name="value">The value to check.</param>
    /// <returns><see langword="true" /> if the value is contained in the collection; <see langword="false" /> otherwise.</returns>
    bool ContainsStandalone(object collection, object value);

    /// <summary>
    ///     Removes a value from the passed collection.
    /// </summary>
    /// <param name="collection">The collection.</param>
    /// <param name="value">The value to check.</param>
    /// <returns><see langword="true" /> if the value was contained in the collection; <see langword="false" /> otherwise.</returns>
    bool RemoveStandalone(object? collection, object value);

    /// <summary>
    ///     Creates a new collection instance of the appropriate type for the navigation property.
    /// </summary>
    /// <returns>The collection instance.</returns>
    object Create();

    /// <summary>
    ///     Either returns the existing collection instance set on the navigation property, or if none
    ///     exists, then creates a new instance, sets it, and returns it.
    /// </summary>
    /// <param name="entity">The entity instance.</param>
    /// <param name="forMaterialization">If true, then this is happening as part of query materialization; <see langword="false" /> otherwise.</param>
    /// <returns>The existing or new collection.</returns>
    object GetOrCreate(object entity, bool forMaterialization);

    /// <summary>
    ///     The collection type.
    /// </summary>
    Type CollectionType { get; }
}
