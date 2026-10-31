// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     Represents a SQL Server full-text catalog in the model that can be mutated directly.
/// </summary>
/// <remarks>
///     See <see href="https://learn.microsoft.com/sql/relational-databases/search/full-text-search">Full-Text Search</see>
///     for more information on SQL Server full-text search.
/// </remarks>
public interface IMutableSqlServerFullTextCatalog : IReadOnlySqlServerFullTextCatalog, IMutableAnnotatable
{
    /// <summary>
    ///     Gets the <see cref="IMutableModel" /> in which this full-text catalog is defined.
    /// </summary>
    new IMutableModel Model { get; }

    /// <summary>
    ///     Gets or sets a value indicating whether this is the default full-text catalog for the database.
    /// </summary>
    new bool IsDefault { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether the full-text catalog is accent-sensitive.
    /// </summary>
    new bool IsAccentSensitive { get; set; }
}
