// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    /// <summary>
    ///     A convention that manipulates names of database objects for entity types that share a table to avoid clashes.
    /// </summary>
    public class SharedTableConvention : IModelFinalizedConvention
    {
        /// <summary>
        ///     Creates a new instance of <see cref="SharedTableConvention" />.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this convention. </param>
        /// <param name="relationalDependencies">  Parameter object containing relational dependencies for this convention. </param>
        public SharedTableConvention(
            [NotNull] ProviderConventionSetBuilderDependencies dependencies,
            [NotNull] RelationalConventionSetBuilderDependencies relationalDependencies)
        {
            Dependencies = dependencies;
        }

        /// <summary>
        ///     Parameter object containing service dependencies.
        /// </summary>
        protected virtual ProviderConventionSetBuilderDependencies Dependencies { get; }

        /// <summary>
        ///     Called after a model is finalized.
        /// </summary>
        /// <param name="modelBuilder"> The builder for the model. </param>
        /// <param name="context"> Additional information associated with convention execution. </param>
        public virtual void ProcessModelFinalized(IConventionModelBuilder modelBuilder, IConventionContext<IConventionModelBuilder> context)
        {
            var maxLength = modelBuilder.Metadata.GetMaxIdentifierLength();
            var tables = new Dictionary<(string, string), List<IConventionEntityType>>();

            TryUniquifyTableNames(modelBuilder.Metadata, tables, maxLength);

            var columns = new Dictionary<string, IConventionProperty>(StringComparer.Ordinal);
            var keys = new Dictionary<string, IConventionKey>(StringComparer.Ordinal);
            var foreignKeys = new Dictionary<string, IConventionForeignKey>(StringComparer.Ordinal);
            var indexes = new Dictionary<string, IConventionIndex>(StringComparer.Ordinal);
            foreach (var entityTypes in tables.Values)
            {
                columns.Clear();
                keys.Clear();
                foreignKeys.Clear();

                foreach (var entityType in entityTypes)
                {
                    TryUniquifyColumnNames(entityType, columns, maxLength);
                    TryUniquifyKeyNames(entityType, keys, maxLength);
                    TryUniquifyForeignKeyNames(entityType, foreignKeys, maxLength);
                    TryUniquifyIndexNames(entityType, indexes, maxLength);
                }
            }
        }

        private static void TryUniquifyTableNames(
            IConventionModel model, Dictionary<(string, string), List<IConventionEntityType>> tables, int maxLength)
        {
            foreach (var entityType in model.GetEntityTypes())
            {
                var tableName = (Schema: entityType.GetSchema(), TableName: entityType.GetTableName());
                if (!tables.TryGetValue(tableName, out var entityTypes))
                {
                    entityTypes = new List<IConventionEntityType>();
                    tables[tableName] = entityTypes;
                }

                if (entityTypes.Count > 0)
                {
                    var shouldUniquifyTable = ShouldUniquify(entityType, entityTypes);

                    if (shouldUniquifyTable)
                    {
                        if (entityType[RelationalAnnotationNames.TableName] == null)
                        {
                            var uniqueName = Uniquifier.Uniquify(
                                tableName.TableName, tables, n => (tableName.Schema, n), maxLength);
                            if (entityType.Builder.ToTable(uniqueName) != null)
                            {
                                tables[(tableName.Schema, uniqueName)] = new List<IConventionEntityType> { entityType };
                                continue;
                            }
                        }

                        if (entityTypes.Count == 1)
                        {
                            var otherEntityType = entityTypes.First();
                            if (otherEntityType[RelationalAnnotationNames.TableName] == null)
                            {
                                var uniqueName = Uniquifier.Uniquify(
                                    tableName.TableName, tables, n => (tableName.Schema, n), maxLength);
                                if (otherEntityType.Builder.ToTable(uniqueName) != null)
                                {
                                    entityTypes.Remove(otherEntityType);
                                    tables[(tableName.Schema, uniqueName)] = new List<IConventionEntityType> { otherEntityType };
                                }
                            }
                        }
                    }
                }

                entityTypes.Add(entityType);
            }
        }

        private static bool ShouldUniquify(IConventionEntityType entityType, ICollection<IConventionEntityType> entityTypes)
        {
            var rootType = entityType.GetRootType();
            var pkProperty = entityType.FindPrimaryKey()?.Properties[0];
            var rootSharedTableType = pkProperty?.FindSharedTableRootPrimaryKeyProperty()?.DeclaringEntityType;

            foreach (var otherEntityType in entityTypes)
            {
                if (rootSharedTableType == otherEntityType
                    || rootType == otherEntityType.GetRootType())
                {
                    return false;
                }

                var otherPkProperty = otherEntityType.FindPrimaryKey()?.Properties[0];
                var otherRootSharedTableType = otherPkProperty?.FindSharedTableRootPrimaryKeyProperty()?.DeclaringEntityType;
                if (otherRootSharedTableType == entityType
                    || (otherRootSharedTableType == rootSharedTableType
                        && otherRootSharedTableType != null))
                {
                    return false;
                }
            }

            return true;
        }

        private static void TryUniquifyColumnNames(
            IConventionEntityType entityType, Dictionary<string, IConventionProperty> properties, int maxLength)
        {
            foreach (var property in entityType.GetDeclaredProperties())
            {
                var columnName = property.GetColumnName();
                if (!properties.TryGetValue(columnName, out var otherProperty))
                {
                    properties[columnName] = property;
                    continue;
                }

                var identifyingMemberInfo = property.GetIdentifyingMemberInfo();
                if (identifyingMemberInfo != null
                    && identifyingMemberInfo.IsSameAs(otherProperty.GetIdentifyingMemberInfo()))
                {
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
            IConventionProperty property, string columnName, Dictionary<string, IConventionProperty> properties, bool usePrefix,
            int maxLength)
        {
            if (property.Builder.CanSetColumnName(null))
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
                property.Builder.HasColumnName(columnName);
                properties[columnName] = property;
                return columnName;
            }

            return null;
        }

        private static void TryUniquifyKeyNames(IConventionEntityType entityType, Dictionary<string, IConventionKey> keys, int maxLength)
        {
            foreach (var key in entityType.GetDeclaredKeys())
            {
                var keyName = key.GetName();
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
            IConventionKey key, string keyName, Dictionary<string, T> keys, int maxLength)
        {
            if (key.Builder.CanSetName(null))
            {
                keyName = Uniquifier.Uniquify(keyName, keys, maxLength);
                key.Builder.HasName(keyName);
                return keyName;
            }

            return null;
        }

        private static void TryUniquifyIndexNames(
            IConventionEntityType entityType, Dictionary<string, IConventionIndex> indexes, int maxLength)
        {
            foreach (var index in entityType.GetDeclaredIndexes())
            {
                var indexName = index.GetName();
                if (!indexes.TryGetValue(indexName, out var otherIndex))
                {
                    indexes[indexName] = index;
                    continue;
                }

                if (index.Builder.CanSetName(null))
                {
                    if (index.GetConfigurationSource() == ConfigurationSource.Convention
                        && otherIndex.GetConfigurationSource() == ConfigurationSource.Convention
                        && otherIndex.Builder.CanSetName(null))
                    {
                        var associatedForeignKey = index.DeclaringEntityType.FindDeclaredForeignKeys(index.Properties).FirstOrDefault();
                        var otherAssociatedForeignKey =
                            otherIndex.DeclaringEntityType.FindDeclaredForeignKeys(index.Properties).FirstOrDefault();
                        if (associatedForeignKey != null
                            && otherAssociatedForeignKey != null
                            && associatedForeignKey.GetConstraintName() == otherAssociatedForeignKey.GetConstraintName()
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
            IConventionIndex index, string indexName, Dictionary<string, T> indexes, int maxLength)
        {
            if (index.Builder.CanSetName(null))
            {
                indexName = Uniquifier.Uniquify(indexName, indexes, maxLength);
                index.Builder.HasName(indexName);
                return indexName;
            }

            return null;
        }

        private static void TryUniquifyForeignKeyNames(
            IConventionEntityType entityType, Dictionary<string, IConventionForeignKey> foreignKeys, int maxLength)
        {
            foreach (var foreignKey in entityType.GetDeclaredForeignKeys())
            {
                if (foreignKey.DeclaringEntityType.GetTableName() == foreignKey.PrincipalEntityType.GetTableName()
                    && foreignKey.DeclaringEntityType.GetSchema() == foreignKey.PrincipalEntityType.GetSchema())
                {
                    continue;
                }

                var foreignKeyName = foreignKey.GetConstraintName();
                if (!foreignKeys.TryGetValue(foreignKeyName, out var otherForeignKey))
                {
                    foreignKeys[foreignKeyName] = foreignKey;
                    continue;
                }

                if (foreignKey.Builder.CanSetConstraintName(null))
                {
                    if (otherForeignKey.Builder.CanSetConstraintName(null)
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
            IConventionForeignKey foreignKey, string foreignKeyName, Dictionary<string, T> foreignKeys, int maxLength)
        {
            if (foreignKey.Builder.CanSetConstraintName(null))
            {
                foreignKeyName = Uniquifier.Uniquify(foreignKeyName, foreignKeys, maxLength);
                foreignKey.Builder.HasConstraintName(foreignKeyName);
                return foreignKeyName;
            }

            return null;
        }
    }
}
