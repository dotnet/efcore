// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     Represents an index on a set of properties.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
/// </remarks>
public interface IIndex : IReadOnlyIndex, IAnnotatable
{
    /// <summary>
    ///     Gets the properties that this index is defined on.
    /// </summary>
    new IReadOnlyList<IProperty> Properties { get; }

    /// <summary>
    ///     Gets the entity type the index is defined on. This may be different from the type that <see cref="Properties" />
    ///     are defined on when the index is defined a derived type in an inheritance hierarchy (since the properties
    ///     may be defined on a base type).
    /// </summary>
    new IEntityType DeclaringEntityType { get; }

    /// <summary>
    ///     <para>
    ///         Gets a factory for key values based on the index key values taken from various forms of entity data.
    ///     </para>
    ///     <para>
    ///         This method is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    /// <typeparam name="TKey">The type of the index instance.</typeparam>
    /// <returns>The factory.</returns>
    IDependentKeyValueFactory<TKey> GetNullableValueFactory<TKey>();
}
