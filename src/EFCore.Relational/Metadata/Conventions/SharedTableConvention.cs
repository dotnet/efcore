// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions;

/// <summary>
///     A convention that manipulates names of database objects for entity types that share a table to avoid clashes.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see> for more information and examples.
/// </remarks>
public class SharedTableConvention : IModelFinalizingConvention
{
    /// <summary>
    ///     Creates a new instance of <see cref="SharedTableConvention" />.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this convention.</param>
    /// <param name="relationalDependencies"> Parameter object containing relational dependencies for this convention.</param>
    public SharedTableConvention(
        ProviderConventionSetBuilderDependencies dependencies,
        RelationalConventionSetBuilderDependencies relationalDependencies)
    {
        Dependencies = dependencies;
        RelationalDependencies = relationalDependencies;
    }

    /// <summary>
    ///     Dependencies for this service.
    /// </summary>
    protected virtual ProviderConventionSetBuilderDependencies Dependencies { get; }

    /// <summary>
    ///     Relational provider-specific dependencies for this service.
    /// </summary>
    protected virtual RelationalConventionSetBuilderDependencies RelationalDependencies { get; }

    /// <inheritdoc />
    public virtual void ProcessModelFinalizing(
        IConventionModelBuilder modelBuilder,
        IConventionContext<IConventionModelBuilder> context)
    {
        var maxLength = modelBuilder.Metadata.GetMaxIdentifierLength();
        var tables = new Dictionary<(string TableName, string? Schema), List<IConventionEntityType>>();

        TryUniquifyTableNames(modelBuilder.Metadata, tables, maxLength);

        var columns = new Dictionary<string, IConventionProperty>();
        var keys = new Dictionary<string, (IConventionKey, StoreObjectIdentifier)>();
        var foreignKeys = new Dictionary<string, (IConventionForeignKey, StoreObjectIdentifier)>();
        var indexes = new Dictionary<string, (IConventionIndex, StoreObjectIdentifier)>();
        var checkConstraints = new Dictionary<(string, string?), (IConventionCheckConstraint, StoreObjectIdentifier)>();
        var triggers = new Dictionary<string, (IConventionTrigger, StoreObjectIdentifier)>();
        foreach (var ((tableName, schema), conventionEntityTypes) in tables)
        {
            columns.Clear();

            if (!KeysUniqueAcrossTables)
            {
                keys.Clear();
            }

            if (!ForeignKeysUniqueAcrossTables)
            {
                foreignKeys.Clear();
            }

            if (!IndexesUniqueAcrossTables)
            {
                indexes.Clear();
            }

            if (!CheckConstraintsUniqueAcrossTables)
            {
                checkConstraints.Clear();
            }

            if (!TriggersUniqueAcrossTables)
            {
                triggers.Clear();
            }

            var storeObject = StoreObjectIdentifier.Table(tableName, schema);
            foreach (var entityType in conventionEntityTypes)
            {
                UniquifyColumnNames(entityType, columns, storeObject, maxLength);
                UniquifyKeyNames(entityType, keys, storeObject, maxLength);
                UniquifyForeignKeyNames(entityType, foreignKeys, storeObject, maxLength);
                UniquifyIndexNames(entityType, indexes, storeObject, maxLength);
                UniquifyCheckConstraintNames(entityType, checkConstraints, storeObject, maxLength);
                UniquifyTriggerNames(entityType, triggers, storeObject, maxLength);
            }
        }
    }

    /// <summary>
    ///     Gets a value indicating whether key names should be unique across tables.
    /// </summary>
    protected virtual bool KeysUniqueAcrossTables
        => false;

    /// <summary>
    ///     Gets a value indicating whether foreign key names should be unique across tables.
    /// </summary>
    protected virtual bool ForeignKeysUniqueAcrossTables
        => false;

    /// <summary>
    ///     Gets a value indicating whether index names should be unique across tables.
    /// </summary>
    protected virtual bool IndexesUniqueAcrossTables
        => true;

    /// <summary>
    ///     Gets a value indicating whether check constraint names should be unique across tables.
    /// </summary>
    protected virtual bool CheckConstraintsUniqueAcrossTables
        => true;

    /// <summary>
    ///     Gets a value indicating whether trigger names should be unique across tables.
    /// </summary>
    protected virtual bool TriggersUniqueAcrossTables
        => true;

    private static void TryUniquifyTableNames(
        IConventionModel model,
        Dictionary<(string Name, string? Schema), List<IConventionEntityType>> tables,
        int maxLength)
    {
        Dictionary<(string Name, string? Schema), Dictionary<(string Name, string? Schema), List<IConventionEntityType>>>?
            clashingTables
                = null;
        foreach (var entityType in model.GetEntityTypes())
        {
            var tableName = entityType.GetTableName();
            if (tableName == null
                || entityType.FindPrimaryKey() == null)
            {
                continue;
            }

            var table = (Name: tableName, Schema: entityType.GetSchema());

            if (!tables.TryGetValue(table, out var entityTypes))
            {
                entityTypes = [];
                tables[table] = entityTypes;
            }

            if (entityTypes.Count > 0
                && !entityType.FindRowInternalForeignKeys(StoreObjectIdentifier.Table(table.Name, table.Schema)).Any()
                && !entityTypes.Any(t => t.IsAssignableFrom(entityType)))
            {
                entityTypes.Insert(0, entityType);
            }
            else
            {
                entityTypes.Add(entityType);
            }

            if (table.Name.Length == maxLength)
            {
                var originalName = entityType.GetDefaultTableName(truncate: false)!;
                if (originalName.Length == maxLength)
                {
                    continue;
                }

                clashingTables ??=
                    new Dictionary<(string Name, string? Schema), Dictionary<(string Name, string? Schema), List<IConventionEntityType>>>();

                if (!clashingTables.TryGetValue(table, out var clashingSubTables))
                {
                    clashingSubTables = new Dictionary<(string Name, string? Schema), List<IConventionEntityType>>();
                    clashingTables[table] = clashingSubTables;
                }

                if (!clashingSubTables.TryGetValue((originalName, table.Schema), out var subTable))
                {
                    subTable = [];
                    clashingSubTables[(originalName, table.Schema)] = subTable;
                }

                subTable.Add(entityType);
            }
        }

        if (clashingTables == null)
        {
            return;
        }

        // Some entity types might end up mapped to the same table after the table name is truncated,
        // so we need to map them to different tables as was intended initially
        foreach (var (table, value) in clashingTables)
        {
            var oldTable = tables[table];
            foreach (var subTable in value.Values.Skip(1))
            {
                var uniqueName = Uniquifier.Uniquify(table.Name, tables, n => (n, table.Schema), maxLength);
                tables[(uniqueName, table.Schema)] = subTable;
                foreach (var entityType in subTable)
                {
                    entityType.Builder.ToTable(uniqueName);
                    oldTable.Remove(entityType);
                }
            }
        }
    }

    private static void UniquifyColumnNames(
        IConventionTypeBase type,
        Dictionary<string, IConventionProperty> columns,
        in StoreObjectIdentifier storeObject,
        int maxLength)
    {
        foreach (var property in type.GetProperties())
        {
            var columnName = property.GetColumnName(storeObject);
            if (columnName == null)
            {
                continue;
            }

            if (!columns.TryGetValue(columnName, out var otherProperty))
            {
                columns[columnName] = property;
                continue;
            }

            if (property == otherProperty)
            {
                continue;
            }

            var identifyingMemberInfo = property.PropertyInfo ?? (MemberInfo?)property.FieldInfo;
            if ((identifyingMemberInfo != null
                    && identifyingMemberInfo.IsSameAs(otherProperty.PropertyInfo ?? (MemberInfo?)otherProperty.FieldInfo))
                || (property.IsPrimaryKey() && otherProperty.IsPrimaryKey())
                || (property.IsConcurrencyToken && otherProperty.IsConcurrencyToken)
                || (!property.Builder.CanSetColumnName(null) && !otherProperty.Builder.CanSetColumnName(null)))
            {
                // Handle this with a default value convention #9329
                if (property.GetAfterSaveBehavior() == PropertySaveBehavior.Save
                    && property.ValueGenerated is ValueGenerated.Never or ValueGenerated.OnUpdateSometimes)
                {
                    property.Builder.ValueGenerated(ValueGenerated.OnUpdateSometimes);
                }

                if (otherProperty.GetAfterSaveBehavior() == PropertySaveBehavior.Save
                    && otherProperty.ValueGenerated is ValueGenerated.Never or ValueGenerated.OnUpdateSometimes)
                {
                    otherProperty.Builder.ValueGenerated(ValueGenerated.OnUpdateSometimes);
                }

                continue;
            }

            var usePrefix = property.DeclaringType != otherProperty.DeclaringType;
            if (!usePrefix
                || (!property.DeclaringType.IsStrictlyDerivedFrom(otherProperty.DeclaringType)
                    && !otherProperty.DeclaringType.IsStrictlyDerivedFrom(property.DeclaringType))
                || (property.DeclaringType as IConventionEntityType)?.FindRowInternalForeignKeys(storeObject).Any() == true)
            {
                var newColumnName = TryUniquify(property, columnName, columns, storeObject, usePrefix, maxLength);
                if (newColumnName != null)
                {
                    columns[newColumnName] = property;
                    continue;
                }
            }

            if (!usePrefix
                || (!property.DeclaringType.IsStrictlyDerivedFrom(otherProperty.DeclaringType)
                    && !otherProperty.DeclaringType.IsStrictlyDerivedFrom(property.DeclaringType))
                || (otherProperty.DeclaringType as IConventionEntityType)?.FindRowInternalForeignKeys(storeObject).Any() == true)
            {
                var newOtherColumnName = TryUniquify(otherProperty, columnName, columns, storeObject, usePrefix, maxLength);
                if (newOtherColumnName != null)
                {
                    columns[columnName] = property;
                    columns[newOtherColumnName] = otherProperty;
                }
            }
        }

        foreach (var complexProperty in type.GetDeclaredComplexProperties())
        {
            UniquifyColumnNames(complexProperty.ComplexType, columns, storeObject, maxLength);
        }
    }

    private static string? TryUniquify(
        IConventionProperty property,
        string columnName,
        Dictionary<string, IConventionProperty> properties,
        in StoreObjectIdentifier storeObject,
        bool usePrefix,
        int maxLength)
    {
        if (property.Builder.CanSetColumnName(null)
            && property.Builder.CanSetColumnName(null, storeObject))
        {
            if (usePrefix)
            {
                var prefix = property.DeclaringType.ShortName();
                if (!columnName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    columnName = prefix + "_" + columnName;
                }
            }

            columnName = Uniquifier.Uniquify(columnName, properties, maxLength);
            if (property.Builder.HasColumnName(columnName, storeObject) == null)
            {
                return null;
            }

            properties[columnName] = property;
            return columnName;
        }

        return null;
    }

    private void UniquifyKeyNames(
        IConventionEntityType entityType,
        Dictionary<string, (IConventionKey, StoreObjectIdentifier)> keys,
        in StoreObjectIdentifier storeObject,
        int maxLength)
    {
        foreach (var key in entityType.GetDeclaredKeys())
        {
            var keyName = key.GetName(storeObject);
            if (keyName == null)
            {
                continue;
            }

            if (!keys.TryGetValue(keyName, out var otherKeyPair))
            {
                keys[keyName] = (key, storeObject);
                continue;
            }

            var (otherKey, otherStoreObject) = otherKeyPair;
            if (storeObject == otherStoreObject
                && ((key.IsPrimaryKey()
                        && otherKey.IsPrimaryKey())
                    || AreCompatible(key, otherKey, storeObject)))
            {
                continue;
            }

            var newKeyName = TryUniquify(key, keyName, keys, maxLength);
            if (newKeyName != null)
            {
                keys[newKeyName] = (key, storeObject);
                continue;
            }

            var newOtherKeyName = TryUniquify(otherKey, keyName, keys, maxLength);
            if (newOtherKeyName != null)
            {
                keys[keyName] = (key, storeObject);
                keys[newOtherKeyName] = otherKeyPair;
            }
        }
    }

    /// <summary>
    ///     Gets a value indicating whether two key mapped to the same constraint are compatible.
    /// </summary>
    /// <param name="key">A key.</param>
    /// <param name="duplicateKey">Another key.</param>
    /// <param name="storeObject">The identifier of the store object.</param>
    /// <returns><see langword="true" /> if compatible</returns>
    protected virtual bool AreCompatible(
        IReadOnlyKey key,
        IReadOnlyKey duplicateKey,
        in StoreObjectIdentifier storeObject)
        => key.AreCompatible(duplicateKey, storeObject, shouldThrow: false);

    private static string? TryUniquify(
        IConventionKey key,
        string keyName,
        Dictionary<string, (IConventionKey, StoreObjectIdentifier)> keys,
        int maxLength)
    {
        if (key.Builder.CanSetName(null))
        {
            keyName = Uniquifier.Uniquify(keyName, keys, maxLength);
            key.Builder.HasName(keyName);
            return keyName;
        }

        return null;
    }

    private void UniquifyIndexNames(
        IConventionEntityType entityType,
        Dictionary<string, (IConventionIndex, StoreObjectIdentifier)> indexes,
        in StoreObjectIdentifier storeObject,
        int maxLength)
    {
        foreach (var index in entityType.GetDeclaredIndexes())
        {
            var indexName = index.GetDatabaseName(storeObject);
            if (indexName == null)
            {
                continue;
            }

            if (!indexes.TryGetValue(indexName, out var otherIndexPair))
            {
                indexes[indexName] = (index, storeObject);
                continue;
            }

            var (otherIndex, otherStoreObject) = otherIndexPair;
            if (storeObject == otherStoreObject
                && AreCompatible(index, otherIndex, storeObject))
            {
                continue;
            }

            var newIndexName = TryUniquify(index, indexName, indexes, maxLength);
            if (newIndexName != null)
            {
                indexes[newIndexName] = (index, storeObject);
                continue;
            }

            var newOtherIndexName = TryUniquify(otherIndex, indexName, indexes, maxLength);
            if (newOtherIndexName != null)
            {
                indexes[indexName] = (index, storeObject);
                indexes[newOtherIndexName] = otherIndexPair;
            }
        }
    }

    /// <summary>
    ///     Gets a value indicating whether two indexes mapped to the same table index are compatible.
    /// </summary>
    /// <param name="index">An index.</param>
    /// <param name="duplicateIndex">Another index.</param>
    /// <param name="storeObject">The identifier of the store object.</param>
    /// <returns><see langword="true" /> if compatible</returns>
    protected virtual bool AreCompatible(
        IReadOnlyIndex index,
        IReadOnlyIndex duplicateIndex,
        in StoreObjectIdentifier storeObject)
        => index.AreCompatible(duplicateIndex, storeObject, shouldThrow: false);

    private static string? TryUniquify(
        IConventionIndex index,
        string indexName,
        Dictionary<string, (IConventionIndex, StoreObjectIdentifier)> indexes,
        int maxLength)
    {
        if (index.Builder.CanSetDatabaseName(null))
        {
            indexName = Uniquifier.Uniquify(indexName, indexes, maxLength);
            index.Builder.HasDatabaseName(indexName);
            return indexName;
        }

        return null;
    }

    private void UniquifyForeignKeyNames(
        IConventionEntityType entityType,
        Dictionary<string, (IConventionForeignKey, StoreObjectIdentifier)> foreignKeys,
        in StoreObjectIdentifier storeObject,
        int maxLength)
    {
        foreach (var foreignKey in entityType.GetForeignKeys())
        {
            if (foreignKey.DeclaringEntityType != entityType
                && StoreObjectIdentifier.Create(foreignKey.DeclaringEntityType, StoreObjectType.Table) == storeObject)
            {
                continue;
            }

            var principalTable = foreignKey.PrincipalKey.IsPrimaryKey()
                ? StoreObjectIdentifier.Create(foreignKey.PrincipalEntityType, StoreObjectType.Table)
                : StoreObjectIdentifier.Create(foreignKey.PrincipalKey.DeclaringEntityType, StoreObjectType.Table);
            if (principalTable == null
                || storeObject == principalTable.Value)
            {
                continue;
            }

            var foreignKeyName = foreignKey.GetConstraintName(storeObject, principalTable.Value);
            if (foreignKeyName == null)
            {
                continue;
            }

            if (!foreignKeys.TryGetValue(foreignKeyName, out var otherForeignKeyPair))
            {
                foreignKeys[foreignKeyName] = (foreignKey, storeObject);
                continue;
            }

            var (otherForeignKey, otherStoreObject) = otherForeignKeyPair;
            if (storeObject == otherStoreObject
                && AreCompatible(foreignKey, otherForeignKey, storeObject))
            {
                continue;
            }

            var newForeignKeyName = TryUniquify(foreignKey, foreignKeyName, foreignKeys, maxLength);
            if (newForeignKeyName != null)
            {
                foreignKeys[newForeignKeyName] = (foreignKey, storeObject);
                continue;
            }

            if (!otherForeignKey.DeclaringEntityType.IsAssignableFrom(entityType)
                && !entityType.IsAssignableFrom(otherForeignKey.DeclaringEntityType))
            {
                continue;
            }

            var newOtherForeignKeyName = TryUniquify(otherForeignKey, foreignKeyName, foreignKeys, maxLength);
            if (newOtherForeignKeyName != null)
            {
                foreignKeys[foreignKeyName] = (foreignKey, storeObject);
                foreignKeys[newOtherForeignKeyName] = otherForeignKeyPair;
            }
        }
    }

    /// <summary>
    ///     Gets a value indicating whether two foreign keys mapped to the same foreign key constraint are compatible.
    /// </summary>
    /// <param name="foreignKey">A foreign key.</param>
    /// <param name="duplicateForeignKey">Another foreign key.</param>
    /// <param name="storeObject">The identifier of the store object.</param>
    /// <returns><see langword="true" /> if compatible</returns>
    protected virtual bool AreCompatible(
        IReadOnlyForeignKey foreignKey,
        IReadOnlyForeignKey duplicateForeignKey,
        in StoreObjectIdentifier storeObject)
        => foreignKey.AreCompatible(duplicateForeignKey, storeObject, shouldThrow: false);

    private static string? TryUniquify(
        IConventionForeignKey foreignKey,
        string foreignKeyName,
        Dictionary<string, (IConventionForeignKey, StoreObjectIdentifier)> foreignKeys,
        int maxLength)
    {
        if (foreignKey.Builder.CanSetConstraintName(null))
        {
            foreignKeyName = Uniquifier.Uniquify(foreignKeyName, foreignKeys, maxLength);
            foreignKey.Builder.HasConstraintName(foreignKeyName);
            return foreignKeyName;
        }

        return null;
    }

    private void UniquifyCheckConstraintNames(
        IConventionEntityType entityType,
        Dictionary<(string, string?), (IConventionCheckConstraint, StoreObjectIdentifier)> checkConstraints,
        in StoreObjectIdentifier storeObject,
        int maxLength)
    {
        foreach (var checkConstraint in entityType.GetDeclaredCheckConstraints())
        {
            var constraintName = checkConstraint.GetName(storeObject);
            if (constraintName == null)
            {
                continue;
            }

            if (!checkConstraints.TryGetValue((constraintName, storeObject.Schema), out var otherCheckConstraintPair))
            {
                checkConstraints[(constraintName, storeObject.Schema)] = (checkConstraint, storeObject);
                continue;
            }

            var (otherCheckConstraint, otherStoreObject) = otherCheckConstraintPair;
            if (storeObject == otherStoreObject
                && AreCompatible(checkConstraint, otherCheckConstraint, storeObject))
            {
                continue;
            }

            var newConstraintName = TryUniquify(checkConstraint, constraintName, storeObject.Schema, checkConstraints, maxLength);
            if (newConstraintName != null)
            {
                checkConstraints[(newConstraintName, storeObject.Schema)] = (checkConstraint, storeObject);
                continue;
            }

            var newOtherConstraintName = TryUniquify(otherCheckConstraint, constraintName, storeObject.Schema, checkConstraints, maxLength);
            if (newOtherConstraintName != null)
            {
                checkConstraints[(constraintName, storeObject.Schema)] = (checkConstraint, storeObject);
                checkConstraints[(newOtherConstraintName, otherStoreObject.Schema)] = otherCheckConstraintPair;
            }
        }
    }

    /// <summary>
    ///     Gets a value indicating whether two check constraints with the same name are compatible.
    /// </summary>
    /// <param name="checkConstraint">An check constraints.</param>
    /// <param name="duplicateCheckConstraint">Another check constraints.</param>
    /// <param name="storeObject">The identifier of the store object.</param>
    /// <returns><see langword="true" /> if compatible</returns>
    protected virtual bool AreCompatible(
        IReadOnlyCheckConstraint checkConstraint,
        IReadOnlyCheckConstraint duplicateCheckConstraint,
        in StoreObjectIdentifier storeObject)
        => CheckConstraint.AreCompatible(checkConstraint, duplicateCheckConstraint, storeObject, shouldThrow: false);

    private static string? TryUniquify(
        IConventionCheckConstraint checkConstraint,
        string checkConstraintName,
        string? schema,
        Dictionary<(string, string?), (IConventionCheckConstraint, StoreObjectIdentifier)> checkConstraints,
        int maxLength)
    {
        if (checkConstraint.Builder.CanSetName(null))
        {
            checkConstraintName = Uniquifier.Uniquify(checkConstraintName, checkConstraints, n => (n, schema), maxLength);
            checkConstraint.Builder.HasName(checkConstraintName);
            return checkConstraintName;
        }

        return null;
    }

    private void UniquifyTriggerNames(
        IConventionEntityType entityType,
        Dictionary<string, (IConventionTrigger, StoreObjectIdentifier)> triggers,
        in StoreObjectIdentifier storeObject,
        int maxLength)
    {
        foreach (var trigger in entityType.GetDeclaredTriggers())
        {
            var triggerName = trigger.GetDatabaseName(storeObject);
            if (triggerName == null)
            {
                continue;
            }

            if (!triggers.TryGetValue(triggerName, out var otherTriggerPair))
            {
                triggers[triggerName] = (trigger, storeObject);
                continue;
            }

            var (otherTrigger, otherStoreObject) = otherTriggerPair;
            if (otherStoreObject == storeObject
                && AreCompatible(trigger, otherTrigger, storeObject))
            {
                continue;
            }

            var newTriggerName = TryUniquify(trigger, triggerName, triggers, maxLength);
            if (newTriggerName != null)
            {
                triggers[newTriggerName] = (trigger, storeObject);
                continue;
            }

            var newOtherTrigger = TryUniquify(otherTrigger, triggerName, triggers, maxLength);
            if (newOtherTrigger != null)
            {
                triggers[triggerName] = (trigger, storeObject);
                triggers[newOtherTrigger] = otherTriggerPair;
            }
        }
    }

    /// <summary>
    ///     Gets a value indicating whether two triggers with the same name are compatible.
    /// </summary>
    /// <param name="trigger">A trigger.</param>
    /// <param name="duplicateTrigger">Another trigger.</param>
    /// <param name="storeObject">The identifier of the store object.</param>
    /// <returns><see langword="true" /> if compatible</returns>
    protected virtual bool AreCompatible(
        IReadOnlyTrigger trigger,
        IReadOnlyTrigger duplicateTrigger,
        in StoreObjectIdentifier storeObject)
        => true;

    private static string? TryUniquify(
        IConventionTrigger trigger,
        string triggerName,
        Dictionary<string, (IConventionTrigger, StoreObjectIdentifier)> triggers,
        int maxLength)
    {
        if (trigger.Builder.CanSetDatabaseName(null))
        {
            triggerName = Uniquifier.Uniquify(triggerName, triggers, n => n, maxLength);
            trigger.Builder.HasDatabaseName(triggerName);
            return triggerName;
        }

        return null;
    }
}
