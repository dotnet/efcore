// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable once CheckNamespace

namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     Cosmos-specific extension methods for <see cref="ComplexCollectionTypePropertyBuilder" />.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
///     <see href="https://aka.ms/efcore-docs-cosmos">Accessing Azure Cosmos DB with EF Core</see> for more information and examples.
/// </remarks>
public static class CosmosComplexCollectionTypePropertyBuilderExtensions
{
    /// <summary>
    ///     Configures the property name that the property is mapped to when targeting Azure Cosmos.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
    ///     <see href="https://aka.ms/efcore-docs-cosmos">Accessing Azure Cosmos DB with EF Core</see> for more information and examples.
    /// </remarks>
    /// <param name="propertyBuilder">The builder for the property being configured.</param>
    /// <param name="name">The name of the property.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static ComplexCollectionTypePropertyBuilder ToJsonProperty(
        this ComplexCollectionTypePropertyBuilder propertyBuilder,
        string name)
    {
        Check.NotEmpty(name);

        propertyBuilder.Metadata.SetJsonPropertyName(name);

        return propertyBuilder;
    }

    /// <summary>
    ///     Configures the property name that the property is mapped to when targeting Azure Cosmos.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
    ///     <see href="https://aka.ms/efcore-docs-cosmos">Accessing Azure Cosmos DB with EF Core</see> for more information and examples.
    /// </remarks>
    /// <typeparam name="TProperty">The type of the property being configured.</typeparam>
    /// <param name="propertyBuilder">The builder for the property being configured.</param>
    /// <param name="name">The name of the property.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static ComplexCollectionTypePropertyBuilder<TProperty> ToJsonProperty<TProperty>(
        this ComplexCollectionTypePropertyBuilder<TProperty> propertyBuilder,
        string name)
        => (ComplexCollectionTypePropertyBuilder<TProperty>)ToJsonProperty((ComplexCollectionTypePropertyBuilder)propertyBuilder, name);

    /// <summary>
    ///     Enables full-text search for this property using a specified language.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
    ///     <see href="https://aka.ms/efcore-docs-cosmos">Accessing Azure Cosmos DB with EF Core</see> for more information and examples.
    /// </remarks>
    /// <param name="propertyBuilder">The builder for the property being configured.</param>
    /// <param name="language">
    ///     The language used for full-text search. Setting this to <see langword="null" /> will use the default language for
    ///     the container, or "en-US" if default language was not specified.
    /// </param>
    /// <param name="enabled">The value indicating whether full-text search should be enabled for this property.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static ComplexCollectionTypePropertyBuilder EnableFullTextSearch(
        this ComplexCollectionTypePropertyBuilder propertyBuilder,
        string? language = null,
        bool enabled = true)
    {
        propertyBuilder.Metadata.SetIsFullTextSearchEnabled(enabled);
        propertyBuilder.Metadata.SetFullTextSearchLanguage(language);

        return propertyBuilder;
    }

    /// <summary>
    ///     Enables full-text search for this property using a specified language.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
    ///     <see href="https://aka.ms/efcore-docs-cosmos">Accessing Azure Cosmos DB with EF Core</see> for more information and examples.
    /// </remarks>
    /// <typeparam name="TProperty">The type of the property being configured.</typeparam>
    /// <param name="propertyBuilder">The builder for the property being configured.</param>
    /// <param name="language">
    ///     The language used for full-text search. Setting this to <see langword="null" /> will use the default language for
    ///     the container, or "en-US" if default language was not specified.
    /// </param>
    /// <param name="enabled">The value indicating whether full-text search should be enabled for this property.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static ComplexCollectionTypePropertyBuilder<TProperty> EnableFullTextSearch<TProperty>(
        this ComplexCollectionTypePropertyBuilder<TProperty> propertyBuilder,
        string? language = null,
        bool enabled = true)
        => (ComplexCollectionTypePropertyBuilder<TProperty>)EnableFullTextSearch(
            (ComplexCollectionTypePropertyBuilder)propertyBuilder, language, enabled);

    /// <summary>
    ///     Configures the property as a vector for Azure Cosmos DB.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
    ///     <see href="https://aka.ms/efcore-docs-cosmos">Accessing Azure Cosmos DB with EF Core</see> for more information and examples.
    /// </remarks>
    /// <param name="propertyBuilder">The builder for the property being configured.</param>
    /// <param name="distanceFunction">The distance function for a vector comparisons.</param>
    /// <param name="dimensions">The number of dimensions in the vector.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static ComplexCollectionTypePropertyBuilder IsVectorProperty(
        this ComplexCollectionTypePropertyBuilder propertyBuilder,
        DistanceFunction distanceFunction,
        int dimensions)
    {
        propertyBuilder.Metadata.SetVectorDistanceFunction(ValidateVectorDistanceFunction(distanceFunction));
        propertyBuilder.Metadata.SetVectorDimensions(dimensions);

        return propertyBuilder;
    }

    /// <summary>
    ///     Configures the property as a vector for Azure Cosmos DB.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
    ///     <see href="https://aka.ms/efcore-docs-cosmos">Accessing Azure Cosmos DB with EF Core</see> for more information and examples.
    /// </remarks>
    /// <typeparam name="TProperty">The type of the property being configured.</typeparam>
    /// <param name="propertyBuilder">The builder for the property being configured.</param>
    /// <param name="distanceFunction">The distance function for a vector comparisons.</param>
    /// <param name="dimensions">The number of dimensions in the vector.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static ComplexCollectionTypePropertyBuilder<TProperty> IsVectorProperty<TProperty>(
        this ComplexCollectionTypePropertyBuilder<TProperty> propertyBuilder,
        DistanceFunction distanceFunction,
        int dimensions)
        => (ComplexCollectionTypePropertyBuilder<TProperty>)IsVectorProperty(
            (ComplexCollectionTypePropertyBuilder)propertyBuilder, distanceFunction, dimensions);

    private static DistanceFunction ValidateVectorDistanceFunction(DistanceFunction distanceFunction)
        => Enum.IsDefined(distanceFunction)
            ? distanceFunction
            : throw new ArgumentException(
                CoreStrings.InvalidEnumValue(
                    distanceFunction,
                    nameof(distanceFunction),
                    typeof(DistanceFunction)));
}
