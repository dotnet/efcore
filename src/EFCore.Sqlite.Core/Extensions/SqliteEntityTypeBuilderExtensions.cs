// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Sqlite.Metadata.Internal;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     Entity type builder extension methods for Sqlite-specific metadata.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
///     <see href="https://aka.ms/efcore-docs-sqlite">Accessing Sqlite databases with EF Core</see> for more information and examples.
/// </remarks>
public static class SqliteEntityTypeBuilderExtensions
{
    /// <summary>
    ///     Sets a value indicating whether to use the SQL RETURNING clause when saving changes to the table.
    ///     The RETURNING clause is incompatible with certain Sqlite features, such as virtual tables or tables with AFTER triggers.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <param name="entityTypeBuilder">The builder for the entity type being configured.</param>
    /// <param name="useSqlReturningClause">The value to set.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     The same builder instance if the configuration was applied,
    ///     <see langword="null" /> otherwise.
    /// </returns>
    public static IConventionEntityTypeBuilder? UseSqlReturningClause(
        this IConventionEntityTypeBuilder entityTypeBuilder,
        bool? useSqlReturningClause,
        bool fromDataAnnotation = false)
    {
        if (!entityTypeBuilder.CanUseSqlReturningClause(useSqlReturningClause, fromDataAnnotation))
        {
            return null;
        }

        entityTypeBuilder.Metadata.UseSqlReturningClause(useSqlReturningClause, fromDataAnnotation);
        return entityTypeBuilder;
    }

    /// <summary>
    ///     Sets a value indicating whether to use the SQL RETURNING clause when saving changes to the table.
    ///     The RETURNING clause is incompatible with certain Sqlite features, such as virtual tables or tables with AFTER triggers.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <param name="entityTypeBuilder">The builder for the entity type being configured.</param>
    /// <param name="useSqlReturningClause">The value to set.</param>
    /// <param name="storeObject">The identifier of the table-like store object.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     The same builder instance if the configuration was applied,
    ///     <see langword="null" /> otherwise.
    /// </returns>
    public static IConventionEntityTypeBuilder? UseSqlReturningClause(
        this IConventionEntityTypeBuilder entityTypeBuilder,
        bool? useSqlReturningClause,
        in StoreObjectIdentifier storeObject,
        bool fromDataAnnotation = false)
    {
        if (!entityTypeBuilder.CanUseSqlReturningClause(useSqlReturningClause, storeObject, fromDataAnnotation))
        {
            return null;
        }

        entityTypeBuilder.Metadata.UseSqlReturningClause(useSqlReturningClause, storeObject, fromDataAnnotation);
        return entityTypeBuilder;
    }

    /// <summary>
    ///     Returns a value indicating whether this entity type can be configured to use the SQL RETURNING clause
    ///     using the specified configuration source.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <param name="entityTypeBuilder">The builder for the entity type being configured.</param>
    /// <param name="useSqlReturningClause">The value to set.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns><see langword="true" /> if the configuration can be applied.</returns>
    public static bool CanUseSqlReturningClause(
        this IConventionEntityTypeBuilder entityTypeBuilder,
        bool? useSqlReturningClause,
        bool fromDataAnnotation = false)
        => entityTypeBuilder.CanSetAnnotation(
            SqliteAnnotationNames.UseSqlReturningClause,
            useSqlReturningClause,
            fromDataAnnotation);

    /// <summary>
    ///     Returns a value indicating whether this entity type can be configured to use the SQL RETURNING clause
    ///     using the specified configuration source.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <param name="entityTypeBuilder">The builder for the entity type being configured.</param>
    /// <param name="useSqlReturningClause">The value to set.</param>
    /// <param name="storeObject">The identifier of the table-like store object.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns><see langword="true" /> if the configuration can be applied.</returns>
    public static bool CanUseSqlReturningClause(
        this IConventionEntityTypeBuilder entityTypeBuilder,
        bool? useSqlReturningClause,
        in StoreObjectIdentifier storeObject,
        bool fromDataAnnotation = false)
        => StoreObjectIdentifier.Create(entityTypeBuilder.Metadata, storeObject.StoreObjectType) == storeObject
            ? entityTypeBuilder.CanSetAnnotation(
                SqliteAnnotationNames.UseSqlReturningClause,
                useSqlReturningClause,
                fromDataAnnotation)
            : entityTypeBuilder.Metadata.GetOrCreateMappingFragment(storeObject, fromDataAnnotation).Builder.CanSetAnnotation(
                SqliteAnnotationNames.UseSqlReturningClause,
                useSqlReturningClause,
                fromDataAnnotation);
}
