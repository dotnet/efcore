// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.ChangeTracking;

/// <summary>
///     <para>
///         Represents a factory for key values based on the primary/principal key values taken from various forms of entity data.
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
public interface IPrincipalKeyValueFactory
{
    /// <summary>
    ///     Creates an equatable key object from the key values in the given entry.
    /// </summary>
    /// <param name="entry">The entry tracking an entity instance.</param>
    /// <param name="fromOriginalValues">Whether the original or current value should be used.</param>
    /// <returns>The key value.</returns>
    object CreateEquatableKey(IUpdateEntry entry, bool fromOriginalValues = false);
}
