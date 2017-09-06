// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

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
            [NotNull] string schema,
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
        public virtual IEnumerable<IProperty> GetProperties()
            => EntityTypes.SelectMany(EntityTypeExtensions.GetDeclaredProperties)
                .Distinct((x, y) => x.Relational().ColumnName == y.Relational().ColumnName);

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
            foreach (var entityType in model.GetEntityTypes().Where(et => !et.IsQueryType()))
            {
                var fullName = (entityType.Relational().Schema, entityType.Relational().TableName);
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
    }
}
