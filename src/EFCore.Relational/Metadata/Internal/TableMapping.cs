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
        private readonly IRelationalAnnotationProvider _annotations;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public TableMapping(
            [NotNull] string schema,
            [NotNull] string name,
            [NotNull] IReadOnlyList<IEntityType> entityTypes,
            [NotNull] IRelationalAnnotationProvider annotations)
        {
            Schema = schema;
            Name = name;
            EntityTypes = entityTypes;
            _annotations = annotations;
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
                .Distinct((x, y) => _annotations.For(x).ColumnName == _annotations.For(y).ColumnName);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IEnumerable<IKey> GetKeys()
            => EntityTypes.SelectMany(EntityTypeExtensions.GetDeclaredKeys)
                .Distinct((x, y) => _annotations.For(x).Name == _annotations.For(y).Name);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IEnumerable<IIndex> GetIndexes()
            => EntityTypes.SelectMany(EntityTypeExtensions.GetDeclaredIndexes)
                .Distinct((x, y) => _annotations.For(x).Name == _annotations.For(y).Name);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IEnumerable<IForeignKey> GetForeignKeys()
            => EntityTypes.SelectMany(EntityTypeExtensions.GetDeclaredForeignKeys)
                .Distinct((x, y) => _annotations.For(x).Name == _annotations.For(y).Name)
                .Where(fk => !(EntityTypes.Contains(fk.PrincipalEntityType)
                               && fk.Properties.Select(p => _annotations.For(p).ColumnName)
                                   .SequenceEqual(fk.PrincipalKey.Properties.Select(p => _annotations.For(p).ColumnName))));

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static IReadOnlyList<TableMapping> GetTableMappings([NotNull] IModel model, [NotNull] IRelationalAnnotationProvider annotations)
        {
            var tables = new Dictionary<(string Schema, string TableName), List<IEntityType>>();
            foreach (var entityType in model.GetEntityTypes())
            {
                var fullName = (annotations.For(entityType).Schema, annotations.For(entityType).TableName);
                if (!tables.TryGetValue(fullName, out var mappedEntityTypes))
                {
                    mappedEntityTypes = new List<IEntityType>();
                    tables.Add(fullName, mappedEntityTypes);
                }

                // TODO: Consider sorting to keep hierarchies together
                mappedEntityTypes.Add(entityType);
            }

            return tables.Select(kv => new TableMapping(kv.Key.Schema, kv.Key.TableName, kv.Value, annotations))
                .OrderBy(t => t.Schema).ThenBy(t => t.Name).ToList();
        }
    }
}
