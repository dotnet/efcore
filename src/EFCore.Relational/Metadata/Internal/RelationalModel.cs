// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Migrations;

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
                if (tableName != null)
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
                    var schema = entityType.GetViewSchema();
                    if (!views.TryGetValue((viewName, schema), out var view))
                    {
                        view = new View(viewName, schema);
                        views.Add((viewName, schema), view);
                    }

                    var viewMapping = new ViewMapping(entityType, view, includesDerivedTypes: true);
                    foreach (var property in entityType.GetDeclaredProperties())
                    {
                        var typeMapping = property.FindRelationalTypeMapping();
                        var columnName = property.GetViewColumnName();
                        var column = (ViewColumn)view.FindColumn(columnName);
                        if (column == null)
                        {
                            column = new ViewColumn(columnName, property.GetColumnType() ?? typeMapping.StoreType, view);
                            column.IsNullable = property.IsViewColumnNullable();
                            view.Columns.Add(columnName, column);
                        }
                        else if (!property.IsViewColumnNullable())
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
                    PopulateForeignKeyConstraints(table);
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

        private static void PopulateForeignKeyConstraints(Table table)
        {
            foreach (var entityTypeMapping in ((ITable)table).EntityTypeMappings)
            {
                var entityType = entityTypeMapping.EntityType;
                foreach (IConventionForeignKey foreignKey in entityType.GetForeignKeys())
                {
                    var principalMappings = foreignKey.PrincipalEntityType.GetTableMappings();
                    if (principalMappings == null)
                    {
                        continue;
                    }

                    var name = foreignKey.GetConstraintName();
                    var foreignKeyConstraints = foreignKey[RelationalAnnotationNames.ForeignKeyMappings] as SortedSet<ForeignKeyConstraint>;
                    if (table.ForeignKeyConstraints.TryGetValue(name, out var constraint))
                    {
                        if (foreignKeyConstraints == null)
                        {
                            foreignKeyConstraints = new SortedSet<ForeignKeyConstraint>(ForeignKeyConstraintComparer.Instance);
                            foreignKey.SetOrRemoveAnnotation(RelationalAnnotationNames.ForeignKeyMappings, foreignKeyConstraints);
                        }

                        foreignKeyConstraints.Add(constraint);

                        constraint.ForeignKeyMappings.Add(foreignKey);
                        continue;
                    }

                    var principalColumns = new Column[foreignKey.Properties.Count];
                    Table principalTable = null;
                    for (var i = 0; i < principalColumns.Length; i++)
                    {
                        var property = foreignKey.PrincipalKey.Properties[i];
                        foreach (var columnMapping in property.GetTableColumnMappings())
                        {
                            if (principalColumns[i] != null
                                && principalColumns[i] != columnMapping.Column)
                            {
                                // Principal property is mapped to multiple columns, so the constraint is not enforceable
                                principalColumns[i] = null;
                                break;
                            }

                            principalColumns[i] = (Column)columnMapping.Column;
                        }

                        if (principalColumns[i] == null)
                        {
                            principalColumns = null;
                            break;
                        }

                        if (principalTable == null)
                        {
                            principalTable = (Table)principalColumns[i].Table;
                        }
                        else if (principalTable != principalColumns[i].Table)
                        {
                            // Principal properties are mapped to several tables, so the constraint is not enforceable
                            principalColumns = null;
                        }
                    }

                    if (principalColumns == null)
                    {
                        continue;
                    }

                    var columns = new Column[foreignKey.Properties.Count];
                    for (var i = 0; i < columns.Length; i++)
                    {
                        var property = foreignKey.Properties[i];
                        foreach (var columnMapping in property.GetTableColumnMappings())
                        {
                            if (columnMapping.TableMapping.Table == table)
                            {
                                columns[i] = (Column)columnMapping.Column;
                                break;
                            }
                        }

                        if (columns[i] == null)
                        {
                            columns = null;
                            break;
                        }
                    }

                    if (columns == null
                        || columns.SequenceEqual(principalColumns))
                    {
                        continue;
                    }

                    constraint = new ForeignKeyConstraint(
                        name, table, principalTable, columns, principalColumns, ToReferentialAction(foreignKey.DeleteBehavior));
                    constraint.ForeignKeyMappings.Add(foreignKey);

                    if (foreignKeyConstraints == null)
                    {
                        foreignKeyConstraints = new SortedSet<ForeignKeyConstraint>(ForeignKeyConstraintComparer.Instance);
                        foreignKey.SetOrRemoveAnnotation(RelationalAnnotationNames.ForeignKeyMappings, foreignKeyConstraints);
                    }

                    foreignKeyConstraints.Add(constraint);

                    table.ForeignKeyConstraints.Add(name, constraint);
                }
            }
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

        private static ReferentialAction ToReferentialAction(DeleteBehavior deleteBehavior)
        {
            switch (deleteBehavior)
            {
                case DeleteBehavior.SetNull:
                    return ReferentialAction.SetNull;
                case DeleteBehavior.Cascade:
                    return ReferentialAction.Cascade;
                case DeleteBehavior.NoAction:
                case DeleteBehavior.ClientNoAction:
                case DeleteBehavior.ClientSetNull:
                case DeleteBehavior.ClientCascade:
                    return ReferentialAction.NoAction;
                case DeleteBehavior.Restrict:
                    return ReferentialAction.Restrict;
                default:
                    throw new NotImplementedException(deleteBehavior.ToString());
            }
        }
    }
}
