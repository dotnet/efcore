// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable once CheckNamespace

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     Defines two strategies to use across the EF Core stack when generating key values
///     from SQL Server database columns.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see>, and
///     <see href="https://aka.ms/efcore-docs-sqlserver">Accessing SQL Server and Azure SQL databases with EF Core</see>
///     for more information and examples.
/// </remarks>
public enum SqlServerValueGenerationStrategy
{
    /// <summary>
    ///     No SQL Server-specific strategy
    /// </summary>
    None,

    /// <summary>
    ///     A sequence-based hi-lo pattern where blocks of IDs are allocated from the server and
    ///     used client-side for generating keys.
    /// </summary>
    /// <remarks>
    ///     This is an advanced pattern--only use this strategy if you are certain it is what you need.
    /// </remarks>
    SequenceHiLo,

    /// <summary>
    ///     A pattern that uses a normal SQL Server <c>Identity</c> column in the same way as EF6 and earlier.
    /// </summary>
    IdentityColumn,

    /// <summary>
    ///     A pattern that uses a database sequence to generate values for the column.
    /// </summary>
    Sequence
}
