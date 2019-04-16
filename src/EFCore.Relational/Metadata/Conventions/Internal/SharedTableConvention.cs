// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class SharedTableConvention :
        IEntityTypeAddedConvention,
        IEntityTypeAnnotationChangedConvention,
        IForeignKeyOwnershipChangedConvention,
        IForeignKeyUniquenessChangedConvention,
        IModelBuiltConvention
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalEntityTypeBuilder Apply(InternalEntityTypeBuilder entityTypeBuilder)
        {
            var ownership = entityTypeBuilder.Metadata.GetForeignKeys().SingleOrDefault(fk => fk.IsOwnership && fk.IsUnique);
            if (ownership != null)
            {
                SetOwnedTable(ownership);
            }

            return entityTypeBuilder;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Annotation Apply(
            InternalEntityTypeBuilder entityTypeBuilder, string name, Annotation annotation, Annotation oldAnnotation)
        {
            var entityType = entityTypeBuilder.Metadata;
            if (name == RelationalAnnotationNames.TableName
                || name == RelationalAnnotationNames.Schema)
            {
                foreach (var foreignKey in entityType.GetReferencingForeignKeys())
                {
                    if (foreignKey.IsOwnership
                        && foreignKey.IsUnique)
                    {
                        SetOwnedTable(foreignKey);
                    }
                }
            }

            return annotation;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalRelationshipBuilder Apply(InternalRelationshipBuilder relationshipBuilder)
        {
            var foreignKey = relationshipBuilder.Metadata;
            if (foreignKey.IsOwnership
                && foreignKey.IsUnique)
            {
                SetOwnedTable(foreignKey);
            }

            return relationshipBuilder;
        }

        private static void SetOwnedTable(ForeignKey foreignKey)
        {
            var ownerType = foreignKey.PrincipalEntityType;
            foreignKey.DeclaringEntityType.Builder.Relational(ConfigurationSource.Convention)
                .ToTable(ownerType.Relational().TableName, ownerType.Relational().Schema);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalModelBuilder Apply(InternalModelBuilder modelBuilder)
        {
            var maxLength = modelBuilder.Relational(ConfigurationSource.Convention).MaxIdentifierLength;
            var tables = new Dictionary<(string, string),
                (Dictionary<string, Property> Columns,
                Dictionary<string, Key> Keys,
                Dictionary<string, ForeignKey> ForeignKeys,
                Dictionary<string, Index> Indexes)>();
            foreach (var entityType in modelBuilder.Metadata.GetEntityTypes())
            {
                if (entityType.IsQueryType)
                {
                    continue;
                }

                var annotations = entityType.Relational();
                var tableName = (annotations.Schema, annotations.TableName);
                if (!tables.TryGetValue(tableName, out var tableObjects))
                {
                    tableObjects = (new Dictionary<string, Property>(StringComparer.Ordinal),
                        new Dictionary<string, Key>(StringComparer.Ordinal),
                        new Dictionary<string, ForeignKey>(StringComparer.Ordinal),
                        new Dictionary<string, Index>(StringComparer.Ordinal));
                    tables[tableName] = tableObjects;
                }

                TryUniquifyColumnNames(entityType, tableObjects.Columns, maxLength);
                TryUniquifyKeyNames(entityType, tableObjects.Keys, maxLength);
                TryUniquifyForeignKeyNames(entityType, tableObjects.ForeignKeys, maxLength);
                TryUniquifyIndexNames(entityType, tableObjects.Indexes, maxLength);
            }

            return modelBuilder;
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

                if (!property.IsPrimaryKey())
                {
                    var relationalPropertyBuilder = property.Builder.Relational(ConfigurationSource.Convention);
                    if (relationalPropertyBuilder.CanSetColumnName(null))
                    {
                        columnName = Uniquify(columnName, property.DeclaringEntityType.ShortName(), properties, maxLength);
                        relationalPropertyBuilder.ColumnName = columnName;
                        properties[columnName] = property;
                        continue;
                    }
                }

                if (!otherProperty.IsPrimaryKey())
                {
                    var otherRelationalPropertyBuilder = otherProperty.Builder.Relational(ConfigurationSource.Convention);
                    if (otherRelationalPropertyBuilder.CanSetColumnName(null))
                    {
                        properties[columnName] = property;
                        columnName = Uniquify(columnName, otherProperty.DeclaringEntityType.ShortName(), properties, maxLength);
                        otherRelationalPropertyBuilder.ColumnName = columnName;
                        properties[columnName] = otherProperty;
                    }
                }
            }
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
                    var relationalKeyBuilder = key.Builder.Relational(ConfigurationSource.Convention);
                    if (relationalKeyBuilder.CanSetName(null))
                    {
                        keyName = Uniquify(keyName, null, keys, maxLength);
                        relationalKeyBuilder.Name = keyName;
                        keys[keyName] = key;
                        continue;
                    }
                }

                if (!otherKey.IsPrimaryKey())
                {
                    var otherRelationalKeyBuilder = otherKey.Builder.Relational(ConfigurationSource.Convention);
                    if (otherRelationalKeyBuilder.CanSetName(null))
                    {
                        keys[keyName] = key;
                        keyName = Uniquify(keyName, null, keys, maxLength);
                        otherRelationalKeyBuilder.Name = keyName;
                        keys[keyName] = otherKey;
                    }
                }
            }
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

                    indexName = Uniquify(indexName, null, indexes, maxLength);
                    relationalIndexBuilder.Name = indexName;
                    indexes[indexName] = index;
                    continue;
                }

                if (otherRelationalIndexBuilder.CanSetName(null))
                {
                    indexes[indexName] = index;
                    indexName = Uniquify(indexName, null, indexes, maxLength);
                    otherRelationalIndexBuilder.Name = indexName;
                    indexes[indexName] = otherIndex;
                }
            }
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

                    foreignKeyName = Uniquify(foreignKeyName, null, foreignKeys, maxLength);
                    relationalKeyBuilder.Name = foreignKeyName;
                    foreignKeys[foreignKeyName] = foreignKey;
                    continue;
                }

                if (otherRelationalKeyBuilder.CanSetName(null))
                {
                    foreignKeys[foreignKeyName] = foreignKey;
                    foreignKeyName = Uniquify(foreignKeyName, null, foreignKeys, maxLength);
                    otherRelationalKeyBuilder.Name = foreignKeyName;
                    foreignKeys[foreignKeyName] = otherForeignKey;
                }
            }
        }

        private static string Uniquify<T>(string baseIdentifier, string prefix, Dictionary<string, T> existingIdentifiers, int maxLength)
        {
            if (!string.IsNullOrEmpty(prefix)
                && !baseIdentifier.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                baseIdentifier = prefix + "_" + baseIdentifier;
            }

            var finalIdentifier = ConstraintNamer.Truncate(baseIdentifier, null, maxLength);
            var suffix = 1;
            while (existingIdentifiers.ContainsKey(finalIdentifier))
            {
                finalIdentifier = ConstraintNamer.Truncate(baseIdentifier, suffix++, maxLength);
            }

            return finalIdentifier;
        }
    }
}
