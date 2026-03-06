// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Cosmos.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     Cosmos-specific extension methods for <see cref="PrimitiveCollectionBuilder" />.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
///     <see href="https://aka.ms/efcore-docs-cosmos">Accessing Azure Cosmos DB with EF Core</see> for more information and examples.
/// </remarks>
public static class CosmosPrimitiveCollectionBuilderExtensions
{
    /// <summary>
    ///     Configures the property name that the property is mapped to when targeting Azure Cosmos.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         If an empty string is supplied, the property will not be persisted.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
    ///         <see href="https://aka.ms/efcore-docs-cosmos">Accessing Azure Cosmos DB with EF Core</see> for more information and examples.
    ///     </para>
    /// </remarks>
    /// <param name="primitiveCollectionBuilder">The builder for the property being configured.</param>
    /// <param name="name">The name of the property.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static PrimitiveCollectionBuilder HasJsonPropertyName(
        this PrimitiveCollectionBuilder primitiveCollectionBuilder,
        string name)
    {
        Check.NotNull(name);

        primitiveCollectionBuilder.Metadata.SetJsonPropertyName(name);

        return primitiveCollectionBuilder;
    }

    /// <summary>
    ///     Configures the property name that the property is mapped to when targeting Azure Cosmos.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
    ///     <see href="https://aka.ms/efcore-docs-cosmos">Accessing Azure Cosmos DB with EF Core</see> for more information and examples.
    /// </remarks>
    /// <typeparam name="TProperty">The type of the property being configured.</typeparam>
    /// <param name="primitiveCollectionBuilder">The builder for the property being configured.</param>
    /// <param name="name">The name of the property.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static PrimitiveCollectionBuilder<TProperty> HasJsonPropertyName<TProperty>(
        this PrimitiveCollectionBuilder<TProperty> primitiveCollectionBuilder,
        string name)
        => (PrimitiveCollectionBuilder<TProperty>)HasJsonPropertyName((PrimitiveCollectionBuilder)primitiveCollectionBuilder, name);

    /// <summary>
    ///     Configures the property name that the property is mapped to when targeting Azure Cosmos. If an empty string is
    ///     supplied then the property will not be persisted.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
    ///     <see href="https://aka.ms/efcore-docs-cosmos">Accessing Azure Cosmos DB with EF Core</see> for more information and examples.
    /// </remarks>
    /// <param name="propertyBuilder">The builder for the property being configured.</param>
    /// <param name="name">The name of the property.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     The same builder instance if the configuration was applied,
    ///     <see langword="null" /> otherwise.
    /// </returns>
    public static IConventionPropertyBuilder? HasJsonPropertyName(
        this IConventionPropertyBuilder propertyBuilder,
        string? name,
        bool fromDataAnnotation = false)
    {
        if (!propertyBuilder.CanSetJsonPropertyName(name, fromDataAnnotation))
        {
            return null;
        }

        propertyBuilder.Metadata.SetJsonPropertyName(name, fromDataAnnotation);

        return propertyBuilder;
    }

    /// <summary>
    ///     Returns a value indicating whether the given property name can be set.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
    ///     <see href="https://aka.ms/efcore-docs-cosmos">Accessing Azure Cosmos DB with EF Core</see> for more information and examples.
    /// </remarks>
    /// <param name="propertyBuilder">The builder for the property being configured.</param>
    /// <param name="name">The name of the property.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns><see langword="true" /> if the property name can be set.</returns>
    public static bool CanSetJsonPropertyName(
        this IConventionPropertyBuilder propertyBuilder,
        string? name,
        bool fromDataAnnotation = false)
        => propertyBuilder.CanSetAnnotation(CosmosAnnotationNames.PropertyName, name, fromDataAnnotation);

    /// <summary>
    ///     Configures the property name that the property is mapped to when targeting Azure Cosmos.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         If an empty string is supplied, the property will not be persisted.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
    ///         <see href="https://aka.ms/efcore-docs-cosmos">Accessing Azure Cosmos DB with EF Core</see> for more information and examples.
    ///     </para>
    /// </remarks>
    /// <param name="primitiveCollectionBuilder">The builder for the property being configured.</param>
    /// <param name="name">The name of the property.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    [Obsolete("Use HasJsonPropertyName() instead.")]
    public static PrimitiveCollectionBuilder ToJsonProperty(
        this PrimitiveCollectionBuilder primitiveCollectionBuilder,
        string name)
        => HasJsonPropertyName(primitiveCollectionBuilder, name);

    /// <summary>
    ///     Configures the property name that the property is mapped to when targeting Azure Cosmos.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
    ///     <see href="https://aka.ms/efcore-docs-cosmos">Accessing Azure Cosmos DB with EF Core</see> for more information and examples.
    /// </remarks>
    /// <typeparam name="TProperty">The type of the property being configured.</typeparam>
    /// <param name="primitiveCollectionBuilder">The builder for the property being configured.</param>
    /// <param name="name">The name of the property.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    [Obsolete("Use HasJsonPropertyName() instead.")]
    public static PrimitiveCollectionBuilder<TProperty> ToJsonProperty<TProperty>(
        this PrimitiveCollectionBuilder<TProperty> primitiveCollectionBuilder,
        string name)
        => (PrimitiveCollectionBuilder<TProperty>)HasJsonPropertyName((PrimitiveCollectionBuilder)primitiveCollectionBuilder, name);
}
