// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     Represents a SQL Server full-text catalog in the model.
/// </summary>
/// <remarks>
///     See <see href="https://learn.microsoft.com/sql/relational-databases/search/full-text-search">Full-Text Search</see>
///     for more information on SQL Server full-text search.
/// </remarks>
public interface IReadOnlySqlServerFullTextCatalog : IReadOnlyAnnotatable
{
    /// <summary>
    ///     Gets the name of the full-text catalog.
    /// </summary>
    string Name { get; }

    /// <summary>
    ///     Gets the model in which this full-text catalog is defined.
    /// </summary>
    IReadOnlyModel Model { get; }

    /// <summary>
    ///     Gets a value indicating whether this is the default full-text catalog for the database.
    /// </summary>
    bool IsDefault { get; }

    /// <summary>
    ///     Gets a value indicating whether the full-text catalog is accent-sensitive.
    /// </summary>
    bool IsAccentSensitive { get; }
}
