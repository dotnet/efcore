// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable once CheckNamespace

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     Defines strategies to use across the EF Core stack when generating key values
///     from SQLite database columns.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see>, and
///     <see href="https://aka.ms/efcore-docs-sqlite">Accessing SQLite databases with EF Core</see>
///     for more information and examples.
/// </remarks>
public enum SqliteValueGenerationStrategy
{
    /// <summary>
    ///     No SQLite-specific strategy
    /// </summary>
    None,

    /// <summary>
    ///     A pattern that uses SQLite's AUTOINCREMENT feature to generate values for new entities.
    /// </summary>
    /// <remarks>
    ///     AUTOINCREMENT can only be used on integer primary key columns in SQLite.
    /// </remarks>
    Autoincrement
}