// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public static class RelationalKeyExtensions
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static bool AreCompatible(
        this IReadOnlyKey key,
        IReadOnlyKey duplicateKey,
        in StoreObjectIdentifier storeObject,
        bool shouldThrow)
    {
        var columnNames = key.Properties.GetColumnNames(storeObject);
        var duplicateColumnNames = duplicateKey.Properties.GetColumnNames(storeObject);
        if (columnNames == null
            || duplicateColumnNames == null)
        {
            if (shouldThrow)
            {
                throw new InvalidOperationException(
                    RelationalStrings.DuplicateKeyTableMismatch(
                        key.Properties.Format(),
                        key.DeclaringEntityType.DisplayName(),
                        duplicateKey.Properties.Format(),
                        duplicateKey.DeclaringEntityType.DisplayName(),
                        key.GetName(storeObject),
                        key.DeclaringEntityType.GetSchemaQualifiedTableName(),
                        duplicateKey.DeclaringEntityType.GetSchemaQualifiedTableName()));
            }

            return false;
        }

        if (!columnNames.SequenceEqual(duplicateColumnNames))
        {
            if (shouldThrow)
            {
                throw new InvalidOperationException(
                    RelationalStrings.DuplicateKeyColumnMismatch(
                        key.Properties.Format(),
                        key.DeclaringEntityType.DisplayName(),
                        duplicateKey.Properties.Format(),
                        duplicateKey.DeclaringEntityType.DisplayName(),
                        key.DeclaringEntityType.GetSchemaQualifiedTableName(),
                        key.GetName(storeObject),
                        key.Properties.FormatColumns(storeObject),
                        duplicateKey.Properties.FormatColumns(storeObject)));
            }

            return false;
        }

        return true;
    }
}
