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
public static class RelationalForeignKeyExtensions
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static bool AreCompatible(
        this IReadOnlyForeignKey foreignKey,
        IReadOnlyForeignKey duplicateForeignKey,
        in StoreObjectIdentifier storeObject,
        bool shouldThrow)
    {
        var principalType = foreignKey.PrincipalKey.IsPrimaryKey()
            ? foreignKey.PrincipalEntityType
            : foreignKey.PrincipalKey.DeclaringEntityType;
        var principalTable = StoreObjectIdentifier.Create(principalType, storeObject.StoreObjectType);

        var duplicatePrincipalType = duplicateForeignKey.PrincipalKey.IsPrimaryKey()
            ? duplicateForeignKey.PrincipalEntityType
            : duplicateForeignKey.PrincipalKey.DeclaringEntityType;
        var duplicatePrincipalTable = StoreObjectIdentifier.Create(duplicatePrincipalType, storeObject.StoreObjectType);

        var columnNames = foreignKey.Properties.GetColumnNames(storeObject);
        var duplicateColumnNames = duplicateForeignKey.Properties.GetColumnNames(storeObject);
        if (columnNames is null
            || duplicateColumnNames is null)
        {
            if (shouldThrow)
            {
                throw new InvalidOperationException(
                    RelationalStrings.DuplicateForeignKeyTableMismatch(
                        foreignKey.Properties.Format(),
                        foreignKey.DeclaringEntityType.DisplayName(),
                        duplicateForeignKey.Properties.Format(),
                        duplicateForeignKey.DeclaringEntityType.DisplayName(),
                        principalTable.HasValue
                            ? foreignKey.GetConstraintName(storeObject, principalTable.Value)
                            : foreignKey.GetDefaultName(),
                        foreignKey.DeclaringEntityType.GetSchemaQualifiedTableName(),
                        duplicateForeignKey.DeclaringEntityType.GetSchemaQualifiedTableName()));
            }

            return false;
        }

        if (principalTable is null
            || duplicatePrincipalTable is null
            || principalTable != duplicatePrincipalTable
            || foreignKey.PrincipalKey.Properties.GetColumnNames(principalTable.Value)
                is not { } principalColumns
            || duplicateForeignKey.PrincipalKey.Properties.GetColumnNames(principalTable.Value)
                is not { } duplicatePrincipalColumns)
        {
            if (shouldThrow)
            {
                throw new InvalidOperationException(
                    RelationalStrings.DuplicateForeignKeyPrincipalTableMismatch(
                        foreignKey.Properties.Format(),
                        foreignKey.DeclaringEntityType.DisplayName(),
                        duplicateForeignKey.Properties.Format(),
                        duplicateForeignKey.DeclaringEntityType.DisplayName(),
                        foreignKey.DeclaringEntityType.GetSchemaQualifiedTableName(),
                        principalTable.HasValue
                            ? foreignKey.GetConstraintName(storeObject, principalTable.Value)
                            : foreignKey.GetDefaultName(),
                        principalType.GetSchemaQualifiedTableName(),
                        duplicatePrincipalType.GetSchemaQualifiedTableName()));
            }

            return false;
        }

        if (!columnNames.SequenceEqual(duplicateColumnNames))
        {
            if (shouldThrow)
            {
                throw new InvalidOperationException(
                    RelationalStrings.DuplicateForeignKeyColumnMismatch(
                        foreignKey.Properties.Format(),
                        foreignKey.DeclaringEntityType.DisplayName(),
                        duplicateForeignKey.Properties.Format(),
                        duplicateForeignKey.DeclaringEntityType.DisplayName(),
                        foreignKey.DeclaringEntityType.GetSchemaQualifiedTableName(),
                        foreignKey.GetConstraintName(storeObject, principalTable.Value),
                        foreignKey.Properties.FormatColumns(storeObject),
                        duplicateForeignKey.Properties.FormatColumns(storeObject)));
            }

            return false;
        }

        if (!principalColumns.SequenceEqual(duplicatePrincipalColumns))
        {
            if (shouldThrow)
            {
                throw new InvalidOperationException(
                    RelationalStrings.DuplicateForeignKeyPrincipalColumnMismatch(
                        foreignKey.Properties.Format(),
                        foreignKey.DeclaringEntityType.DisplayName(),
                        duplicateForeignKey.Properties.Format(),
                        duplicateForeignKey.DeclaringEntityType.DisplayName(),
                        foreignKey.DeclaringEntityType.GetSchemaQualifiedTableName(),
                        foreignKey.GetConstraintName(storeObject, principalTable.Value),
                        foreignKey.PrincipalKey.Properties.FormatColumns(principalTable.Value),
                        duplicateForeignKey.PrincipalKey.Properties.FormatColumns(principalTable.Value)));
            }

            return false;
        }

        if (foreignKey.IsUnique != duplicateForeignKey.IsUnique)
        {
            if (shouldThrow)
            {
                throw new InvalidOperationException(
                    RelationalStrings.DuplicateForeignKeyUniquenessMismatch(
                        foreignKey.Properties.Format(),
                        foreignKey.DeclaringEntityType.DisplayName(),
                        duplicateForeignKey.Properties.Format(),
                        duplicateForeignKey.DeclaringEntityType.DisplayName(),
                        foreignKey.DeclaringEntityType.GetSchemaQualifiedTableName(),
                        foreignKey.GetConstraintName(storeObject, principalTable.Value)));
            }

            return false;
        }

        var referentialAction = RelationalModel.ToReferentialAction(foreignKey.DeleteBehavior);
        var duplicateReferentialAction = RelationalModel.ToReferentialAction(duplicateForeignKey.DeleteBehavior);
        if (referentialAction != duplicateReferentialAction)
        {
            if (shouldThrow)
            {
                throw new InvalidOperationException(
                    RelationalStrings.DuplicateForeignKeyDeleteBehaviorMismatch(
                        foreignKey.Properties.Format(),
                        foreignKey.DeclaringEntityType.DisplayName(),
                        duplicateForeignKey.Properties.Format(),
                        duplicateForeignKey.DeclaringEntityType.DisplayName(),
                        foreignKey.DeclaringEntityType.GetSchemaQualifiedTableName(),
                        foreignKey.GetConstraintName(storeObject, principalTable.Value),
                        referentialAction,
                        duplicateReferentialAction));
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
    public static string? GetConstraintName(
        this IReadOnlyForeignKey foreignKey,
        in StoreObjectIdentifier storeObject,
        in StoreObjectIdentifier principalStoreObject,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation>? logger)
    {
        if (storeObject.StoreObjectType != StoreObjectType.Table
            || principalStoreObject.StoreObjectType != StoreObjectType.Table)
        {
            return null;
        }

        var defaultName = foreignKey.GetDefaultName(storeObject, principalStoreObject, logger);
        var annotation = foreignKey.FindAnnotation(RelationalAnnotationNames.Name);
        return annotation != null && defaultName != null
            ? (string?)annotation.Value
            : defaultName;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static string? GetDefaultName(
        this IReadOnlyForeignKey foreignKey,
        in StoreObjectIdentifier storeObject,
        in StoreObjectIdentifier principalStoreObject,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation>? logger)
    {
        if (storeObject.StoreObjectType != StoreObjectType.Table
            || principalStoreObject.StoreObjectType != StoreObjectType.Table
            || foreignKey.DeclaringEntityType.IsMappedToJson())
        {
            return null;
        }

        var propertyNames = foreignKey.Properties.GetColumnNames(storeObject);
        var principalPropertyNames = foreignKey.PrincipalKey.Properties.GetColumnNames(principalStoreObject);
        if (propertyNames == null
            || principalPropertyNames == null)
        {
            if (logger != null)
            {
                var principalTable = principalStoreObject;
                var derivedTables = foreignKey.DeclaringEntityType.GetDerivedTypes()
                    .Select(t => StoreObjectIdentifier.Create(t, StoreObjectType.Table))
                    .Where(t => t != null);
                if (foreignKey.GetConstraintName() != null
                    && derivedTables.All(
                        t => foreignKey.GetConstraintName(
                                t!.Value,
                                principalTable)
                            == null))
                {
                    logger.ForeignKeyPropertiesMappedToUnrelatedTables((IForeignKey)foreignKey);
                }
            }

            return null;
        }

        if (foreignKey.PrincipalEntityType.GetMappingStrategy() == RelationalAnnotationNames.TpcMappingStrategy
            && foreignKey.PrincipalEntityType.GetDerivedTypes().Any(et => StoreObjectIdentifier.Create(et, StoreObjectType.Table) != null))
        {
            logger?.ForeignKeyTpcPrincipalWarning((IForeignKey)foreignKey);
            return null;
        }

        if (storeObject == principalStoreObject
            && propertyNames.SequenceEqual(principalPropertyNames))
        {
            // Redundant FK
            return null;
        }

        var rootForeignKey = foreignKey;

        // Limit traversal to avoid getting stuck in a cycle (validation will throw for these later)
        // Using a hashset is detrimental to the perf when there are no cycles
        for (var i = 0; i < RelationalEntityTypeExtensions.MaxEntityTypesSharingTable; i++)
        {
            IReadOnlyForeignKey? linkedForeignKey = null;
            foreach (var otherForeignKey in rootForeignKey.DeclaringEntityType
                         .FindRowInternalForeignKeys(storeObject)
                         .SelectMany(fk => fk.PrincipalEntityType.GetForeignKeys()))
            {
                if (principalStoreObject.Name == otherForeignKey.PrincipalEntityType.GetTableName()
                    && principalStoreObject.Schema == otherForeignKey.PrincipalEntityType.GetSchema())
                {
                    var otherColumnNames = otherForeignKey.Properties.GetColumnNames(storeObject);
                    var otherPrincipalColumnNames = otherForeignKey.PrincipalKey.Properties.GetColumnNames(principalStoreObject);
                    if (otherColumnNames != null
                        && otherPrincipalColumnNames != null
                        && propertyNames.SequenceEqual(otherColumnNames)
                        && principalPropertyNames.SequenceEqual(otherPrincipalColumnNames))
                    {
                        var nameAnnotation = otherForeignKey.FindAnnotation(RelationalAnnotationNames.Name);
                        if (nameAnnotation != null)
                        {
                            return (string?)nameAnnotation.Value;
                        }

                        linkedForeignKey = otherForeignKey;
                        break;
                    }
                }
            }

            if (linkedForeignKey == null)
            {
                break;
            }

            rootForeignKey = linkedForeignKey;
        }

        var onDependentMainFragment = foreignKey.DeclaringEntityType.IsMainFragment(storeObject);
        var onPrincipalMainFragment = foreignKey.PrincipalEntityType.IsMainFragment(principalStoreObject);
        if (foreignKey.PrincipalKey.IsPrimaryKey()
            && foreignKey.DeclaringEntityType.FindPrimaryKey() is IKey pk
            && foreignKey.Properties.SequenceEqual(pk.Properties))
        {
            if (!foreignKey.PrincipalEntityType.IsAssignableFrom(foreignKey.DeclaringEntityType)
                && (!onDependentMainFragment
                    || !onPrincipalMainFragment)
                && ShareAnyFragments(foreignKey.DeclaringEntityType, foreignKey.PrincipalEntityType))
            {
                // Only create table-sharing linking FKs between the main fragments
                return null;
            }

            if (foreignKey.PrincipalEntityType == foreignKey.DeclaringEntityType
                && !onPrincipalMainFragment)
            {
                // Only create entity-splitting linking FKs to the main fragment
                return null;
            }
        }

        if (foreignKey.DeclaringEntityType.GetMappingStrategy() == RelationalAnnotationNames.TptMappingStrategy
            && !onDependentMainFragment
            && foreignKey.DeclaringEntityType.FindPrimaryKey() is IKey primaryKey
            && foreignKey.Properties.SequenceEqual(primaryKey.Properties))
        {
            // The identifying FK constraint is needed to be created only on the table that corresponds
            // to the least derived mapped entity type
            return null;
        }

        var baseName = new StringBuilder()
            .Append("FK_")
            .Append(storeObject.Name)
            .Append('_')
            .Append(principalStoreObject.Name)
            .Append('_')
            .AppendJoin(propertyNames, "_")
            .ToString();

        return Uniquifier.Truncate(baseName, foreignKey.DeclaringEntityType.Model.GetMaxIdentifierLength());

        static bool ShareAnyFragments(IReadOnlyEntityType entityType1, IReadOnlyEntityType entityType2)
        {
            var commonTables = GetMappedStoreObjects(entityType1, StoreObjectType.Table);
            commonTables.IntersectWith(GetMappedStoreObjects(entityType2, StoreObjectType.Table));
            return commonTables.Any();
        }

        static HashSet<StoreObjectIdentifier> GetMappedStoreObjects(
            IReadOnlyTypeBase type,
            StoreObjectType storeObjectType)
            => AddMappedStoreObjects(type, storeObjectType, []);

        static HashSet<StoreObjectIdentifier> AddMappedStoreObjects(
            IReadOnlyTypeBase type,
            StoreObjectType storeObjectType,
            HashSet<StoreObjectIdentifier> storeObjects)
        {
            var mainStoreObject = StoreObjectIdentifier.Create(type, storeObjectType);
            if (mainStoreObject != null)
            {
                storeObjects.Add(mainStoreObject.Value);
                storeObjects.UnionWith(type.GetMappingFragments(StoreObjectType.Table).Select(f => f.StoreObject));
                return storeObjects;
            }

            if (storeObjectType is StoreObjectType.Function or StoreObjectType.SqlQuery)
            {
                return storeObjects;
            }

            if (type is IReadOnlyEntityType entityType)
            {
                foreach (var derivedType in entityType.GetDirectlyDerivedTypes())
                {
                    AddMappedStoreObjects(derivedType, storeObjectType, storeObjects);
                }
            }

            return storeObjects;
        }
    }
}
