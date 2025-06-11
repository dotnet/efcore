// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Cosmos.Metadata.Internal;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     Azure Cosmos DB-specific extension methods for <see cref="IndexBuilder" />.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
///     <see href="https://aka.ms/efcore-docs-cosmos">Accessing Azure Cosmos DB with EF Core</see> for more information and examples.
/// </remarks>
public static class CosmosIndexBuilderExtensions
{
    /// <summary>
    ///     Configures the index as a vector index with the given vector index type, such as "flat", "diskANN", or "quantizedFlat".
    ///     See <see href="https://aka.ms/ef-cosmos-vectors">Vector Search in Azure Cosmos DB for NoSQL</see> for more information.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
    ///     <see href="https://aka.ms/efcore-docs-cosmos">Accessing Azure Cosmos DB with EF Core</see> for more information and examples.
    /// </remarks>
    /// <param name="indexBuilder">The builder for the index being configured.</param>
    /// <param name="indexType">The type of vector index to create.</param>
    /// <returns>A builder to further configure the index.</returns>
    public static IndexBuilder IsVectorIndex(this IndexBuilder indexBuilder, VectorIndexType? indexType)
    {
        indexBuilder.Metadata.SetVectorIndexType(indexType);

        return indexBuilder;
    }

    /// <summary>
    ///     Configures whether the index as a vector index with the given vector index type, such as "flat", "diskANN", or "quantizedFlat".
    ///     See <see href="https://aka.ms/ef-cosmos-vectors">Vector Search in Azure Cosmos DB for NoSQL</see> for more information.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
    ///     <see href="https://aka.ms/efcore-docs-cosmos">Accessing Azure Cosmos DB with EF Core</see> for more information and examples.
    /// </remarks>
    /// <param name="indexBuilder">The builder for the index being configured.</param>
    /// <param name="indexType">The type of vector index to create.</param>
    /// <returns>A builder to further configure the index.</returns>
    public static IndexBuilder<TEntity> IsVectorIndex<TEntity>(
        this IndexBuilder<TEntity> indexBuilder,
        VectorIndexType? indexType)
        => (IndexBuilder<TEntity>)IsVectorIndex((IndexBuilder)indexBuilder, indexType);

    /// <summary>
    ///     Configures whether the index as a vector index with the given vector index type, such as "flat", "diskANN", or "quantizedFlat".
    ///     See <see href="https://aka.ms/ef-cosmos-vectors">Vector Search in Azure Cosmos DB for NoSQL</see> for more information.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
    ///     <see href="https://aka.ms/efcore-docs-cosmos">Accessing Azure Cosmos DB with EF Core</see> for more information and examples.
    /// </remarks>
    /// <param name="indexBuilder">The builder for the index being configured.</param>
    /// <param name="indexType">The type of vector index to create.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     The same builder instance if the configuration was applied,
    ///     <see langword="null" /> otherwise.
    /// </returns>
    public static IConventionIndexBuilder? IsVectorIndex(
        this IConventionIndexBuilder indexBuilder,
        VectorIndexType? indexType,
        bool fromDataAnnotation = false)
    {
        if (indexBuilder.CanSetVectorIndexType(indexType, fromDataAnnotation))
        {
            indexBuilder.Metadata.SetVectorIndexType(indexType, fromDataAnnotation);
            return indexBuilder;
        }

        return null;
    }

    /// <summary>
    ///     Returns a value indicating whether the vector index can be configured for vectors.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
    ///     <see href="https://aka.ms/efcore-docs-cosmos">Accessing Azure Cosmos DB with EF Core</see> for more information and examples.
    /// </remarks>
    /// <param name="indexBuilder">The builder for the index being configured.</param>
    /// <param name="indexType">The index type to use.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns><see langword="true" /> if the index can be configured for vectors.</returns>
    public static bool CanSetVectorIndexType(
        this IConventionIndexBuilder indexBuilder,
        VectorIndexType? indexType,
        bool fromDataAnnotation = false)
        => indexBuilder.CanSetAnnotation(CosmosAnnotationNames.VectorIndexType, indexType, fromDataAnnotation);

    /// <summary>
    ///     Configures the index as a full-text index.
    ///     See <see href="https://learn.microsoft.com/azure/cosmos-db/gen-ai/full-text-search">Full-text search in Azure Cosmos DB for NoSQL</see> for more information.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
    ///     <see href="https://aka.ms/efcore-docs-cosmos">Accessing Azure Cosmos DB with EF Core</see> for more information and examples.
    /// </remarks>
    /// <param name="indexBuilder">The builder for the index being configured.</param>
    /// <param name="value">The value indicating whether the index is configured for Full-text search.</param>
    /// <returns>A builder to further configure the index.</returns>
    public static IndexBuilder IsFullTextIndex(this IndexBuilder indexBuilder, bool? value = true)
    {
        indexBuilder.Metadata.SetIsFullTextIndex(value);

        return indexBuilder;
    }

    /// <summary>
    ///     Configures the index as a full-text index.
    ///     See <see href="https://learn.microsoft.com/azure/cosmos-db/gen-ai/full-text-search">Full-text search in Azure Cosmos DB for NoSQL</see> for more information.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
    ///     <see href="https://aka.ms/efcore-docs-cosmos">Accessing Azure Cosmos DB with EF Core</see> for more information and examples.
    /// </remarks>
    /// <param name="indexBuilder">The builder for the index being configured.</param>
    /// <param name="value">The value indicating whether the index is configured for Full-text search.</param>
    /// <returns>A builder to further configure the index.</returns>
    public static IndexBuilder<TEntity> IsFullTextIndex<TEntity>(
        this IndexBuilder<TEntity> indexBuilder,
        bool? value = true)
        => (IndexBuilder<TEntity>)IsFullTextIndex((IndexBuilder)indexBuilder, value);

    /// <summary>
    ///     Configures the index as a full-text index.
    ///     See <see href="https://learn.microsoft.com/azure/cosmos-db/gen-ai/full-text-search">Full-text search in Azure Cosmos DB for NoSQL</see> for more information.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
    ///     <see href="https://aka.ms/efcore-docs-cosmos">Accessing Azure Cosmos DB with EF Core</see> for more information and examples.
    /// </remarks>
    /// <param name="indexBuilder">The builder for the index being configured.</param>
    /// <param name="value">The value indicating whether the index is configured for Full-text search.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     The same builder instance if the configuration was applied,
    ///     <see langword="null" /> otherwise.
    /// </returns>
    public static IConventionIndexBuilder? IsFullTextIndex(
        this IConventionIndexBuilder indexBuilder,
        bool? value,
        bool fromDataAnnotation = false)
    {
        if (indexBuilder.CanSetIsFullTextIndex(fromDataAnnotation))
        {
            indexBuilder.Metadata.SetIsFullTextIndex(value, fromDataAnnotation);
            return indexBuilder;
        }

        return null;
    }

    /// <summary>
    ///     Returns a value indicating whether the index can be configured as a full-text index.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
    ///     <see href="https://aka.ms/efcore-docs-cosmos">Accessing Azure Cosmos DB with EF Core</see> for more information and examples.
    /// </remarks>
    /// <param name="indexBuilder">The builder for the index being configured.</param>
    /// <param name="value">The value indicating whether the index is configured for Full-text search.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns><see langword="true" /> if the index can be configured as a Full-text index.</returns>
    public static bool CanSetIsFullTextIndex(
        this IConventionIndexBuilder indexBuilder,
        bool? value,
        bool fromDataAnnotation = false)
        => indexBuilder.CanSetAnnotation(CosmosAnnotationNames.FullTextIndex, value, fromDataAnnotation);
}
