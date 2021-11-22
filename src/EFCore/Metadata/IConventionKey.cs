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
///         See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see> for more information and examples.
///     </para>
/// </remarks>
public interface IConventionKey : IReadOnlyKey, IConventionAnnotatable
{
    /// <summary>
    ///     Gets the builder that can be used to configure this key.
    /// </summary>
    /// <exception cref="InvalidOperationException">If the index has been removed from the model.</exception>
    new IConventionKeyBuilder Builder { get; }

    /// <summary>
    ///     Gets the properties that make up the key.
    /// </summary>
    new IReadOnlyList<IConventionProperty> Properties { get; }

    /// <summary>
    ///     Gets the entity type the key is defined on. This may be different from the type that <see cref="Properties" />
    ///     are defined on when the key is defined a derived type in an inheritance hierarchy (since the properties
    ///     may be defined on a base type).
    /// </summary>
    new IConventionEntityType DeclaringEntityType { get; }

    /// <summary>
    ///     Returns the configuration source for this key.
    /// </summary>
    /// <returns>The configuration source.</returns>
    ConfigurationSource GetConfigurationSource();

    /// <summary>
    ///     Gets all foreign keys that target a given primary or alternate key.
    /// </summary>
    /// <returns>The foreign keys that reference the given key.</returns>
    new IEnumerable<IConventionForeignKey> GetReferencingForeignKeys()
        => ((IReadOnlyKey)this).GetReferencingForeignKeys().Cast<IConventionForeignKey>();
}
