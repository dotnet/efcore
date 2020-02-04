// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class RelationalModel
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static IModel AddRelationalModel([NotNull] IConventionModel model)
        {
            var tables = new SortedDictionary<(string, string), Table>();
            foreach (var entityType in model.GetEntityTypes())
            {
                var tableName = entityType.GetTableName();
                if (tableName == null
                    || entityType.GetViewName() != null)
                {
                    continue;
                }

                var schema = entityType.GetSchema();
                if (!tables.TryGetValue((tableName, schema), out var table))
                {
                    table = new Table(tableName, schema);
                    tables.Add((tableName, schema), table);
                }

                table.IsMigratable = true;

                var tableMapping = new TableMapping(entityType, table, includesDerivedTypes: true);
                foreach (var property in entityType.GetDeclaredProperties())
                {
                    var typeMapping = property.GetRelationalTypeMapping();
                    var columnName = property.GetColumnName();
                    var column = table.FindColumn(columnName) as Column;
                    if (column == null)
                    {
                        column = new Column(columnName, property.GetColumnType() ?? typeMapping.StoreType, table);
                        column.IsNullable = property.IsColumnNullable();
                        table.Columns.Add(columnName, column);
                    }
                    else if (!property.IsColumnNullable())
                    {
                        column.IsNullable = false;
                    }

                    var columnMapping = new ColumnMapping(property, column, typeMapping, tableMapping);
                    tableMapping.ColumnMappings.Add(columnMapping);
                    column.PropertyMappings.Add(columnMapping);

                    var columnMappings = property[RelationalAnnotationNames.TableColumnMappings] as SortedSet<ColumnMapping>;
                    if (columnMappings == null)
                    {
                        columnMappings = new SortedSet<ColumnMapping>(ColumnMappingComparer.Instance);
                        property.SetAnnotation(RelationalAnnotationNames.TableColumnMappings, columnMappings);
                    }

                    columnMappings.Add(columnMapping);
                }

                var tableMappings = entityType[RelationalAnnotationNames.TableMappings] as SortedSet<TableMapping>;
                if (tableMappings == null)
                {
                    tableMappings = new SortedSet<TableMapping>(TableMappingComparer.Instance);
                    entityType.SetAnnotation(RelationalAnnotationNames.TableMappings, tableMappings);
                }

                tableMappings.Add(tableMapping);
                table.EntityTypeMappings.Add(tableMapping);
            }

            foreach (var table in tables.Values)
            {
                SortedDictionary<IEntityType, IEnumerable<IForeignKey>> internalForeignKeyMap = null;
                SortedDictionary<IEntityType, IEnumerable<IForeignKey>> referencingInternalForeignKeyMap = null;
                foreach (var entityTypeMapping in table.EntityTypeMappings)
                {
                    var entityType = entityTypeMapping.EntityType;
                    var primaryKey = entityType.FindPrimaryKey();
                    if (primaryKey == null)
                    {
                        continue;
                    }

                    SortedSet<IForeignKey> internalForeignKeys = null;
                    foreach (var foreignKey in entityType.FindForeignKeys(primaryKey.Properties))
                    {
                        if (foreignKey.IsUnique
                            && foreignKey.PrincipalKey.IsPrimaryKey()
                            && !foreignKey.IsIntraHierarchical()
                            && table.EntityTypeMappings.Any(m => m.EntityType == foreignKey.PrincipalEntityType))
                        {
                            if (internalForeignKeys == null)
                            {
                                internalForeignKeys = new SortedSet<IForeignKey>(ForeignKeyComparer.Instance);
                            }
                            internalForeignKeys.Add(foreignKey);

                            if (referencingInternalForeignKeyMap == null)
                            {
                                referencingInternalForeignKeyMap =
                                    new SortedDictionary<IEntityType, IEnumerable<IForeignKey>>(EntityTypePathComparer.Instance);
                            }

                            var principalEntityType = foreignKey.PrincipalEntityType;
                            if (!referencingInternalForeignKeyMap.TryGetValue(principalEntityType, out var internalReferencingForeignKeys))
                            {
                                internalReferencingForeignKeys = new SortedSet<IForeignKey>(ForeignKeyComparer.Instance);
                                referencingInternalForeignKeyMap[principalEntityType] = internalReferencingForeignKeys;
                            }
                            ((SortedSet<IForeignKey>)internalReferencingForeignKeys).Add(foreignKey);
                        }
                    }

                    if (internalForeignKeys != null)
                    {
                        if (internalForeignKeyMap == null)
                        {
                            internalForeignKeyMap =
                                new SortedDictionary<IEntityType, IEnumerable<IForeignKey>>(EntityTypePathComparer.Instance);
                            table.InternalForeignKeys = internalForeignKeyMap;
                        }

                        internalForeignKeyMap[entityType] = internalForeignKeys;
                    }

                    if (internalForeignKeys == null
                        && table.EntityTypeMappings.Any(m => !m.EntityType.IsSameHierarchy(entityType)))
                    {
                        table.IsSplit = true;
                    }
                }

                if (referencingInternalForeignKeyMap != null)
                {
                    table.ReferencingInternalForeignKeys = referencingInternalForeignKeyMap;
                }
            }

            model.SetAnnotation(RelationalAnnotationNames.Tables, tables);
            return model;
        }
    }
}
