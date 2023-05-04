// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Sqlite.Metadata.Internal;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     Entity type extension methods for Sqlite-specific metadata.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
///     <see href="https://aka.ms/efcore-docs-sqlite">Accessing Sqlite databases with EF Core</see> for more information and examples.
/// </remarks>
public static class SqliteEntityTypeExtensions
{
    /// <summary>
    ///     Returns a value indicating whether to use the SQL RETURNING clause when saving changes to the table.
    ///     The RETURNING clause is incompatible with certain Sqlite features, such as virtual tables or tables with AFTER triggers.
    /// </summary>
    /// <param name="entityType">The entity type.</param>
    /// <returns><see langword="true" /> if the SQL RETURNING clause is used to save changes to the table.</returns>
    public static bool IsSqlReturningClauseUsed(this IReadOnlyEntityType entityType)
    {
        if (entityType.FindAnnotation(SqliteAnnotationNames.UseSqlReturningClause) is { Value: bool useSqlOutputClause })
        {
            return useSqlOutputClause;
        }

        if (entityType.FindOwnership() is { } ownership
            && StoreObjectIdentifier.Create(entityType, StoreObjectType.Table) is { } tableIdentifier
            && ownership.FindSharedObjectRootForeignKey(tableIdentifier) is { } rootForeignKey)
        {
            return rootForeignKey.PrincipalEntityType.IsSqlReturningClauseUsed();
        }

        if (entityType.BaseType is not null && entityType.GetMappingStrategy() == RelationalAnnotationNames.TphMappingStrategy)
        {
            return entityType.GetRootType().IsSqlReturningClauseUsed();
        }

        return true;
    }

    /// <summary>
    ///     Sets a value indicating whether to use the SQL RETURNING clause when saving changes to the table.
    ///     The RETURNING clause is incompatible with certain Sqlite features, such as virtual tables or tables with AFTER triggers.
    /// </summary>
    /// <param name="entityType">The entity type.</param>
    /// <param name="useSqlReturningClause">The value to set.</param>
    public static void UseSqlReturningClause(this IMutableEntityType entityType, bool? useSqlReturningClause)
        => entityType.SetOrRemoveAnnotation(SqliteAnnotationNames.UseSqlReturningClause, useSqlReturningClause);

    /// <summary>
    ///     Sets a value indicating whether to use the SQL RETURNING clause when saving changes to the table.
    ///     The RETURNING clause is incompatible with certain Sqlite features, such as virtual tables or tables with AFTER triggers.
    /// </summary>
    /// <param name="entityType">The entity type.</param>
    /// <param name="useSqlReturningClause">The value to set.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The configured value.</returns>
    public static bool? UseSqlReturningClause(
        this IConventionEntityType entityType,
        bool? useSqlReturningClause,
        bool fromDataAnnotation = false)
        => (bool?)entityType.SetOrRemoveAnnotation(
            SqliteAnnotationNames.UseSqlReturningClause,
            useSqlReturningClause,
            fromDataAnnotation)?.Value;

    /// <summary>
    ///     Gets the configuration source for whether to use the SQL RETURNING clause when saving changes to the table.
    /// </summary>
    /// <param name="entityType">The entity type.</param>
    /// <returns>The configuration source for the memory-optimized setting.</returns>
    public static ConfigurationSource? GetUseSqlReturningClauseConfigurationSource(this IConventionEntityType entityType)
        => entityType.FindAnnotation(SqliteAnnotationNames.UseSqlReturningClause)?.GetConfigurationSource();

    /// <summary>
    ///     Returns a value indicating whether to use the SQL RETURNING clause when saving changes to the table.
    ///     The RETURNING clause is incompatible with certain Sqlite features, such as virtual tables or tables with AFTER triggers.
    /// </summary>
    /// <param name="entityType">The entity type.</param>
    /// <param name="storeObject">The identifier of the table-like store object.</param>
    /// <returns><see langword="true" /> if the SQL RETURNING clause is used to save changes to the table.</returns>
    public static bool IsSqlReturningClauseUsed(this IReadOnlyEntityType entityType, in StoreObjectIdentifier storeObject)
    {
        if (entityType.FindMappingFragment(storeObject) is { } overrides
            && overrides.FindAnnotation(SqliteAnnotationNames.UseSqlReturningClause) is { Value: bool useSqlOutputClause })
        {
            return useSqlOutputClause;
        }

        if (StoreObjectIdentifier.Create(entityType, storeObject.StoreObjectType) == storeObject)
        {
            return entityType.IsSqlReturningClauseUsed();
        }

        if (entityType.FindOwnership() is { } ownership
            && ownership.FindSharedObjectRootForeignKey(storeObject) is { } rootForeignKey)
        {
            return rootForeignKey.PrincipalEntityType.IsSqlReturningClauseUsed(storeObject);
        }

        if (entityType.BaseType is not null && entityType.GetMappingStrategy() == RelationalAnnotationNames.TphMappingStrategy)
        {
            return entityType.GetRootType().IsSqlReturningClauseUsed(storeObject);
        }

        return true;
    }

    /// <summary>
    ///     Sets a value indicating whether to use the SQL RETURNING clause when saving changes to the table.
    ///     The RETURNING clause is incompatible with certain Sqlite features, such as virtual tables or tables with AFTER triggers.
    /// </summary>
    /// <param name="entityType">The entity type.</param>
    /// <param name="useSqlReturningClause">The value to set.</param>
    /// <param name="storeObject">The identifier of the table-like store object.</param>
    public static void UseSqlReturningClause(
        this IMutableEntityType entityType,
        bool? useSqlReturningClause,
        in StoreObjectIdentifier storeObject)
    {
        if (StoreObjectIdentifier.Create(entityType, storeObject.StoreObjectType) == storeObject)
        {
            entityType.UseSqlReturningClause(useSqlReturningClause);
            return;
        }

        entityType
            .GetOrCreateMappingFragment(storeObject)
            .UseSqlReturningClause(useSqlReturningClause);
    }

    /// <summary>
    ///     Sets a value indicating whether to use the SQL RETURNING clause when saving changes to the table.
    ///     The RETURNING clause is incompatible with certain Sqlite features, such as virtual tables or tables with AFTER triggers.
    /// </summary>
    /// <param name="entityType">The entity type.</param>
    /// <param name="useSqlReturningClause">The value to set.</param>
    /// <param name="storeObject">The identifier of the table-like store object.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The configured value.</returns>
    public static bool? UseSqlReturningClause(
        this IConventionEntityType entityType,
        bool? useSqlReturningClause,
        in StoreObjectIdentifier storeObject,
        bool fromDataAnnotation = false)
        => StoreObjectIdentifier.Create(entityType, storeObject.StoreObjectType) == storeObject
            ? entityType.UseSqlReturningClause(useSqlReturningClause, fromDataAnnotation)
            : entityType
                .GetOrCreateMappingFragment(storeObject, fromDataAnnotation)
                .UseSqlReturningClause(useSqlReturningClause, fromDataAnnotation);
}
