// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.SqlServer.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>
///     Provides a simple API for configuring a SQL Server full-text catalog.
/// </summary>
/// <remarks>
///     <para>
///         Instances of this class are returned from methods when using the <see cref="ModelBuilder" /> API
///         and it is not designed to be directly constructed in your application code.
///     </para>
///     <para>
///         See <see href="https://learn.microsoft.com/sql/relational-databases/search/full-text-search">Full-Text Search</see>
///         for more information on SQL Server full-text search.
///     </para>
/// </remarks>
/// <param name="catalog">The underlying full-text catalog.</param>
public class SqlServerFullTextCatalogBuilder(SqlServerFullTextCatalog catalog)
{
    /// <summary>
    ///     The underlying full-text catalog being configured.
    /// </summary>
    public virtual SqlServerFullTextCatalog Metadata { get; } = catalog;

    /// <summary>
    ///     Marks this catalog as the default full-text catalog for the database.
    /// </summary>
    /// <param name="default">Whether this is the default catalog.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public virtual SqlServerFullTextCatalogBuilder IsDefault(bool @default = true)
    {
        Metadata.IsDefault = @default;

        return this;
    }

    /// <summary>
    ///     Sets whether the full-text catalog is accent-sensitive.
    /// </summary>
    /// <param name="accentSensitive">Whether the catalog is accent-sensitive.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public virtual SqlServerFullTextCatalogBuilder IsAccentSensitive(bool accentSensitive = true)
    {
        Metadata.IsAccentSensitive = accentSensitive;

        return this;
    }
}
