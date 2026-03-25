// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>
///     Provides a simple API for configuring a full-text index on SQL Server.
/// </summary>
/// <remarks>
///     <para>
///         Instances of this class are returned from methods when using the <see cref="EntityTypeBuilder{TEntity}" /> API
///         and it is not designed to be directly constructed in your application code.
///     </para>
///     <para>
///         See <see href="https://learn.microsoft.com/sql/relational-databases/search/full-text-search">Full-Text Search</see>
///         for more information on SQL Server full-text search.
///     </para>
/// </remarks>
/// <typeparam name="TEntity">The entity type being configured.</typeparam>
/// <param name="indexBuilder">The index builder.</param>
public class SqlServerFullTextIndexBuilder<TEntity>(IndexBuilder<TEntity> indexBuilder)
{
    /// <summary>
    ///     The index being configured.
    /// </summary>
    public virtual IMutableIndex Metadata
        => indexBuilder.Metadata;

    /// <summary>
    ///     Configures the name of the index in the database when targeting a relational database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-indexes">Indexes</see> for more information and examples.
    /// </remarks>
    /// <param name="name">The name of the index.</param>
    /// <returns>A builder to further configure the index.</returns>
    public virtual SqlServerFullTextIndexBuilder<TEntity> HasDatabaseName(string? name)
    {
        Metadata.SetDatabaseName(name);

        return this;
    }

    /// <summary>
    ///     Configures the KEY INDEX for the full-text index. This is the unique, non-nullable, single-column index
    ///     used as the unique key for the full-text index.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://learn.microsoft.com/sql/t-sql/statements/create-fulltext-index-transact-sql">
    ///         SQL Server documentation for <c>CREATE FULLTEXT INDEX</c>
    ///     </see>.
    /// </remarks>
    /// <param name="keyIndexName">The name of the KEY INDEX.</param>
    /// <returns>A builder to further configure the full-text index.</returns>
    public virtual SqlServerFullTextIndexBuilder<TEntity> HasKeyIndex(string keyIndexName)
    {
        Check.NotEmpty(keyIndexName);

        Metadata.SetFullTextKeyIndex(keyIndexName);

        return this;
    }

    /// <summary>
    ///     Configures the full-text catalog for the full-text index.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://learn.microsoft.com/sql/t-sql/statements/create-fulltext-index-transact-sql">
    ///         SQL Server documentation for <c>CREATE FULLTEXT INDEX</c>
    ///     </see>.
    /// </remarks>
    /// <param name="catalogName">The name of the full-text catalog.</param>
    /// <returns>A builder to further configure the full-text index.</returns>
    public virtual SqlServerFullTextIndexBuilder<TEntity> OnCatalog(string catalogName)
    {
        Check.NotEmpty(catalogName);

        Metadata.SetFullTextCatalog(catalogName);

        return this;
    }

    /// <summary>
    ///     Configures the change tracking mode for the full-text index.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://learn.microsoft.com/sql/t-sql/statements/create-fulltext-index-transact-sql">
    ///         SQL Server documentation for <c>CREATE FULLTEXT INDEX</c>
    ///     </see>.
    /// </remarks>
    /// <param name="changeTracking">The change tracking mode.</param>
    /// <returns>A builder to further configure the full-text index.</returns>
    public virtual SqlServerFullTextIndexBuilder<TEntity> WithChangeTracking(FullTextChangeTracking changeTracking)
    {
        Metadata.SetFullTextChangeTracking(changeTracking);

        return this;
    }

    /// <summary>
    ///     Configures the language for a specific property in the full-text index.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://learn.microsoft.com/sql/t-sql/statements/create-fulltext-index-transact-sql">
    ///         SQL Server documentation for <c>CREATE FULLTEXT INDEX</c>
    ///     </see>.
    /// </remarks>
    /// <param name="propertyName">The name of the property.</param>
    /// <param name="language">The language term (e.g. "English", "1033").</param>
    /// <returns>A builder to further configure the full-text index.</returns>
    public virtual SqlServerFullTextIndexBuilder<TEntity> HasLanguage(string propertyName, string language)
    {
        Check.NotEmpty(propertyName);
        Check.NotEmpty(language);

        Metadata.SetFullTextLanguage(propertyName, language);

        return this;
    }
}
