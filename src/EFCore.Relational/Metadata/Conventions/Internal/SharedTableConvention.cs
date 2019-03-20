// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class SharedTableConvention :
        IModelBuiltConvention
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public SharedTableConvention([NotNull] IDiagnosticsLogger<DbLoggerCategory.Model> logger)
        {
            Logger = logger;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual IDiagnosticsLogger<DbLoggerCategory.Model> Logger { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalModelBuilder Apply(InternalModelBuilder modelBuilder)
        {
            var maxLength = modelBuilder.Relational(ConfigurationSource.Convention).MaxIdentifierLength;
            var tables = new Dictionary<(string, string), List<EntityType>>();

            TryUniquifyTableNames(modelBuilder.Metadata, tables, maxLength);

            var columns = new Dictionary<string, Property>(StringComparer.Ordinal);
            var keys = new Dictionary<string, Key>(StringComparer.Ordinal);
            var foreignKeys = new Dictionary<string, ForeignKey>(StringComparer.Ordinal);
            var indexes = new Dictionary<string, Index>(StringComparer.Ordinal);
            foreach (var entityTypes in tables.Values)
            {
                columns.Clear();
                keys.Clear();
                foreignKeys.Clear();
                indexes.Clear();

                foreach (var entityType in entityTypes)
                {
                    TryUniquifyColumnNames(entityType, columns, maxLength);
                    TryUniquifyKeyNames(entityType, keys, maxLength);
                    TryUniquifyForeignKeyNames(entityType, foreignKeys, maxLength);
                    TryUniquifyIndexNames(entityType, indexes, maxLength);
                }
            }

            return modelBuilder;
        }

        private static void TryUniquifyTableNames(
            Model model, Dictionary<(string, string), List<EntityType>> tables, int maxLength)
        {
            foreach (var entityType in model.GetEntityTypes())
            {
                if (entityType.FindPrimaryKey() == null)
                {
                    continue;
                }

                var annotations = entityType.Relational();
                var tableName = (annotations.Schema, annotations.TableName);
                if (!tables.TryGetValue(tableName, out var entityTypes))
                {
                    entityTypes = new List<EntityType>();
                    tables[tableName] = entityTypes;
                }

                if (entityTypes.Count > 0)
                {
                    var shouldUniquifyTable = ShouldUniquify(entityType, entityTypes);

                    if (shouldUniquifyTable)
                    {
                        if (entityType[RelationalAnnotationNames.TableName] == null)
                        {
                            var uniqueName = ConstraintNamer.Uniquify(
                                tableName.TableName, tables, n => (tableName.Schema, n), maxLength);
                            if (entityType.Builder.Relational(ConfigurationSource.Convention).ToTable(uniqueName))
                            {
                                tables[(tableName.Schema, uniqueName)] = new List<EntityType> { entityType };
                                continue;
                            }
                        }

                        if (entityTypes.Count == 1)
                        {
                            var otherEntityType = entityTypes.First();
                            if (otherEntityType[RelationalAnnotationNames.TableName] == null)
                            {
                                var uniqueName = ConstraintNamer.Uniquify(
                                    tableName.TableName, tables, n => (tableName.Schema, n), maxLength);
                                if (otherEntityType.Builder.Relational(ConfigurationSource.Convention).ToTable(uniqueName))
                                {
                                    entityTypes.Remove(otherEntityType);
                                    tables[(tableName.Schema, uniqueName)] = new List<EntityType> { otherEntityType };
                                }
                            }
                        }
                    }
                }

                entityTypes.Add(entityType);
            }
        }

        private static bool ShouldUniquify(EntityType entityType, ICollection<EntityType> entityTypes)
        {
            var rootType = entityType.RootType();
            var pkProperty = entityType.FindPrimaryKey().Properties[0];
            var rootSharedTableType = pkProperty.FindSharedTableRootPrimaryKeyProperty()?.DeclaringEntityType;

            foreach (var otherEntityType in entityTypes)
            {
                if (rootSharedTableType == otherEntityType
                    || rootType == otherEntityType.RootType())
                {
                    return false;
                }

                var otherPkProperty = otherEntityType.FindPrimaryKey().Properties[0];
                var otherRootSharedTableType = otherPkProperty.FindSharedTableRootPrimaryKeyProperty()?.DeclaringEntityType;
                if (otherRootSharedTableType == entityType
                    || (otherRootSharedTableType == rootSharedTableType
                        && otherRootSharedTableType != null))
                {
                    return false;
                }
            }

            return true;
        }

        private static void TryUniquifyColumnNames(EntityType entityType, Dictionary<string, Property> properties, int maxLength)
        {
            foreach (var property in entityType.GetDeclaredProperties())
            {
                var columnName = property.Relational().ColumnName;
                if (!properties.TryGetValue(columnName, out var otherProperty))
                {
                    properties[columnName] = property;
                    continue;
                }

                var usePrefix = property.DeclaringEntityType != otherProperty.DeclaringEntityType
                    || property.IsPrimaryKey()
                    || otherProperty.IsPrimaryKey();
                if (!property.IsPrimaryKey())
                {
                    var newColumnName = TryUniquify(property, columnName, properties, usePrefix, maxLength);
                    if (newColumnName != null)
                    {
                        properties[newColumnName] = property;
                        continue;
                    }
                }

                if (!otherProperty.IsPrimaryKey())
                {
                    var newColumnName = TryUniquify(otherProperty, columnName, properties, usePrefix, maxLength);
                    if (newColumnName != null)
                    {
                        properties[columnName] = property;
                        properties[newColumnName] = otherProperty;
                    }
                }
            }
        }

        private static string TryUniquify(
            Property property, string columnName, Dictionary<string, Property> properties, bool usePrefix, int maxLength)
        {
            var relationalPropertyBuilder = property.Builder.Relational(ConfigurationSource.Convention);
            if (relationalPropertyBuilder.CanSetColumnName(null))
            {
                if (usePrefix)
                {
                    var prefix = property.DeclaringEntityType.ShortName();
                    if (!columnName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                    {
                        columnName = prefix + "_" + columnName;
                    }
                }

                columnName = ConstraintNamer.Uniquify(columnName, properties, maxLength);
                relationalPropertyBuilder.ColumnName = columnName;
                properties[columnName] = property;
                return columnName;
            }

            return null;
        }

        private static void TryUniquifyKeyNames(EntityType entityType, Dictionary<string, Key> keys, int maxLength)
        {
            foreach (var key in entityType.GetDeclaredKeys())
            {
                var keyName = key.Relational().Name;
                if (!keys.TryGetValue(keyName, out var otherKey))
                {
                    keys[keyName] = key;
                    continue;
                }

                if (!key.IsPrimaryKey())
                {
                    var newKeyName = TryUniquify(key, keyName, keys, maxLength);
                    if (newKeyName != null)
                    {
                        keys[newKeyName] = key;
                        continue;
                    }
                }

                if (!otherKey.IsPrimaryKey())
                {
                    var newKeyName = TryUniquify(otherKey, keyName, keys, maxLength);
                    if (newKeyName != null)
                    {
                        keys[keyName] = key;
                        keys[newKeyName] = otherKey;
                    }
                }
            }
        }

        private static string TryUniquify<T>(
           Key key, string keyName, Dictionary<string, T> keys, int maxLength)
        {
            var relationalKeyBuilder = key.Builder.Relational(ConfigurationSource.Convention);
            if (relationalKeyBuilder.CanSetName(null))
            {
                keyName = ConstraintNamer.Uniquify(keyName, keys, maxLength);
                relationalKeyBuilder.Name = keyName;
                return keyName;
            }

            return null;
        }

        private static void TryUniquifyIndexNames(EntityType entityType, Dictionary<string, Index> indexes, int maxLength)
        {
            foreach (var index in entityType.GetDeclaredIndexes())
            {
                var indexName = index.Relational().Name;
                if (!indexes.TryGetValue(indexName, out var otherIndex))
                {
                    indexes[indexName] = index;
                    continue;
                }

                var relationalIndexBuilder = index.Builder.Relational(ConfigurationSource.Convention);
                var otherRelationalIndexBuilder = otherIndex.Builder.Relational(ConfigurationSource.Convention);
                if (relationalIndexBuilder.CanSetName(null))
                {
                    if (index.GetConfigurationSource() == ConfigurationSource.Convention
                        && otherIndex.GetConfigurationSource() == ConfigurationSource.Convention
                        && otherRelationalIndexBuilder.CanSetName(null))
                    {
                        var associatedForeignKey = index.DeclaringEntityType.FindDeclaredForeignKeys(index.Properties).FirstOrDefault();
                        var otherAssociatedForeignKey = otherIndex.DeclaringEntityType.FindDeclaredForeignKeys(index.Properties).FirstOrDefault();
                        if (associatedForeignKey != null
                            && otherAssociatedForeignKey != null
                            && associatedForeignKey.Relational().Name == otherAssociatedForeignKey.Relational().Name
                            && index.AreCompatible(otherIndex, shouldThrow: false))
                        {
                            continue;
                        }
                    }

                    var newIndexName = TryUniquify(index, indexName, indexes, maxLength);
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

        private static string TryUniquify<T>(
           Index index, string indexName, Dictionary<string, T> indexes, int maxLength)
        {
            var relationalIndexBuilder = index.Builder.Relational(ConfigurationSource.Convention);
            if (relationalIndexBuilder.CanSetName(null))
            {
                indexName = ConstraintNamer.Uniquify(indexName, indexes, maxLength);
                relationalIndexBuilder.Name = indexName;
                return indexName;
            }

            return null;
        }

        private static void TryUniquifyForeignKeyNames(EntityType entityType, Dictionary<string, ForeignKey> foreignKeys, int maxLength)
        {
            foreach (var foreignKey in entityType.GetDeclaredForeignKeys())
            {
                var declaringAnnotations = foreignKey.DeclaringEntityType.Relational();
                var principalAnnotations = foreignKey.PrincipalEntityType.Relational();
                if (declaringAnnotations.TableName == principalAnnotations.TableName
                    && declaringAnnotations.Schema == principalAnnotations.Schema)
                {
                    continue;
                }

                var foreignKeyName = foreignKey.Relational().Name;
                if (!foreignKeys.TryGetValue(foreignKeyName, out var otherForeignKey))
                {
                    foreignKeys[foreignKeyName] = foreignKey;
                    continue;
                }

                var relationalKeyBuilder = foreignKey.Builder.Relational(ConfigurationSource.Convention);
                var otherRelationalKeyBuilder = otherForeignKey.Builder.Relational(ConfigurationSource.Convention);
                if (relationalKeyBuilder.CanSetName(null))
                {
                    if (otherRelationalKeyBuilder.CanSetName(null)
                        && (foreignKey.PrincipalToDependent != null
                            || foreignKey.DependentToPrincipal != null)
                        && (foreignKey.PrincipalToDependent?.GetIdentifyingMemberInfo()).IsSameAs(
                            otherForeignKey.PrincipalToDependent?.GetIdentifyingMemberInfo())
                        && (foreignKey.DependentToPrincipal?.GetIdentifyingMemberInfo()).IsSameAs(
                            otherForeignKey.DependentToPrincipal?.GetIdentifyingMemberInfo())
                        && foreignKey.AreCompatible(otherForeignKey, shouldThrow: false))
                    {
                        continue;
                    }

                    var newForeignKeyName = TryUniquify(foreignKey, foreignKeyName, foreignKeys, maxLength);
                    foreignKeys[newForeignKeyName] = foreignKey;
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

        private static string TryUniquify<T>(
           ForeignKey foreignKey, string foreignKeyName, Dictionary<string, T> foreignKeys, int maxLength)
        {
            var relationalKeyBuilder = foreignKey.Builder.Relational(ConfigurationSource.Convention);
            if (relationalKeyBuilder.CanSetName(null))
            {
                foreignKeyName = ConstraintNamer.Uniquify(foreignKeyName, foreignKeys, maxLength);
                relationalKeyBuilder.Name = foreignKeyName;
                return foreignKeyName;
            }

            return null;
        }
    }
}
