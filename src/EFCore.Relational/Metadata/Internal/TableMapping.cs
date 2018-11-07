// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class TableMapping
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public TableMapping(
            [CanBeNull] string schema,
            [NotNull] string name,
            [NotNull] IReadOnlyList<IEntityType> entityTypes)
        {
            Schema = schema;
            Name = name;
            EntityTypes = entityTypes;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual string Schema { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual string Name { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IReadOnlyList<IEntityType> EntityTypes { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IEntityType GetRootType()
            => EntityTypes.SingleOrDefault(
                t => t.BaseType == null
                     && t.FindForeignKeys(t.FindDeclaredPrimaryKey().Properties)
                         .All(
                             fk => !fk.PrincipalKey.IsPrimaryKey()
                                   || fk.PrincipalEntityType.RootType() == t
                                   || t.Relational().TableName != fk.PrincipalEntityType.Relational().TableName
                                   || t.Relational().Schema != fk.PrincipalEntityType.Relational().Schema));

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IEnumerable<IProperty> GetProperties() => GetPropertyMap().Values;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Dictionary<string, IProperty> GetPropertyMap()
        {
            var dictionary = new Dictionary<string, IProperty>(StringComparer.Ordinal);
            foreach (var property in EntityTypes.SelectMany(EntityTypeExtensions.GetDeclaredProperties))
            {
                var columnName = property.Relational().ColumnName;
                if (!dictionary.ContainsKey(columnName))
                {
                    dictionary[columnName] = property;
                }
            }

            return dictionary;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IEnumerable<IKey> GetKeys()
            => EntityTypes.SelectMany(EntityTypeExtensions.GetDeclaredKeys)
                .Distinct((x, y) => x.Relational().Name == y.Relational().Name);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IEnumerable<IIndex> GetIndexes()
            => EntityTypes.SelectMany(EntityTypeExtensions.GetDeclaredIndexes)
                .Distinct((x, y) => x.Relational().Name == y.Relational().Name);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IEnumerable<IForeignKey> GetForeignKeys()
            => EntityTypes.SelectMany(EntityTypeExtensions.GetDeclaredForeignKeys)
                .Distinct((x, y) => x.Relational().Name == y.Relational().Name)
                .Where(
                    fk => !(EntityTypes.Contains(fk.PrincipalEntityType)
                            && fk.Properties.Select(p => p.Relational().ColumnName)
                                .SequenceEqual(fk.PrincipalKey.Properties.Select(p => p.Relational().ColumnName))));

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static IReadOnlyList<TableMapping> GetTableMappings([NotNull] IModel model)
        {
            var tables = new Dictionary<(string Schema, string TableName), List<IEntityType>>();
            foreach (var entityType in model.GetEntityTypes().Where(et => !et.IsQueryType))
            {
                var relationalExtentions = entityType.Relational();
                var fullName = (relationalExtentions.Schema, relationalExtentions.TableName);
                if (!tables.TryGetValue(fullName, out var mappedEntityTypes))
                {
                    mappedEntityTypes = new List<IEntityType>();
                    tables.Add(fullName, mappedEntityTypes);
                }

                // TODO: Consider sorting to keep hierarchies together
                mappedEntityTypes.Add(entityType);
            }

            return tables.Select(kv => new TableMapping(kv.Key.Schema, kv.Key.TableName, kv.Value))
                .OrderBy(t => t.Schema).ThenBy(t => t.Name).ToList();
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static TableMapping GetTableMapping([NotNull] IModel model, [NotNull] string table, [CanBeNull] string schema)
        {
            var mappedEntities = new List<IEntityType>();
            foreach (var entityType in model.GetEntityTypes().Where(et => !et.IsQueryType))
            {
                var relationalExtentions = entityType.Relational();
                if (table == relationalExtentions.TableName
                    && schema == relationalExtentions.Schema)
                {
                    mappedEntities.Add(entityType);
                }
            }

            return mappedEntities.Count > 0
                ? new TableMapping(schema, table, mappedEntities)
                : null;
        }
    }
}
