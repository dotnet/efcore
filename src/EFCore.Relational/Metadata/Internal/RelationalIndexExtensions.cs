// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

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
}
