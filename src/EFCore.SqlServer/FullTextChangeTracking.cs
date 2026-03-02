// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable once CheckNamespace

namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     Indicates the change tracking mode for a SQL Server full-text index.
/// </summary>
/// <remarks>
///     See <see href="https://learn.microsoft.com/sql/relational-databases/search/full-text-search">Full-Text Search</see> for more
///     information on SQL Server full-text search.
/// </remarks>
public enum FullTextChangeTracking
{
    /// <summary>
    ///     SQL Server automatically maintains the full-text index as the underlying data changes.
    /// </summary>
    Auto,

    /// <summary>
    ///     Changes are tracked but not propagated automatically. Changes must be propagated manually
    ///     via <c>ALTER FULLTEXT INDEX ... START UPDATE POPULATION</c>.
    /// </summary>
    Manual,

    /// <summary>
    ///     Change tracking is disabled. The full-text index must be fully repopulated manually.
    /// </summary>
    Off,

    /// <summary>
    ///     No population is performed after the full-text index is created. The index must be populated
    ///     manually later.
    /// </summary>
    OffNoPopulation
}
