// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
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
            var tables = new Dictionary<string, (List<EntityType> MappedEntityTypes, Dictionary<string, Property> Columns)>();
            foreach (var entityType in modelBuilder.Metadata.GetEntityTypes())
            {
                var annotations = entityType.Relational();
                var tableName = Format(annotations.Schema, annotations.TableName);

                if (tables.TryGetValue(tableName, out var tableMapping))
                {
                    if (tableMapping.MappedEntityTypes.Count == 1)
                    {
                        TryUniquifyColumnNames(tableMapping.MappedEntityTypes[0], tableMapping.Columns);
                    }
                    tableMapping.MappedEntityTypes.Add(entityType);
                    TryUniquifyColumnNames(entityType, tableMapping.Columns);
                }
                else
                {
                    var mappedEntityTypes = new List<EntityType>();
                    tables[tableName] = (mappedEntityTypes, new Dictionary<string, Property>());
                    mappedEntityTypes.Add(entityType);
                }
            }

            return modelBuilder;
        }

        private static void TryUniquifyColumnNames(EntityType entityType, Dictionary<string, Property> properties)
        {
            foreach (var property in entityType.GetDeclaredProperties())
            {
                var columnName = property.Relational().ColumnName;
                if (properties.TryGetValue(columnName, out var otherProperty)
                    && !property.IsPrimaryKey())
                {
                    var relationalPropertyBuilder = property.Builder.Relational(ConfigurationSource.Convention);
                    if (relationalPropertyBuilder.CanSetColumnName(null))
                    {
                        relationalPropertyBuilder.ColumnName = Uniquify(columnName, properties);
                        continue;
                    }

                    var otherRelationalPropertyBuilder = otherProperty.Builder.Relational(ConfigurationSource.Convention);
                    if (!otherRelationalPropertyBuilder.CanSetColumnName(null))
                    {
                        continue;
                    }
                    otherRelationalPropertyBuilder.ColumnName = Uniquify(columnName, properties);
                }
                properties[columnName] = property;
            }
        }

        private static string Uniquify<T>(string baseIdentifier, Dictionary<string, T> existingIdentifiers)
        {
            var finalIdentifier = baseIdentifier;
            var suffix = 1;
            while (existingIdentifiers.ContainsKey(finalIdentifier))
            {
                finalIdentifier = baseIdentifier + suffix;
                suffix++;
            }

            return finalIdentifier;
        }

        private static string Format(string schema, string name)
            => (string.IsNullOrEmpty(schema) ? "" : schema + ".") + name;
    }
}
