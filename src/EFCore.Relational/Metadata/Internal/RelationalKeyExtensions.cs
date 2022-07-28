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

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static string? GetName(
        this IReadOnlyKey key,
        in StoreObjectIdentifier storeObject,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation>? logger)
    {
        if (storeObject.StoreObjectType != StoreObjectType.Table)
        {
            return null;
        }

        var defaultName = key.GetDefaultName(storeObject, logger);
        var declaringType = key.DeclaringEntityType;
        var fragment = declaringType.FindMappingFragment(storeObject);
        if (fragment != null)
        {
            return defaultName != null
                ? (string?)key[RelationalAnnotationNames.Name] ?? defaultName
                : null;
        }

        foreach (var containingType in declaringType.GetDerivedTypesInclusive())
        {
            if (StoreObjectIdentifier.Create(containingType, storeObject.StoreObjectType) == storeObject)
            {
                return defaultName != null
                    ? (string?)key[RelationalAnnotationNames.Name] ?? defaultName
                    : null;
            }
        }

        return null;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static string? GetDefaultName(
        this IReadOnlyKey key,
        in StoreObjectIdentifier storeObject,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation>? logger)
    {
        if (storeObject.StoreObjectType != StoreObjectType.Table
            || key.DeclaringEntityType.IsMappedToJson())
        {
            return null;
        }

        if (key.DeclaringEntityType.IsMappedToJson())
        {
            return null;
        }

        string? name;
        if (key.IsPrimaryKey())
        {
            var rootKey = key;
            // Limit traversal to avoid getting stuck in a cycle (validation will throw for these later)
            // Using a hashset is detrimental to the perf when there are no cycles
            for (var i = 0; i < RelationalEntityTypeExtensions.MaxEntityTypesSharingTable; i++)
            {
                var linkingFk = rootKey!.DeclaringEntityType.FindRowInternalForeignKeys(storeObject)
                    .FirstOrDefault();
                if (linkingFk == null)
                {
                    break;
                }

                rootKey = linkingFk.PrincipalEntityType.FindPrimaryKey();
            }

            if (rootKey != null
                && rootKey != key)
            {
                return rootKey.GetName(storeObject);
            }

            name = "PK_" + storeObject.Name;
        }
        else
        {
            var columnNames = key.Properties.GetColumnNames(storeObject);
            if (columnNames == null)
            {
                if (logger != null)
                {
                    var table = storeObject;
                    if (key.DeclaringEntityType.GetMappingFragments(StoreObjectType.Table)
                        .Any(t => t.StoreObject != table && key.Properties.GetColumnNames(t.StoreObject) != null))
                    {
                        return null;
                    }

                    if (key.DeclaringEntityType.GetMappingStrategy() != RelationalAnnotationNames.TphMappingStrategy
                        && key.DeclaringEntityType.GetDerivedTypes()
                            .Select(e => StoreObjectIdentifier.Create(e, StoreObjectType.Table))
                            .Any(t => t != null && key.Properties.GetColumnNames(t.Value) != null))
                    {
                        return null;
                    }

                    logger.KeyPropertiesNotMappedToTable((IKey)key);
                }

                return null;
            }

            var rootKey = key;

            // Limit traversal to avoid getting stuck in a cycle (validation will throw for these later)
            // Using a hashset is detrimental to the perf when there are no cycles
            for (var i = 0; i < RelationalEntityTypeExtensions.MaxEntityTypesSharingTable; i++)
            {
                IReadOnlyKey? linkedKey = null;
                foreach (var otherKey in rootKey.DeclaringEntityType
                             .FindRowInternalForeignKeys(storeObject)
                             .SelectMany(fk => fk.PrincipalEntityType.GetKeys()))
                {
                    var otherColumnNames = otherKey.Properties.GetColumnNames(storeObject);
                    if ((otherColumnNames != null)
                        && otherColumnNames.SequenceEqual(columnNames))
                    {
                        linkedKey = otherKey;
                        break;
                    }
                }

                if (linkedKey == null)
                {
                    break;
                }

                rootKey = linkedKey;
            }

            if (rootKey != key)
            {
                return rootKey.GetName(storeObject);
            }

            name = new StringBuilder()
                .Append("AK_")
                .Append(storeObject.Name)
                .Append('_')
                .AppendJoin(columnNames, "_")
                .ToString();
        }

        return Uniquifier.Truncate(name, key.DeclaringEntityType.Model.GetMaxIdentifierLength());
    }
}
