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
                var viewName = entityType.GetViewName();
                if (tableName != null)
                {
                    var schema = entityType.GetSchema();
                    if (!databaseModel.Tables.TryGetValue((tableName, schema), out var table))
                    {
                        table = new Table(tableName, schema, databaseModel);
                        databaseModel.Tables.Add((tableName, schema), table);
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
                            columnMappings = new SortedSet<ColumnMapping>(ColumnMappingBaseComparer.Instance);
                            property.SetAnnotation(RelationalAnnotationNames.TableColumnMappings, columnMappings);
                        }

                        columnMappings.Add(columnMapping);
                    }

                    var tableMappings = entityType[RelationalAnnotationNames.TableMappings] as SortedSet<TableMapping>;
                    if (tableMappings == null)
                    {
                        tableMappings = new SortedSet<TableMapping>(TableMappingBaseComparer.Instance);
                        entityType.SetAnnotation(RelationalAnnotationNames.TableMappings, tableMappings);
                    }

                    tableMappings.Add(tableMapping);
                    table.EntityTypeMappings.Add(tableMapping);
                }

                if (viewName != null)
                {
                    var schema = entityType.GetViewSchema();
                    if (!databaseModel.Views.TryGetValue((viewName, schema), out var view))
                    {
                        view = new View(viewName, schema, databaseModel);
                        databaseModel.Views.Add((viewName, schema), view);
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
                            columnMappings = new SortedSet<ViewColumnMapping>(ColumnMappingBaseComparer.Instance);
                            property.SetAnnotation(RelationalAnnotationNames.ViewColumnMappings, columnMappings);
                        }

                        columnMappings.Add(columnMapping);
                    }

                    var tableMappings = entityType[RelationalAnnotationNames.ViewMappings] as SortedSet<ViewMapping>;
                    if (tableMappings == null)
                    {
                        tableMappings = new SortedSet<ViewMapping>(TableMappingBaseComparer.Instance);
                        entityType.SetAnnotation(RelationalAnnotationNames.ViewMappings, tableMappings);
                    }

                    tableMappings.Add(viewMapping);
                    view.EntityTypeMappings.Add(viewMapping);
                }
            }

            foreach (var table in databaseModel.Tables.Values)
            {
                if (relationalAnnotationProvider != null)
                {
                    foreach (var column in table.Columns.Values)
                    {
                        column.AddAnnotations(relationalAnnotationProvider.For(column));
                    }
                }

                PopulateInternalForeignKeys(table);
                PopulateConstraints(table, relationalAnnotationProvider);

                if (relationalAnnotationProvider != null)
                {
                    table.AddAnnotations(relationalAnnotationProvider.For(table));
                }
            }

            foreach (var view in databaseModel.Views.Values)
            {
                if (relationalAnnotationProvider != null)
                {
                    foreach (var viewColumn in view.Columns.Values)
                    {
                        viewColumn.AddAnnotations(relationalAnnotationProvider.For(viewColumn));
                    }
                }

                PopulateInternalForeignKeys(view);

                if (relationalAnnotationProvider != null)
                {
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

        private static void PopulateConstraints(Table table, IRelationalAnnotationProvider relationalAnnotationProvider)
        {
            foreach (var entityTypeMapping in ((ITable)table).EntityTypeMappings)
            {
                var entityType = (IConventionEntityType)entityTypeMapping.EntityType;
                foreach (var foreignKey in entityType.GetForeignKeys())
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

                        constraint.MappedForeignKeys.Add(foreignKey);
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
                    constraint.MappedForeignKeys.Add(foreignKey);

                    if (foreignKeyConstraints == null)
                    {
                        foreignKeyConstraints = new SortedSet<ForeignKeyConstraint>(ForeignKeyConstraintComparer.Instance);
                        foreignKey.SetOrRemoveAnnotation(RelationalAnnotationNames.ForeignKeyMappings, foreignKeyConstraints);
                    }

                    foreignKeyConstraints.Add(constraint);
                    table.ForeignKeyConstraints.Add(name, constraint);
                }

                foreach (var key in entityType.GetKeys())
                {
                    var name = key.GetName();
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

                        constraint = new UniqueConstraint(name, table, columns, key.IsPrimaryKey());
                        if (constraint.IsPrimaryKey)
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
                    var name = index.GetName();
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

                        tableIndex = new TableIndex(name, table, columns, index.GetFilter(), index.IsUnique);

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

            if (relationalAnnotationProvider != null)
            {
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

                if (internalForeignKeys != null)
                {
                    if (internalForeignKeyMap == null)
                    {
                        internalForeignKeyMap =
                            new SortedDictionary<IEntityType, IEnumerable<IForeignKey>>(EntityTypeFullNameComparer.Instance);
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
