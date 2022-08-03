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
        var keys = new Dictionary<string, IConventionKey>();
        var foreignKeys = new Dictionary<string, IConventionForeignKey>();
        var indexes = new Dictionary<string, IConventionIndex>();
        var checkConstraints = new Dictionary<(string, string?), IConventionCheckConstraint>();
        var triggers = new Dictionary<string, IConventionTrigger>();
        foreach (var ((tableName, schema), conventionEntityTypes) in tables)
        {
            columns.Clear();
            keys.Clear();
            foreignKeys.Clear();

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
                TryUniquifyColumnNames(entityType, columns, storeObject, maxLength);
                TryUniquifyKeyNames(entityType, keys, storeObject, maxLength);
                TryUniquifyForeignKeyNames(entityType, foreignKeys, storeObject, maxLength);
                TryUniquifyIndexNames(entityType, indexes, storeObject, maxLength);
                TryUniquifyCheckConstraintNames(entityType, checkConstraints, storeObject, maxLength);
                TryUniquifyTriggerNames(entityType, triggers, storeObject, maxLength);
            }
        }
    }

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
                entityTypes = new List<IConventionEntityType>();
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
                    new Dictionary<(string Name, string? Schema),
                        Dictionary<(string Name, string? Schema), List<IConventionEntityType>>>();

                if (!clashingTables.TryGetValue(table, out var clashingSubTables))
                {
                    clashingSubTables = new Dictionary<(string Name, string? Schema), List<IConventionEntityType>>();
                    clashingTables[table] = clashingSubTables;
                }

                if (!clashingSubTables.TryGetValue((originalName, table.Schema), out var subTable))
                {
                    subTable = new List<IConventionEntityType>();
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

    private static void TryUniquifyColumnNames(
        IConventionEntityType entityType,
        Dictionary<string, IConventionProperty> properties,
        in StoreObjectIdentifier storeObject,
        int maxLength)
    {
        foreach (var property in entityType.GetDeclaredProperties())
        {
            var columnName = property.GetColumnName(storeObject);
            if (columnName == null)
            {
                continue;
            }

            if (!properties.TryGetValue(columnName, out var otherProperty))
            {
                properties[columnName] = property;
                continue;
            }

            var identifyingMemberInfo = property.PropertyInfo ?? (MemberInfo?)property.FieldInfo;
            if ((identifyingMemberInfo != null
                    && identifyingMemberInfo.IsSameAs(otherProperty.PropertyInfo ?? (MemberInfo?)otherProperty.FieldInfo))
                || (property.IsPrimaryKey() && otherProperty.IsPrimaryKey())
                || (property.IsConcurrencyToken && otherProperty.IsConcurrencyToken)
                || (!property.Builder.CanSetColumnName(null) && !otherProperty.Builder.CanSetColumnName(null)))
            {
                if (property.GetAfterSaveBehavior() == PropertySaveBehavior.Save
                    && otherProperty.GetAfterSaveBehavior() == PropertySaveBehavior.Save
                    && (property.ValueGenerated == ValueGenerated.Never
                        || property.ValueGenerated == ValueGenerated.OnUpdateSometimes)
                    && (otherProperty.ValueGenerated == ValueGenerated.Never
                        || otherProperty.ValueGenerated == ValueGenerated.OnUpdateSometimes))
                {
                    // Handle this with a default value convention #9329
                    property.Builder.ValueGenerated(ValueGenerated.OnUpdateSometimes);
                    otherProperty.Builder.ValueGenerated(ValueGenerated.OnUpdateSometimes);
                }

                continue;
            }

            var usePrefix = property.DeclaringEntityType != otherProperty.DeclaringEntityType;
            if (!usePrefix
                || (!property.DeclaringEntityType.IsStrictlyDerivedFrom(otherProperty.DeclaringEntityType)
                    && !otherProperty.DeclaringEntityType.IsStrictlyDerivedFrom(property.DeclaringEntityType))
                || property.DeclaringEntityType.FindRowInternalForeignKeys(storeObject).Any())
            {
                var newColumnName = TryUniquify(property, columnName, properties, storeObject, usePrefix, maxLength);
                if (newColumnName != null)
                {
                    properties[newColumnName] = property;
                    continue;
                }
            }

            if (!usePrefix
                || (!property.DeclaringEntityType.IsStrictlyDerivedFrom(otherProperty.DeclaringEntityType)
                    && !otherProperty.DeclaringEntityType.IsStrictlyDerivedFrom(property.DeclaringEntityType))
                || otherProperty.DeclaringEntityType.FindRowInternalForeignKeys(storeObject).Any())
            {
                var newOtherColumnName = TryUniquify(otherProperty, columnName, properties, storeObject, usePrefix, maxLength);
                if (newOtherColumnName != null)
                {
                    properties[columnName] = property;
                    properties[newOtherColumnName] = otherProperty;
                }
            }
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
                var prefix = property.DeclaringEntityType.ShortName();
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

    private void TryUniquifyKeyNames(
        IConventionEntityType entityType,
        Dictionary<string, IConventionKey> keys,
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

            if (!keys.TryGetValue(keyName, out var otherKey))
            {
                keys[keyName] = key;
                continue;
            }

            if ((key.IsPrimaryKey()
                    && otherKey.IsPrimaryKey())
                || AreCompatible(key, otherKey, storeObject))
            {
                continue;
            }

            var newKeyName = TryUniquify(key, keyName, keys, maxLength);
            if (newKeyName != null)
            {
                keys[newKeyName] = key;
                continue;
            }

            var newOtherKeyName = TryUniquify(otherKey, keyName, keys, maxLength);
            if (newOtherKeyName != null)
            {
                keys[keyName] = key;
                keys[newOtherKeyName] = otherKey;
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

    private static string? TryUniquify<T>(
        IConventionKey key,
        string keyName,
        Dictionary<string, T> keys,
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

    private void TryUniquifyIndexNames(
        IConventionEntityType entityType,
        Dictionary<string, IConventionIndex> indexes,
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

            if (!indexes.TryGetValue(indexName, out var otherIndex))
            {
                indexes[indexName] = index;
                continue;
            }

            if (AreCompatible(index, otherIndex, storeObject))
            {
                continue;
            }

            var newIndexName = TryUniquify(index, indexName, indexes, maxLength);
            if (newIndexName != null)
            {
                indexes[newIndexName] = index;
                continue;
            }

            var newOtherIndexName = TryUniquify(otherIndex, indexName, indexes, maxLength);
            if (newOtherIndexName != null)
            {
                indexes[indexName] = index;
                indexes[newOtherIndexName] = otherIndex;
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

    private static string? TryUniquify<T>(
        IConventionIndex index,
        string indexName,
        Dictionary<string, T> indexes,
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

    private void TryUniquifyForeignKeyNames(
        IConventionEntityType entityType,
        Dictionary<string, IConventionForeignKey> foreignKeys,
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

            if (!foreignKeys.TryGetValue(foreignKeyName, out var otherForeignKey))
            {
                foreignKeys[foreignKeyName] = foreignKey;
                continue;
            }

            if (AreCompatible(foreignKey, otherForeignKey, storeObject))
            {
                continue;
            }

            var newForeignKeyName = TryUniquify(foreignKey, foreignKeyName, foreignKeys, maxLength);
            if (newForeignKeyName != null)
            {
                foreignKeys[newForeignKeyName] = foreignKey;
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
                foreignKeys[foreignKeyName] = foreignKey;
                foreignKeys[newOtherForeignKeyName] = otherForeignKey;
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

    private static string? TryUniquify<T>(
        IConventionForeignKey foreignKey,
        string foreignKeyName,
        Dictionary<string, T> foreignKeys,
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

    private void TryUniquifyCheckConstraintNames(
        IConventionEntityType entityType,
        Dictionary<(string, string?), IConventionCheckConstraint> checkConstraints,
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

            if (!checkConstraints.TryGetValue((constraintName, storeObject.Schema), out var otherCheckConstraint))
            {
                checkConstraints[(constraintName, storeObject.Schema)] = checkConstraint;
                continue;
            }

            if (AreCompatible(checkConstraint, otherCheckConstraint, storeObject))
            {
                continue;
            }

            var newConstraintName = TryUniquify(checkConstraint, constraintName, storeObject.Schema, checkConstraints, maxLength);
            if (newConstraintName != null)
            {
                checkConstraints[(newConstraintName, storeObject.Schema)] = checkConstraint;
                continue;
            }

            var newOtherConstraintName = TryUniquify(otherCheckConstraint, constraintName, storeObject.Schema, checkConstraints, maxLength);
            if (newOtherConstraintName != null)
            {
                checkConstraints[(constraintName, storeObject.Schema)] = checkConstraint;
                checkConstraints[(newOtherConstraintName, storeObject.Schema)] = otherCheckConstraint;
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

    private static string? TryUniquify<T>(
        IConventionCheckConstraint checkConstraint,
        string checkConstraintName,
        string? schema,
        Dictionary<(string, string?), T> checkConstraints,
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

    private void TryUniquifyTriggerNames(
        IConventionEntityType entityType,
        Dictionary<string, IConventionTrigger> triggers,
        in StoreObjectIdentifier storeObject,
        int maxLength)
    {
        foreach (var trigger in entityType.GetDeclaredTriggers())
        {
            var triggerName = trigger.GetName(storeObject);
            if (triggerName == null)
            {
                continue;
            }

            if (!triggers.TryGetValue(triggerName, out var otherTrigger))
            {
                triggers[triggerName] = trigger;
                continue;
            }

            if (AreCompatible(trigger, otherTrigger, storeObject))
            {
                continue;
            }

            var newTriggerName = TryUniquify(trigger, triggerName, triggers, maxLength);
            if (newTriggerName != null)
            {
                triggers[newTriggerName] = trigger;
                continue;
            }

            var newOtherTrigger = TryUniquify(otherTrigger, triggerName, triggers, maxLength);
            if (newOtherTrigger != null)
            {
                triggers[triggerName] = trigger;
                triggers[newOtherTrigger] = otherTrigger;
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

    private static string? TryUniquify<T>(
        IConventionTrigger trigger,
        string triggerName,
        Dictionary<string, T> triggers,
        int maxLength)
    {
        if (trigger.Builder.CanSetName(null))
        {
            triggerName = Uniquifier.Uniquify(triggerName, triggers, n => n, maxLength);
            trigger.Builder.HasName(triggerName);
            return triggerName;
        }

        return null;
    }
}
