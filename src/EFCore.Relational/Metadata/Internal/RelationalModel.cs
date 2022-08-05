// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Internal;

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
    public override bool IsReadOnly
        => _isReadOnly;

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

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual SortedDictionary<(string, string?), StoreStoredProcedure> StoredProcedures { get; }
        = new();

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

    /// <inheritdoc />
    public virtual IStoreStoredProcedure? FindStoredProcedure(string name, string? schema)
        => StoredProcedures.TryGetValue((name, schema), out var storedProcedure)
            ? storedProcedure
            : null;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static IModel Add(
        IModel model,
        IRelationalAnnotationProvider relationalAnnotationProvider,
        IRelationalTypeMappingSource relationalTypeMappingSource,
        bool designTime)
    {
        model.AddRuntimeAnnotation(
            RelationalAnnotationNames.RelationalModel,
            Create(
                model,
                relationalAnnotationProvider, 
                relationalTypeMappingSource,
                designTime));
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
        IRelationalAnnotationProvider relationalAnnotationProvider,
        IRelationalTypeMappingSource relationalTypeMappingSource,
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

            AddStoredProcedures(databaseModel, entityType, relationalTypeMappingSource);
        }

        AddTvfs(databaseModel);

        foreach (var table in databaseModel.Tables.Values)
        {
            PopulateRowInternalForeignKeys<ColumnMapping>(table);
            PopulateTableConfiguration(table, designTime);

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

                if (designTime)
                {
                    foreach (var checkConstraint in table.CheckConstraints.Values)
                    {
                        checkConstraint.AddAnnotations(relationalAnnotationProvider.For(checkConstraint, designTime));
                    }
                }

                foreach (var trigger in table.Triggers.Values)
                {
                    ((AnnotatableBase)trigger).AddAnnotations(relationalAnnotationProvider.For(trigger, designTime));
                }
            }
        }

        foreach (var table in databaseModel.Tables.Values)
        {
            PopulateForeignKeyConstraints(table);

            if (relationalAnnotationProvider != null)
            {
                foreach (var constraint in table.ForeignKeyConstraints)
                {
                    constraint.AddAnnotations(relationalAnnotationProvider.For(constraint, designTime));
                }

                table.AddAnnotations(relationalAnnotationProvider.For(table, designTime));
            }
        }

        foreach (var view in databaseModel.Views.Values)
        {
            PopulateRowInternalForeignKeys<ViewColumnMapping>(view);

            if (relationalAnnotationProvider != null)
            {
                foreach (ViewColumn viewColumn in view.Columns.Values)
                {
                    viewColumn.AddAnnotations(relationalAnnotationProvider.For(viewColumn, designTime));
                }

                view.AddAnnotations(relationalAnnotationProvider.For(view, designTime));
            }
        }

        if (relationalAnnotationProvider != null)
        {
            foreach (var query in databaseModel.Queries.Values)
            {
                foreach (SqlQueryColumn queryColumn in query.Columns.Values)
                {
                    queryColumn.AddAnnotations(relationalAnnotationProvider.For(queryColumn, designTime));
                }

                query.AddAnnotations(relationalAnnotationProvider.For(query, designTime));
            }

            foreach (var function in databaseModel.Functions.Values)
            {
                foreach (FunctionColumn functionColumn in function.Columns.Values)
                {
                    functionColumn.AddAnnotations(relationalAnnotationProvider.For(functionColumn, designTime));
                }

                function.AddAnnotations(relationalAnnotationProvider.For(function, designTime));
            }

            foreach (var storedProcedure in databaseModel.StoredProcedures.Values)
            {
                foreach (StoreStoredProcedureParameter parameter in storedProcedure.Parameters)
                {
                    parameter.AddAnnotations(relationalAnnotationProvider.For(parameter, designTime));
                }

                foreach (StoreStoredProcedureResultColumn resultColumn in storedProcedure.ResultColumns)
                {
                    resultColumn.AddAnnotations(relationalAnnotationProvider.For(resultColumn, designTime));
                }

                storedProcedure.AddAnnotations(relationalAnnotationProvider.For(storedProcedure, designTime));
            }

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
        var mappedType = entityType;
        Check.DebugAssert(entityType.FindRuntimeAnnotationValue(RelationalAnnotationNames.DefaultMappings) == null, "not null");
        var tableMappings = new List<TableMappingBase<ColumnMappingBase>>();
        entityType.AddRuntimeAnnotation(RelationalAnnotationNames.DefaultMappings, tableMappings);

        var isTpc = entityType.GetMappingStrategy() == RelationalAnnotationNames.TpcMappingStrategy;
        var isTph = entityType.FindDiscriminatorProperty() != null;
        while (mappedType != null)
        {
            var mappedTableName = isTph ? entityType.GetRootType().Name : mappedType.Name;
            if (!databaseModel.DefaultTables.TryGetValue(mappedTableName, out var defaultTable))
            {
                defaultTable = new TableBase(mappedTableName, null, databaseModel);
                databaseModel.DefaultTables.Add(mappedTableName, defaultTable);
            }

            var tableMapping = new TableMappingBase<ColumnMappingBase>(
                entityType, defaultTable, includesDerivedTypes: !isTpc && mappedType == entityType);

            foreach (var property in entityType.GetProperties())
            {
                var columnName = property.IsPrimaryKey() || isTpc || isTph || property.DeclaringEntityType == mappedType
                    ? property.GetColumnName()
                    : null;
                if (columnName == null)
                {
                    continue;
                }

                var column = (ColumnBase<ColumnMappingBase>?)defaultTable.FindColumn(columnName);
                if (column == null)
                {
                    column = new ColumnBase<ColumnMappingBase>(columnName, property.GetColumnType(), defaultTable)
                    {
                        IsNullable = property.IsColumnNullable()
                    };
                    defaultTable.Columns.Add(columnName, column);
                }
                else if (!property.IsColumnNullable())
                {
                    column.IsNullable = false;
                }

                var columnMapping = new ColumnMappingBase(property, column, tableMapping);
                tableMapping.AddColumnMapping(columnMapping);
                column.AddPropertyMapping(columnMapping);

                if (property.FindRuntimeAnnotationValue(RelationalAnnotationNames.DefaultColumnMappings)
                    is not SortedSet<ColumnMappingBase> columnMappings)
                {
                    columnMappings = new SortedSet<ColumnMappingBase>(ColumnMappingBaseComparer.Instance);
                    property.AddRuntimeAnnotation(RelationalAnnotationNames.DefaultColumnMappings, columnMappings);
                }

                columnMappings.Add(columnMapping);
            }

            if (((ITableMappingBase)tableMapping).ColumnMappings.Any()
                || tableMappings.Count == 0)
            {
                tableMappings.Add(tableMapping);
                defaultTable.EntityTypeMappings.Add(tableMapping);
            }

            if (isTpc || isTph)
            {
                break;
            }

            mappedType = mappedType.BaseType;
        }

        tableMappings.Reverse();
    }

    private static void AddTables(RelationalModel databaseModel, IEntityType entityType)
    {
        if (entityType.GetTableName() == null)
        {
            return;
        }

        var mappedType = entityType;

        Check.DebugAssert(entityType.FindRuntimeAnnotationValue(RelationalAnnotationNames.TableMappings) == null, "not null");
        var tableMappings = new List<TableMapping>();
        entityType.SetRuntimeAnnotation(RelationalAnnotationNames.TableMappings, tableMappings);

        var mappingStrategy = entityType.GetMappingStrategy();
        var isTpc = mappingStrategy == RelationalAnnotationNames.TpcMappingStrategy;
        while (mappedType != null)
        {
            var mappedTableName = mappedType.GetTableName();
            var mappedSchema = mappedType.GetSchema();

            if (mappedTableName == null)
            {
                if (isTpc || mappingStrategy == RelationalAnnotationNames.TphMappingStrategy)
                {
                    break;
                }

                mappedType = mappedType.BaseType;
                continue;
            }

            foreach (var fragment in mappedType.GetMappingFragments(StoreObjectType.Table))
            {
                CreateTableMapping(
                    entityType,
                    mappedType,
                    fragment.StoreObject,
                    databaseModel,
                    tableMappings,
                    includesDerivedTypes: !isTpc && mappedType == entityType,
                    isSplitEntityTypePrincipal: false);
            }

            CreateTableMapping(
                entityType,
                mappedType,
                StoreObjectIdentifier.Table(mappedTableName, mappedSchema),
                databaseModel,
                tableMappings,
                includesDerivedTypes: !isTpc && mappedType == entityType,
                isSplitEntityTypePrincipal: mappedType.GetMappingFragments(StoreObjectType.Table).Any() ? true : null);

            if (isTpc || mappingStrategy == RelationalAnnotationNames.TphMappingStrategy)
            {
                break;
            }

            mappedType = mappedType.BaseType;
        }

        tableMappings.Reverse();
    }

    private static void CreateTableMapping(
        IEntityType entityType,
        IEntityType mappedType,
        StoreObjectIdentifier mappedTable,
        RelationalModel databaseModel,
        List<TableMapping> tableMappings,
        bool includesDerivedTypes,
        bool? isSplitEntityTypePrincipal = null)
    {
        if (!databaseModel.Tables.TryGetValue((mappedTable.Name, mappedTable.Schema), out var table))
        {
            table = new Table(mappedTable.Name, mappedTable.Schema, databaseModel);
            databaseModel.Tables.Add((mappedTable.Name, mappedTable.Schema), table);
        }

        var tableMapping = new TableMapping(entityType, table, includesDerivedTypes)
        {
            IsSplitEntityTypePrincipal = isSplitEntityTypePrincipal
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
                column = new(columnName, property.GetColumnType(mappedTable), table)
                {
                    IsNullable = property.IsColumnNullable(mappedTable)
                };
                table.Columns.Add(columnName, column);
            }
            else if (!property.IsColumnNullable(mappedTable))
            {
                column.IsNullable = false;
            }

            var columnMapping = new ColumnMapping(property, column, tableMapping);
            tableMapping.AddColumnMapping(columnMapping);
            column.AddPropertyMapping(columnMapping);

            if (property.FindRuntimeAnnotationValue(RelationalAnnotationNames.TableColumnMappings)
                is not SortedSet<ColumnMapping> columnMappings)
            {
                columnMappings = new SortedSet<ColumnMapping>(ColumnMappingBaseComparer.Instance);
                property.AddRuntimeAnnotation(RelationalAnnotationNames.TableColumnMappings, columnMappings);
            }

            columnMappings.Add(columnMapping);
        }

        if (((ITableMappingBase)tableMapping).ColumnMappings.Any()
            || tableMappings.Count == 0)
        {
            tableMappings.Add(tableMapping);
            table.EntityTypeMappings.Add(tableMapping);
        }
    }

    private static void AddViews(RelationalModel databaseModel, IEntityType entityType)
    {
        if (entityType.GetViewName() == null)
        {
            return;
        }

        var mappedType = entityType;

        Check.DebugAssert(entityType.FindRuntimeAnnotationValue(RelationalAnnotationNames.ViewMappings) == null, "not null");
        var viewMappings = new List<ViewMapping>();
        entityType.SetRuntimeAnnotation(RelationalAnnotationNames.ViewMappings, viewMappings);

        var mappingStrategy = entityType.GetMappingStrategy();
        var isTpc = mappingStrategy == RelationalAnnotationNames.TpcMappingStrategy;
        while (mappedType != null)
        {
            var mappedViewName = mappedType.GetViewName();
            var mappedSchema = mappedType.GetViewSchema();

            if (mappedViewName == null)
            {
                if (isTpc || mappingStrategy == RelationalAnnotationNames.TphMappingStrategy)
                {
                    break;
                }

                mappedType = mappedType.BaseType;
                continue;
            }

            foreach (var fragment in mappedType.GetMappingFragments(StoreObjectType.View))
            {
                CreateViewMapping(
                    entityType,
                    mappedType,
                    fragment.StoreObject,
                    databaseModel,
                    viewMappings,
                    includesDerivedTypes: !isTpc && mappedType == entityType,
                    isSplitEntityTypePrincipal: false);
            }

            CreateViewMapping(
                entityType,
                mappedType,
                StoreObjectIdentifier.View(mappedViewName, mappedSchema),
                databaseModel,
                viewMappings,
                includesDerivedTypes: !isTpc && mappedType == entityType,
                isSplitEntityTypePrincipal: mappedType.GetMappingFragments(StoreObjectType.View).Any() ? true : null);

            if (isTpc || mappingStrategy == RelationalAnnotationNames.TphMappingStrategy)
            {
                break;
            }

            mappedType = mappedType.BaseType;
        }

        viewMappings.Reverse();
    }

    private static void CreateViewMapping(
        IEntityType entityType,
        IEntityType mappedType,
        StoreObjectIdentifier mappedView,
        RelationalModel databaseModel,
        List<ViewMapping> viewMappings,
        bool includesDerivedTypes,
        bool? isSplitEntityTypePrincipal = null)
    {
        if (!databaseModel.Views.TryGetValue((mappedView.Name, mappedView.Schema), out var view))
        {
            view = new View(mappedView.Name, mappedView.Schema, databaseModel);
            databaseModel.Views.Add((mappedView.Name, mappedView.Schema), view);
        }

        var viewMapping = new ViewMapping(entityType, view, includesDerivedTypes)
        {
            IsSplitEntityTypePrincipal = isSplitEntityTypePrincipal
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
                column = new ViewColumn(columnName, property.GetColumnType(mappedView), view)
                {
                    IsNullable = property.IsColumnNullable(mappedView)
                };
                view.Columns.Add(columnName, column);
            }
            else if (!property.IsColumnNullable(mappedView))
            {
                column.IsNullable = false;
            }

            var columnMapping = new ViewColumnMapping(property, column, viewMapping);
            viewMapping.AddColumnMapping(columnMapping);
            column.AddPropertyMapping(columnMapping);

            if (property.FindRuntimeAnnotationValue(RelationalAnnotationNames.ViewColumnMappings)
                is not SortedSet<ViewColumnMapping> columnMappings)
            {
                columnMappings = new SortedSet<ViewColumnMapping>(ColumnMappingBaseComparer.Instance);
                property.AddRuntimeAnnotation(RelationalAnnotationNames.ViewColumnMappings, columnMappings);
            }

            columnMappings.Add(columnMapping);
        }

        if (((ITableMappingBase)viewMapping).ColumnMappings.Any()
            || viewMappings.Count == 0)
        {
            viewMappings.Add(viewMapping);
            view.EntityTypeMappings.Add(viewMapping);
        }
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

            var queryMapping = new SqlQueryMapping(entityType, sqlQuery, includesDerivedTypes: true) { IsDefaultSqlQueryMapping = true };

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
                    column = new SqlQueryColumn(columnName, property.GetColumnType(mappedQuery), sqlQuery);
                    column.IsNullable = property.IsColumnNullable(mappedQuery);
                    sqlQuery.Columns.Add(columnName, column);
                }
                else if (!property.IsColumnNullable(mappedQuery))
                {
                    column.IsNullable = false;
                }

                var columnMapping = new SqlQueryColumnMapping(property, column, queryMapping);
                queryMapping.AddColumnMapping(columnMapping);
                column.AddPropertyMapping(columnMapping);

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

            if (((ITableMappingBase)queryMapping).ColumnMappings.Any()
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

            functionMappings =
                entityType.FindRuntimeAnnotationValue(RelationalAnnotationNames.FunctionMappings) as List<FunctionMapping>;
            if (functionMappings == null)
            {
                functionMappings = new List<FunctionMapping>();
                entityType.AddRuntimeAnnotation(RelationalAnnotationNames.FunctionMappings, functionMappings);
            }

            if (((ITableMappingBase)functionMapping).ColumnMappings.Any()
                || functionMappings.Count == 0)
            {
                functionMappings.Add(functionMapping);
                ((StoreFunction)functionMapping.StoreFunction).EntityTypeMappings.Add(functionMapping);
            }
        }

        functionMappings?.Reverse();
    }

    private static void AddTvfs(RelationalModel relationalModel)
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
            IsDefaultFunctionMapping = @default
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
                column = new FunctionColumn(columnName, property.GetColumnType(mappedFunction), storeFunction)
                {
                    IsNullable = property.IsColumnNullable(mappedFunction)
                };
                storeFunction.Columns.Add(columnName, column);
            }
            else if (!property.IsColumnNullable(mappedFunction))
            {
                column.IsNullable = false;
            }

            var columnMapping = new FunctionColumnMapping(property, column, functionMapping);
            functionMapping.AddColumnMapping(columnMapping);
            column.AddPropertyMapping(columnMapping);

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
            var parameterTypes = dbFunction.Parameters.Select(p => p.StoreType).ToArray();
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

    private static void AddStoredProcedures(
        RelationalModel databaseModel,
        IEntityType entityType,
        IRelationalTypeMappingSource relationalTypeMappingSource)
    {
        var mappedType = entityType;

        Check.DebugAssert(
            entityType.FindRuntimeAnnotationValue(RelationalAnnotationNames.InsertStoredProcedureMappings) == null, "not null");
        var insertStoredProcedureMappings = new List<StoredProcedureMapping>();

        Check.DebugAssert(
            entityType.FindRuntimeAnnotationValue(RelationalAnnotationNames.DeleteStoredProcedureMappings) == null, "not null");
        var deleteStoredProcedureMappings = new List<StoredProcedureMapping>();

        Check.DebugAssert(
            entityType.FindRuntimeAnnotationValue(RelationalAnnotationNames.UpdateStoredProcedureMappings) == null, "not null");
        var updateStoredProcedureMappings = new List<StoredProcedureMapping>();

        var mappingStrategy = entityType.GetMappingStrategy();
        var isTpc = mappingStrategy == RelationalAnnotationNames.TpcMappingStrategy;
        while (mappedType != null)
        {
            var includesDerivedTypes = !isTpc && mappedType == entityType;

            var tableMappings = entityType.GetTableMappings().Where(m
                => m.Table.Name == mappedType.GetTableName()
                && m.Table.Schema == mappedType.GetSchema()
                && m.IsSplitEntityTypePrincipal != false
                && m.IncludesDerivedTypes == includesDerivedTypes);
            var tableMapping = (TableMapping?)tableMappings.FirstOrDefault();

            Check.DebugAssert(tableMapping == null || tableMappings.Count() == 1, "Expected table mapping to be unique");

            var insertSproc = (IRuntimeStoredProcedure?)mappedType.GetInsertStoredProcedure();
            if (insertSproc != null
                && insertStoredProcedureMappings != null)
            {
                var insertProcedureMapping = CreateStoredProcedureMapping(
                    entityType,
                    mappedType,
                    insertSproc,
                    tableMapping,
                    databaseModel,
                    insertStoredProcedureMappings,
                    includesDerivedTypes,
                    relationalTypeMappingSource);
                
                if (tableMapping != null)
                {
                    tableMapping.InsertStoredProcedureMapping = insertProcedureMapping;
                }
            }
            else if (entityType == mappedType)
            {
                insertStoredProcedureMappings = null;
            }

            var deleteSproc = (IRuntimeStoredProcedure?)mappedType.GetDeleteStoredProcedure();
            if (deleteSproc != null
                && deleteStoredProcedureMappings != null)
            {
                var deleteProcedureMapping = CreateStoredProcedureMapping(
                    entityType,
                    mappedType,
                    deleteSproc,
                    tableMapping,
                    databaseModel,
                    deleteStoredProcedureMappings,
                    includesDerivedTypes,
                    relationalTypeMappingSource);
                
                if (tableMapping != null)
                {
                    tableMapping.DeleteStoredProcedureMapping = deleteProcedureMapping;
                }
            }
            else if (entityType == mappedType)
            {
                deleteStoredProcedureMappings = null;
            }

            var updateSproc = (IRuntimeStoredProcedure?)mappedType.GetUpdateStoredProcedure();
            if (updateSproc != null
                && updateStoredProcedureMappings != null)
            {
                var updateProcedureMapping = CreateStoredProcedureMapping(
                    entityType,
                    mappedType,
                    updateSproc,
                    tableMapping,
                    databaseModel,
                    updateStoredProcedureMappings,
                    includesDerivedTypes,
                    relationalTypeMappingSource);
                
                if (tableMapping != null)
                {
                    tableMapping.UpdateStoredProcedureMapping = updateProcedureMapping;
                }
            }
            else if (entityType == mappedType)
            {
                updateStoredProcedureMappings = null;
            }

            if (isTpc || mappingStrategy == RelationalAnnotationNames.TphMappingStrategy)
            {
                break;
            }

            mappedType = mappedType.BaseType;
        }
        
        if (insertStoredProcedureMappings?.Count > 0)
        {
            insertStoredProcedureMappings.Reverse();
            entityType.SetRuntimeAnnotation(RelationalAnnotationNames.InsertStoredProcedureMappings, insertStoredProcedureMappings);
        }

        if (deleteStoredProcedureMappings?.Count > 0)
        {
            deleteStoredProcedureMappings.Reverse();
            entityType.SetRuntimeAnnotation(RelationalAnnotationNames.DeleteStoredProcedureMappings, deleteStoredProcedureMappings);
        }

        if (updateStoredProcedureMappings?.Count > 0)
        {
            updateStoredProcedureMappings.Reverse();
            entityType.SetRuntimeAnnotation(RelationalAnnotationNames.UpdateStoredProcedureMappings, updateStoredProcedureMappings);
        }
    }

    private static StoredProcedureMapping CreateStoredProcedureMapping(
        IEntityType entityType,
        IEntityType mappedType,
        IRuntimeStoredProcedure storedProcedure,
        ITableMapping? tableMapping,
        RelationalModel model,
        List<StoredProcedureMapping> storedProcedureMappings,
        bool includesDerivedTypes,
        IRelationalTypeMappingSource relationalTypeMappingSource)
    {
        var storeStoredProcedure = GetOrCreateStoreStoredProcedure(storedProcedure, model, relationalTypeMappingSource);

        var identifier = storedProcedure.GetStoreIdentifier();
        var storedProcedureMapping = new StoredProcedureMapping(
            entityType, storeStoredProcedure, storedProcedure, tableMapping, includesDerivedTypes);
        var (parameterMappingAnnotationName, columnMappingAnnotationName) = identifier.StoreObjectType switch
        {
            StoreObjectType.InsertStoredProcedure
                => (RelationalAnnotationNames.InsertStoredProcedureParameterMappings,
                    RelationalAnnotationNames.InsertStoredProcedureResultColumnMappings),
            StoreObjectType.DeleteStoredProcedure
                => (RelationalAnnotationNames.DeleteStoredProcedureParameterMappings, ""),
            StoreObjectType.UpdateStoredProcedure
                => (RelationalAnnotationNames.UpdateStoredProcedureParameterMappings,
                    RelationalAnnotationNames.UpdateStoredProcedureResultColumnMappings),
            _ => throw new Exception("Unexpected stored procedure type: " + identifier.StoreObjectType)
        };

        var position = -1;
        foreach (var parameter in storedProcedure.Parameters)
        {
            position++;
            if (parameter.PropertyName == null)
            {
                GetOrCreateStoreStoredProcedureParameter(
                    parameter,
                    null,
                    position,
                    storeStoredProcedure,
                    identifier,
                    relationalTypeMappingSource);

                continue;
            }

            var property = mappedType.FindProperty(parameter.PropertyName);
            if (property == null)
            {
                Check.DebugAssert(
                    entityType.GetMappingStrategy() == RelationalAnnotationNames.TphMappingStrategy,
                    "Expected TPH for " + entityType.DisplayName());

                if (entityType.BaseType == null)
                {
                    foreach (var derivedProperty in entityType.GetDerivedProperties())
                    {
                        if (derivedProperty.Name == parameter.PropertyName)
                        {
                            GetOrCreateStoreStoredProcedureParameter(
                                parameter,
                                derivedProperty,
                                position,
                                storeStoredProcedure,
                                identifier,
                                relationalTypeMappingSource);
                            break;
                        }
                    }
                }

                continue;
            }

            var storeParameter = GetOrCreateStoreStoredProcedureParameter(
                parameter,
                property,
                position,
                storeStoredProcedure,
                identifier,
                relationalTypeMappingSource);

            var columnMapping = new StoredProcedureParameterMapping(
                property, parameter, storeParameter, storedProcedureMapping);
            storedProcedureMapping.AddParameterMapping(columnMapping);
            storeParameter.AddPropertyMapping(columnMapping);

            if (property.FindRuntimeAnnotationValue(parameterMappingAnnotationName)
                is not SortedSet<StoredProcedureParameterMapping> columnMappings)
            {
                columnMappings = new SortedSet<StoredProcedureParameterMapping>(ColumnMappingBaseComparer.Instance);
                property.AddRuntimeAnnotation(parameterMappingAnnotationName, columnMappings);
            }

            columnMappings.Add(columnMapping);
        }

        foreach (var resultColumn in storedProcedure.ResultColumns)
        {
            if (resultColumn.PropertyName == null)
            {
                GetOrCreateStoreStoredProcedureResultColumn(
                    resultColumn,
                    null,
                    storeStoredProcedure,
                    identifier,
                    relationalTypeMappingSource);

                continue;
            }

            var property = mappedType.FindProperty(resultColumn.PropertyName);
            if (property == null)
            {
                Check.DebugAssert(
                    entityType.GetMappingStrategy() == RelationalAnnotationNames.TphMappingStrategy,
                    "Expected TPH for " + entityType.DisplayName());

                if (entityType.BaseType == null)
                {
                    foreach (var derivedProperty in entityType.GetDerivedProperties())
                    {
                        if (derivedProperty.Name == resultColumn.PropertyName)
                        {
                            GetOrCreateStoreStoredProcedureResultColumn(
                                resultColumn,
                                derivedProperty,
                                storeStoredProcedure,
                                identifier,
                                relationalTypeMappingSource);
                            break;
                        }
                    }
                }

                continue;
            }

            var column = GetOrCreateStoreStoredProcedureResultColumn(
                resultColumn,
                property,
                storeStoredProcedure,
                identifier,
                relationalTypeMappingSource);

            var columnMapping = new StoredProcedureResultColumnMapping(
                property, resultColumn, column, storedProcedureMapping);
            storedProcedureMapping.AddColumnMapping(columnMapping);
            column.AddPropertyMapping(columnMapping);

            if (property.FindRuntimeAnnotationValue(columnMappingAnnotationName)
                is not SortedSet<StoredProcedureResultColumnMapping> columnMappings)
            {
                columnMappings = new SortedSet<StoredProcedureResultColumnMapping>(ColumnMappingBaseComparer.Instance);
                property.AddRuntimeAnnotation(columnMappingAnnotationName, columnMappings);
            }

            columnMappings.Add(columnMapping);
        }

        storedProcedureMappings.Add(storedProcedureMapping);
        storeStoredProcedure.EntityTypeMappings.Add(storedProcedureMapping);

        return storedProcedureMapping;

        static StoreStoredProcedure GetOrCreateStoreStoredProcedure(
            IRuntimeStoredProcedure storedProcedure,
            RelationalModel model,
            IRelationalTypeMappingSource relationalTypeMappingSource)
        {
            var storeStoredProcedure = (StoreStoredProcedure?)storedProcedure.StoreStoredProcedure;
            if (storeStoredProcedure == null)
            {
                storeStoredProcedure = (StoreStoredProcedure?)model.FindStoredProcedure(storedProcedure.Name, storedProcedure.Schema);
                if (storeStoredProcedure == null)
                {
                    storeStoredProcedure = new StoreStoredProcedure(storedProcedure, model);
                    if (storedProcedure.AreRowsAffectedReturned)
                    {
                        var typeMapping = relationalTypeMappingSource.FindMapping(typeof(int))!;
                        storeStoredProcedure.Return = new StoreStoredProcedureReturn(
                                "",
                                typeMapping.StoreType,
                                storeStoredProcedure,
                                typeMapping);
                    }
                    model.StoredProcedures.Add((storeStoredProcedure.Name, storeStoredProcedure.Schema), storeStoredProcedure);
                }
                else
                {
                    storedProcedure.StoreStoredProcedure = storeStoredProcedure;
                    storeStoredProcedure.StoredProcedures.Add(storedProcedure);
                }
            }

            return storeStoredProcedure;
        }

        static StoreStoredProcedureParameter GetOrCreateStoreStoredProcedureParameter(
            IStoredProcedureParameter parameter,
            IProperty? property,
            int position,
            StoreStoredProcedure storeStoredProcedure,
            StoreObjectIdentifier identifier,
            IRelationalTypeMappingSource relationalTypeMappingSource)
        {
            var name = parameter.Name;
            var storeParameter = (StoreStoredProcedureParameter?)storeStoredProcedure.FindParameter(name);
            if (storeParameter == null)
            {
                if (property == null)
                {
                    var typeMapping = relationalTypeMappingSource.FindMapping(typeof(int))!;
                    storeParameter = new StoreStoredProcedureParameter(
                        name,
                        typeMapping.StoreType,
                        position,
                        storeStoredProcedure,
                        parameter.Direction,
                        typeMapping);
                }
                else
                {
                    storeParameter = new StoreStoredProcedureParameter(
                        name,
                        property.GetColumnType(identifier),
                        position,
                        storeStoredProcedure,
                        parameter.Direction)
                    {
                        IsNullable = property.IsColumnNullable(identifier)
                    };
                }

                storeStoredProcedure.AddParameter(storeParameter);
            }
            else if (property?.IsColumnNullable(identifier) == false)
            {
                storeParameter.IsNullable = false;
            }
            
            ((IRuntimeStoredProcedureParameter)parameter).StoreParameter = storeParameter;
            return storeParameter;
        }

        static StoreStoredProcedureResultColumn GetOrCreateStoreStoredProcedureResultColumn(
            IStoredProcedureResultColumn resultColumn,
            IProperty? property,
            StoreStoredProcedure storeStoredProcedure,
            StoreObjectIdentifier identifier,
            IRelationalTypeMappingSource relationalTypeMappingSource)
        {
            var name = resultColumn.Name;
            var column = (StoreStoredProcedureResultColumn?)storeStoredProcedure.FindResultColumn(name);
            if (column == null)
            {
                if (property == null)
                {
                    var typeMapping = relationalTypeMappingSource.FindMapping(typeof(int))!;
                    column = new StoreStoredProcedureResultColumn(
                        name,
                        typeMapping.StoreType,
                        storeStoredProcedure);
                }
                else
                {
                    column = new StoreStoredProcedureResultColumn(
                        name,
                        property.GetColumnType(identifier),
                        storeStoredProcedure)
                    {
                        IsNullable = property.IsColumnNullable(identifier)
                    };
                }

                storeStoredProcedure.AddResultColumn(column);
            }
            else if (property?.IsColumnNullable(identifier) == false)
            {
                column.IsNullable = false;
            }

            ((IRuntimeStoredProcedureResultColumn)resultColumn).StoreResultColumn = column;
            return column;
        }
    }

    private static void PopulateTableConfiguration(Table table, bool designTime)
    {
        var storeObject = StoreObjectIdentifier.Table(table.Name, table.Schema);
        foreach (var entityTypeMapping in ((ITable)table).EntityTypeMappings)
        {
            if (!entityTypeMapping.IncludesDerivedTypes
                && entityTypeMapping.EntityType.GetTableMappings().Any(m => m.IncludesDerivedTypes))
            {
                continue;
            }

            var entityType = entityTypeMapping.EntityType;
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

            if (designTime)
            {
                foreach (var checkConstraint in entityType.GetCheckConstraints())
                {
                    var name = checkConstraint.GetName(storeObject);
                    if (name == null)
                    {
                        continue;
                    }

                    if (!table.CheckConstraints.ContainsKey(name))
                    {
                        table.CheckConstraints.Add(name, (CheckConstraint)checkConstraint);
                    }
                }
            }

            // Triggers cannot be inherited
            foreach (var trigger in entityType.GetDeclaredTriggers())
            {
                var name = trigger.GetName(storeObject);
                if (name == null)
                {
                    continue;
                }

                Check.DebugAssert(trigger.TableName == table.Name, "Mismatch in trigger table name");
                Check.DebugAssert(trigger.TableSchema == table.Schema, "Mismatch in trigger table schema");

                if (!table.Triggers.ContainsKey(name))
                {
                    table.Triggers.Add(name, trigger);
                }
            }
        }
    }

    private static void PopulateRowInternalForeignKeys<TColumnMapping>(TableBase table)
        where TColumnMapping : class, IColumnMappingBase
    {
        SortedDictionary<IEntityType, IEnumerable<IForeignKey>>? internalForeignKeyMap = null;
        SortedDictionary<IEntityType, IEnumerable<IForeignKey>>? referencingInternalForeignKeyMap = null;
        TableMappingBase<TColumnMapping>? mainMapping = null;
        var mappedEntityTypes = new HashSet<IEntityType>();
        foreach (TableMappingBase<TColumnMapping> entityTypeMapping in table.EntityTypeMappings)
        {
            if (table.EntityTypeMappings.Count > 1)
            {
                entityTypeMapping.IsSharedTablePrincipal = false;
            }

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
                    rowInternalForeignKeys ??= new SortedSet<IForeignKey>(ForeignKeyComparer.Instance);

                    rowInternalForeignKeys.Add(foreignKey);

                    referencingInternalForeignKeyMap ??=
                        new SortedDictionary<IEntityType, IEnumerable<IForeignKey>>(EntityTypeFullNameComparer.Instance);

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

        Check.DebugAssert(
            mainMapping is not null,
            $"{nameof(mainMapping)} is neither a {nameof(TableMapping)} nor a {nameof(ViewMapping)}");

        if (table.EntityTypeMappings.Count > 1)
        {
            // Re-add the mapping to update the order
            mainMapping.Table.EntityTypeMappings.Remove(mainMapping);
            mainMapping.IsSharedTablePrincipal = true;
            mainMapping.Table.EntityTypeMappings.Add(mainMapping);
        }

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
        else
        {
            table.OptionalEntityTypes = table.EntityTypeMappings.ToDictionary(etm => etm.EntityType, _ => false);
        }
    }

    private static void PopulateForeignKeyConstraints(Table table)
    {
        var storeObject = StoreObjectIdentifier.Table(table.Name, table.Schema);
        foreach (var entityTypeMapping in ((ITable)table).EntityTypeMappings)
        {
            if (!entityTypeMapping.IncludesDerivedTypes
                && entityTypeMapping.EntityType.GetTableMappings().Any(m => m.IncludesDerivedTypes))
            {
                continue;
            }

            var entityType = entityTypeMapping.EntityType;
            foreach (var foreignKey in entityType.GetForeignKeys())
            {
                foreach (var principalMapping in foreignKey.PrincipalEntityType.GetTableMappings().Reverse())
                {
                    var principalTable = (Table)principalMapping.Table;
                    var principalStoreObject = StoreObjectIdentifier.Table(principalTable.Name, principalTable.Schema);
                    var name = foreignKey.GetConstraintName(storeObject, principalStoreObject);
                    if (name == null)
                    {
                        continue;
                    }

                    var foreignKeyConstraints = foreignKey.FindRuntimeAnnotationValue(RelationalAnnotationNames.ForeignKeyMappings)
                        as SortedSet<ForeignKeyConstraint>;
                    var constraint = table.ForeignKeyConstraints.FirstOrDefault(fk => fk.Name == name);
                    if (constraint != null)
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
                        Check.DebugAssert(false, "Should not get here if name is not null");
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

                    if (columns == null
                        || columns.SequenceEqual(principalColumns))
                    {
                        Check.DebugAssert(false, "Should not get here if name is not null");
                        break;
                    }

                    if (entityTypeMapping.IncludesDerivedTypes
                        && foreignKey.DeclaringEntityType != entityType
                        && entityType.FindPrimaryKey() is IKey primaryKey
                        && foreignKey.Properties.SequenceEqual(primaryKey.Properties))
                    {
                        Check.DebugAssert(false, "Should not get here if name is not null");
                        break;
                    }

                    var principalUniqueConstraintName = foreignKey.PrincipalKey.GetName(principalStoreObject);
                    if (principalUniqueConstraintName == null)
                    {
                        continue;
                    }

                    var principalUniqueConstraint = principalTable.FindUniqueConstraint(principalUniqueConstraintName)!;

                    Check.DebugAssert(principalUniqueConstraint != null, "Invalid unique constraint " + principalUniqueConstraintName);

                    constraint = new ForeignKeyConstraint(
                        name, table, principalTable, columns, principalUniqueConstraint, ToReferentialAction(foreignKey.DeleteBehavior));
                    constraint.MappedForeignKeys.Add(foreignKey);

                    if (foreignKeyConstraints == null)
                    {
                        foreignKeyConstraints = new SortedSet<ForeignKeyConstraint>(ForeignKeyConstraintComparer.Instance);
                        foreignKey.AddRuntimeAnnotation(RelationalAnnotationNames.ForeignKeyMappings, foreignKeyConstraints);
                    }

                    foreignKeyConstraints.Add(constraint);
                    table.ForeignKeyConstraints.Add(constraint);
                    principalTable.ReferencingForeignKeyConstraints.Add(constraint);
                    break;
                }
            }
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
            DeleteBehavior.NoAction or DeleteBehavior.ClientSetNull or DeleteBehavior.ClientCascade or DeleteBehavior.ClientNoAction =>
                ReferentialAction.NoAction,
            DeleteBehavior.Restrict => ReferentialAction.Restrict,
            _ => throw new NotSupportedException(deleteBehavior.ToString())
        };

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual DebugView DebugView
        => new(
            () => ((IRelationalModel)this).ToDebugString(),
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

    IEnumerable<IStoreStoredProcedure> IRelationalModel.StoredProcedures
    {
        [DebuggerStepThrough]
        get => StoredProcedures.Values;
    }

    IEnumerable<ISqlQuery> IRelationalModel.Queries
    {
        [DebuggerStepThrough]
        get => Queries.Values;
    }
}
