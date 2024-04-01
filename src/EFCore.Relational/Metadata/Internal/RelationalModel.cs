// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text;
using System.Text.Json;

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
    public virtual Dictionary<string, TableBase> DefaultTables { get; } = new();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual Dictionary<(string, string?), Table> Tables { get; } = new();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual Dictionary<(string, string?), View> Views { get; } = new();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual Dictionary<string, SqlQuery> Queries { get; } = new();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual Dictionary<(string, string?, IReadOnlyList<string>), StoreFunction> Functions { get; }
        = new(NamedListComparer.Instance);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual Dictionary<(string, string?), StoreStoredProcedure> StoredProcedures { get; }
        = new();

    /// <inheritdoc />
    public virtual ITable? FindTable(string name, string? schema)
        => Tables.GetValueOrDefault((name, schema));

    // TODO: Confirm that this makes sense
    /// <inheritdoc />
    public virtual TableBase? FindDefaultTable(string name)
        => DefaultTables.GetValueOrDefault(name);

    /// <inheritdoc />
    public virtual IView? FindView(string name, string? schema)
        => Views.GetValueOrDefault((name, schema));

    /// <inheritdoc />
    public virtual ISqlQuery? FindQuery(string name)
        => Queries.GetValueOrDefault(name);

    /// <inheritdoc />
    public virtual IStoreFunction? FindFunction(string name, string? schema, IReadOnlyList<string> parameters)
        => Functions.GetValueOrDefault((name, schema, parameters));

    /// <inheritdoc />
    public virtual IStoreStoredProcedure? FindStoredProcedure(string name, string? schema)
        => StoredProcedures.GetValueOrDefault((name, schema));

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
            AddDefaultMappings(databaseModel, entityType, relationalTypeMappingSource);

            AddTables(databaseModel, entityType, relationalTypeMappingSource);

            AddViews(databaseModel, entityType, relationalTypeMappingSource);

            AddSqlQueries(databaseModel, entityType);

            AddMappedFunctions(databaseModel, entityType);

            AddStoredProcedures(databaseModel, entityType, relationalTypeMappingSource);
        }

        AddTvfs(databaseModel);

        var tables = ((IRelationalModel)databaseModel).Tables;
        foreach (Table table in tables)
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
            }
        }

        foreach (Table table in tables)
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

        foreach (View view in ((IRelationalModel)databaseModel).Views)
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
            foreach (SqlQuery query in ((IRelationalModel)databaseModel).Queries)
            {
                foreach (SqlQueryColumn queryColumn in query.Columns.Values)
                {
                    queryColumn.AddAnnotations(relationalAnnotationProvider.For(queryColumn, designTime));
                }

                query.AddAnnotations(relationalAnnotationProvider.For(query, designTime));
            }

            foreach (StoreFunction function in ((IRelationalModel)databaseModel).Functions)
            {
                foreach (var parameter in function.Parameters)
                {
                    parameter.AddAnnotations(relationalAnnotationProvider.For(parameter, designTime));
                }

                foreach (FunctionColumn functionColumn in function.Columns.Values)
                {
                    functionColumn.AddAnnotations(relationalAnnotationProvider.For(functionColumn, designTime));
                }

                function.AddAnnotations(relationalAnnotationProvider.For(function, designTime));
            }

            foreach (StoreStoredProcedure storedProcedure in ((IRelationalModel)databaseModel).StoredProcedures)
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

            databaseModel.AddAnnotations(relationalAnnotationProvider.For(databaseModel, designTime));
        }

        return databaseModel.MakeReadOnly();
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual RelationalModel MakeReadOnly()
    {
        _isReadOnly = true;

        return this;
    }

    private static void AddDefaultMappings(
        RelationalModel databaseModel,
        IEntityType entityType,
        IRelationalTypeMappingSource relationalTypeMappingSource)
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
                entityType, defaultTable,
                includesDerivedTypes: entityType.GetDirectlyDerivedTypes().Any()
                    ? !isTpc && mappedType == entityType
                    : null);
            var containerColumnName = mappedType.GetContainerColumnName();
            if (!string.IsNullOrEmpty(containerColumnName))
            {
                CreateContainerColumn(
                    defaultTable, containerColumnName, mappedType, relationalTypeMappingSource,
                    static (c, t, m) => new JsonColumnBase(c, m.StoreType, t, m));
            }
            else
            {
                CreateDefaultColumnMapping(entityType, mappedType, defaultTable, tableMapping, isTph, isTpc);
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

    private static void CreateDefaultColumnMapping(
        ITypeBase typeBase,
        ITypeBase mappedType,
        TableBase defaultTable,
        TableMappingBase<ColumnMappingBase> tableMapping,
        bool isTph,
        bool isTpc)
    {
        foreach (var property in typeBase.GetProperties())
        {
            var columnName = property.IsPrimaryKey() || isTpc || isTph || property.DeclaringType == mappedType
                ? GetColumnName(property)
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

            CreateColumnMapping(column, property, tableMapping);
        }

        foreach (var complexProperty in typeBase.GetDeclaredComplexProperties())
        {
            var complexType = complexProperty.ComplexType;
            tableMapping = new TableMappingBase<ColumnMappingBase>(complexType, defaultTable, includesDerivedTypes: null);

            CreateDefaultColumnMapping(complexType, complexType, defaultTable, tableMapping, isTph, isTpc);

            var tableMappings = (List<TableMappingBase<ColumnMappingBase>>?)complexType
                .FindRuntimeAnnotationValue(RelationalAnnotationNames.DefaultMappings);
            if (tableMappings == null)
            {
                tableMappings = new List<TableMappingBase<ColumnMappingBase>>();
                complexType.AddRuntimeAnnotation(RelationalAnnotationNames.DefaultMappings, tableMappings);
            }
            tableMappings.Add(tableMapping);

            defaultTable.ComplexTypeMappings.Add(tableMapping);
        }

        static string GetColumnName(IProperty property)
        {
            var complexType = property.DeclaringType as IComplexType;
            if (complexType != null)
            {
                var builder = new StringBuilder();
                builder.Append(property.Name);
                while (complexType != null)
                {
                    builder.Insert(0, "_");
                    builder.Insert(0, complexType.ComplexProperty.Name);

                    complexType = complexType.ComplexProperty.DeclaringType as IComplexType;
                }

                return builder.ToString();
            }

            return property.GetColumnName();
        }
    }

    private static void AddTables(
        RelationalModel databaseModel,
        IEntityType entityType,
        IRelationalTypeMappingSource relationalTypeMappingSource)
    {
        if (entityType.GetTableName() == null)
        {
            return;
        }

        var mappedType = entityType;

        Check.DebugAssert(entityType.FindRuntimeAnnotationValue(RelationalAnnotationNames.TableMappings) == null, "not null");
        var tableMappings = new List<TableMapping>();
        entityType.AddRuntimeAnnotation(RelationalAnnotationNames.TableMappings, tableMappings);

        var mappingStrategy = entityType.GetMappingStrategy();
        var isTpc = mappingStrategy == RelationalAnnotationNames.TpcMappingStrategy;
        var includesDerivedTypes = entityType.GetDirectlyDerivedTypes().Any()
                ? !isTpc && mappedType == entityType
                : (bool?)null;
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
                    relationalTypeMappingSource,
                    entityType,
                    mappedType,
                    fragment.StoreObject,
                    databaseModel,
                    tableMappings,
                    includesDerivedTypes: includesDerivedTypes,
                    isSplitEntityTypePrincipal: false);
            }

            CreateTableMapping(
                relationalTypeMappingSource,
                entityType,
                mappedType,
                StoreObjectIdentifier.Table(mappedTableName, mappedSchema),
                databaseModel,
                tableMappings,
                includesDerivedTypes: includesDerivedTypes,
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
        IRelationalTypeMappingSource relationalTypeMappingSource,
        ITypeBase typeBase,
        ITypeBase mappedType,
        StoreObjectIdentifier mappedTable,
        RelationalModel databaseModel,
        List<TableMapping> tableMappings,
        bool? includesDerivedTypes,
        bool? isSplitEntityTypePrincipal = null)
    {
        if (!databaseModel.Tables.TryGetValue((mappedTable.Name, mappedTable.Schema), out var table))
        {
            table = new Table(mappedTable.Name, mappedTable.Schema, databaseModel);
            databaseModel.Tables.Add(
                (mappedTable.Name, mappedTable.Schema), table);
        }

        var tableMapping = new TableMapping(typeBase, table, includesDerivedTypes)
        {
            IsSplitEntityTypePrincipal = isSplitEntityTypePrincipal
        };

        var containerColumnName = mappedType.GetContainerColumnName();
        if (!string.IsNullOrEmpty(containerColumnName))
        {
            CreateContainerColumn(
                table, containerColumnName, (IEntityType)mappedType, relationalTypeMappingSource,
                static (c, t, m) => new JsonColumn(c, m.StoreType, (Table)t, m));
        }
        else
        {
            foreach (var property in mappedType.GetProperties())
            {
                var columnName = property.GetColumnName(mappedTable);
                if (columnName == null)
                {
                    continue;
                }

                var column = table.FindColumn(columnName);
                if (column == null)
                {
                    column = new Column(columnName, property.GetColumnType(mappedTable), table)
                    {
                        IsNullable = property.IsColumnNullable(mappedTable)
                    };

                    table.Columns.Add(columnName, column);
                }
                else if (!property.IsColumnNullable(mappedTable))
                {
                    column.IsNullable = false;
                }

                CreateColumnMapping(column, property, tableMapping);
            }

            foreach (var complexProperty in mappedType.GetDeclaredComplexProperties())
            {
                var complexType = complexProperty.ComplexType;

                var complexTableMappings = (List<TableMapping>?)complexType.FindRuntimeAnnotationValue(RelationalAnnotationNames.TableMappings);
                if (complexTableMappings == null)
                {
                    complexTableMappings = [];
                    complexType.AddRuntimeAnnotation(RelationalAnnotationNames.TableMappings, complexTableMappings);
                }

                CreateTableMapping(
                    relationalTypeMappingSource,
                    complexType,
                    complexType,
                    mappedTable,
                    databaseModel,
                    complexTableMappings,
                    includesDerivedTypes: true,
                    isSplitEntityTypePrincipal: isSplitEntityTypePrincipal == true ? false : isSplitEntityTypePrincipal);
            }
        }

        if (((ITableMappingBase)tableMapping).ColumnMappings.Any()
            || tableMappings.Count == 0)
        {
            tableMappings.Add(tableMapping);
            if (typeBase is IEntityType)
            {
                table.EntityTypeMappings.Add(tableMapping);
            }
            else
            {
                table.ComplexTypeMappings.Add(tableMapping);
            }
        }
    }

    private static void CreateContainerColumn<TColumnMappingBase>(
        TableBase tableBase,
        string containerColumnName,
        IEntityType mappedType,
        IRelationalTypeMappingSource relationalTypeMappingSource,
        Func<string, TableBase, RelationalTypeMapping, ColumnBase<TColumnMappingBase>> createColumn)
        where TColumnMappingBase : class, IColumnMappingBase
    {
        var ownership = mappedType.GetForeignKeys().Single(fk => fk.IsOwnership);
        if (!ownership.PrincipalEntityType.IsMappedToJson())
        {
            Check.DebugAssert(tableBase.FindColumn(containerColumnName) == null, $"Table does not have column '{containerColumnName}'.");

            var jsonColumnTypeMapping = relationalTypeMappingSource.FindMapping(typeof(JsonElement), mappedType.Model)!;
            var jsonColumn = createColumn(containerColumnName, tableBase, jsonColumnTypeMapping);
            tableBase.Columns.Add(containerColumnName, jsonColumn);
            jsonColumn.IsNullable = !ownership.IsRequiredDependent || !ownership.IsUnique;

            if (ownership.PrincipalEntityType.BaseType != null)
            {
                // if navigation is defined on a derived type, the column must be made nullable
                jsonColumn.IsNullable = true;
            }
        }
    }

    private static void AddViews(
        RelationalModel databaseModel,
        IEntityType entityType,
        IRelationalTypeMappingSource relationalTypeMappingSource)
    {
        if (entityType.GetViewName() == null)
        {
            return;
        }

        var mappedType = entityType;

        Check.DebugAssert(entityType.FindRuntimeAnnotationValue(RelationalAnnotationNames.ViewMappings) == null, "not null");
        var viewMappings = new List<ViewMapping>();
        entityType.AddRuntimeAnnotation(RelationalAnnotationNames.ViewMappings, viewMappings);

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

            var includesDerivedTypes = entityType.GetDirectlyDerivedTypes().Any()
                    ? !isTpc && mappedType == entityType
                    : (bool?)null;
            foreach (var fragment in mappedType.GetMappingFragments(StoreObjectType.View))
            {
                CreateViewMapping(
                    relationalTypeMappingSource,
                    entityType,
                    mappedType,
                    fragment.StoreObject,
                    databaseModel,
                    viewMappings,
                    includesDerivedTypes: includesDerivedTypes,
                    isSplitEntityTypePrincipal: false);
            }

            CreateViewMapping(
                relationalTypeMappingSource,
                entityType,
                mappedType,
                StoreObjectIdentifier.View(mappedViewName, mappedSchema),
                databaseModel,
                viewMappings,
                includesDerivedTypes: includesDerivedTypes,
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
        IRelationalTypeMappingSource relationalTypeMappingSource,
        IEntityType entityType,
        IEntityType mappedType,
        StoreObjectIdentifier mappedView,
        RelationalModel databaseModel,
        List<ViewMapping> viewMappings,
        bool? includesDerivedTypes,
        bool? isSplitEntityTypePrincipal = null)
    {
        if (!databaseModel.Views.TryGetValue((mappedView.Name, mappedView.Schema), out var view))
        {
            view = new View(mappedView.Name, mappedView.Schema, databaseModel);
            databaseModel.Views.Add(
                (mappedView.Name, mappedView.Schema), view);
        }

        var viewMapping = new ViewMapping(entityType, view, includesDerivedTypes)
        {
            IsSplitEntityTypePrincipal = isSplitEntityTypePrincipal
        };

        var containerColumnName = mappedType.GetContainerColumnName();
        if (!string.IsNullOrEmpty(containerColumnName))
        {
            CreateContainerColumn(
                view, containerColumnName, mappedType, relationalTypeMappingSource,
                static (c, t, m) => new JsonViewColumn(c, m.StoreType, (View)t, m));
        }
        else
        {
            foreach (var property in mappedType.GetProperties())
            {
                var columnName = property.GetColumnName(mappedView);
                if (columnName == null)
                {
                    continue;
                }

                var column = view.FindColumn(columnName);
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

                CreateViewColumnMapping(column, property, viewMapping);
            }
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

            var queryMapping = new SqlQueryMapping(entityType, sqlQuery,
                includesDerivedTypes: entityType.GetDirectlyDerivedTypes().Any() ? true : null)
            {
                IsDefaultSqlQueryMapping = true
            };

            foreach (var property in mappedType.GetProperties())
            {
                var columnName = property.GetColumnName(mappedQuery);
                if (columnName == null)
                {
                    continue;
                }

                var column = sqlQuery.FindColumn(columnName);
                if (column == null)
                {
                    column = new SqlQueryColumn(columnName, property.GetColumnType(mappedQuery), sqlQuery)
                    {
                        IsNullable = property.IsColumnNullable(mappedQuery)
                    };
                    sqlQuery.Columns.Add(columnName, column);
                }
                else if (!property.IsColumnNullable(mappedQuery))
                {
                    column.IsNullable = false;
                }

                CreateSqlQueryColumnMapping(column, property, queryMapping);
            }

            mappedType = mappedType.BaseType;

            queryMappings = entityType.FindRuntimeAnnotationValue(RelationalAnnotationNames.SqlQueryMappings) as List<SqlQueryMapping>;
            if (queryMappings == null)
            {
                queryMappings = [];
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
                functionMappings = [];
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
                functionMappings = [];
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
        var functionMapping = new FunctionMapping(entityType, storeFunction, dbFunction,
            includesDerivedTypes: entityType.GetDirectlyDerivedTypes().Any() ? true : null)
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

            var column = storeFunction.FindColumn(columnName);
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

            CreateFunctionColumnMapping(column, property, functionMapping);
        }

        return functionMapping;
    }

    private static StoreFunction GetOrCreateStoreFunction(IRuntimeDbFunction dbFunction, RelationalModel databaseModel)
    {
        var storeFunction = (StoreFunction?)dbFunction.StoreFunction;
        if (storeFunction == null)
        {
            var parameterTypes = dbFunction.Parameters.Select(p => p.StoreType).ToArray();
            storeFunction = (StoreFunction?)databaseModel.FindFunction(dbFunction.Name, dbFunction.Schema, parameterTypes);
            if (storeFunction == null)
            {
                storeFunction = new StoreFunction(dbFunction, databaseModel);
                databaseModel.Functions.Add(
                    (storeFunction.Name, storeFunction.Schema, parameterTypes), storeFunction);
            }
            else
            {
                storeFunction.AddDbFunction(dbFunction);
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
        var isTpt = mappingStrategy == RelationalAnnotationNames.TptMappingStrategy;
        var isTpc = mappingStrategy == RelationalAnnotationNames.TpcMappingStrategy;
        var isTph = mappingStrategy == RelationalAnnotationNames.TphMappingStrategy;
        while (mappedType != null)
        {
            var includesDerivedTypes = entityType.GetDirectlyDerivedTypes().Any()
                    ? !isTpc && mappedType == entityType
                    : (bool?)null;

            var tableMappings = entityType.GetTableMappings().Where(
                m => m.Table.Name == mappedType.GetTableName()
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
                    Check.DebugAssert(
                        tableMapping.InsertStoredProcedureMapping == null,
                        "Expected sproc mapping to be unique");
                    tableMapping.InsertStoredProcedureMapping = insertProcedureMapping;
                }
            }
            else if (entityType == mappedType && !isTpt)
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
                    Check.DebugAssert(
                        tableMapping.DeleteStoredProcedureMapping == null,
                        "Expected sproc mapping to be unique");
                    tableMapping.DeleteStoredProcedureMapping = deleteProcedureMapping;
                }
            }
            else if (entityType == mappedType && !isTpt)
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
                    Check.DebugAssert(
                        tableMapping.UpdateStoredProcedureMapping == null,
                        "Expected sproc mapping to be unique");
                    tableMapping.UpdateStoredProcedureMapping = updateProcedureMapping;
                }
            }
            else if (entityType == mappedType && !isTpt)
            {
                updateStoredProcedureMappings = null;
            }

            if (isTpc || isTph)
            {
                break;
            }

            mappedType = mappedType.BaseType;
        }

        if (insertStoredProcedureMappings?.Count > 0)
        {
            insertStoredProcedureMappings.Reverse();
            entityType.AddRuntimeAnnotation(RelationalAnnotationNames.InsertStoredProcedureMappings, insertStoredProcedureMappings);
        }

        if (deleteStoredProcedureMappings?.Count > 0)
        {
            deleteStoredProcedureMappings.Reverse();
            entityType.AddRuntimeAnnotation(RelationalAnnotationNames.DeleteStoredProcedureMappings, deleteStoredProcedureMappings);
        }

        if (updateStoredProcedureMappings?.Count > 0)
        {
            updateStoredProcedureMappings.Reverse();
            entityType.AddRuntimeAnnotation(RelationalAnnotationNames.UpdateStoredProcedureMappings, updateStoredProcedureMappings);
        }
    }

    private static StoredProcedureMapping CreateStoredProcedureMapping(
        IEntityType entityType,
        IEntityType mappedType,
        IRuntimeStoredProcedure storedProcedure,
        ITableMapping? tableMapping,
        RelationalModel model,
        List<StoredProcedureMapping> storedProcedureMappings,
        bool? includesDerivedTypes,
        IRelationalTypeMappingSource relationalTypeMappingSource)
    {
        var storeStoredProcedure = GetOrCreateStoreStoredProcedure(storedProcedure, model, relationalTypeMappingSource);

        var identifier = storedProcedure.GetStoreIdentifier();
        var storedProcedureMapping = new StoredProcedureMapping(
            entityType, storeStoredProcedure, storedProcedure, tableMapping, includesDerivedTypes);

        foreach (var parameter in storedProcedure.Parameters)
        {
            if (parameter.PropertyName == null)
            {
                GetOrCreateStoreStoredProcedureParameter(
                    parameter,
                    null,
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

                foreach (var derivedType in entityType.GetRootType().GetDerivedTypes())
                {
                    var derivedProperty = derivedType.FindProperty(parameter.PropertyName);
                    if (derivedProperty != null)
                    {
                        GetOrCreateStoreStoredProcedureParameter(
                            parameter,
                            derivedProperty,
                            storeStoredProcedure,
                            identifier,
                            relationalTypeMappingSource);
                        break;
                    }
                }

                continue;
            }

            var storeParameter = GetOrCreateStoreStoredProcedureParameter(
                parameter,
                property,
                storeStoredProcedure,
                identifier,
                relationalTypeMappingSource);

            CreateStoredProcedureParameterMapping(storeParameter, parameter, property, storedProcedureMapping);
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

                foreach (var derivedType in entityType.GetRootType().GetDerivedTypes())
                {
                    var derivedProperty = derivedType.FindProperty(resultColumn.PropertyName);
                    if (derivedProperty != null)
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

                continue;
            }

            var column = GetOrCreateStoreStoredProcedureResultColumn(
                resultColumn,
                property,
                storeStoredProcedure,
                identifier,
                relationalTypeMappingSource);

            CreateStoredProcedureResultColumnMapping(column, resultColumn, property, storedProcedureMapping);
        }

        storedProcedureMappings.Add(storedProcedureMapping);
        storeStoredProcedure.EntityTypeMappings.Add(storedProcedureMapping);

        return storedProcedureMapping;

        static StoreStoredProcedure GetOrCreateStoreStoredProcedure(
            IRuntimeStoredProcedure storedProcedure,
            RelationalModel databaseModel,
            IRelationalTypeMappingSource relationalTypeMappingSource)
        {
            var storeStoredProcedure = (StoreStoredProcedure?)storedProcedure.StoreStoredProcedure;
            if (storeStoredProcedure == null)
            {
                storeStoredProcedure = (StoreStoredProcedure?)databaseModel.FindStoredProcedure(storedProcedure.Name, storedProcedure.Schema);
                if (storeStoredProcedure == null)
                {
                    storeStoredProcedure = new StoreStoredProcedure(storedProcedure.Name, storedProcedure.Schema, databaseModel);
                    if (storedProcedure.IsRowsAffectedReturned)
                    {
                        var typeMapping = relationalTypeMappingSource.FindMapping(typeof(int), databaseModel.Model)!;
                        storeStoredProcedure.ReturnValue = new StoreStoredProcedureReturnValue(
                            "",
                            typeMapping.StoreType,
                            storeStoredProcedure,
                            typeMapping);
                    }

                    databaseModel.StoredProcedures.Add(
                        (storeStoredProcedure.Name, storeStoredProcedure.Schema), storeStoredProcedure);
                }

                storeStoredProcedure.StoredProcedures.Add(storedProcedure);
                storedProcedure.StoreStoredProcedure = storeStoredProcedure;
            }

            return storeStoredProcedure;
        }

        static StoreStoredProcedureParameter GetOrCreateStoreStoredProcedureParameter(
            IStoredProcedureParameter parameter,
            IProperty? property,
            StoreStoredProcedure storeStoredProcedure,
            StoreObjectIdentifier identifier,
            IRelationalTypeMappingSource relationalTypeMappingSource)
        {
            var name = parameter.Name;
            var storeParameter = (StoreStoredProcedureParameter?)storeStoredProcedure.FindParameter(name);
            if (storeParameter == null)
            {
                var position = storeStoredProcedure.Parameters.Count;
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
                        parameter.Direction) { IsNullable = property.IsColumnNullable(identifier) };
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
                var position = storeStoredProcedure.ResultColumns.Count;
                if (property == null)
                {
                    var typeMapping = relationalTypeMappingSource.FindMapping(typeof(int))!;
                    column = new StoreStoredProcedureResultColumn(
                        name,
                        typeMapping.StoreType,
                        position,
                        storeStoredProcedure,
                        typeMapping);
                }
                else
                {
                    column = new StoreStoredProcedureResultColumn(
                        name,
                        property.GetColumnType(identifier),
                        position,
                        storeStoredProcedure) { IsNullable = property.IsColumnNullable(identifier) };
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
            var includeInherited = entityTypeMapping.TypeBase.GetMappingStrategy() != RelationalAnnotationNames.TphMappingStrategy;
            var entityType = (IEntityType)entityTypeMapping.TypeBase;
            foreach (var key in includeInherited ? entityType.GetKeys() : entityType.GetDeclaredKeys())
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

                GetOrCreateUniqueConstraints(key).Add(constraint);
                constraint.MappedKeys.Add(key);
            }

            foreach (var index in includeInherited ? entityType.GetIndexes() : entityType.GetDeclaredIndexes())
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

                GetOrCreateTableIndexes(index).Add(tableIndex);
                tableIndex.MappedIndexes.Add(index);
            }

            if (designTime)
            {
                foreach (var checkConstraint in includeInherited ? entityType.GetCheckConstraints() : entityType.GetDeclaredCheckConstraints())
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

            // Triggers are not inherited
            foreach (var trigger in entityType.GetDeclaredTriggers())
            {
                var name = trigger.GetDatabaseName(storeObject);
                if (name == null)
                {
                    continue;
                }

                Check.DebugAssert(trigger.GetTableName() == table.Name, "Mismatch in trigger table name");
                Check.DebugAssert(trigger.GetTableSchema() == table.Schema, "Mismatch in trigger table schema");

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
        TableMappingBase<TColumnMapping>? mainMapping = null;
        var mappedEntityTypes = new HashSet<IEntityType>();
        foreach (TableMappingBase<TColumnMapping> entityTypeMapping in table.EntityTypeMappings.ToList())
        {
            if (table.EntityTypeMappings.Count > 1)
            {
                entityTypeMapping.SetIsSharedTablePrincipal(false);
            }

            var entityType = (IEntityType)entityTypeMapping.TypeBase;
            mappedEntityTypes.Add(entityType);
            var primaryKey = entityType.FindPrimaryKey();
            if (primaryKey == null)
            {
                if (mainMapping == null
                    || entityTypeMapping.TypeBase.IsAssignableFrom(mainMapping.TypeBase))
                {
                    mainMapping = entityTypeMapping;
                }

                continue;
            }

            var foreignKeys = entityType.IsMappedToJson()
                ? new[] { entityType.FindOwnership()! }
                : entityType.FindForeignKeys(primaryKey.Properties);

            var isMainMapping = true;
            foreach (var foreignKey in foreignKeys)
            {
                // for JSON mapped entities we can have row internal FKs for collection navigations
                if ((foreignKey.IsUnique || entityType.IsMappedToJson())
                    && foreignKey.PrincipalKey.IsPrimaryKey()
                    && !foreignKey.DeclaringEntityType.IsAssignableFrom(foreignKey.PrincipalEntityType)
                    && !foreignKey.PrincipalEntityType.IsAssignableFrom(foreignKey.DeclaringEntityType)
                    && ((ITableBase)table).EntityTypeMappings.Any(m => m.TypeBase == foreignKey.PrincipalEntityType))
                {
                    isMainMapping = false;
                    table.AddRowInternalForeignKey(entityType, foreignKey);
                }
            }

            if (isMainMapping
                && (mainMapping == null
                    || entityTypeMapping.TypeBase.IsAssignableFrom(mainMapping.TypeBase)))
            {
                mainMapping = entityTypeMapping;
            }
        }

        Check.DebugAssert(
            mainMapping is not null,
            $"{nameof(mainMapping)} is neither a {nameof(TableMapping)} nor a {nameof(ViewMapping)}");

        if (table.EntityTypeMappings.Count > 1)
        {
            mainMapping.SetIsSharedTablePrincipal(true);
        }

        var referencingInternalForeignKeyMap = table.ReferencingRowInternalForeignKeys;
        if (referencingInternalForeignKeyMap != null)
        {
            var optionalTypes = new Dictionary<ITypeBase, bool>();
            var entityTypesToVisit = new Queue<(ITypeBase, bool)>();
            entityTypesToVisit.Enqueue(((IEntityType)mainMapping.TypeBase, false));

            while (entityTypesToVisit.Count > 0)
            {
                var (typeBase, optional) = entityTypesToVisit.Dequeue();
                if (optionalTypes.TryGetValue(typeBase, out var previouslyOptional)
                    && (!previouslyOptional || optional))
                {
                    continue;
                }

                optionalTypes[typeBase] = optional;

                if (typeBase is IComplexType complexType)
                {
                    var complexProperty = complexType.ComplexProperty;
                    entityTypesToVisit.Enqueue((complexProperty.DeclaringType, optional || complexProperty.IsNullable));
                    continue;
                }

                var entityType = (IEntityType)typeBase;
                if (referencingInternalForeignKeyMap.TryGetValue(entityType, out var referencingInternalForeignKeys))
                {
                    foreach (var referencingForeignKey in referencingInternalForeignKeys)
                    {
                        entityTypesToVisit.Enqueue(
                            (referencingForeignKey.DeclaringEntityType, optional || !referencingForeignKey.IsRequiredDependent));
                    }
                }

                if (table.EntityTypeMappings.Single(etm => etm.TypeBase == typeBase).IncludesDerivedTypes == true)
                {
                    foreach (var directlyDerivedEntityType in entityType.GetDirectlyDerivedTypes())
                    {
                        if (mappedEntityTypes.Contains(directlyDerivedEntityType)
                            && !optionalTypes.ContainsKey(directlyDerivedEntityType))
                        {
                            entityTypesToVisit.Enqueue((directlyDerivedEntityType, optional));
                        }
                    }
                }
            }

            table.OptionalTypes = optionalTypes;
        }
    }

    private static void PopulateForeignKeyConstraints(Table table)
    {
        var storeObject = StoreObjectIdentifier.Table(table.Name, table.Schema);
        foreach (var entityTypeMapping in ((ITable)table).EntityTypeMappings)
        {
            var entityType = (IEntityType)entityTypeMapping.TypeBase;
            var includeInherited = entityType.GetMappingStrategy() == RelationalAnnotationNames.TpcMappingStrategy;
            foreach (var foreignKey in includeInherited ? entityType.GetForeignKeys() : entityType.GetDeclaredForeignKeys())
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

                    var constraint = table.ForeignKeyConstraints.FirstOrDefault(fk => fk.Name == name);
                    if (constraint != null)
                    {
                        GetOrCreateForeignKeyConstraints(foreignKey).Add(constraint);

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

                    if (entityTypeMapping.IncludesDerivedTypes == true
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

                    GetOrCreateForeignKeyConstraints(foreignKey).Add(constraint);
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
    public static void CreateColumnMapping(
        ColumnBase<ColumnMappingBase> column,
        IProperty property,
        TableMappingBase<ColumnMappingBase> tableMapping)
    {
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

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static void CreateColumnMapping(Column column, IProperty property, TableMapping tableMapping)
    {
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

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static void CreateViewColumnMapping(ViewColumn column, IProperty property, ViewMapping viewMapping)
    {
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

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static void CreateSqlQueryColumnMapping(SqlQueryColumn column, IProperty property, SqlQueryMapping queryMapping)
    {
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

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static void CreateFunctionColumnMapping(FunctionColumn column, IProperty property, FunctionMapping functionMapping)
    {
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

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static void CreateStoredProcedureParameterMapping(
        StoreStoredProcedureParameter storeParameter,
        IStoredProcedureParameter parameter,
        IProperty property,
        StoredProcedureMapping storedProcedureMapping)
    {
        var columnMapping = new StoredProcedureParameterMapping(property, parameter, storeParameter, storedProcedureMapping);
        storedProcedureMapping.AddParameterMapping(columnMapping);
        storeParameter.AddPropertyMapping(columnMapping);

        var parameterMappingAnnotationName = storedProcedureMapping.StoredProcedureIdentifier.StoreObjectType switch
        {
            StoreObjectType.InsertStoredProcedure
                => RelationalAnnotationNames.InsertStoredProcedureParameterMappings,
            StoreObjectType.DeleteStoredProcedure
                => RelationalAnnotationNames.DeleteStoredProcedureParameterMappings,
            StoreObjectType.UpdateStoredProcedure
                => RelationalAnnotationNames.UpdateStoredProcedureParameterMappings,
            _ => throw new Exception(
                "Unexpected stored procedure type: "
                + storedProcedureMapping.StoredProcedureIdentifier.StoreObjectType)
        };

        if (property.FindRuntimeAnnotationValue(parameterMappingAnnotationName)
            is not SortedSet<StoredProcedureParameterMapping> columnMappings)
        {
            columnMappings = new SortedSet<StoredProcedureParameterMapping>(ColumnMappingBaseComparer.Instance);
            property.AddRuntimeAnnotation(parameterMappingAnnotationName, columnMappings);
        }

        columnMappings.Add(columnMapping);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static void CreateStoredProcedureResultColumnMapping(
        StoreStoredProcedureResultColumn storeResultColumn,
        IStoredProcedureResultColumn resultColumn,
        IProperty property,
        StoredProcedureMapping storedProcedureMapping)
    {
        var columnMapping = new StoredProcedureResultColumnMapping(property, resultColumn, storeResultColumn, storedProcedureMapping);
        storedProcedureMapping.AddColumnMapping(columnMapping);
        storeResultColumn.AddPropertyMapping(columnMapping);

        var columnMappingAnnotationName = storedProcedureMapping.StoredProcedureIdentifier.StoreObjectType switch
        {
            StoreObjectType.InsertStoredProcedure
                => RelationalAnnotationNames.InsertStoredProcedureResultColumnMappings,
            StoreObjectType.UpdateStoredProcedure
                => RelationalAnnotationNames.UpdateStoredProcedureResultColumnMappings,
            _ => throw new Exception(
                "Unexpected stored procedure type: "
                + storedProcedureMapping.StoredProcedureIdentifier.StoreObjectType)
        };

        if (property.FindRuntimeAnnotationValue(columnMappingAnnotationName)
            is not SortedSet<StoredProcedureResultColumnMapping> columnMappings)
        {
            columnMappings = new SortedSet<StoredProcedureResultColumnMapping>(ColumnMappingBaseComparer.Instance);
            property.AddRuntimeAnnotation(columnMappingAnnotationName, columnMappings);
        }

        columnMappings.Add(columnMapping);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static SortedSet<UniqueConstraint> GetOrCreateUniqueConstraints(IKey key)
    {
        if (key.FindRuntimeAnnotationValue(RelationalAnnotationNames.UniqueConstraintMappings)
            is not SortedSet<UniqueConstraint> uniqueConstraints)
        {
            uniqueConstraints = new SortedSet<UniqueConstraint>(UniqueConstraintComparer.Instance);
            key.AddRuntimeAnnotation(RelationalAnnotationNames.UniqueConstraintMappings, uniqueConstraints);
        }

        return uniqueConstraints;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static SortedSet<TableIndex> GetOrCreateTableIndexes(IIndex index)
    {
        if (index.FindRuntimeAnnotationValue(RelationalAnnotationNames.TableIndexMappings)
            is not SortedSet<TableIndex> tableIndexes)
        {
            tableIndexes = new SortedSet<TableIndex>(TableIndexComparer.Instance);
            index.AddRuntimeAnnotation(RelationalAnnotationNames.TableIndexMappings, tableIndexes);
        }

        return tableIndexes;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static SortedSet<ForeignKeyConstraint> GetOrCreateForeignKeyConstraints(IForeignKey foreignKey)
    {
        if (foreignKey.FindRuntimeAnnotationValue(RelationalAnnotationNames.ForeignKeyMappings)
            is not SortedSet<ForeignKeyConstraint> foreignKeyConstraints)
        {
            foreignKeyConstraints = new SortedSet<ForeignKeyConstraint>(ForeignKeyConstraintComparer.Instance);
            foreignKey.AddRuntimeAnnotation(RelationalAnnotationNames.ForeignKeyMappings, foreignKeyConstraints);
        }

        return foreignKeyConstraints;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static IKey GetKey(
        IModel model,
        string declaringEntityTypeName,
        IReadOnlyList<string> properties)
    {
        var declaringEntityType = model.FindEntityType(declaringEntityTypeName)!;

        return declaringEntityType.FindKey(properties.Select(p => declaringEntityType.FindProperty(p)!).ToArray())!;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static IIndex GetIndex(
        IModel model,
        string declaringEntityTypeName,
        string indexName)
    {
        var declaringEntityType = model.FindEntityType(declaringEntityTypeName)!;

        return declaringEntityType.FindIndex(indexName)!;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static IIndex GetIndex(
        IModel model,
        string declaringEntityTypeName,
        IReadOnlyList<string> properties)
    {
        var declaringEntityType = model.FindEntityType(declaringEntityTypeName)!;

        return declaringEntityType.FindIndex(properties.Select(p => declaringEntityType.FindProperty(p)!).ToArray())!;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static IForeignKey GetForeignKey(
        IModel model,
        string declaringEntityTypeName,
        IReadOnlyList<string> properties,
        string principalEntityTypeName,
        IReadOnlyList<string> principalProperties)
    {
        var declaringEntityType = model.FindEntityType(declaringEntityTypeName)!;
        var principalEntityType = model.FindEntityType(principalEntityTypeName)!;

        return declaringEntityType.FindForeignKey(
            properties.Select(p => declaringEntityType.FindProperty(p)!).ToArray(),
            principalEntityType.FindKey(principalProperties.Select(p => principalEntityType.FindProperty(p)!).ToArray())!,
            principalEntityType)!;
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
        get => Tables.OrderBy(t => t.Key).Select(t => t.Value);
    }

    IEnumerable<IView> IRelationalModel.Views
    {
        [DebuggerStepThrough]
        get => Views.OrderBy(v => v.Key).Select(v => v.Value);
    }

    IEnumerable<IStoreFunction> IRelationalModel.Functions
    {
        [DebuggerStepThrough]
        get => Functions.OrderBy(f => f.Key).Select(t => t.Value);
    }

    IEnumerable<IStoreStoredProcedure> IRelationalModel.StoredProcedures
    {
        [DebuggerStepThrough]
        get => StoredProcedures.OrderBy(p => p.Key).Select(t => t.Value);
    }

    IEnumerable<ISqlQuery> IRelationalModel.Queries
    {
        [DebuggerStepThrough]
        get => Queries.OrderBy(q => q.Key).Select(t => t.Value);
    }
}
