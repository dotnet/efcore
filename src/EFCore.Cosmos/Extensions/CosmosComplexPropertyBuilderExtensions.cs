// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     Cosmos-specific extension methods for <see cref="ComplexPropertyBuilder" />.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
///     <see href="https://aka.ms/efcore-docs-cosmos">Accessing Azure Cosmos DB with EF Core</see> for more information and examples.
/// </remarks>
public static class CosmosComplexPropertyBuilderExtensions
{
    /// <summary>
    ///     Configures the property name that the complex property is mapped to when stored as an embedded document.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
    ///     <see href="https://aka.ms/efcore-docs-cosmos">Accessing Azure Cosmos DB with EF Core</see> for more information and examples.
    /// </remarks>
    /// <param name="complexPropertyBuilder">The builder for the complex type being configured.</param>
    /// <param name="name">The name of the parent property.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static ComplexPropertyBuilder ToJsonProperty(
        this ComplexPropertyBuilder complexPropertyBuilder,
        string? name)
    {
        complexPropertyBuilder.Metadata.SetJsonPropertyName(name);
        return complexPropertyBuilder;
    }

    /// <summary>
    ///     Configures the property name that the complex property is mapped to when stored as an embedded document.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
    ///     <see href="https://aka.ms/efcore-docs-cosmos">Accessing Azure Cosmos DB with EF Core</see> for more information and examples.
    /// </remarks>
    /// <param name="complexPropertyBuilder">The builder for the complex type being configured.</param>
    /// <param name="name">The name of the parent property.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static ComplexPropertyBuilder<TComplex> ToJsonProperty<TComplex>(
        this ComplexPropertyBuilder<TComplex> complexPropertyBuilder,
        string? name)
        where TComplex : notnull
    {
        complexPropertyBuilder.Metadata.SetJsonPropertyName(name);
        return complexPropertyBuilder;
    }
}
