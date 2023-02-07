// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     Represents an index on a set of properties.
/// </summary>
/// <remarks>
///     <para>
///         This interface is used during model creation and allows the metadata to be modified.
///         Once the model is built, <see cref="IIndex" /> represents a read-only view of the same metadata.
///     </para>
///     <para>
///         See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and
///         examples.
///     </para>
/// </remarks>
public interface IMutableIndex : IReadOnlyIndex, IMutableAnnotatable
{
    /// <summary>
    ///     Gets or sets a value indicating whether the values assigned to the indexed properties are unique.
    /// </summary>
    new bool IsUnique { get; set; }

    /// <summary>
    ///     A set of values indicating whether each corresponding index column has descending sort order.
    /// </summary>
    new IReadOnlyList<bool>? IsDescending { get; set; }

    /// <summary>
    ///     Gets the properties that this index is defined on.
    /// </summary>
    new IReadOnlyList<IMutableProperty> Properties { get; }

    /// <summary>
    ///     Gets the entity type the index is defined on. This may be different from the type that <see cref="Properties" />
    ///     are defined on when the index is defined a derived type in an inheritance hierarchy (since the properties
    ///     may be defined on a base type).
    /// </summary>
    new IMutableEntityType DeclaringEntityType { get; }
}
