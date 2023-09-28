// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Storage;

/// <summary>
///     <para>
///         Provides access to <see cref="IDatabaseFacadeDependencies" /> for providers and extensions.
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
public interface IDatabaseFacadeDependenciesAccessor
{
    /// <summary>
    ///     The dependencies.
    /// </summary>
    IDatabaseFacadeDependencies Dependencies { get; }

    /// <summary>
    ///     The DbContext instance associated with the database facade.
    /// </summary>
    DbContext Context { get; }
}
