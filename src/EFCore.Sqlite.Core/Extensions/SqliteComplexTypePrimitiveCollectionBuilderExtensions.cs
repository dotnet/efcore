// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     SQLite-specific extension methods for <see cref="ComplexTypePrimitiveCollectionBuilder" />.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
///     <see href="https://aka.ms/efcore-docs-sqlite">Accessing SQLite databases with EF Core</see> for more information and examples.
/// </remarks>
public static class SqliteComplexTypePrimitiveCollectionBuilderExtensions
{
    /// <summary>
    ///     Configures the SRID of the column that the property maps to when targeting SQLite.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-spatial">Spatial data</see>, and
    ///     <see href="https://aka.ms/efcore-docs-sqlite">Accessing SQLite databases with EF Core</see> for more information and examples.
    /// </remarks>
    /// <param name="primitiveCollectionBuilder">The builder for the property being configured.</param>
    /// <param name="srid">The SRID.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static ComplexTypePrimitiveCollectionBuilder HasSrid(
        this ComplexTypePrimitiveCollectionBuilder primitiveCollectionBuilder,
        int srid)
    {
        primitiveCollectionBuilder.Metadata.SetSrid(srid);

        return primitiveCollectionBuilder;
    }

    /// <summary>
    ///     Configures the SRID of the column that the property maps to when targeting SQLite.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-spatial">Spatial data</see>, and
    ///     <see href="https://aka.ms/efcore-docs-sqlite">Accessing SQLite databases with EF Core</see> for more information and examples.
    /// </remarks>
    /// <param name="primitiveCollectionBuilder">The builder for the property being configured.</param>
    /// <param name="srid">The SRID.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static ComplexTypePrimitiveCollectionBuilder<TProperty> HasSrid<TProperty>(
        this ComplexTypePrimitiveCollectionBuilder<TProperty> primitiveCollectionBuilder,
        int srid)
        => (ComplexTypePrimitiveCollectionBuilder<TProperty>)HasSrid(
            (ComplexTypePrimitiveCollectionBuilder)primitiveCollectionBuilder, srid);
}
