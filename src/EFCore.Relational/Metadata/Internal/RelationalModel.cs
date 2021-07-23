﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Utilities;

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
        private bool _isReadOnly;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public RelationalModel(IModel model)
        {
            Model = model;
        }

        /// <inheritdoc />
        public virtual IModel Model { get; }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override bool IsReadOnly => _isReadOnly;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual SortedDictionary<string, TableBase> DefaultTables { get; }
            = new();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual SortedDictionary<(string, string?), Table> Tables { get; }
            = new();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual SortedDictionary<(string, string?), View> Views { get; }
            = new();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual SortedDictionary<string, SqlQuery> Queries { get; }
            = new();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual SortedDictionary<(string, string?, IReadOnlyList<string>), StoreFunction> Functions { get; }
            = new(NamedListComparer.Instance);

        /// <inheritdoc />
        public virtual ITable? FindTable(string name, string? schema)
            => Tables.TryGetValue((name, schema), out var table)
                ? table
                : null;

        /// <inheritdoc />
        public virtual IView? FindView(string name, string? schema)
            => Views.TryGetValue((name, schema), out var view)
                ? view
                : null;

        /// <inheritdoc />
        public virtual ISqlQuery? FindQuery(string name)
            => Queries.TryGetValue(name, out var query)
                ? query
                : null;

        /// <inheritdoc />
        public virtual IStoreFunction? FindFunction(string name, string? schema, IReadOnlyList<string> parameters)
            => Functions.TryGetValue((name, schema, parameters), out var function)
                ? function
                : null;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static IModel Add(
            IModel model,
            IRelationalAnnotationProvider? relationalAnnotationProvider,
            bool designTime)
        {
            model.AddRuntimeAnnotation(RelationalAnnotationNames.RelationalModel, Create(model, relationalAnnotationProvider, designTime));
            return model;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static IRelationalModel Create(
            IModel model,
            IRelationalAnnotationProvider? relationalAnnotationProvider,
            bool designTime)
        {
            var databaseModel = new RelationalModel(model);

            foreach (var entityType in model.GetEntityTypes())
            {
                AddDefaultMappings(databaseModel, entityType);

                AddTables(databaseModel, entityType);

                AddViews(databaseModel, entityType);

                AddSqlQueries(databaseModel, entityType);

                AddMappedFunctions(databaseModel, entityType);
            }

            AddTVFs(databaseModel);

            foreach (var table in databaseModel.Tables.Values)
            {
                PopulateRowInternalForeignKeys(table);
                PopulateConstraints(table);

                if (relationalAnnotationProvider != null)
                {
                    foreach (Column column in table.Columns.Values)
                    {
                        column.AddAnnotations(relationalAnnotationProvider.For(column, designTime));
                    }

                    foreach (var constraint in table.UniqueConstraints.Values)
                    {
                        constraint.AddAnnotations(relationalAnnotationProvider.For(constraint, designTime));
                    }

                    foreach (var index in table.Indexes.Values)
                    {
                        index.AddAnnotations(relationalAnnotationProvider.For(index, designTime));
                    }

                    foreach (var constraint in table.ForeignKeyConstraints.Values)
                    {
                        constraint.AddAnnotations(relationalAnnotationProvider.For(constraint, designTime));
                    }

                    if (designTime)
                    {
                        foreach (var checkConstraint in ((ITable)table).CheckConstraints)
                        {
                            ((AnnotatableBase)checkConstraint).AddAnnotations(relationalAnnotationProvider.For(checkConstraint, designTime));
                        }
                    }

                    table.AddAnnotations(relationalAnnotationProvider.For(table, designTime));
                }
            }

            foreach (var view in databaseModel.Views.Values)
            {
                PopulateRowInternalForeignKeys(view);

                if (relationalAnnotationProvider != null)
                {
                    foreach (ViewColumn viewColumn in view.Columns.Values)
                    {
                        viewColumn.AddAnnotations(relationalAnnotationProvider.For(viewColumn, designTime));
                    }

                    view.AddAnnotations(relationalAnnotationProvider.For(view, designTime));
                }
            }

            foreach (var query in databaseModel.Queries.Values)
            {
                if (relationalAnnotationProvider != null)
                {
                    foreach (SqlQueryColumn queryColumn in query.Columns.Values)
                    {
                        queryColumn.AddAnnotations(relationalAnnotationProvider.For(queryColumn, designTime));
                    }

                    query.AddAnnotations(relationalAnnotationProvider.For(query, designTime));
                }
            }

            foreach (var function in databaseModel.Functions.Values)
            {
                if (relationalAnnotationProvider != null)
                {
                    foreach (FunctionColumn functionColumn in function.Columns.Values)
                    {
                        functionColumn.AddAnnotations(relationalAnnotationProvider.For(functionColumn, designTime));
                    }

                    function.AddAnnotations(relationalAnnotationProvider.For(function, designTime));
                }
            }

            if (relationalAnnotationProvider != null)
            {
                foreach (var sequence in ((IRelationalModel)databaseModel).Sequences)
                {
                    ((AnnotatableBase)sequence).AddAnnotations(relationalAnnotationProvider.For(sequence, designTime));
                }

                databaseModel.AddAnnotations(relationalAnnotationProvider.For(databaseModel, designTime));
            }

            databaseModel._isReadOnly = true;
            return databaseModel;
        }

        private static void AddDefaultMappings(RelationalModel databaseModel, IEntityType entityType)
        {
            var rootType = entityType.GetRootType();
            var name = rootType.HasSharedClrType ? rootType.Name : rootType.ShortName();
            if (!databaseModel.DefaultTables.TryGetValue(name, out var defaultTable))
            {
                defaultTable = new TableBase(name, null, databaseModel);
                databaseModel.DefaultTables.Add(name, defaultTable);
            }

            var tableMapping = new TableMappingBase(entityType, defaultTable, includesDerivedTypes: true)
            {
                IsSharedTablePrincipal = true,
                IsSplitEntityTypePrincipal = true
            };

            foreach (var property in entityType.GetProperties())
            {
                var columnName = property.GetColumnBaseName();
                if (columnName == null)
                {
                    continue;
                }

                var column = (ColumnBase?)defaultTable.FindColumn(columnName);
                if (column == null)
                {
                    column = new(columnName, property.GetColumnType(), defaultTable);
                    column.IsNullable = property.IsColumnNullable();
                    defaultTable.Columns.Add(columnName, column);
                }
                else if (!property.IsColumnNullable())
                {
                    column.IsNullable = false;
                }

                var columnMapping = new ColumnMappingBase(property, column, tableMapping);
                tableMapping.ColumnMappings.Add(columnMapping);
                column.PropertyMappings.Add(columnMapping);

                if (property.FindRuntimeAnnotationValue(RelationalAnnotationNames.DefaultColumnMappings)
                    is not SortedSet<ColumnMappingBase> columnMappings)
                {
                    columnMappings = new SortedSet<ColumnMappingBase>(ColumnMappingBaseComparer.Instance);
                    property.AddRuntimeAnnotation(RelationalAnnotationNames.DefaultColumnMappings, columnMappings);
                }

                columnMappings.Add(columnMapping);
            }

            if (entityType.FindRuntimeAnnotationValue(RelationalAnnotationNames.DefaultMappings)
                is not List<TableMappingBase> tableMappings)
            {
                tableMappings = new List<TableMappingBase>();
                entityType.AddRuntimeAnnotation(RelationalAnnotationNames.DefaultMappings, tableMappings);
            }

            if (tableMapping.ColumnMappings.Count != 0
                || tableMappings.Count == 0)
            {
                tableMappings.Add(tableMapping);
                defaultTable.EntityTypeMappings.Add(tableMapping);
            }

            tableMappings.Reverse();
        }

        private static void AddTables(RelationalModel databaseModel, IEntityType entityType)
        {
            var tableName = entityType.GetTableName();
            if (tableName != null)
            {
                var schema = entityType.GetSchema();
                var mappedType = entityType;
                List<TableMapping>? tableMappings = null;
                while (mappedType != null)
                {
                    var mappedTableName = mappedType.GetTableName();
                    var mappedSchema = mappedType.GetSchema();

                    if (mappedTableName == null
                        || (mappedTableName == tableName
                            && mappedSchema == schema
                            && mappedType != entityType))
                    {
                        break;
                    }

                    var mappedTable = StoreObjectIdentifier.Table(mappedTableName, mappedSchema);
                    if (!databaseModel.Tables.TryGetValue((mappedTableName, mappedSchema), out var table))
                    {
                        table = new Table(mappedTableName, mappedSchema, databaseModel);
                        databaseModel.Tables.Add((mappedTableName, mappedSchema), table);
                    }

                    var tableMapping = new TableMapping(entityType, table, includesDerivedTypes: mappedType == entityType)
                    {
                        IsSplitEntityTypePrincipal = true
                    };
                    foreach (var property in mappedType.GetProperties())
                    {
                        var columnName = property.GetColumnName(mappedTable);
                        if (columnName == null)
                        {
                            continue;
                        }

                        var column = (Column?)table.FindColumn(columnName);
                        if (column == null)
                        {
                            column = new(columnName, property.GetColumnType(mappedTable), table);
                            column.IsNullable = property.IsColumnNullable(mappedTable);
                            table.Columns.Add(columnName, column);
                        }
                        else if (!property.IsColumnNullable(mappedTable))
                        {
                            column.IsNullable = false;
                        }

                        var columnMapping = new ColumnMapping(property, column, tableMapping);
                        tableMapping.ColumnMappings.Add(columnMapping);
                        column.PropertyMappings.Add(columnMapping);

                        if (property.FindRuntimeAnnotationValue(RelationalAnnotationNames.TableColumnMappings)
                            is not SortedSet<ColumnMapping> columnMappings)
                        {
                            columnMappings = new SortedSet<ColumnMapping>(ColumnMappingBaseComparer.Instance);
                            property.AddRuntimeAnnotation(RelationalAnnotationNames.TableColumnMappings, columnMappings);
                        }

                        columnMappings.Add(columnMapping);
                    }

                    mappedType = mappedType.BaseType;

                    tableMappings = entityType.FindRuntimeAnnotationValue(RelationalAnnotationNames.TableMappings)
                        as List<TableMapping>;
                    if (tableMappings == null)
                    {
                        tableMappings = new List<TableMapping>();
                        entityType.AddRuntimeAnnotation(RelationalAnnotationNames.TableMappings, tableMappings);
                    }

                    if (tableMapping.ColumnMappings.Count != 0
                        || tableMappings.Count == 0)
                    {
                        tableMappings.Add(tableMapping);
                        table.EntityTypeMappings.Add(tableMapping);
                    }
                }

                tableMappings?.Reverse();
            }
        }

        private static void AddViews(RelationalModel databaseModel, IEntityType entityType)
        {
            var viewName = entityType.GetViewName();
            if (viewName == null)
            {
                return;
            }

            var schema = entityType.GetViewSchema();
            List<ViewMapping>? viewMappings = null;
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

                var mappedView = StoreObjectIdentifier.View(mappedViewName, mappedSchema);
                var viewMapping = new ViewMapping(entityType, view, includesDerivedTypes: mappedType == entityType)
                {
                    IsSplitEntityTypePrincipal = true
                };
                foreach (var property in mappedType.GetProperties())
                {
                    var columnName = property.GetColumnName(mappedView);
                    if (columnName == null)
                    {
                        continue;
                    }

                    var column = (ViewColumn?)view.FindColumn(columnName);
                    if (column == null)
                    {
                        column = new(columnName, property.GetColumnType(mappedView), view);
                        column.IsNullable = property.IsColumnNullable(mappedView);
                        view.Columns.Add(columnName, column);
                    }
                    else if (!property.IsColumnNullable(mappedView))
                    {
                        column.IsNullable = false;
                    }

                    var columnMapping = new ViewColumnMapping(property, column, viewMapping);
                    viewMapping.ColumnMappings.Add(columnMapping);
                    column.PropertyMappings.Add(columnMapping);

                    if (property.FindRuntimeAnnotationValue(RelationalAnnotationNames.ViewColumnMappings)
                        is not SortedSet<ViewColumnMapping> columnMappings)
                    {
                        columnMappings = new SortedSet<ViewColumnMapping>(ColumnMappingBaseComparer.Instance);
                        property.AddRuntimeAnnotation(RelationalAnnotationNames.ViewColumnMappings, columnMappings);
                    }

                    columnMappings.Add(columnMapping);
                }

                mappedType = mappedType.BaseType;

                viewMappings = entityType.FindRuntimeAnnotationValue(RelationalAnnotationNames.ViewMappings) as List<ViewMapping>;
                if (viewMappings == null)
                {
                    viewMappings = new List<ViewMapping>();
                    entityType.AddRuntimeAnnotation(RelationalAnnotationNames.ViewMappings, viewMappings);
                }

                if (viewMapping.ColumnMappings.Count != 0
                    || viewMappings.Count == 0)
                {
                    viewMappings.Add(viewMapping);
                    view.EntityTypeMappings.Add(viewMapping);
                }
            }

            viewMappings?.Reverse();
        }

        private static void AddSqlQueries(RelationalModel databaseModel, IEntityType entityType)
        {
            var entityTypeSqlQuery = entityType.GetSqlQuery();
            if (entityTypeSqlQuery == null)
            {
                return;
            }

            List<SqlQueryMapping>? queryMappings = null;
            var definingType = entityType;
            while (definingType != null)
            {
                var definingTypeSqlQuery = definingType.GetSqlQuery();
                if (definingTypeSqlQuery == null
                    || definingType.BaseType == null
                    || (definingTypeSqlQuery == entityTypeSqlQuery
                        && definingType != entityType))
                {
                    break;
                }

                definingType = definingType.BaseType;
            }

            Check.DebugAssert(definingType is not null, $"Could not find defining type for {entityType}");

            var mappedType = entityType;
            while (mappedType != null)
            {
                var mappedTypeSqlQuery = mappedType.GetSqlQuery();
                if (mappedTypeSqlQuery == null
                    || (mappedTypeSqlQuery == entityTypeSqlQuery
                        && mappedType != entityType))
                {
                    break;
                }

                var mappedQuery = StoreObjectIdentifier.SqlQuery(definingType);
                if (!databaseModel.Queries.TryGetValue(mappedQuery.Name, out var sqlQuery))
                {
                    sqlQuery = new SqlQuery(mappedQuery.Name, databaseModel, mappedTypeSqlQuery);
                    databaseModel.Queries.Add(mappedQuery.Name, sqlQuery);
                }

                var queryMapping = new SqlQueryMapping(entityType, sqlQuery, includesDerivedTypes: true)
                {
                    IsDefaultSqlQueryMapping = true,
                    IsSharedTablePrincipal = true,
                    IsSplitEntityTypePrincipal = true
                };

                foreach (var property in mappedType.GetProperties())
                {
                    var columnName = property.GetColumnName(mappedQuery);
                    if (columnName == null)
                    {
                        continue;
                    }

                    var column = (SqlQueryColumn?)sqlQuery.FindColumn(columnName);
                    if (column == null)
                    {
                        column = new(columnName, property.GetColumnType(mappedQuery), sqlQuery);
                        column.IsNullable = property.IsColumnNullable(mappedQuery);
                        sqlQuery.Columns.Add(columnName, column);
                    }
                    else if (!property.IsColumnNullable(mappedQuery))
                    {
                        column.IsNullable = false;
                    }

                    var columnMapping = new SqlQueryColumnMapping(property, column, queryMapping);
                    queryMapping.ColumnMappings.Add(columnMapping);
                    column.PropertyMappings.Add(columnMapping);

                    if (property.FindRuntimeAnnotationValue(RelationalAnnotationNames.SqlQueryColumnMappings)
                        is not SortedSet<SqlQueryColumnMapping> columnMappings)
                    {
                        columnMappings = new SortedSet<SqlQueryColumnMapping>(ColumnMappingBaseComparer.Instance);
                        property.AddRuntimeAnnotation(RelationalAnnotationNames.SqlQueryColumnMappings, columnMappings);
                    }

                    columnMappings.Add(columnMapping);
                }

                mappedType = mappedType.BaseType;

                queryMappings = entityType.FindRuntimeAnnotationValue(RelationalAnnotationNames.SqlQueryMappings) as List<SqlQueryMapping>;
                if (queryMappings == null)
                {
                    queryMappings = new List<SqlQueryMapping>();
                    entityType.AddRuntimeAnnotation(RelationalAnnotationNames.SqlQueryMappings, queryMappings);
                }

                if (queryMapping.ColumnMappings.Count != 0
                    || queryMappings.Count == 0)
                {
                    queryMappings.Add(queryMapping);
                    sqlQuery.EntityTypeMappings.Add(queryMapping);
                }
            }

            queryMappings?.Reverse();
        }

        private static void AddMappedFunctions(RelationalModel databaseModel, IEntityType entityType)
        {
            var model = databaseModel.Model;
            var functionName = entityType.GetFunctionName();
            if (functionName == null)
            {
                return;
            }

            List<FunctionMapping>? functionMappings = null;
            var mappedType = entityType;
            while (mappedType != null)
            {
                var mappedFunctionName = mappedType.GetFunctionName();
                if (mappedFunctionName == null
                    || (mappedFunctionName == functionName
                        && mappedType != entityType))
                {
                    break;
                }

                var dbFunction = (IRuntimeDbFunction)model.FindDbFunction(mappedFunctionName)!;
                var functionMapping = CreateFunctionMapping(entityType, mappedType, dbFunction, databaseModel, @default: true);

                mappedType = mappedType.BaseType;

                functionMappings = entityType.FindRuntimeAnnotationValue(RelationalAnnotationNames.FunctionMappings) as List<FunctionMapping>;
                if (functionMappings == null)
                {
                    functionMappings = new List<FunctionMapping>();
                    entityType.AddRuntimeAnnotation(RelationalAnnotationNames.FunctionMappings, functionMappings);
                }

                if (functionMapping.ColumnMappings.Count != 0
                    || functionMappings.Count == 0)
                {
                    functionMappings.Add(functionMapping);
                    ((StoreFunction)functionMapping.StoreFunction).EntityTypeMappings.Add(functionMapping);
                }
            }

            functionMappings?.Reverse();
        }

        private static void AddTVFs(RelationalModel relationalModel)
        {
            var model = relationalModel.Model;
            foreach (IRuntimeDbFunction function in model.GetDbFunctions())
            {
                var entityType = function.IsScalar
                    ? null
                    : model.FindEntityType(function.ReturnType.GetGenericArguments()[0]);
                if (entityType == null)
                {
                    GetOrCreateStoreFunction(function, relationalModel);
                    continue;
                }

                if (entityType.GetFunctionName() == function.ModelName)
                {
                    continue;
                }

                var functionMapping = CreateFunctionMapping(entityType, entityType, function, relationalModel, @default: false);

                if (entityType.FindRuntimeAnnotationValue(RelationalAnnotationNames.FunctionMappings)
                    is not List<FunctionMapping> functionMappings)
                {
                    functionMappings = new List<FunctionMapping>();
                    entityType.AddRuntimeAnnotation(RelationalAnnotationNames.FunctionMappings, functionMappings);
                }

                functionMappings.Add(functionMapping);
                ((StoreFunction)functionMapping.StoreFunction).EntityTypeMappings.Add(functionMapping);
            }
        }

        private static FunctionMapping CreateFunctionMapping(
            IEntityType entityType,
            IEntityType mappedType,
            IRuntimeDbFunction dbFunction,
            RelationalModel model,
            bool @default)
        {
            var storeFunction = GetOrCreateStoreFunction(dbFunction, model);

            var mappedFunction = StoreObjectIdentifier.DbFunction(dbFunction.Name);
            var functionMapping = new FunctionMapping(entityType, storeFunction, dbFunction, includesDerivedTypes: true)
            {
                IsDefaultFunctionMapping = @default,
                // See Issue #19970
                IsSharedTablePrincipal = true,
                IsSplitEntityTypePrincipal = true
            };

            foreach (var property in mappedType.GetProperties())
            {
                var columnName = property.GetColumnName(mappedFunction);
                if (columnName == null)
                {
                    continue;
                }

                var column = (FunctionColumn?)storeFunction.FindColumn(columnName);
                if (column == null)
                {
                    column = new(columnName, property.GetColumnType(mappedFunction), storeFunction);
                    column.IsNullable = property.IsColumnNullable(mappedFunction);
                    storeFunction.Columns.Add(columnName, column);
                }
                else if (!property.IsColumnNullable(mappedFunction))
                {
                    column.IsNullable = false;
                }

                var columnMapping = new FunctionColumnMapping(property, column, functionMapping);
                functionMapping.ColumnMappings.Add(columnMapping);
                column.PropertyMappings.Add(columnMapping);

                if (property.FindRuntimeAnnotationValue(RelationalAnnotationNames.FunctionColumnMappings)
                    is not SortedSet<FunctionColumnMapping> columnMappings)
                {
                    columnMappings = new SortedSet<FunctionColumnMapping>(ColumnMappingBaseComparer.Instance);
                    property.AddRuntimeAnnotation(RelationalAnnotationNames.FunctionColumnMappings, columnMappings);
                }

                columnMappings.Add(columnMapping);
            }

            return functionMapping;
        }

        private static StoreFunction GetOrCreateStoreFunction(IRuntimeDbFunction dbFunction, RelationalModel model)
        {
            var storeFunction = (StoreFunction?)dbFunction.StoreFunction;
            if (storeFunction == null)
            {
                var parameterTypes = dbFunction.Parameters.Select(p => p.StoreType!).ToArray();
                storeFunction = (StoreFunction?)model.FindFunction(dbFunction.Name, dbFunction.Schema, parameterTypes);
                if (storeFunction == null)
                {
                    storeFunction = new StoreFunction(dbFunction, model);
                    model.Functions.Add((storeFunction.Name, storeFunction.Schema, parameterTypes), storeFunction);
                }
                else
                {
                    dbFunction.StoreFunction = storeFunction;
                    for (var i = 0; i < dbFunction.Parameters.Count; i++)
                    {
                        storeFunction.Parameters[i].DbFunctionParameters.Add(dbFunction.Parameters[i]);
                    }

                    storeFunction.DbFunctions.Add(dbFunction.ModelName, dbFunction);
                }
            }

            return storeFunction;
        }

        private static void PopulateConstraints(Table table)
        {
            var storeObject = StoreObjectIdentifier.Table(table.Name, table.Schema);
            foreach (var entityTypeMapping in ((ITable)table).EntityTypeMappings)
            {
                if (!entityTypeMapping.IncludesDerivedTypes
                    && entityTypeMapping.EntityType.GetTableMappings().Any(m => m.IncludesDerivedTypes))
                {
                    continue;
                }

                var entityType = (IEntityType)entityTypeMapping.EntityType;
                foreach (var foreignKey in entityType.GetForeignKeys())
                {
                    var firstPrincipalMapping = true;
                    foreach (var principalMapping in foreignKey.PrincipalEntityType.GetTableMappings().Reverse())
                    {
                        if (firstPrincipalMapping
                            && !principalMapping.IncludesDerivedTypes
                            && foreignKey.PrincipalEntityType.GetDirectlyDerivedTypes().Any())
                        {
                            // Derived principal entity types are mapped to different tables, so the constraint is not enforceable
                            break;
                        }

                        firstPrincipalMapping = false;

                        var principalTable = (Table)principalMapping.Table;
                        var name = foreignKey.GetConstraintName(
                            storeObject,
                            StoreObjectIdentifier.Table(principalTable.Name, principalTable.Schema));
                        if (name == null)
                        {
                            continue;
                        }

                        var foreignKeyConstraints = foreignKey.FindRuntimeAnnotationValue(RelationalAnnotationNames.ForeignKeyMappings)
                            as SortedSet<ForeignKeyConstraint>;
                        if (table.ForeignKeyConstraints.TryGetValue(name, out var constraint))
                        {
                            if (foreignKeyConstraints == null)
                            {
                                foreignKeyConstraints = new SortedSet<ForeignKeyConstraint>(ForeignKeyConstraintComparer.Instance);
                                foreignKey.AddRuntimeAnnotation(RelationalAnnotationNames.ForeignKeyMappings, foreignKeyConstraints);
                            }

                            foreignKeyConstraints.Add(constraint);

                            constraint.MappedForeignKeys.Add(foreignKey);
                            break;
                        }

                        var principalColumns = new Column[foreignKey.Properties.Count];
                        for (var i = 0; i < principalColumns.Length; i++)
                        {
                            if (principalTable.FindColumn(foreignKey.PrincipalKey.Properties[i]) is Column principalColumn)
                            {
                                principalColumns[i] = principalColumn;
                            }
                            else
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
                            if (table.FindColumn(foreignKey.Properties[i]) is Column foreignKeyColumn)
                            {
                                columns[i] = foreignKeyColumn;
                            }
                            else
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
                            break;
                        }

                        if (entityTypeMapping.IncludesDerivedTypes
                            && foreignKey.DeclaringEntityType != entityType
                            && entityType.FindPrimaryKey() is IKey primaryKey
                            && foreignKey.Properties.SequenceEqual(primaryKey.Properties))
                        {
                            // The identifying FK constraint is needed to be created only on the table that corresponds
                            // to the declaring entity type
                            break;
                        }

                        constraint = new ForeignKeyConstraint(
                            name, table, principalTable, columns, principalColumns, ToReferentialAction(foreignKey.DeleteBehavior));
                        constraint.MappedForeignKeys.Add(foreignKey);

                        if (foreignKeyConstraints == null)
                        {
                            foreignKeyConstraints = new SortedSet<ForeignKeyConstraint>(ForeignKeyConstraintComparer.Instance);
                            foreignKey.AddRuntimeAnnotation(RelationalAnnotationNames.ForeignKeyMappings, foreignKeyConstraints);
                        }

                        foreignKeyConstraints.Add(constraint);
                        table.ForeignKeyConstraints.Add(name, constraint);
                        break;
                    }
                }

                foreach (var key in entityType.GetKeys())
                {
                    var name = key.GetName(storeObject);
                    if (name == null)
                    {
                        continue;
                    }

                    var constraint = table.FindUniqueConstraint(name);
                    if (constraint == null)
                    {
                        var columns = new Column[key.Properties.Count];
                        for (var i = 0; i < columns.Length; i++)
                        {
                            if (table.FindColumn(key.Properties[i]) is Column uniqueConstraintColumn)
                            {
                                columns[i] = uniqueConstraintColumn;
                            }
                            else
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

                    if (key.FindRuntimeAnnotationValue(RelationalAnnotationNames.UniqueConstraintMappings)
                        is not SortedSet<UniqueConstraint> uniqueConstraints)
                    {
                        uniqueConstraints = new SortedSet<UniqueConstraint>(UniqueConstraintComparer.Instance);
                        key.AddRuntimeAnnotation(RelationalAnnotationNames.UniqueConstraintMappings, uniqueConstraints);
                    }

                    uniqueConstraints.Add(constraint);
                    constraint.MappedKeys.Add(key);
                }

                foreach (var index in entityType.GetIndexes())
                {
                    var name = index.GetDatabaseName(storeObject);
                    if (name == null)
                    {
                        continue;
                    }

                    if (!table.Indexes.TryGetValue(name, out var tableIndex))
                    {
                        var columns = new Column[index.Properties.Count];
                        for (var i = 0; i < columns.Length; i++)
                        {
                            if (table.FindColumn(index.Properties[i]) is Column indexColumn)
                            {
                                columns[i] = indexColumn;
                            }
                            else
                            {
                                columns = null;
                                break;
                            }
                        }

                        if (columns == null)
                        {
                            continue;
                        }

                        tableIndex = new TableIndex(name, table, columns, index.IsUnique);

                        table.Indexes.Add(name, tableIndex);
                    }

                    if (index.FindRuntimeAnnotationValue(RelationalAnnotationNames.TableIndexMappings)
                        is not SortedSet<TableIndex> tableIndexes)
                    {
                        tableIndexes = new SortedSet<TableIndex>(TableIndexComparer.Instance);
                        index.AddRuntimeAnnotation(RelationalAnnotationNames.TableIndexMappings, tableIndexes);
                    }

                    tableIndexes.Add(tableIndex);
                    tableIndex.MappedIndexes.Add(index);
                }
            }
        }

        private static void PopulateRowInternalForeignKeys(TableBase table)
        {
            SortedDictionary<IEntityType, IEnumerable<IForeignKey>>? internalForeignKeyMap = null;
            SortedDictionary<IEntityType, IEnumerable<IForeignKey>>? referencingInternalForeignKeyMap = null;
            TableMappingBase? mainMapping = null;
            var mappedEntityTypes = new HashSet<IEntityType>();
            foreach (TableMappingBase entityTypeMapping in ((ITableBase)table).EntityTypeMappings)
            {
                var entityType = entityTypeMapping.EntityType;
                mappedEntityTypes.Add(entityType);
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

                SortedSet<IForeignKey>? rowInternalForeignKeys = null;
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
                    table.IsShared = true;
                }

                if (rowInternalForeignKeys == null)
                {
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
                mainMapping.IsSharedTablePrincipal = true;
                ((Table)mainMapping.Table).EntityTypeMappings.Add(mainTableMapping);
            }
            else if (mainMapping is ViewMapping mainViewMapping)
            {
                ((View)mainMapping.Table).EntityTypeMappings.Remove(mainViewMapping);
                mainMapping.IsSharedTablePrincipal = true;
                ((View)mainMapping.Table).EntityTypeMappings.Add(mainViewMapping);
            }

            Check.DebugAssert(mainMapping is not null,
                $"{nameof(mainMapping)} is neither a {nameof(TableMapping)} nor a {nameof(ViewMapping)}");

            if (referencingInternalForeignKeyMap != null)
            {
                table.ReferencingRowInternalForeignKeys = referencingInternalForeignKeyMap;

                var optionalTypes = new Dictionary<IEntityType, bool>();
                var entityTypesToVisit = new Queue<(IEntityType, bool)>();
                entityTypesToVisit.Enqueue((mainMapping.EntityType, false));

                while (entityTypesToVisit.Count > 0)
                {
                    var (entityType, optional) = entityTypesToVisit.Dequeue();
                    if (optionalTypes.TryGetValue(entityType, out var previouslyOptional)
                        && (!previouslyOptional || optional))
                    {
                        continue;
                    }

                    optionalTypes[entityType] = optional;

                    if (referencingInternalForeignKeyMap.TryGetValue(entityType, out var referencingInternalForeignKeys))
                    {
                        foreach (var referencingForeignKey in referencingInternalForeignKeys)
                        {
                            entityTypesToVisit.Enqueue(
                                (referencingForeignKey.DeclaringEntityType, optional || !referencingForeignKey.IsRequiredDependent));
                        }
                    }

                    if (table.EntityTypeMappings.Single(etm => etm.EntityType == entityType).IncludesDerivedTypes)
                    {
                        foreach (var directlyDerivedEntityType in entityType.GetDerivedTypes())
                        {
                            if (mappedEntityTypes.Contains(directlyDerivedEntityType)
                                && !optionalTypes.ContainsKey(directlyDerivedEntityType))
                            {
                                entityTypesToVisit.Enqueue((directlyDerivedEntityType, optional));
                            }
                        }
                    }
                }

                table.OptionalEntityTypes = optionalTypes;
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static ReferentialAction ToReferentialAction(DeleteBehavior deleteBehavior)
            => deleteBehavior switch
            {
                DeleteBehavior.SetNull => ReferentialAction.SetNull,
                DeleteBehavior.Cascade => ReferentialAction.Cascade,
                DeleteBehavior.NoAction or DeleteBehavior.ClientNoAction => ReferentialAction.NoAction,
                DeleteBehavior.Restrict or DeleteBehavior.ClientSetNull or DeleteBehavior.ClientCascade => ReferentialAction.Restrict,
                _ => throw new NotSupportedException(deleteBehavior.ToString()),
            };

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual DebugView DebugView
            => new(
                () => ((IRelationalModel)this).ToDebugString(MetadataDebugStringOptions.ShortDefault),
                () => ((IRelationalModel)this).ToDebugString(MetadataDebugStringOptions.LongDefault));

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

        IEnumerable<IStoreFunction> IRelationalModel.Functions
        {
            [DebuggerStepThrough]
            get => Functions.Values;
        }

        IEnumerable<ISqlQuery> IRelationalModel.Queries
        {
            [DebuggerStepThrough]
            get => Queries.Values;
        }
    }
}
