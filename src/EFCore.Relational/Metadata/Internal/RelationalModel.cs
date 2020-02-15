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
            var views = new SortedDictionary<(string, string), View>();
            foreach (var entityType in model.GetEntityTypes())
            {
                var tableName = entityType.GetTableName();
                var viewName = entityType.GetViewName();
                if (tableName != null
                    && viewName == null)
                {
                    var schema = entityType.GetSchema();
                    if (!tables.TryGetValue((tableName, schema), out var table))
                    {
                        table = new Table(tableName, schema);
                        tables.Add((tableName, schema), table);
                    }

                    table.IsMigratable = table.IsMigratable
                        || entityType.FindAnnotation(RelationalAnnotationNames.ViewDefinition) == null;

                    var tableMapping = new TableMapping(entityType, table, includesDerivedTypes: true);
                    foreach (var property in entityType.GetDeclaredProperties())
                    {
                        var typeMapping = property.FindRelationalTypeMapping();
                        var columnName = property.GetColumnName();
                        var column = (Column)table.FindColumn(columnName);
                        if (column == null)
                        {
                            column = new Column(columnName, property.GetColumnType() ?? typeMapping?.StoreType, table);
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

                if (viewName != null)
                {
                    var schema = entityType.GetSchema();
                    if (!views.TryGetValue((viewName, schema), out var view))
                    {
                        view = new View(viewName, schema);
                        views.Add((viewName, schema), view);
                    }

                    var viewMapping = new ViewMapping(entityType, view, includesDerivedTypes: true);
                    foreach (var property in entityType.GetDeclaredProperties())
                    {
                        var typeMapping = property.FindRelationalTypeMapping();
                        var columnName = property.GetColumnName();
                        var column = (ViewColumn)view.FindColumn(columnName);
                        if (column == null)
                        {
                            column = new ViewColumn(columnName, property.GetColumnType() ?? typeMapping.StoreType, view);
                            column.IsNullable = property.IsColumnNullable();
                            view.Columns.Add(columnName, column);
                        }
                        else if (!property.IsColumnNullable())
                        {
                            column.IsNullable = false;
                        }

                        var columnMapping = new ViewColumnMapping(property, column, typeMapping, viewMapping);
                        viewMapping.ColumnMappings.Add(columnMapping);
                        column.PropertyMappings.Add(columnMapping);

                        var columnMappings = property[RelationalAnnotationNames.ViewColumnMappings] as SortedSet<ViewColumnMapping>;
                        if (columnMappings == null)
                        {
                            columnMappings = new SortedSet<ViewColumnMapping>(ViewColumnMappingComparer.Instance);
                            property.SetAnnotation(RelationalAnnotationNames.ViewColumnMappings, columnMappings);
                        }

                        columnMappings.Add(columnMapping);
                    }

                    var tableMappings = entityType[RelationalAnnotationNames.ViewMappings] as SortedSet<ViewMapping>;
                    if (tableMappings == null)
                    {
                        tableMappings = new SortedSet<ViewMapping>(ViewMappingComparer.Instance);
                        entityType.SetAnnotation(RelationalAnnotationNames.ViewMappings, tableMappings);
                    }

                    tableMappings.Add(viewMapping);
                    view.EntityTypeMappings.Add(viewMapping);
                }
            }

            if (tables.Any())
            {
                foreach (var table in tables.Values)
                {
                    PopulateInternalForeignKeys(table);
                }

                model.SetAnnotation(RelationalAnnotationNames.Tables, tables);
            }

            if (views.Any())
            {
                foreach (var view in views.Values)
                {
                    PopulateInternalForeignKeys(view);
                }

                model.SetAnnotation(RelationalAnnotationNames.Views, views);
            }

            return model;
        }

        private static void PopulateInternalForeignKeys(TableBase table)
        {
            SortedDictionary<IEntityType, IEnumerable<IForeignKey>> internalForeignKeyMap = null;
            SortedDictionary<IEntityType, IEnumerable<IForeignKey>> referencingInternalForeignKeyMap = null;
            foreach (var entityTypeMapping in ((ITableBase)table).EntityTypeMappings)
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
                        && ((ITableBase)table).EntityTypeMappings.Any(m => m.EntityType == foreignKey.PrincipalEntityType))
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
                    && ((ITableBase)table).EntityTypeMappings.Any(m => !m.EntityType.IsSameHierarchy(entityType)))
                {
                    table.IsSplit = true;
                }
            }

            if (referencingInternalForeignKeyMap != null)
            {
                table.ReferencingInternalForeignKeys = referencingInternalForeignKeyMap;
            }
        }
    }
}
