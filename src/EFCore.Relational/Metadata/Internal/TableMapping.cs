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
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class TableMapping
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
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
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual string Schema { get; }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual string Name { get; }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IReadOnlyList<IEntityType> EntityTypes { get; }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IEntityType GetRootType()
            => EntityTypes.SingleOrDefault(
                t => t.BaseType == null
                     && (t.FindDeclaredPrimaryKey() == null || t.FindForeignKeys(t.FindDeclaredPrimaryKey().Properties)
                         .All(
                             fk => !fk.PrincipalKey.IsPrimaryKey()
                                   || fk.PrincipalEntityType.GetRootType() == t
                                   || t.GetTableName() != fk.PrincipalEntityType.GetTableName()
                                   || t.GetSchema() != fk.PrincipalEntityType.GetSchema())));

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IEnumerable<IProperty> GetProperties() => GetPropertyMap().Values;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual Dictionary<string, IProperty> GetPropertyMap()
        {
            var dictionary = new Dictionary<string, IProperty>(StringComparer.Ordinal);
            foreach (var property in EntityTypes.SelectMany(EntityFrameworkCore.EntityTypeExtensions.GetDeclaredProperties))
            {
                var columnName = property.GetColumnName();
                if (!dictionary.ContainsKey(columnName))
                {
                    dictionary[columnName] = property;
                }
            }

            return dictionary;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IEnumerable<IKey> GetKeys()
            => EntityTypes.SelectMany(EntityFrameworkCore.EntityTypeExtensions.GetDeclaredKeys)
                .Distinct((x, y) => x.GetName() == y.GetName());

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IEnumerable<IIndex> GetIndexes()
            => EntityTypes.SelectMany(EntityFrameworkCore.EntityTypeExtensions.GetDeclaredIndexes)
                .Distinct((x, y) => x.GetName() == y.GetName());

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IEnumerable<IForeignKey> GetForeignKeys()
            => EntityTypes.SelectMany(EntityFrameworkCore.EntityTypeExtensions.GetDeclaredForeignKeys)
                .Distinct((x, y) => x.GetConstraintName() == y.GetConstraintName())
                .Where(
                    fk => !(EntityTypes.Contains(fk.PrincipalEntityType)
                            && fk.Properties.Select(p => p.GetColumnName())
                                .SequenceEqual(fk.PrincipalKey.Properties.Select(p => p.GetColumnName()))));

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IEnumerable<ICheckConstraint> GetCheckConstraints()
            => EntityTypes.SelectMany(CheckConstraint.GetCheckConstraints)
                .Distinct((x, y) => x.Name == y.Name);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static IReadOnlyList<TableMapping> GetTableMappings([NotNull] IModel model)
        {
            var tables = new Dictionary<(string Schema, string TableName), List<IEntityType>>();
            foreach (var entityType in model.GetEntityTypes().Where(et => !et.IsIgnoredByMigrations()))
            {
                var fullName = (entityType.GetSchema(), entityType.GetTableName());
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
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static TableMapping GetTableMapping([NotNull] IModel model, [NotNull] string table, [CanBeNull] string schema)
        {
            var mappedEntities = new List<IEntityType>();
            foreach (var entityType in model.GetEntityTypes().Where(et => et.FindPrimaryKey() != null))
            {
                if (table == entityType.GetTableName()
                    && schema == entityType.GetSchema())
                {
                    mappedEntities.Add(entityType);
                }
            }

            return mappedEntities.Count > 0
                ? new TableMapping(schema, table, mappedEntities)
                : null;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual string GetComment()
            => EntityTypes.Select(e => e.GetComment()).FirstOrDefault(c => c != null);

    }
}
