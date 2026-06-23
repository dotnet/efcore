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
public interface ISqlServerFullTextCatalog : IReadOnlySqlServerFullTextCatalog, IAnnotatable
{
    /// <summary>
    ///     Gets the model in which this full-text catalog is defined.
    /// </summary>
    new IModel Model { get; }
}
