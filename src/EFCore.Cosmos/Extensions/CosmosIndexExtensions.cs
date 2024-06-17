// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Cosmos.Metadata.Internal;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     Index extension methods for Azure Cosmos DB-specific metadata.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
///     <see href="https://aka.ms/efcore-docs-cosmos">Accessing Azure Cosmos DB with EF Core</see> for more information and examples.
/// </remarks>
public static class CosmosIndexExtensions
{
    /// <summary>
    ///     Returns the <see cref="VectorIndexType"/> to use for this index.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <returns>The index type to use, or <see langword="null" /> if none is set.</returns>
    public static VectorIndexType? GetVectorIndexType(this IReadOnlyIndex index)
        => (index is RuntimeIndex)
            ? throw new InvalidOperationException(CoreStrings.RuntimeModelMissingData)
            : (VectorIndexType?)index[CosmosAnnotationNames.VectorIndexType];

    /// <summary>
    ///     Sets the <see cref="VectorIndexType"/> to use for this index.
    /// </summary>
    /// <param name="indexType">The index type to use.</param>
    /// <param name="index">The index.</param>
    public static void SetVectorIndexType(this IMutableIndex index, VectorIndexType? indexType)
        => index.SetAnnotation(CosmosAnnotationNames.VectorIndexType, indexType);

    /// <summary>
    ///     Sets the <see cref="VectorIndexType"/> to use for this index.
    /// </summary>
    /// <param name="indexType">The index type to use.</param>
    /// <param name="index">The index.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The configured value.</returns>
    public static VectorIndexType? SetVectorIndexType(
        this IConventionIndex index,
        VectorIndexType? indexType,
        bool fromDataAnnotation = false)
        => (VectorIndexType?)index.SetAnnotation(
            CosmosAnnotationNames.VectorIndexType,
            indexType,
            fromDataAnnotation)?.Value;

    /// <summary>
    ///     Returns the <see cref="ConfigurationSource" /> for whether the <see cref="GetVectorIndexType"/>.
    /// </summary>
    /// <param name="property">The property.</param>
    /// <returns>The <see cref="ConfigurationSource" /> for whether the index is clustered.</returns>
    public static ConfigurationSource? GetVectorIndexTypeConfigurationSource(this IConventionIndex property)
        => property.FindAnnotation(CosmosAnnotationNames.VectorIndexType)?.GetConfigurationSource();
}
