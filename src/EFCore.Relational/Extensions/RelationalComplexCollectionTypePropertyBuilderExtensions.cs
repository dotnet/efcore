// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable once CheckNamespace

namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     Relational database specific extension methods for <see cref="ComplexCollectionTypePropertyBuilder" />.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
/// </remarks>
public static class RelationalComplexCollectionTypePropertyBuilderExtensions
{
    /// <summary>
    ///     Configures the property of an entity mapped to a JSON column, mapping the entity property to a specific JSON property,
    ///     rather than using the entity property name.
    /// </summary>
    /// <param name="propertyBuilder">The builder for the property being configured.</param>
    /// <param name="name">JSON property name to be used.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static ComplexCollectionTypePropertyBuilder HasJsonPropertyName(
        this ComplexCollectionTypePropertyBuilder propertyBuilder,
        string? name)
    {
        Check.NullButNotEmpty(name);

        propertyBuilder.Metadata.SetJsonPropertyName(name);

        return propertyBuilder;
    }

    /// <summary>
    ///     Configures the property of an entity mapped to a JSON column, mapping the entity property to a specific JSON property,
    ///     rather than using the entity property name.
    /// </summary>
    /// <param name="propertyBuilder">The builder for the property being configured.</param>
    /// <param name="name">JSON property name to be used.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static ComplexCollectionTypePropertyBuilder<TProperty> HasJsonPropertyName<TProperty>(
        this ComplexCollectionTypePropertyBuilder<TProperty> propertyBuilder,
        string? name)
        => (ComplexCollectionTypePropertyBuilder<TProperty>)HasJsonPropertyName(
            (ComplexCollectionTypePropertyBuilder)propertyBuilder, name);
}
