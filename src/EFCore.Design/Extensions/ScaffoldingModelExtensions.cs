// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Metadata.Internal;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore;

/// <summary>
/// Design-time model extensions.
/// </summary>
public static class ScaffoldingModelExtensions
{
    /// <summary>
    /// Check whether an entity type could be considered a many-to-many join entity type.
    /// </summary>
    /// <param name="entityType">The entity type to check.</param>
    /// <returns><see langword="true"/> if the entity type could be considered a join entity type.</returns>
    public static bool IsSimpleManyToManyJoinEntityType(this IEntityType entityType)
    {
        if (!entityType.GetNavigations().Any()
            && !entityType.GetSkipNavigations().Any())
        {
            var primaryKey = entityType.FindPrimaryKey();
            var properties = entityType.GetProperties().ToList();
            var foreignKeys = entityType.GetForeignKeys().ToList();
            if (primaryKey != null
                && primaryKey.Properties.Count > 1
                && foreignKeys.Count == 2
                && primaryKey.Properties.Count == properties.Count
                && foreignKeys[0].Properties.Count + foreignKeys[1].Properties.Count == properties.Count
                && !foreignKeys[0].Properties.Intersect(foreignKeys[1].Properties).Any()
                && foreignKeys[0].IsRequired
                && foreignKeys[1].IsRequired
                && !foreignKeys[0].IsUnique
                && !foreignKeys[1].IsUnique)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Gets the errors encountered while reverse engineering the model.
    /// </summary>
    /// <param name="model">The model.</param>
    /// <returns>The errors.</returns>
    public static IEnumerable<string> GetReverseEngineeringErrors(this IReadOnlyModel model)
        => (IEnumerable<string>?)model[ScaffoldingAnnotationNames.ReverseEngineeringErrors] ?? new List<string>();

    /// <summary>
    /// Gets the name that should be used for the <see cref="DbSet{TEntity}"/> property on the <see cref="DbContext"/> class for this entity type.
    /// </summary>
    /// <param name="entityType">The entity type.</param>
    /// <returns>The property name.</returns>
    public static string GetDbSetName(this IReadOnlyEntityType entityType)
        => (string?)entityType[ScaffoldingAnnotationNames.DbSetName]
            ?? entityType.ShortName();
}
