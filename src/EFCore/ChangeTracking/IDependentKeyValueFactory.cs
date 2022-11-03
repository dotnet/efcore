// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.ChangeTracking;

/// <summary>
///     <para>
///         A factory for key values based on the foreign key values taken from various forms of entity data.
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
public interface IDependentKeyValueFactory
{
    /// <summary>
    ///     Creates an equatable key object from the key values in the given entry.
    /// </summary>
    /// <param name="entry">The entry tracking an entity instance.</param>
    /// <param name="fromOriginalValues">Whether the original or current values should be used.</param>
    /// <returns>The key object.</returns>
    object CreatePrincipalEquatableKey(IUpdateEntry entry, bool fromOriginalValues = false);

    /// <summary>
    ///     Creates an equatable key object from the foreign key values in the given entry.
    /// </summary>
    /// <param name="entry">The entry tracking an entity instance.</param>
    /// <param name="fromOriginalValues">Whether the original or current values should be used.</param>
    /// <returns>The key object.</returns>
    object? CreateDependentEquatableKey(IUpdateEntry entry, bool fromOriginalValues = false);
}
