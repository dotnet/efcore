// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.ChangeTracking;

/// <summary>
///     <para>
///         A factory for key values based on the primary/principal key values taken from various forms of entity data.
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
/// <typeparam name="TKey">The key type.</typeparam>
public interface IPrincipalKeyValueFactory<TKey> : IPrincipalKeyValueFactory
{
    /// <summary>
    ///     Creates a key object from key values obtained in-order from the given enumerable.
    /// </summary>
    /// <param name="keyValues">The key values.</param>
    /// <returns>The key object, or null if any of the key values were null.</returns>
    object? CreateFromKeyValues(IReadOnlyList<object?> keyValues);

    /// <summary>
    ///     Creates a key object from key values obtained from their indexed position in the given <see cref="ValueBuffer" />.
    /// </summary>
    /// <param name="valueBuffer">The buffer containing key values.</param>
    /// <returns>The key object, or null if any of the key values were null.</returns>
    [Obsolete]
    object? CreateFromBuffer(ValueBuffer valueBuffer);

    /// <summary>
    ///     Finds the first null in the given in-order array of key values and returns the associated <see cref="IProperty" />.
    /// </summary>
    /// <param name="keyValues">The key values.</param>
    /// <returns>The associated property.</returns>
    IProperty? FindNullPropertyInKeyValues(IReadOnlyList<object?> keyValues);

    /// <summary>
    ///     Creates a key object from the key values in the given entry.
    /// </summary>
    /// <param name="entry">The entry tracking an entity instance.</param>
    /// <returns>The key value.</returns>
    TKey? CreateFromCurrentValues(IUpdateEntry entry);

    /// <summary>
    ///     Finds the first null key value in the given entry and returns the associated <see cref="IProperty" />.
    /// </summary>
    /// <param name="entry">The entry tracking an entity instance.</param>
    /// <returns>The associated property.</returns>
    IProperty? FindNullPropertyInCurrentValues(IUpdateEntry entry);

    /// <summary>
    ///     Creates a key object from the original key values in the given entry.
    /// </summary>
    /// <param name="entry">The entry tracking an entity instance.</param>
    /// <returns>The key value.</returns>
    TKey? CreateFromOriginalValues(IUpdateEntry entry);

    /// <summary>
    ///     Creates a key object from the relationship snapshot key values in the given entry.
    /// </summary>
    /// <param name="entry">The entry tracking an entity instance.</param>
    /// <returns>The key value.</returns>
    TKey CreateFromRelationshipSnapshot(IUpdateEntry entry);

    /// <summary>
    ///     An <see cref="IEqualityComparer{T}" /> for comparing key objects.
    /// </summary>
    IEqualityComparer<TKey> EqualityComparer { get; }
}
