// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class RelationalModel : Annotatable, IRelationalModel
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public RelationalModel([NotNull] IModel model)
        {
            Model = model;
        }

        /// <inheritdoc/>
        public virtual IModel Model { get; }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual SortedDictionary<(string, string), Table> Tables { get; } = new SortedDictionary<(string, string), Table>();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual SortedDictionary<(string, string), View> Views { get; } = new SortedDictionary<(string, string), View>();

        /// <inheritdoc/>
        public virtual ITable FindTable(string name, string schema)
            => Tables.TryGetValue((name, schema), out var table)
                ? table
                : null;

        /// <inheritdoc/>
        public virtual IView FindView(string name, string schema)
            => Views.TryGetValue((name, schema), out var view)
                ? view
                : null;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static IModel Add(
            [NotNull] IConventionModel model, [CanBeNull] IRelationalAnnotationProvider relationalAnnotationProvider)
        {
            var databaseModel = new RelationalModel(model);
            model.SetAnnotation(RelationalAnnotationNames.RelationalModel, databaseModel);

            foreach (var entityType in model.GetEntityTypes())
            {
                var tableName = entityType.GetTableName();
                if (tableName != null)
                {
                    var schema = entityType.GetSchema();
                    TableMapping lastMapping = null;
                    var mappedType = entityType;
                    SortedSet<TableMapping> tableMappings = null;
                    while (mappedType != null)
                    {
                        var mappedTable = mappedType.GetTableName();
                        var mappedSchema = mappedType.GetSchema();

                        if (mappedTable == null
                            || (mappedTable == tableName
                                && mappedSchema == schema
                                && mappedType != entityType))
                        {
                            break;
                        }

                        if (!databaseModel.Tables.TryGetValue((mappedTable, mappedSchema), out var table))
                        {
                            table = new Table(mappedTable, mappedSchema, databaseModel);
                            databaseModel.Tables.Add((mappedTable, mappedSchema), table);
                        }

                        table.IsExcludedFromMigrations = table.IsExcludedFromMigrations
                            && entityType.FindAnnotation(RelationalAnnotationNames.ViewDefinitionSql) != null;

                        var tableMapping = new TableMapping(entityType, table, includesDerivedTypes: true);
                        foreach (var property in mappedType.GetProperties())
                        {
                            var columnName = property.GetColumnName(mappedTable, mappedSchema);
                            if (columnName == null)
                            {
                                continue;
                            }

                            var typeMapping = property.FindRelationalTypeMapping(mappedTable, mappedSchema);
                            var column = (Column)table.FindColumn(columnName);
                            if (column == null)
                            {
                                column = new Column(columnName, property.GetColumnType(mappedTable, mappedSchema)
                                    ?? typeMapping?.StoreType, table);
                                column.IsNullable = property.IsColumnNullable(mappedTable, mappedSchema);
                                table.Columns.Add(columnName, column);
                            }
                            else if (!property.IsColumnNullable(mappedTable, mappedSchema))
                            {
                                column.IsNullable = false;
                            }

                            var columnMapping = new ColumnMapping(property, column, typeMapping, tableMapping);
                            tableMapping.ColumnMappings.Add(columnMapping);
                            column.PropertyMappings.Add(columnMapping);

                            var columnMappings = property[RelationalAnnotationNames.TableColumnMappings] as SortedSet<ColumnMapping>;
                            if (columnMappings == null)
                            {
                                columnMappings = new SortedSet<ColumnMapping>(ColumnMappingBaseComparer.Instance);
                                property.SetAnnotation(RelationalAnnotationNames.TableColumnMappings, columnMappings);
                            }

                            columnMappings.Add(columnMapping);
                        }

                        mappedType = mappedType.BaseType;
                        if (lastMapping != null
                            && tableMapping.ColumnMappings.Count == 0)
                        {
                            continue;
                        }

                        tableMappings = entityType[RelationalAnnotationNames.TableMappings] as SortedSet<TableMapping>;
                        if (tableMappings == null)
                        {
                            tableMappings = new SortedSet<TableMapping>(TableMappingBaseComparer.EntityTypeInstance);
                            entityType.SetAnnotation(RelationalAnnotationNames.TableMappings, tableMappings);
                        }

                        tableMappings.Add(tableMapping);
                        table.EntityTypeMappings.Add(tableMapping);

                        lastMapping = tableMapping;
                    }

                    // Re-add the mapping to update the order
                    tableMappings.Remove(lastMapping);
                    lastMapping.IsMainTableMapping = true;
                    tableMappings.Add(lastMapping);
                }

                var viewName = entityType.GetViewName();
                if (viewName != null)
                {
                    var schema = entityType.GetViewSchema();
                    ViewMapping lastMapping = null;
                    SortedSet<ViewMapping> viewMappings = null;
                    var mappedType = entityType;
                    while (mappedType != null)
                    {
                        var mappedViewName = mappedType.GetViewName();
                        var mappedSchema = mappedType.GetViewSchema();

                        if (mappedViewName == null
                            || (mappedViewName == viewName
                                && mappedSchema == schema
                                && mappedType != entityType))
                        {
                            break;
                        }

                        if (!databaseModel.Views.TryGetValue((mappedViewName, mappedSchema), out var view))
                        {
                            view = new View(mappedViewName, mappedSchema, databaseModel);
                            databaseModel.Views.Add((mappedViewName, mappedSchema), view);
                        }

                        var viewMapping = new ViewMapping(entityType, view, includesDerivedTypes: true);
                        foreach (var property in mappedType.GetProperties())
                        {
                            var columnName = property.GetViewColumnName(mappedViewName, mappedSchema);
                            if (columnName == null)
                            {
                                continue;
                            }

                            var typeMapping = property.FindRelationalTypeMapping();
                            var column = (ViewColumn)view.FindColumn(columnName);
                            if (column == null)
                            {
                                column = new ViewColumn(columnName, property.GetColumnType(mappedViewName, mappedSchema)
                                    ?? typeMapping.StoreType, view);
                                column.IsNullable = property.IsViewColumnNullable(mappedViewName, mappedSchema);
                                view.Columns.Add(columnName, column);
                            }
                            else if (!property.IsViewColumnNullable(mappedViewName, mappedSchema))
                            {
                                column.IsNullable = false;
                            }

                            var columnMapping = new ViewColumnMapping(property, column, typeMapping, viewMapping);
                            viewMapping.ColumnMappings.Add(columnMapping);
                            column.PropertyMappings.Add(columnMapping);

                            var columnMappings = property[RelationalAnnotationNames.ViewColumnMappings] as SortedSet<ViewColumnMapping>;
                            if (columnMappings == null)
                            {
                                columnMappings = new SortedSet<ViewColumnMapping>(ColumnMappingBaseComparer.Instance);
                                property.SetAnnotation(RelationalAnnotationNames.ViewColumnMappings, columnMappings);
                            }

                            columnMappings.Add(columnMapping);
                        }

                        mappedType = mappedType.BaseType;
                        if (lastMapping != null
                            && viewMapping.ColumnMappings.Count == 0)
                        {
                            continue;
                        }

                        viewMappings = entityType[RelationalAnnotationNames.ViewMappings] as SortedSet<ViewMapping>;
                        if (viewMappings == null)
                        {
                            viewMappings = new SortedSet<ViewMapping>(TableMappingBaseComparer.EntityTypeInstance);
                            entityType.SetAnnotation(RelationalAnnotationNames.ViewMappings, viewMappings);
                        }

                        viewMappings.Add(viewMapping);
                        view.EntityTypeMappings.Add(viewMapping);

                        lastMapping = viewMapping;
                    }

                    // Re-add the mapping to update the order
                    viewMappings.Remove(lastMapping);
                    lastMapping.IsMainTableMapping = true;
                    viewMappings.Add(lastMapping);
                }
            }

            foreach (var table in databaseModel.Tables.Values)
            {
                PopulateRowInternalForeignKeys(table);
                PopulateConstraints(table);

                if (relationalAnnotationProvider != null)
                {
                    foreach (var column in table.Columns.Values)
                    {
                        column.AddAnnotations(relationalAnnotationProvider.For(column));
                    }

                    foreach (var constraint in table.UniqueConstraints.Values)
                    {
                        constraint.AddAnnotations(relationalAnnotationProvider.For(constraint));
                    }

                    foreach (var index in table.Indexes.Values)
                    {
                        index.AddAnnotations(relationalAnnotationProvider.For(index));
                    }

                    foreach (var constraint in table.ForeignKeyConstraints.Values)
                    {
                        constraint.AddAnnotations(relationalAnnotationProvider.For(constraint));
                    }

                    foreach (CheckConstraint checkConstraint in ((ITable)table).CheckConstraints)
                    {
                        checkConstraint.AddAnnotations(relationalAnnotationProvider.For(checkConstraint));
                    }

                    table.AddAnnotations(relationalAnnotationProvider.For(table));
                }
            }

            foreach (var view in databaseModel.Views.Values)
            {
                PopulateRowInternalForeignKeys(view);

                if (relationalAnnotationProvider != null)
                {
                    foreach (var viewColumn in view.Columns.Values)
                    {
                        viewColumn.AddAnnotations(relationalAnnotationProvider.For(viewColumn));
                    }

                    view.AddAnnotations(relationalAnnotationProvider.For(view));
                }
            }

            if (relationalAnnotationProvider != null)
            {
                foreach (Sequence sequence in ((IRelationalModel)databaseModel).Sequences)
                {
                    sequence.AddAnnotations(relationalAnnotationProvider.For(sequence));
                }

                databaseModel.AddAnnotations(relationalAnnotationProvider.For(databaseModel));
            }

            return model;
        }

        private static void PopulateConstraints(Table table)
        {
            foreach (var entityTypeMapping in ((ITable)table).EntityTypeMappings)
            {
                var entityType = (IConventionEntityType)entityTypeMapping.EntityType;
                foreach (var foreignKey in entityType.GetForeignKeys())
                {
                    foreach (var principalMapping in foreignKey.PrincipalEntityType.GetTableMappings())
                    {
                        if (!principalMapping.IncludesDerivedTypes
                            && foreignKey.PrincipalEntityType.GetDirectlyDerivedTypes().Any())
                        {
                            // Derived principal entity types are mapped to different tables, so the constraint is not enforceable
                            break;
                        }

                        var principalTable = (Table)principalMapping.Table;
                        var name = foreignKey.GetConstraintName(table.Name, table.Schema, principalTable.Name, principalTable.Schema);
                        if (name == null)
                        {
                            continue;
                        }

                        var foreignKeyConstraints = foreignKey[RelationalAnnotationNames.ForeignKeyMappings] as SortedSet<ForeignKeyConstraint>;
                        if (table.ForeignKeyConstraints.TryGetValue(name, out var constraint))
                        {
                            if (foreignKeyConstraints == null)
                            {
                                foreignKeyConstraints = new SortedSet<ForeignKeyConstraint>(ForeignKeyConstraintComparer.Instance);
                                foreignKey.SetOrRemoveAnnotation(RelationalAnnotationNames.ForeignKeyMappings, foreignKeyConstraints);
                            }

                            foreignKeyConstraints.Add(constraint);

                            constraint.MappedForeignKeys.Add(foreignKey);
                            break;
                        }

                        var principalColumns = new Column[foreignKey.Properties.Count];
                        for (var i = 0; i < principalColumns.Length; i++)
                        {
                            var property = foreignKey.PrincipalKey.Properties[i];
                            foreach (var columnMapping in property.GetTableColumnMappings())
                            {
                                if (columnMapping.TableMapping.Table == principalTable)
                                {
                                    principalColumns[i] = (Column)columnMapping.Column;
                                    break;
                                }
                            }

                            if (principalColumns[i] == null)
                            {
                                principalColumns = null;
                                break;
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

                        if (columns == null)
                        {
                            break;
                        }

                        if (columns.SequenceEqual(principalColumns))
                        {
                            // Principal and dependent properties are mapped to the same columns so the constraint is redundant
                            continue;
                        }

                        constraint = new ForeignKeyConstraint(name, table, principalTable, columns, principalColumns,
                            ToReferentialAction(foreignKey.DeleteBehavior));
                        constraint.MappedForeignKeys.Add(foreignKey);

                        if (foreignKeyConstraints == null)
                        {
                            foreignKeyConstraints = new SortedSet<ForeignKeyConstraint>(ForeignKeyConstraintComparer.Instance);
                            foreignKey.SetOrRemoveAnnotation(RelationalAnnotationNames.ForeignKeyMappings, foreignKeyConstraints);
                        }

                        foreignKeyConstraints.Add(constraint);
                        table.ForeignKeyConstraints.Add(name, constraint);
                        break;
                    }
                }

                foreach (var key in entityType.GetKeys())
                {
                    var name = key.GetName(table.Name, table.Schema);
                    var constraint = table.FindUniqueConstraint(name);
                    if (constraint == null)
                    {
                        var columns = new Column[key.Properties.Count];
                        for (var i = 0; i < columns.Length; i++)
                        {
                            var property = key.Properties[i];
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

                        if (columns == null)
                        {
                            continue;
                        }

                        constraint = new UniqueConstraint(name, table, columns);
                        if (key.IsPrimaryKey())
                        {
                            table.PrimaryKey = constraint;
                        }

                        table.UniqueConstraints.Add(name, constraint);
                    }

                    if (!(key[RelationalAnnotationNames.UniqueConstraintMappings] is SortedSet<UniqueConstraint> uniqueConstraints))
                    {
                        uniqueConstraints = new SortedSet<UniqueConstraint>(UniqueConstraintComparer.Instance);
                        key.SetOrRemoveAnnotation(RelationalAnnotationNames.UniqueConstraintMappings, uniqueConstraints);
                    }

                    uniqueConstraints.Add(constraint);
                    constraint.MappedKeys.Add(key);
                }

                foreach (var index in entityType.GetIndexes())
                {
                    var name = index.GetDatabaseName(table.Name, table.Schema);
                    if (!table.Indexes.TryGetValue(name, out var tableIndex))
                    {
                        var columns = new Column[index.Properties.Count];
                        for (var i = 0; i < columns.Length; i++)
                        {
                            var property = index.Properties[i];
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

                        if (columns == null)
                        {
                            continue;
                        }

                        tableIndex = new TableIndex(name, table, columns, index.GetFilter(table.Name, table.Schema), index.IsUnique);

                        table.Indexes.Add(name, tableIndex);
                    }

                    if (!(index[RelationalAnnotationNames.TableIndexMappings] is SortedSet<TableIndex> tableIndexes))
                    {
                        tableIndexes = new SortedSet<TableIndex>(TableIndexComparer.Instance);
                        index.SetOrRemoveAnnotation(RelationalAnnotationNames.TableIndexMappings, tableIndexes);
                    }

                    tableIndexes.Add(tableIndex);
                    tableIndex.MappedIndexes.Add(index);
                }
            }
        }

        private static void PopulateRowInternalForeignKeys(TableBase table)
        {
            SortedDictionary<IEntityType, IEnumerable<IForeignKey>> internalForeignKeyMap = null;
            SortedDictionary<IEntityType, IEnumerable<IForeignKey>> referencingInternalForeignKeyMap = null;
            TableMappingBase mainMapping = null;
            foreach (TableMappingBase entityTypeMapping in ((ITableBase)table).EntityTypeMappings)
            {
                var entityType = entityTypeMapping.EntityType;
                var primaryKey = entityType.FindPrimaryKey();
                if (primaryKey == null)
                {
                    if (mainMapping == null
                        || entityTypeMapping.EntityType.IsAssignableFrom(mainMapping.EntityType))
                    {
                        mainMapping = entityTypeMapping;
                    }

                    continue;
                }

                SortedSet<IForeignKey> rowInternalForeignKeys = null;
                foreach (var foreignKey in entityType.FindForeignKeys(primaryKey.Properties))
                {
                    if (foreignKey.IsUnique
                        && foreignKey.PrincipalKey.IsPrimaryKey()
                        && !foreignKey.DeclaringEntityType.IsAssignableFrom(foreignKey.PrincipalEntityType)
                        && !foreignKey.PrincipalEntityType.IsAssignableFrom(foreignKey.DeclaringEntityType)
                        && ((ITableBase)table).EntityTypeMappings.Any(m => m.EntityType == foreignKey.PrincipalEntityType))
                    {
                        if (rowInternalForeignKeys == null)
                        {
                            rowInternalForeignKeys = new SortedSet<IForeignKey>(ForeignKeyComparer.Instance);
                        }
                        rowInternalForeignKeys.Add(foreignKey);

                        if (referencingInternalForeignKeyMap == null)
                        {
                            referencingInternalForeignKeyMap =
                                new SortedDictionary<IEntityType, IEnumerable<IForeignKey>>(EntityTypeFullNameComparer.Instance);
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

                if (rowInternalForeignKeys != null)
                {
                    if (internalForeignKeyMap == null)
                    {
                        internalForeignKeyMap =
                            new SortedDictionary<IEntityType, IEnumerable<IForeignKey>>(EntityTypeFullNameComparer.Instance);
                        table.RowInternalForeignKeys = internalForeignKeyMap;
                    }

                    internalForeignKeyMap[entityType] = rowInternalForeignKeys;
                }

                if (rowInternalForeignKeys == null)
                {
                    if (((ITableBase)table).EntityTypeMappings.Any(m =>
                        !m.EntityType.IsAssignableFrom(entityType)
                        && !entityType.IsAssignableFrom(m.EntityType)))
                    {
                        table.IsShared = true;
                    }

                    if (mainMapping == null
                        || entityTypeMapping.EntityType.IsAssignableFrom(mainMapping.EntityType))
                    {
                        mainMapping = entityTypeMapping;
                    }
                }
            }

            // Re-add the mapping to update the order
            if (mainMapping is TableMapping mainTableMapping)
            {
                ((Table)mainMapping.Table).EntityTypeMappings.Remove(mainTableMapping);
                mainMapping.IsMainEntityTypeMapping = true;
                ((Table)mainMapping.Table).EntityTypeMappings.Add(mainTableMapping);
            }
            else
            {
                ((View)mainMapping.Table).EntityTypeMappings.Remove((ViewMapping)mainMapping);
                mainMapping.IsMainEntityTypeMapping = true;
                ((View)mainMapping.Table).EntityTypeMappings.Add((ViewMapping)mainMapping);
            }

            if (referencingInternalForeignKeyMap != null)
            {
                table.ReferencingRowInternalForeignKeys = referencingInternalForeignKeyMap;
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
                    return ReferentialAction.NoAction;
                case DeleteBehavior.Restrict:
                case DeleteBehavior.ClientSetNull:
                case DeleteBehavior.ClientCascade:
                    return ReferentialAction.Restrict;
                default:
                    throw new NotImplementedException(deleteBehavior.ToString());
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual DebugView DebugView
            => new DebugView(
                () => this.ToDebugString(MetadataDebugStringOptions.ShortDefault),
                () => this.ToDebugString(MetadataDebugStringOptions.LongDefault));

        IEnumerable<ITable> IRelationalModel.Tables
        {
            [DebuggerStepThrough]
            get => Tables.Values;
        }

        IEnumerable<IView> IRelationalModel.Views
        {
            [DebuggerStepThrough]
            get => Views.Values;
        }
    }
}
