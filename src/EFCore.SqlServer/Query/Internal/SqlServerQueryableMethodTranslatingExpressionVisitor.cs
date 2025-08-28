// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.SqlServer.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Metadata.Internal;
using Microsoft.VisualBasic;

namespace Microsoft.EntityFrameworkCore.SqlServer.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class SqlServerQueryableMethodTranslatingExpressionVisitor : RelationalQueryableMethodTranslatingExpressionVisitor
{
    private readonly SqlServerQueryCompilationContext _queryCompilationContext;
    private readonly IRelationalTypeMappingSource _typeMappingSource;
    private readonly ISqlExpressionFactory _sqlExpressionFactory;
    private readonly ISqlServerSingletonOptions _sqlServerSingletonOptions;

    private HashSet<ColumnExpression>? _columnsWithMultipleSetters;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SqlServerQueryableMethodTranslatingExpressionVisitor(
        QueryableMethodTranslatingExpressionVisitorDependencies dependencies,
        RelationalQueryableMethodTranslatingExpressionVisitorDependencies relationalDependencies,
        SqlServerQueryCompilationContext queryCompilationContext,
        ISqlServerSingletonOptions sqlServerSingletonOptions)
        : base(dependencies, relationalDependencies, queryCompilationContext)
    {
        _queryCompilationContext = queryCompilationContext;
        _typeMappingSource = relationalDependencies.TypeMappingSource;
        _sqlExpressionFactory = relationalDependencies.SqlExpressionFactory;
        _sqlServerSingletonOptions = sqlServerSingletonOptions;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected SqlServerQueryableMethodTranslatingExpressionVisitor(
        SqlServerQueryableMethodTranslatingExpressionVisitor parentVisitor)
        : base(parentVisitor)
    {
        _queryCompilationContext = parentVisitor._queryCompilationContext;
        _typeMappingSource = parentVisitor._typeMappingSource;
        _sqlExpressionFactory = parentVisitor._sqlExpressionFactory;
        _sqlServerSingletonOptions = parentVisitor._sqlServerSingletonOptions;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override QueryableMethodTranslatingExpressionVisitor CreateSubqueryVisitor()
        => new SqlServerQueryableMethodTranslatingExpressionVisitor(this);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitExtension(Expression extensionExpression)
    {
        if (extensionExpression is TemporalQueryRootExpression queryRootExpression)
        {
            var selectExpression = CreateSelect(queryRootExpression.EntityType);
            Func<TableExpression, TableExpressionBase> annotationApplyingFunc = queryRootExpression switch
            {
                TemporalAllQueryRootExpression => te => te
                    .AddAnnotation(SqlServerAnnotationNames.TemporalOperationType, TemporalOperationType.All),
                TemporalAsOfQueryRootExpression asOf => te => te
                    .AddAnnotation(SqlServerAnnotationNames.TemporalOperationType, TemporalOperationType.AsOf)
                    .AddAnnotation(SqlServerAnnotationNames.TemporalAsOfPointInTime, asOf.PointInTime),
                TemporalBetweenQueryRootExpression between => te => te
                    .AddAnnotation(SqlServerAnnotationNames.TemporalOperationType, TemporalOperationType.Between)
                    .AddAnnotation(SqlServerAnnotationNames.TemporalRangeOperationFrom, between.From)
                    .AddAnnotation(SqlServerAnnotationNames.TemporalRangeOperationTo, between.To),
                TemporalContainedInQueryRootExpression containedIn => te => te
                    .AddAnnotation(SqlServerAnnotationNames.TemporalOperationType, TemporalOperationType.ContainedIn)
                    .AddAnnotation(SqlServerAnnotationNames.TemporalRangeOperationFrom, containedIn.From)
                    .AddAnnotation(SqlServerAnnotationNames.TemporalRangeOperationTo, containedIn.To),
                TemporalFromToQueryRootExpression fromTo => te => te
                    .AddAnnotation(SqlServerAnnotationNames.TemporalOperationType, TemporalOperationType.FromTo)
                    .AddAnnotation(SqlServerAnnotationNames.TemporalRangeOperationFrom, fromTo.From)
                    .AddAnnotation(SqlServerAnnotationNames.TemporalRangeOperationTo, fromTo.To),
                _ => throw new InvalidOperationException(queryRootExpression.Print()),
            };

            selectExpression = (SelectExpression)new TemporalAnnotationApplyingExpressionVisitor(annotationApplyingFunc)
                .Visit(selectExpression);

            return new ShapedQueryExpression(
                selectExpression,
                new RelationalStructuralTypeShaperExpression(
                    queryRootExpression.EntityType,
                    new ProjectionBindingExpression(
                        selectExpression,
                        new ProjectionMember(),
                        typeof(ValueBuffer)),
                    false));
        }

        return base.VisitExtension(extensionExpression);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override ShapedQueryExpression? TranslatePrimitiveCollection(
        SqlExpression sqlExpression,
        IProperty? property,
        string tableAlias)
    {
        if (_sqlServerSingletonOptions.EngineType == SqlServerEngineType.SqlServer
            && _sqlServerSingletonOptions.SqlServerCompatibilityLevel < 130)
        {
            AddTranslationErrorDetails(
                SqlServerStrings.CompatibilityLevelTooLowForScalarCollections(_sqlServerSingletonOptions.SqlServerCompatibilityLevel));

            return null;
        }

        if (_sqlServerSingletonOptions.EngineType == SqlServerEngineType.AzureSql
            && _sqlServerSingletonOptions.AzureSqlCompatibilityLevel < 130)
        {
            AddTranslationErrorDetails(
                SqlServerStrings.CompatibilityLevelTooLowForScalarCollections(_sqlServerSingletonOptions.AzureSqlCompatibilityLevel));

            return null;
        }

        // Generate the OPENJSON function expression, and wrap it in a SelectExpression.

        // Note that where the elementTypeMapping is known (i.e. collection columns), we immediately generate OPENJSON with a WITH clause
        // (i.e. with a columnInfo), which determines the type conversion to apply to the JSON elements coming out.
        // For parameter collections, the element type mapping will only be inferred and applied later (see
        // SqlServerInferredTypeMappingApplier below), at which point the we'll apply it to add the WITH clause.
        var elementTypeMapping = (RelationalTypeMapping?)sqlExpression.TypeMapping?.ElementTypeMapping;

        var openJsonExpression = elementTypeMapping is null
            ? new SqlServerOpenJsonExpression(tableAlias, sqlExpression)
            : new SqlServerOpenJsonExpression(
                tableAlias, sqlExpression,
                columnInfos:
                [
                    new SqlServerOpenJsonExpression.ColumnInfo
                    {
                        Name = "value",
                        TypeMapping = elementTypeMapping,
                        Path = []
                    }
                ]);

        var elementClrType = sqlExpression.Type.GetSequenceType();

        // If this is a collection property, get the element's nullability out of metadata. Otherwise, this is a parameter property, in
        // which case we only have the CLR type (note that we cannot produce different SQLs based on the nullability of an *element* in
        // a parameter collection - our caching mechanism only supports varying by the nullability of the parameter itself (i.e. the
        // collection).
        var isElementNullable = property?.GetElementType()!.IsNullable;

        var keyColumnTypeMapping = _typeMappingSource.FindMapping("nvarchar(4000)")!;
#pragma warning disable EF1001 // Internal EF Core API usage.
        var selectExpression = new SelectExpression(
            [openJsonExpression],
            new ColumnExpression(
                "value",
                tableAlias,
                elementClrType.UnwrapNullableType(),
                elementTypeMapping,
                isElementNullable ?? elementClrType.IsNullableType()),
            identifier:
            [
                (new ColumnExpression("key", tableAlias, typeof(string), keyColumnTypeMapping, nullable: false),
                    keyColumnTypeMapping.Comparer)
            ],
            _queryCompilationContext.SqlAliasManager);
#pragma warning restore EF1001 // Internal EF Core API usage.

        // OPENJSON doesn't guarantee the ordering of the elements coming out; when using OPENJSON without WITH, a [key] column is returned
        // with the JSON array's ordering, which we can ORDER BY; this option doesn't exist with OPENJSON with WITH, unfortunately.
        // However, OPENJSON with WITH has better performance, and also applies JSON-specific conversions we cannot be done otherwise
        // (e.g. OPENJSON with WITH does base64 decoding for VARBINARY).
        // Here we generate OPENJSON with WITH, but also add an ordering by [key] - this is a temporary invalid representation.
        // In SqlServerQueryTranslationPostprocessor, we'll post-process the expression; if the ORDER BY was stripped (e.g. because of
        // IN, EXISTS or a set operation), we'll just leave the OPENJSON with WITH. If not, we'll convert the OPENJSON with WITH to an
        // OPENJSON without WITH.
        // Note that the OPENJSON 'key' column is an nvarchar - we convert it to an int before sorting.
        selectExpression.AppendOrdering(
            new OrderingExpression(
                _sqlExpressionFactory.Convert(
                    selectExpression.CreateColumnExpression(
                        openJsonExpression,
                        "key",
                        typeof(string),
                        typeMapping: _typeMappingSource.FindMapping("nvarchar(4000)"),
                        columnNullable: false),
                    typeof(int),
                    _typeMappingSource.FindMapping(typeof(int))),
                ascending: true));

        var shaperExpression = (Expression)new ProjectionBindingExpression(
            selectExpression, new ProjectionMember(), elementClrType.MakeNullable());
        if (shaperExpression.Type != elementClrType)
        {
            Check.DebugAssert(
                elementClrType.MakeNullable() == shaperExpression.Type,
                "expression.Type must be nullable of targetType");

            shaperExpression = Expression.Convert(shaperExpression, elementClrType);
        }

        return new ShapedQueryExpression(selectExpression, shaperExpression);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override ShapedQueryExpression TransformJsonQueryToTable(JsonQueryExpression jsonQueryExpression)
    {
        var structuralType = jsonQueryExpression.StructuralType;

        // Calculate the table alias for the OPENJSON expression based on the last named path segment
        // (or the JSON column name if there are none)
        var lastNamedPathSegment = jsonQueryExpression.Path.LastOrDefault(ps => ps.PropertyName is not null);
        var tableAlias =
            _queryCompilationContext.SqlAliasManager.GenerateTableAlias(
                lastNamedPathSegment.PropertyName ?? jsonQueryExpression.JsonColumn.Name);

        // We now add all of projected entity's the properties and navigations into the OPENJSON's WITH clause. Note that navigations
        // get AS JSON, which projects out the JSON sub-document for them as text, which can be further navigated into.
        var columnInfos = new List<SqlServerOpenJsonExpression.ColumnInfo>();

        // We're only interested in properties which actually exist in the JSON, filter out uninteresting shadow keys
        // (for owned JSON entities)
        foreach (var property in structuralType.GetPropertiesInHierarchy())
        {
            if (property.GetJsonPropertyName() is { } jsonPropertyName)
            {
                columnInfos.Add(
                    new SqlServerOpenJsonExpression.ColumnInfo
                    {
                        Name = jsonPropertyName,
                        TypeMapping = property.GetRelationalTypeMapping(),
                        Path = [new PathSegment(jsonPropertyName)],
                        AsJson = property.GetRelationalTypeMapping().ElementTypeMapping is not null
                    });
            }
        }

        // Find the container column in the relational model to get its type mapping
        // Note that we assume exactly one column with the given name mapped to the entity (despite entity splitting).
        // See #36647 and #36646 about improving this.
        var containerColumnName = structuralType.GetContainerColumnName();
        var containerColumn = structuralType.ContainingEntityType.GetTableMappings()
            .SelectMany(m => m.Table.Columns)
            .Where(c => c.Name == containerColumnName)
            .Single();

        var nestedJsonPropertyNames = jsonQueryExpression.StructuralType switch
        {
            IEntityType entityType
                => entityType.GetNavigationsInHierarchy()
                    .Where(n => n.ForeignKey.IsOwnership
                        && n.TargetEntityType.IsMappedToJson()
                        && n.ForeignKey.PrincipalToDependent == n)
                    .Select(n => n.TargetEntityType.GetJsonPropertyName() ?? throw new UnreachableException()),

            IComplexType complexType
                => complexType.GetComplexProperties().Select(p => p.ComplexType.GetJsonPropertyName() ?? throw new UnreachableException()),

            _ => throw new UnreachableException()
        };

        foreach (var jsonPropertyName in nestedJsonPropertyNames)
        {
            columnInfos.Add(
                new SqlServerOpenJsonExpression.ColumnInfo
                {
                    Name = jsonPropertyName,
                    TypeMapping = containerColumn.StoreTypeMapping,
                    Path = [new PathSegment(jsonPropertyName)],
                    AsJson = true
                });
        }

        var openJsonExpression = new SqlServerOpenJsonExpression(
            tableAlias, jsonQueryExpression.JsonColumn, jsonQueryExpression.Path, columnInfos);

#pragma warning disable EF1001 // Internal EF Core API usage.
        var selectExpression = CreateSelect(
            jsonQueryExpression,
            openJsonExpression,
            "key",
            typeof(string),
            _typeMappingSource.FindMapping("nvarchar(4000)")!);
#pragma warning restore EF1001 // Internal EF Core API usage.

        // See note on OPENJSON and ordering in TranslateCollection
        selectExpression.AppendOrdering(
            new OrderingExpression(
                _sqlExpressionFactory.Convert(
                    selectExpression.CreateColumnExpression(
                        openJsonExpression,
                        "key",
                        typeof(string),
                        typeMapping: _typeMappingSource.FindMapping("nvarchar(4000)"),
                        columnNullable: false),
                    typeof(int),
                    _typeMappingSource.FindMapping(typeof(int))),
                ascending: true));

        return new ShapedQueryExpression(
            selectExpression,
            new RelationalStructuralTypeShaperExpression(
                jsonQueryExpression.StructuralType,
                new ProjectionBindingExpression(
                    selectExpression,
                    new ProjectionMember(),
                    typeof(ValueBuffer)),
                false));
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override ShapedQueryExpression? TranslateElementAtOrDefault(
        ShapedQueryExpression source,
        Expression index,
        bool returnDefault)
    {
        if (!returnDefault)
        {
            switch (source.QueryExpression)
            {
                // index on parameter using a column
                // translate via JSON because it is a better translation
                case SelectExpression
                {
                    Tables: [ValuesExpression { ValuesParameter: { } valuesParameter }],
                    Predicate: null,
                    GroupBy: [],
                    Having: null,
                    IsDistinct: false,
#pragma warning disable EF1001
                    Orderings: [{ Expression: ColumnExpression { Name: ValuesOrderingColumnName }, IsAscending: true }],
#pragma warning restore EF1001
                    Limit: null,
                    Offset: null
                } selectExpression
                    when TranslateExpression(index) is { } translatedIndex
                    && _sqlServerSingletonOptions.SupportsJsonFunctions
                    && TryTranslate(selectExpression, valuesParameter, translatedIndex, out var result):
                    return result;

                // Index on JSON array
                case SelectExpression
                {
                    Tables: [SqlServerOpenJsonExpression { Arguments: [var jsonArrayColumn] } openJsonExpression],
                    Predicate: null,
                    GroupBy: [],
                    Having: null,
                    IsDistinct: false,
                    Limit: null,
                    Offset: null,
                    // We can only apply the indexing if the JSON array is ordered by its natural ordered, i.e. by the "key" column that
                    // we created in TranslateCollection. For example, if another ordering has been applied (e.g. by the JSON elements
                    // themselves), we can no longer simply index into the original array.
                    Orderings:
                        [
                        {
                            Expression: SqlUnaryExpression
                            {
                                OperatorType: ExpressionType.Convert,
                                Operand: ColumnExpression { Name: "key", TableAlias: var orderingTableAlias }
                            }
                        }
                        ]
                } selectExpression
                    when orderingTableAlias == openJsonExpression.Alias
                    && TranslateExpression(index) is { } translatedIndex
                    && TryTranslate(selectExpression, jsonArrayColumn, translatedIndex, out var result):
                    return result;
            }
        }

        return base.TranslateElementAtOrDefault(source, index, returnDefault);

        bool TryTranslate(
            SelectExpression selectExpression,
            SqlExpression jsonArrayColumn,
            SqlExpression translatedIndex,
            [NotNullWhen(true)] out ShapedQueryExpression? result)
        {
            // Extract the column projected out of the source, and simplify the subquery to a simple JsonScalarExpression
            if (!TryGetProjection(source, selectExpression, out var projection))
            {
                result = null;
                return false;
            }

            // OPENJSON's value column is an nvarchar(max); if this is a collection column whose type mapping is know, the projection
            // contains a CAST node which we unwrap
            var projectionColumn = projection switch
            {
                ColumnExpression c => c,
                SqlUnaryExpression { OperatorType: ExpressionType.Convert, Operand: ColumnExpression c } => c,
                _ => null
            };

            if (projectionColumn is null)
            {
                result = null;
                return false;
            }

            // If the inner expression happens to itself be a JsonScalarExpression, simply append the two paths to avoid creating
            // JSON_VALUE within JSON_VALUE.
            var (json, path) = jsonArrayColumn is JsonScalarExpression innerJsonScalarExpression
                ? (innerJsonScalarExpression.Json,
                    innerJsonScalarExpression.Path.Append(new PathSegment(translatedIndex)).ToArray())
                : (jsonArrayColumn, new PathSegment[] { new(translatedIndex) });

            var translation = new JsonScalarExpression(
                json,
                path,
                projection.Type,
                projection.TypeMapping,
                projectionColumn.IsNullable);

#pragma warning disable EF1001
            result = source.UpdateQueryExpression(new SelectExpression(translation, _queryCompilationContext.SqlAliasManager));
#pragma warning restore EF1001
            return true;
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override bool IsNaturallyOrdered(SelectExpression selectExpression)
        => selectExpression is
        {
            Tables: [SqlServerOpenJsonExpression openJsonExpression, ..],
            Orderings:
                [
                {
                    Expression: SqlUnaryExpression
                    {
                        OperatorType: ExpressionType.Convert,
                        Operand: ColumnExpression { Name: "key", TableAlias: var orderingTableAlias }
                    },
                    IsAscending: true
                }
                ]
        }
            && orderingTableAlias == openJsonExpression.Alias;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override bool IsValidSelectExpressionForExecuteDelete(SelectExpression selectExpression)
        => selectExpression.Offset == null
            && selectExpression.GroupBy.Count == 0
            && selectExpression.Having == null
            && selectExpression.Orderings.Count == 0;

    #region ExecuteUpdate

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override bool IsValidSelectExpressionForExecuteUpdate(
        SelectExpression selectExpression,
        TableExpressionBase table,
        [NotNullWhen(true)] out TableExpression? tableExpression)
    {
        if (selectExpression is
            {
                Offset: null,
                IsDistinct: false,
                GroupBy: [],
                Having: null,
                Orderings: []
            })
        {
            if (selectExpression.Tables.Count > 1 && table is JoinExpressionBase joinExpressionBase)
            {
                table = joinExpressionBase.Table;
            }

            if (table is TableExpression te)
            {
                tableExpression = te;
                return true;
            }
        }

        tableExpression = null;
        return false;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
#pragma warning disable EF1001 // Internal EF Core API usage.
    protected override bool TryTranslateSetters(
        ShapedQueryExpression source,
        IReadOnlyList<ExecuteUpdateSetter> setters,
        [NotNullWhen(true)] out IReadOnlyList<ColumnValueSetter>? columnSetters,
        [NotNullWhen(true)] out TableExpressionBase? targetTable)
    {
        // SQL Server 2025 introduced the modify method (https://learn.microsoft.com/sql/t-sql/data-types/json-data-type#modify-method),
        // which works only with the JSON data type introduced in that same version.
        // As of now, modify is only usable if a single property is being modified in the JSON document - it's impossible to modify multiple properties.
        // To work around this limitation, we do a first translation pass which may generate multiple modify invocations on the same JSON column (and
        // which would fail if sent to SQL Server); we then detect this case, populate _columnsWithMultipleSetters with the problematic columns, and then
        // retranslate, using the less efficient JSON_MODIFY() instead for those columns.
        _columnsWithMultipleSetters = new();

        if (!base.TryTranslateSetters(source, setters, out columnSetters, out targetTable))
        {
            return false;
        }

        _columnsWithMultipleSetters = new(columnSetters.GroupBy(s => s.Column).Where(g => g.Count() > 1).Select(g => g.Key));
        if (_columnsWithMultipleSetters.Count > 0)
        {
            return base.TryTranslateSetters(source, setters, out columnSetters, out targetTable);
        }

        return true;
    }
#pragma warning restore EF1001 // Internal EF Core API usage.

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override SqlExpression? GenerateJsonPartialUpdateSetter(
        Expression target,
        SqlExpression value,
        ref SqlExpression? existingSetterValue)
    {
        var (jsonColumn, path) = target switch
        {
            JsonScalarExpression j => ((ColumnExpression)j.Json, j.Path),
            JsonQueryExpression j => (j.JsonColumn, j.Path),

            _ => throw new UnreachableException(),
        };

        // SQL Server 2025 introduced the modify method (https://learn.microsoft.com/sql/t-sql/data-types/json-data-type#modify-method),
        // which works only with the JSON data type introduced in that same version.
        // As of now, modify is only usable if a single property is being modified in the JSON document - it's impossible to modify multiple properties.
        // To work around this limitation, we do a first translation pass which may generate multiple modify invocations on the same JSON column (and
        // which would fail if sent to SQL Server); we then detect this case in TranslateExecuteUpdate, populate _columnsWithMultipleSetters with the
        // problematic columns, and then retranslate, using the less efficient JSON_MODIFY() instead for those columns.
        if (jsonColumn.TypeMapping!.StoreType is "json"
            && (_columnsWithMultipleSetters is null || !_columnsWithMultipleSetters.Contains(jsonColumn)))
        {
            // UPDATE ... SET [x].modify('$.a.b', 'foo')

            // Note that the actual SQL generated contains only the modify function: UPDATE ... SET [x].modify(...), but UpdateExpression's
            // ColumnValueSetter requires both column and value. The column will be ignored in SQL generation,
            // and only the function call will be rendered.
            var setterValue = _sqlExpressionFactory.Function(
                existingSetterValue ?? jsonColumn,
                "modify",
                [
                    // Hack: Rendering of JSONPATH strings happens in value generation. We can have a special expression for modify to hold the
                    // IReadOnlyList<PathSegment> (just like Json{Scalar,Query}Expression), but instead we do the slight hack of packaging it
                    // as a constant argument; it will be unpacked and handled in SQL generation.
                    _sqlExpressionFactory.Constant(path, RelationalTypeMapping.NullMapping),

                // If an inline JSON object (complex type) is being assigned, it would be rendered here as a simple string:
                // [column].modify('$.foo', '{ "x": 8 }')
                // Since it's untyped, modify would treat is as a string rather than a JSON object, and insert it as such into
                // the enclosing object, escaping all the special JSON characters - that's not what we want.
                // We add a cast to JSON to have it interpreted as a JSON object.
                value is SqlConstantExpression { TypeMapping.StoreType: "json" }
                    ? _sqlExpressionFactory.Convert(value, value.Type, _typeMappingSource.FindMapping("json")!)
                    : value
                ],
                nullable: true,
                instancePropagatesNullability: true,
                argumentsPropagateNullability: [true, true],
                typeof(void),
                RelationalTypeMapping.NullMapping);

            return setterValue;
        }

        Check.DebugAssert(existingSetterValue is null or SqlFunctionExpression { Name: "JSON_MODIFY" });

        var jsonModify = _sqlExpressionFactory.Function(
            "JSON_MODIFY",
            arguments:
            [
                existingSetterValue ?? jsonColumn,
                // Hack: Rendering of JSONPATH strings happens in value generation. We can have a special expression for modify to hold the
                // IReadOnlyList<PathSegment> (just like Json{Scalar,Query}Expression), but instead we do the slight hack of packaging it
                // as a constant argument; it will be unpacked and handled in SQL generation.
                _sqlExpressionFactory.Constant(path, RelationalTypeMapping.NullMapping),
                // JSON_MODIFY by default assumes nvarchar(max) is text and escapes it.
                // In order to set a JSON fragment (for nested JSON objects), we need to wrap the JSON text with JSON_QUERY(), which makes
                // JSON_MODIFY understand that it's JSON content and prevents escaping.
                target is JsonQueryExpression && value is not JsonScalarExpression
                    ? _sqlExpressionFactory.Function("JSON_QUERY", [value], nullable: true, argumentsPropagateNullability: [true], typeof(string), value.TypeMapping)
                    : value
            ],
            nullable: true,
            argumentsPropagateNullability: [true, true, true],
            typeof(string),
            jsonColumn.TypeMapping);

        if (existingSetterValue is null)
        {
            return jsonModify;
        }
        else
        {
            existingSetterValue = jsonModify;
            return null;
        }
    }

    #endregion ExecuteUpdate

    private bool TryGetProjection(
        ShapedQueryExpression shapedQueryExpression,
        SelectExpression selectExpression,
        [NotNullWhen(true)] out SqlExpression? projection)
    {
        var shaperExpression = shapedQueryExpression.ShaperExpression;
        // No need to check ConvertChecked since this is convert node which we may have added during projection
        if (shaperExpression is UnaryExpression { NodeType: ExpressionType.Convert } unaryExpression
            && unaryExpression.Operand.Type.IsNullableType()
            && unaryExpression.Operand.Type.UnwrapNullableType() == unaryExpression.Type)
        {
            shaperExpression = unaryExpression.Operand;
        }

        if (shaperExpression is ProjectionBindingExpression projectionBindingExpression
            && selectExpression.GetProjection(projectionBindingExpression) is SqlExpression sqlExpression)
        {
            projection = sqlExpression;
            return true;
        }

        projection = null;
        return false;
    }

    private sealed class TemporalAnnotationApplyingExpressionVisitor(Func<TableExpression, TableExpressionBase> annotationApplyingFunc)
        : ExpressionVisitor
    {
        [return: NotNullIfNotNull(nameof(expression))]
        public override Expression? Visit(Expression? expression)
            => expression is TableExpression tableExpression
                ? annotationApplyingFunc(tableExpression)
                : base.Visit(expression);
    }
}
