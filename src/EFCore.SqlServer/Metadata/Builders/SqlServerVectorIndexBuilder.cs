// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;

namespace Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>
///     Provides a simple API for configuring a vector index on SQL Server.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
///     <see href="https://aka.ms/efcore-docs-sqlserver">Accessing SQL Server and Azure SQL databases with EF Core</see>
///     for more information and examples.
/// </remarks>
/// <param name="indexBuilder">The index builder.</param>
[Experimental(EFDiagnostics.SqlServerVectorSearch)]
public class SqlServerVectorIndexBuilder(IndexBuilder indexBuilder)
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
    public virtual SqlServerVectorIndexBuilder HasDatabaseName(string? name)
    {
        Metadata.SetDatabaseName(name);

        return this;
    }

    /// <summary>
    ///     Configures the similarity metric for the vector index when targeting SQL Server.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://learn.microsoft.com/sql/t-sql/statements/create-vector-index-transact-sql">
    ///         SQL Server documentation for <c>CREATE VECTOR INDEX</c>
    ///     </see>.
    /// </remarks>
    /// <param name="metric">The similarity metric for the vector index (e.g. "cosine", "euclidean", "dot").</param>
    /// <returns>A builder to further configure the vector index.</returns>
    public virtual SqlServerVectorIndexBuilder UseMetric(string metric)
    {
        Check.NotEmpty(metric);

        Metadata.SetVectorMetric(metric);

        return this;
    }

    /// <summary>
    ///     Configures the type of the vector index when targeting SQL Server.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://learn.microsoft.com/sql/t-sql/statements/create-vector-index-transact-sql">
    ///         SQL Server documentation for <c>CREATE VECTOR INDEX</c>
    ///     </see>.
    /// </remarks>
    /// <param name="type">The type of the vector index (e.g. "DiskANN").</param>
    /// <returns>A builder to further configure the vector index.</returns>
    public virtual SqlServerVectorIndexBuilder UseType(string? type)
    {
        Metadata.SetVectorIndexType(type);

        return this;
    }
}
