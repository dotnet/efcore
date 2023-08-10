// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     SQLite-specific extension methods for <see cref="ComplexTypePropertyBuilder" />.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
///     <see href="https://aka.ms/efcore-docs-sqlite">Accessing SQLite databases with EF Core</see> for more information and examples.
/// </remarks>
public static class SqliteComplexTypePropertyBuilderExtensions
{
    /// <summary>
    ///     Configures the SRID of the column that the property maps to when targeting SQLite.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-spatial">Spatial data</see>, and
    ///     <see href="https://aka.ms/efcore-docs-sqlite">Accessing SQLite databases with EF Core</see> for more information and examples.
    /// </remarks>
    /// <param name="propertyBuilder">The builder for the property being configured.</param>
    /// <param name="srid">The SRID.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static ComplexTypePropertyBuilder HasSrid(this ComplexTypePropertyBuilder propertyBuilder, int srid)
    {
        propertyBuilder.Metadata.SetSrid(srid);

        return propertyBuilder;
    }

    /// <summary>
    ///     Configures the SRID of the column that the property maps to when targeting SQLite.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-spatial">Spatial data</see>, and
    ///     <see href="https://aka.ms/efcore-docs-sqlite">Accessing SQLite databases with EF Core</see> for more information and examples.
    /// </remarks>
    /// <param name="propertyBuilder">The builder for the property being configured.</param>
    /// <param name="srid">The SRID.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static ComplexTypePropertyBuilder<TProperty> HasSrid<TProperty>(
        this ComplexTypePropertyBuilder<TProperty> propertyBuilder,
        int srid)
        => (ComplexTypePropertyBuilder<TProperty>)HasSrid((ComplexTypePropertyBuilder)propertyBuilder, srid);
}
