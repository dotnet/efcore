// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Cosmos.Metadata.Internal;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     Azure Cosmos DB-specific extension methods for <see cref="IndexBuilder"/>.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
///     <see href="https://aka.ms/efcore-docs-cosmos">Accessing Azure Cosmos DB with EF Core</see> for more information and examples.
/// </remarks>
public static class CosmosIndexBuilderExtensions
{
    /// <summary>
    ///     Configures whether the index as a vector index with the given <see cref="VectorIndexType"/>.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
    ///     <see href="https://aka.ms/efcore-docs-cosmos">Accessing Azure Cosmos DB with EF Core</see> for more information and examples.
    /// </remarks>
    /// <param name="indexBuilder">The builder for the index being configured.</param>
    /// <param name="indexType">The type of vector index to create.</param>
    /// <returns>A builder to further configure the index.</returns>
    public static IndexBuilder ForVectors(this IndexBuilder indexBuilder, VectorIndexType? indexType)
    {
        indexBuilder.Metadata.SetVectorIndexType(indexType);

        return indexBuilder;
    }

    /// <summary>
    ///     Configures whether the index as a vector index with the given <see cref="VectorIndexType"/>.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
    ///     <see href="https://aka.ms/efcore-docs-cosmos">Accessing Azure Cosmos DB with EF Core</see> for more information and examples.
    /// </remarks>
    /// <param name="indexBuilder">The builder for the index being configured.</param>
    /// <param name="indexType">The type of vector index to create.</param>
    /// <returns>A builder to further configure the index.</returns>
    public static IndexBuilder<TEntity> ForVectors<TEntity>(
        this IndexBuilder<TEntity> indexBuilder,
        VectorIndexType? indexType)
        => (IndexBuilder<TEntity>)ForVectors((IndexBuilder)indexBuilder, indexType);

    /// <summary>
    ///     Configures whether the index as a vector index with the given <see cref="VectorIndexType"/>.
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
    public static IConventionIndexBuilder? ForVectors(
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
    ///     Returns a value indicating whether the index can be configured for vectors.
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
}
