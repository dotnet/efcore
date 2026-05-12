// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public static class RelationalIndexExtensions
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static bool AreCompatible(
        this IReadOnlyIndex index,
        IReadOnlyIndex duplicateIndex,
        in StoreObjectIdentifier storeObject,
        bool shouldThrow)
    {
        var columnNames = index.Properties.GetColumnNames(storeObject);
        var duplicateColumnNames = duplicateIndex.Properties.GetColumnNames(storeObject);
        if (columnNames == null
            || duplicateColumnNames == null)
        {
            if (shouldThrow)
            {
                throw new InvalidOperationException(
                    RelationalStrings.DuplicateIndexTableMismatch(
                        index.DisplayName(),
                        index.DeclaringEntityType.DisplayName(),
                        duplicateIndex.DisplayName(),
                        duplicateIndex.DeclaringEntityType.DisplayName(),
                        index.GetDatabaseName(storeObject),
                        index.DeclaringEntityType.GetSchemaQualifiedTableName(),
                        duplicateIndex.DeclaringEntityType.GetSchemaQualifiedTableName()));
            }

            return false;
        }

        if (!columnNames.SequenceEqual(duplicateColumnNames))
        {
            if (shouldThrow)
            {
                throw new InvalidOperationException(
                    RelationalStrings.DuplicateIndexColumnMismatch(
                        index.DisplayName(),
                        index.DeclaringEntityType.DisplayName(),
                        duplicateIndex.DisplayName(),
                        duplicateIndex.DeclaringEntityType.DisplayName(),
                        index.DeclaringEntityType.GetSchemaQualifiedTableName(),
                        index.GetDatabaseName(storeObject),
                        index.Properties.FormatColumns(storeObject),
                        duplicateIndex.Properties.FormatColumns(storeObject)));
            }

            return false;
        }

        if (index.IsUnique != duplicateIndex.IsUnique)
        {
            if (shouldThrow)
            {
                throw new InvalidOperationException(
                    RelationalStrings.DuplicateIndexUniquenessMismatch(
                        index.DisplayName(),
                        index.DeclaringEntityType.DisplayName(),
                        duplicateIndex.DisplayName(),
                        duplicateIndex.DeclaringEntityType.DisplayName(),
                        index.DeclaringEntityType.GetSchemaQualifiedTableName(),
                        index.GetDatabaseName(storeObject)));
            }

            return false;
        }

        if (index.IsDescending is null != duplicateIndex.IsDescending is null
            || (index.IsDescending is not null
                && duplicateIndex.IsDescending is not null
                && !index.IsDescending.SequenceEqual(duplicateIndex.IsDescending)))
        {
            if (shouldThrow)
            {
                throw new InvalidOperationException(
                    RelationalStrings.DuplicateIndexSortOrdersMismatch(
                        index.DisplayName(),
                        index.DeclaringEntityType.DisplayName(),
                        duplicateIndex.DisplayName(),
                        duplicateIndex.DeclaringEntityType.DisplayName(),
                        index.DeclaringEntityType.GetSchemaQualifiedTableName(),
                        index.GetDatabaseName(storeObject)));
            }

            return false;
        }

        if (index.GetFilter(storeObject) != duplicateIndex.GetFilter(storeObject))
        {
            if (shouldThrow)
            {
                throw new InvalidOperationException(
                    RelationalStrings.DuplicateIndexFiltersMismatch(
                        index.DisplayName(),
                        index.DeclaringEntityType.DisplayName(),
                        duplicateIndex.DisplayName(),
                        duplicateIndex.DeclaringEntityType.DisplayName(),
                        index.DeclaringEntityType.GetSchemaQualifiedTableName(),
                        index.GetDatabaseName(storeObject),
                        index.GetFilter(),
                        duplicateIndex.GetFilter()));
            }

            return false;
        }

        return true;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static string? GetDatabaseName(
        this IReadOnlyIndex index,
        in StoreObjectIdentifier storeObject,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation>? logger)
    {
        if (storeObject.StoreObjectType != StoreObjectType.Table)
        {
            return null;
        }

        var defaultName = index.GetDefaultDatabaseName(storeObject, logger);
        var annotation = index.FindAnnotation(RelationalAnnotationNames.Name);
        return annotation != null && defaultName != null
            ? (string?)annotation.Value
            : defaultName != null
                ? index.Name ?? defaultName
                : defaultName;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static string? GetDefaultDatabaseName(
        this IReadOnlyIndex index,
        in StoreObjectIdentifier storeObject,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation>? logger)
    {
        if (storeObject.StoreObjectType != StoreObjectType.Table)
        {
            return null;
        }

        var columnNames = index.Properties.GetColumnNames(storeObject);
        if (columnNames == null)
        {
            if (logger != null
                && ((IConventionIndex)index).GetConfigurationSource() != ConfigurationSource.Convention)
            {
                IReadOnlyProperty? propertyNotMappedToAnyTable = null;
                (string, List<StoreObjectIdentifier>)? firstPropertyTables = null;
                (string, List<StoreObjectIdentifier>)? lastPropertyTables = null;
                HashSet<StoreObjectIdentifier>? overlappingTables = null;
                foreach (var property in index.Properties)
                {
                    var tablesMappedToProperty = property.GetMappedStoreObjects(storeObject.StoreObjectType).ToList();
                    if (tablesMappedToProperty.Count == 0)
                    {
                        propertyNotMappedToAnyTable = property;
                        overlappingTables = null;

                        if (firstPropertyTables != null)
                        {
                            // Property is not mapped but we already found a property that is mapped.
                            break;
                        }

                        continue;
                    }

                    if (firstPropertyTables == null)
                    {
                        firstPropertyTables = (property.Name, tablesMappedToProperty);
                    }
                    else
                    {
                        lastPropertyTables = (property.Name, tablesMappedToProperty);
                    }

                    if (propertyNotMappedToAnyTable != null)
                    {
                        // Property is mapped but we already found a property that is not mapped.
                        overlappingTables = null;
                        break;
                    }

                    if (overlappingTables == null)
                    {
                        overlappingTables = [..tablesMappedToProperty];
                    }
                    else
                    {
                        overlappingTables.IntersectWith(tablesMappedToProperty);
                        if (overlappingTables.Count == 0)
                        {
                            break;
                        }
                    }
                }

                if (overlappingTables == null)
                {
                    if (firstPropertyTables == null)
                    {
                        logger.AllIndexPropertiesNotToMappedToAnyTable(
                            (IEntityType)index.DeclaringEntityType,
                            (IIndex)index);
                    }
                    else
                    {
                        logger.IndexPropertiesBothMappedAndNotMappedToTable(
                            (IEntityType)index.DeclaringEntityType,
                            (IIndex)index,
                            propertyNotMappedToAnyTable!.Name);
                    }
                }
                else if (overlappingTables.Count == 0)
                {
                    Check.DebugAssert(firstPropertyTables != null, nameof(firstPropertyTables));
                    Check.DebugAssert(lastPropertyTables != null, nameof(lastPropertyTables));

                    logger.IndexPropertiesMappedToNonOverlappingTables(
                        (IEntityType)index.DeclaringEntityType,
                        (IIndex)index,
                        firstPropertyTables.Value.Item1,
                        firstPropertyTables.Value.Item2.Select(t => (t.Name, t.Schema)).ToList(),
                        lastPropertyTables.Value.Item1,
                        lastPropertyTables.Value.Item2.Select(t => (t.Name, t.Schema)).ToList());
                }
            }

            return null;
        }

        var rootIndex = index;

        // Limit traversal to avoid getting stuck in a cycle (validation will throw for these later)
        // Using a hashset is detrimental to the perf when there are no cycles
        for (var i = 0; i < RelationalEntityTypeExtensions.MaxEntityTypesSharingTable; i++)
        {
            IReadOnlyIndex? linkedIndex = null;
            foreach (var otherIndex in rootIndex.DeclaringEntityType
                         .FindRowInternalForeignKeys(storeObject)
                         .SelectMany(fk => fk.PrincipalEntityType.GetIndexes()))
            {
                var otherColumnNames = otherIndex.Properties.GetColumnNames(storeObject);
                if ((otherColumnNames != null)
                    && otherColumnNames.SequenceEqual(columnNames))
                {
                    linkedIndex = otherIndex;
                    break;
                }
            }

            if (linkedIndex == null)
            {
                break;
            }

            rootIndex = linkedIndex;
        }

        if (rootIndex != index)
        {
            return rootIndex.GetDatabaseName(storeObject);
        }

        var baseName = new StringBuilder()
            .Append("IX_")
            .Append(storeObject.Name)
            .Append('_')
            .AppendJoin(columnNames, "_")
            .ToString();

        return Uniquifier.Truncate(baseName, index.DeclaringEntityType.Model.GetMaxIdentifierLength());
    }
}
