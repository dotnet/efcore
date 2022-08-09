// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     Represents a primary or alternate key on an entity.
/// </summary>
/// <remarks>
///     <para>
///         This interface is used during model creation and allows the metadata to be modified.
///         Once the model is built, <see cref="IKey" /> represents a read-only view of the same metadata.
///     </para>
///     <para>
///         See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and
///         examples.
///     </para>
/// </remarks>
public interface IMutableKey : IReadOnlyKey, IMutableAnnotatable
{
    /// <summary>
    ///     Gets the properties that make up the key.
    /// </summary>
    new IReadOnlyList<IMutableProperty> Properties { get; }

    /// <summary>
    ///     Gets the entity type the key is defined on. This may be different from the type that <see cref="Properties" />
    ///     are defined on when the key is defined a derived type in an inheritance hierarchy (since the properties
    ///     may be defined on a base type).
    /// </summary>
    new IMutableEntityType DeclaringEntityType { get; }

    /// <summary>
    ///     Gets all foreign keys that target a given primary or alternate key.
    /// </summary>
    /// <returns>The foreign keys that reference the given key.</returns>
    new IEnumerable<IMutableForeignKey> GetReferencingForeignKeys()
        => ((IReadOnlyKey)this).GetReferencingForeignKeys().Cast<IMutableForeignKey>();
}
