// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     Cosmos-specific extension methods for <see cref="ComplexCollectionBuilder" />.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
///     <see href="https://aka.ms/efcore-docs-cosmos">Accessing Azure Cosmos DB with EF Core</see> for more information and examples.
/// </remarks>
public static class CosmosComplexCollectionBuilderExtensions
{
    /// <summary>
    ///     Configures the property name that the complex collection is mapped to when stored as an embedded document.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
    ///     <see href="https://aka.ms/efcore-docs-cosmos">Accessing Azure Cosmos DB with EF Core</see> for more information and examples.
    /// </remarks>
    /// <param name="complexPropertyBuilder">The builder for the complex type being configured.</param>
    /// <param name="name">The name of the parent property.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static ComplexCollectionBuilder ToJsonProperty(
        this ComplexCollectionBuilder complexPropertyBuilder,
        string? name)
    {
        complexPropertyBuilder.Metadata.SetJsonPropertyName(name);
        return complexPropertyBuilder;
    }

    /// <summary>
    ///     Configures the property name that the complex collection is mapped to when stored as an embedded document.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
    ///     <see href="https://aka.ms/efcore-docs-cosmos">Accessing Azure Cosmos DB with EF Core</see> for more information and examples.
    /// </remarks>
    /// <param name="complexPropertyBuilder">The builder for the complex type being configured.</param>
    /// <param name="name">The name of the parent property.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static ComplexCollectionBuilder<TComplex> ToJsonProperty<TComplex>(
        this ComplexCollectionBuilder<TComplex> complexPropertyBuilder,
        string? name)
        where TComplex : notnull
    {
        complexPropertyBuilder.Metadata.SetJsonPropertyName(name);
        return complexPropertyBuilder;
    }
}
