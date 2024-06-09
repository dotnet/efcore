// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Sqlite.Internal;
using Microsoft.EntityFrameworkCore.Sqlite.Query.SqlExpressions.Internal;
using Microsoft.EntityFrameworkCore.Sqlite.Storage.Internal;

namespace Microsoft.EntityFrameworkCore.Sqlite.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class SqliteQueryableMethodTranslatingExpressionVisitor : RelationalQueryableMethodTranslatingExpressionVisitor
{
    private readonly IRelationalTypeMappingSource _typeMappingSource;
    private readonly SqliteSqlExpressionFactory _sqlExpressionFactory;
    private readonly bool _areJsonFunctionsSupported;

    private static readonly bool UseOldBehavior33932 =
        AppContext.TryGetSwitch("Microsoft.EntityFrameworkCore.Issue33932", out var enabled33932) && enabled33932;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SqliteQueryableMethodTranslatingExpressionVisitor(
        QueryableMethodTranslatingExpressionVisitorDependencies dependencies,
        RelationalQueryableMethodTranslatingExpressionVisitorDependencies relationalDependencies,
        QueryCompilationContext queryCompilationContext)
        : base(dependencies, relationalDependencies, queryCompilationContext)
    {
        _typeMappingSource = relationalDependencies.TypeMappingSource;
        _sqlExpressionFactory = (SqliteSqlExpressionFactory)relationalDependencies.SqlExpressionFactory;

        _areJsonFunctionsSupported = new Version(new SqliteConnection().ServerVersion) >= new Version(3, 38);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected SqliteQueryableMethodTranslatingExpressionVisitor(
        SqliteQueryableMethodTranslatingExpressionVisitor parentVisitor)
        : base(parentVisitor)
    {
        _typeMappingSource = parentVisitor._typeMappingSource;
        _sqlExpressionFactory = parentVisitor._sqlExpressionFactory;

        _areJsonFunctionsSupported = parentVisitor._areJsonFunctionsSupported;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override QueryableMethodTranslatingExpressionVisitor CreateSubqueryVisitor()
        => new SqliteQueryableMethodTranslatingExpressionVisitor(this);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override ShapedQueryExpression? TranslateAny(ShapedQueryExpression source, LambdaExpression? predicate)
    {
        // Simplify x.Array.Any() => json_array_length(x.Array) > 0 instead of WHERE EXISTS (SELECT 1 FROM json_each(x.Array))
        if (predicate is null
            && source.QueryExpression is SelectExpression
            {
                Tables: [TableValuedFunctionExpression { Name: "json_each", Schema: null, IsBuiltIn: true, Arguments: [var array] }],
                GroupBy: [],
                Having: null,
                IsDistinct: false,
                Limit: null,
                Offset: null
            } subquery
            && (UseOldBehavior33932 || subquery.Predicate is null))
        {
            var translation =
                _sqlExpressionFactory.GreaterThan(
                    _sqlExpressionFactory.Function(
                        "json_array_length",
                        new[] { array },
                        nullable: true,
                        argumentsPropagateNullability: new[] { true },
                        typeof(int)),
                    _sqlExpressionFactory.Constant(0));

            return source.UpdateQueryExpression(_sqlExpressionFactory.Select(translation));
        }

        return base.TranslateAny(source, predicate);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override ShapedQueryExpression? TranslateOrderBy(
        ShapedQueryExpression source,
        LambdaExpression keySelector,
        bool ascending)
    {
        var translation = base.TranslateOrderBy(source, keySelector, ascending);
        if (translation == null)
        {
            return null;
        }

        var orderingExpression = ((SelectExpression)translation.QueryExpression).Orderings.Last();
        var orderingExpressionType = GetProviderType(orderingExpression.Expression);
        if (orderingExpressionType == typeof(DateTimeOffset)
            || orderingExpressionType == typeof(decimal)
            || orderingExpressionType == typeof(TimeSpan)
            || orderingExpressionType == typeof(ulong))
        {
            throw new NotSupportedException(
                SqliteStrings.OrderByNotSupported(orderingExpressionType.ShortDisplayName()));
        }

        return translation;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override ShapedQueryExpression? TranslateThenBy(
        ShapedQueryExpression source,
        LambdaExpression keySelector,
        bool ascending)
    {
        var translation = base.TranslateThenBy(source, keySelector, ascending);
        if (translation == null)
        {
            return null;
        }

        var orderingExpression = ((SelectExpression)translation.QueryExpression).Orderings.Last();
        var orderingExpressionType = GetProviderType(orderingExpression.Expression);
        if (orderingExpressionType == typeof(DateTimeOffset)
            || orderingExpressionType == typeof(decimal)
            || orderingExpressionType == typeof(TimeSpan)
            || orderingExpressionType == typeof(ulong))
        {
            throw new NotSupportedException(
                SqliteStrings.OrderByNotSupported(orderingExpressionType.ShortDisplayName()));
        }

        return translation;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override ShapedQueryExpression? TranslateCount(ShapedQueryExpression source, LambdaExpression? predicate)
    {
        // Simplify x.Array.Count() => json_array_length(x.Array) instead of SELECT COUNT(*) FROM json_each(x.Array)
        if (predicate is null
            && source.QueryExpression is SelectExpression
            {
                Tables: [TableValuedFunctionExpression { Name: "json_each", Schema: null, IsBuiltIn: true, Arguments: [var array] }],
                GroupBy: [],
                Having: null,
                IsDistinct: false,
                Limit: null,
                Offset: null
            } subquery
            && (UseOldBehavior33932 || subquery.Predicate is null))
        {
            var translation = _sqlExpressionFactory.Function(
                "json_array_length",
                new[] { array },
                nullable: true,
                argumentsPropagateNullability: new[] { true },
                typeof(int));

            return source.UpdateQueryExpression(_sqlExpressionFactory.Select(translation));
        }

        return base.TranslateCount(source, predicate);
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
        // Support for JSON functions (e.g. json_each) was added in Sqlite 3.38.0 (2022-02-22, see https://www.sqlite.org/json1.html).
        // This determines whether we have json_each, which is needed to query into JSON columns.
        if (!_areJsonFunctionsSupported)
        {
            AddTranslationErrorDetails(SqliteStrings.QueryingIntoJsonCollectionsNotSupported(new SqliteConnection().ServerVersion));

            return null;
        }

        var elementTypeMapping = (RelationalTypeMapping?)sqlExpression.TypeMapping?.ElementTypeMapping;
        var elementClrType = sqlExpression.Type.GetSequenceType();
        var jsonEachExpression = new JsonEachExpression(tableAlias, sqlExpression);

        // If this is a collection property, get the element's nullability out of metadata. Otherwise, this is a parameter property, in
        // which case we only have the CLR type (note that we cannot produce different SQLs based on the nullability of an *element* in
        // a parameter collection - our caching mechanism only supports varying by the nullability of the parameter itself (i.e. the
        // collection).
        var isElementNullable = property?.GetElementType()!.IsNullable;

#pragma warning disable EF1001 // Internal EF Core API usage.
        var selectExpression = new SelectExpression(
            jsonEachExpression,
            columnName: "value",
            columnType: elementClrType,
            columnTypeMapping: elementTypeMapping,
            isElementNullable,
            identifierColumnName: "key",
            identifierColumnType: typeof(int),
            identifierColumnTypeMapping: _typeMappingSource.FindMapping(typeof(int)));
#pragma warning restore EF1001 // Internal EF Core API usage.

        // If we have a collection column, we know the type mapping at this point (as opposed to parameters, whose type mapping will get
        // inferred later based on usage in SqliteInferredTypeMappingApplier); we should be able to apply any SQL logic needed to convert
        // the JSON value out to its relational counterpart (e.g. datetime() for timestamps, see ApplyJsonSqlConversion).
        //
        // However, doing it here would interfere with pattern matching in e.g. TranslateElementAtOrDefault, where we specifically check
        // for a bare column being projected out of the table - if the user composed any operators over the collection, it's no longer
        // possible to apply a specialized translation via the -> operator. We could add a way to recognize the special conversions we
        // compose on top, but instead of going into that complexity, we'll just apply the SQL conversion later, in
        // SqliteInferredTypeMappingApplier, as if we had a parameter collection.

        // Append an ordering for the json_each 'key' column.
        selectExpression.AppendOrdering(
            new OrderingExpression(
                selectExpression.CreateColumnExpression(
                    jsonEachExpression,
                    "key",
                    typeof(int),
                    typeMapping: _typeMappingSource.FindMapping(typeof(int)),
                    columnNullable: false),
                ascending: true));

        Expression shaperExpression = new ProjectionBindingExpression(
            selectExpression, new ProjectionMember(), elementClrType.MakeNullable());

        if (elementClrType != shaperExpression.Type)
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
        var entityType = jsonQueryExpression.EntityType;
        var textTypeMapping = _typeMappingSource.FindMapping(typeof(string));

        // TODO: Refactor this out
        // Calculate the table alias for the json_each expression based on the last named path segment
        // (or the JSON column name if there are none)
        var lastNamedPathSegment = jsonQueryExpression.Path.LastOrDefault(ps => ps.PropertyName is not null);
        var tableAlias = char.ToLowerInvariant((lastNamedPathSegment.PropertyName ?? jsonQueryExpression.JsonColumn.Name)[0]).ToString();

        // Handling a non-primitive JSON array is complicated on SQLite; unlike SQL Server OPENJSON and PostgreSQL jsonb_to_recordset,
        // SQLite's json_each can only project elements of the array, and not properties within those elements. For example:
        // SELECT value FROM json_each('[{"a":1,"b":"foo"}, {"a":2,"b":"bar"}]')
        // This will return two rows, each with a string column representing an array element (i.e. {"a":1,"b":"foo"}). To decompose that
        // into a and b columns, a further extraction is needed:
        // SELECT value ->> 'a' AS a, value ->> 'b' AS b FROM json_each('[{"a":1,"b":"foo"}, {"a":2,"b":"bar"}]')

        // We therefore generate a minimal subquery projecting out all the properties and navigations, wrapped by a SelectExpression
        // containing that:
        // SELECT ...
        // FROM (SELECT value ->> 'a' AS a, value ->> 'b' AS b FROM json_each(<JSON column>, <path>)) AS j
        // WHERE j.a = 8;

        // Unfortunately, while the subquery projects the entity, our EntityProjectionExpression currently supports only bare
        // ColumnExpression (the above requires JsonScalarExpression). So we hack as if the subquery projects an anonymous type instead,
        // with a member for each JSON property that needs to be projected. We then wrap it with a SelectExpression the projects a proper
        // EntityProjectionExpression.

        var jsonEachExpression = new JsonEachExpression(tableAlias, jsonQueryExpression.JsonColumn, jsonQueryExpression.Path);

#pragma warning disable EF1001 // Internal EF Core API usage.
        var selectExpression = new SelectExpression(
            jsonQueryExpression,
            jsonEachExpression,
            "key",
            typeof(int),
            _typeMappingSource.FindMapping(typeof(int))!);
#pragma warning restore EF1001 // Internal EF Core API usage.

        selectExpression.AppendOrdering(
            new OrderingExpression(
                selectExpression.CreateColumnExpression(
                    jsonEachExpression,
                    "key",
                    typeof(int),
                    typeMapping: _typeMappingSource.FindMapping(typeof(int)),
                    columnNullable: false),
                ascending: true));

        var propertyJsonScalarExpression = new Dictionary<ProjectionMember, Expression>();

        var jsonColumn = selectExpression.CreateColumnExpression(
            jsonEachExpression, "value", typeof(string), _typeMappingSource.FindMapping(typeof(string))); // TODO: nullable?

        var containerColumnName = entityType.GetContainerColumnName();
        Check.DebugAssert(containerColumnName is not null, "JsonQueryExpression to entity type without a container column name");

        // First step: build a SelectExpression that will execute json_each and project all properties and navigations out, e.g.
        // (SELECT value ->> 'a' AS a, value ->> 'b' AS b FROM json_each(c."JsonColumn", '$.Something.SomeCollection')

        // We're only interested in properties which actually exist in the JSON, filter out uninteresting shadow keys
        foreach (var property in GetAllPropertiesInHierarchy(entityType))
        {
            if (property.GetJsonPropertyName() is string jsonPropertyName)
            {
                // HACK: currently the only way to project multiple values from a SelectExpression is to simulate a Select out to an anonymous
                // type; this requires the MethodInfos of the anonymous type properties, from which the projection alias gets taken.
                // So we create fake members to hold the JSON property name for the alias.
                var projectionMember = new ProjectionMember().Append(new FakeMemberInfo(jsonPropertyName));

                propertyJsonScalarExpression[projectionMember] = new JsonScalarExpression(
                    jsonColumn,
                    new[] { new PathSegment(property.GetJsonPropertyName()!) },
                    property.ClrType.UnwrapNullableType(),
                    property.GetRelationalTypeMapping(),
                    property.IsNullable);
            }
        }

        foreach (var navigation in GetAllNavigationsInHierarchy(jsonQueryExpression.EntityType)
                     .Where(
                         n => n.ForeignKey.IsOwnership
                             && n.TargetEntityType.IsMappedToJson()
                             && n.ForeignKey.PrincipalToDependent == n))
        {
            var jsonNavigationName = navigation.TargetEntityType.GetJsonPropertyName();
            Check.DebugAssert(jsonNavigationName is not null, "Invalid navigation found on JSON-mapped entity");

            var projectionMember = new ProjectionMember().Append(new FakeMemberInfo(jsonNavigationName));

            propertyJsonScalarExpression[projectionMember] = new JsonScalarExpression(
                jsonColumn,
                new[] { new PathSegment(jsonNavigationName) },
                typeof(string),
                textTypeMapping,
                !navigation.ForeignKey.IsRequiredDependent);
        }

        selectExpression.ReplaceProjection(propertyJsonScalarExpression);

        // Second step: push the above SelectExpression down to a subquery, and project an entity projection from the outer
        // SelectExpression, i.e.
        // SELECT "t"."a", "t"."b"
        // FROM (SELECT value ->> 'a' ... FROM json_each(...))

        selectExpression.PushdownIntoSubquery();
        var subquery = selectExpression.Tables[0];

#pragma warning disable EF1001 // Internal EF Core API usage.
        var newOuterSelectExpression = new SelectExpression(
            jsonQueryExpression,
            subquery,
            "key",
            typeof(int),
            _typeMappingSource.FindMapping(typeof(int))!);
#pragma warning restore EF1001 // Internal EF Core API usage.

        newOuterSelectExpression.AppendOrdering(
            new OrderingExpression(
                selectExpression.CreateColumnExpression(
                    subquery,
                    "key",
                    typeof(int),
                    typeMapping: _typeMappingSource.FindMapping(typeof(int)),
                    columnNullable: false),
                ascending: true));

        return new ShapedQueryExpression(
            newOuterSelectExpression,
            new RelationalStructuralTypeShaperExpression(
                jsonQueryExpression.EntityType,
                new ProjectionBindingExpression(
                    newOuterSelectExpression,
                    new ProjectionMember(),
                    typeof(ValueBuffer)),
                false));

        // TODO: Move these to IEntityType?
        static IEnumerable<IProperty> GetAllPropertiesInHierarchy(IEntityType entityType)
            => entityType.GetAllBaseTypes().Concat(entityType.GetDerivedTypesInclusive())
                .SelectMany(t => t.GetDeclaredProperties());

        static IEnumerable<INavigation> GetAllNavigationsInHierarchy(IEntityType entityType)
            => entityType.GetAllBaseTypes().Concat(entityType.GetDerivedTypesInclusive())
                .SelectMany(t => t.GetDeclaredNavigations());
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
        if (!returnDefault
            && source.QueryExpression is SelectExpression
            {
                Tables:
                [
                    TableValuedFunctionExpression
                    {
                        Name: "json_each", Schema: null, IsBuiltIn: true, Arguments: [var jsonArrayColumn]
                    } jsonEachExpression
                ],
                GroupBy: [],
                Having: null,
                IsDistinct: false,
                Orderings: [{ Expression: ColumnExpression { Name: "key" } orderingColumn, IsAscending: true }],
                Limit: null,
                Offset: null
            } selectExpression
            && orderingColumn.Table == jsonEachExpression
            && TranslateExpression(index) is { } translatedIndex
            && (UseOldBehavior33932 || selectExpression.Predicate is null))
        {
            // Index on JSON array

            // Extract the column projected out of the source, and simplify the subquery to a simple JsonScalarExpression
            var shaperExpression = source.ShaperExpression;
            if (shaperExpression is UnaryExpression { NodeType: ExpressionType.Convert } unaryExpression
                && unaryExpression.Operand.Type.IsNullableType()
                && unaryExpression.Operand.Type.UnwrapNullableType() == unaryExpression.Type)
            {
                shaperExpression = unaryExpression.Operand;
            }

            if (shaperExpression is ProjectionBindingExpression projectionBindingExpression
                && selectExpression.GetProjection(projectionBindingExpression) is ColumnExpression projectionColumn)
            {
                SqlExpression translation = new JsonScalarExpression(
                    jsonArrayColumn,
                    new[] { new PathSegment(translatedIndex) },
                    projectionColumn.Type,
                    projectionColumn.TypeMapping,
                    projectionColumn.IsNullable);

                // If we have a type mapping (i.e. translating over a column rather than a parameter), apply any necessary server-side
                // conversions.
                if (projectionColumn.TypeMapping is not null)
                {
                    translation = ApplyJsonSqlConversion(
                        translation, _sqlExpressionFactory, projectionColumn.TypeMapping, projectionColumn.IsNullable);
                }

                return source.UpdateQueryExpression(_sqlExpressionFactory.Select(translation));
            }
        }

        return base.TranslateElementAtOrDefault(source, index, returnDefault);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override bool IsNaturallyOrdered(SelectExpression selectExpression)
    {
        return selectExpression is
            {
                Tables: [var mainTable, ..],
                Orderings:
                [
                    {
                        Expression: ColumnExpression { Name: "key", Table: var orderingTable } orderingColumn,
                        IsAscending: true
                    }
                ]
            }
            && orderingTable == mainTable
            && IsJsonEachKeyColumn(orderingColumn);

        bool IsJsonEachKeyColumn(ColumnExpression orderingColumn)
            => orderingColumn.Table is JsonEachExpression
                || (orderingColumn.Table is SelectExpression subquery
                    && subquery.Projection.FirstOrDefault(p => p.Alias == "key")?.Expression is ColumnExpression projectedColumn
                    && IsJsonEachKeyColumn(projectedColumn));
    }

    private static Type GetProviderType(SqlExpression expression)
        => expression.TypeMapping?.Converter?.ProviderClrType
            ?? expression.TypeMapping?.ClrType
            ?? expression.Type;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression ApplyInferredTypeMappings(
        Expression expression,
        IReadOnlyDictionary<(TableExpressionBase, string), RelationalTypeMapping?> inferredTypeMappings)
        => new SqliteInferredTypeMappingApplier(
            RelationalDependencies.Model, _typeMappingSource, _sqlExpressionFactory, inferredTypeMappings).Visit(expression);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected class SqliteInferredTypeMappingApplier : RelationalInferredTypeMappingApplier
    {
        private readonly IRelationalTypeMappingSource _typeMappingSource;
        private readonly SqliteSqlExpressionFactory _sqlExpressionFactory;
        private Dictionary<TableExpressionBase, RelationalTypeMapping>? _currentSelectInferredTypeMappings;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public SqliteInferredTypeMappingApplier(
            IModel model,
            IRelationalTypeMappingSource typeMappingSource,
            SqliteSqlExpressionFactory sqlExpressionFactory,
            IReadOnlyDictionary<(TableExpressionBase, string), RelationalTypeMapping?> inferredTypeMappings)
            : base(model, sqlExpressionFactory, inferredTypeMappings)
        {
            (_typeMappingSource, _sqlExpressionFactory) = (typeMappingSource, sqlExpressionFactory);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override Expression VisitExtension(Expression expression)
        {
            switch (expression)
            {
                case TableValuedFunctionExpression { Name: "json_each", Schema: null, IsBuiltIn: true } jsonEachExpression
                    when TryGetInferredTypeMapping(jsonEachExpression, "value", out var typeMapping):
                    return ApplyTypeMappingsOnJsonEachExpression(jsonEachExpression, typeMapping);

                // Above, we applied the type mapping the the parameter that json_each accepts as an argument.
                // But the inferred type mapping also needs to be applied as a SQL conversion on the column projections coming out of the
                // SelectExpression containing the json_each call. So we set state to know about json_each tables and their type mappings
                // in the immediate SelectExpression, and continue visiting down (see ColumnExpression visitation below).
                case SelectExpression selectExpression:
                {
                    Dictionary<TableExpressionBase, RelationalTypeMapping>? previousSelectInferredTypeMappings = null;

                    foreach (var table in selectExpression.Tables)
                    {
                        if (table is TableValuedFunctionExpression { Name: "json_each", Schema: null, IsBuiltIn: true } jsonEachExpression
                            && TryGetInferredTypeMapping(jsonEachExpression, "value", out var inferredTypeMapping))
                        {
                            if (previousSelectInferredTypeMappings is null)
                            {
                                previousSelectInferredTypeMappings = _currentSelectInferredTypeMappings;
                                _currentSelectInferredTypeMappings = new Dictionary<TableExpressionBase, RelationalTypeMapping>();
                            }

                            _currentSelectInferredTypeMappings![jsonEachExpression] = inferredTypeMapping;
                        }
                    }

                    var visited = base.VisitExtension(expression);

                    _currentSelectInferredTypeMappings = previousSelectInferredTypeMappings;

                    return visited;
                }

                // Note that we match also ColumnExpressions which already have a type mapping, i.e. coming out of column collections (as
                // opposed to parameter collections, where the type mapping needs to be inferred). This is in order to apply SQL conversion
                // logic later in the process, see note in TranslateCollection.
                case ColumnExpression { Name: "value" } columnExpression
                    when _currentSelectInferredTypeMappings?.TryGetValue(columnExpression.Table, out var inferredTypeMapping) is true:
                    return ApplyJsonSqlConversion(
                        columnExpression.ApplyTypeMapping(inferredTypeMapping),
                        _sqlExpressionFactory,
                        inferredTypeMapping,
                        columnExpression.IsNullable);

                default:
                    return base.VisitExtension(expression);
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected virtual TableValuedFunctionExpression ApplyTypeMappingsOnJsonEachExpression(
            TableValuedFunctionExpression jsonEachExpression,
            RelationalTypeMapping inferredTypeMapping)
        {
            // Constant queryables are translated to VALUES, no need for JSON.
            // Column queryables have their type mapping from the model, so we don't ever need to apply an inferred mapping on them.
            if (jsonEachExpression.Arguments[0] is not SqlParameterExpression parameterExpression)
            {
                return jsonEachExpression;
            }

            if (_typeMappingSource.FindMapping(parameterExpression.Type, Model, inferredTypeMapping) is not SqliteStringTypeMapping
                parameterTypeMapping)
            {
                throw new InvalidOperationException("Type mapping for 'string' could not be found or was not a SqliteStringTypeMapping");
            }

            Check.DebugAssert(parameterTypeMapping.ElementTypeMapping != null, "Collection type mapping missing element mapping.");

            return jsonEachExpression.Update(new[] { parameterExpression.ApplyTypeMapping(parameterTypeMapping) });
        }
    }

    /// <summary>
    ///     Wraps the given expression with any SQL logic necessary to convert a value coming out of a JSON document into the relational value
    ///     represented by the given type mapping.
    /// </summary>
    private static SqlExpression ApplyJsonSqlConversion(
        SqlExpression expression,
        SqliteSqlExpressionFactory sqlExpressionFactory,
        RelationalTypeMapping typeMapping,
        bool isNullable)
        => typeMapping switch
        {
            ByteArrayTypeMapping
                => sqlExpressionFactory.Function("unhex", new[] { expression }, isNullable, new[] { true }, typeof(byte[]), typeMapping),

            _ => expression
        };

    private sealed class FakeMemberInfo : MemberInfo
    {
        public FakeMemberInfo(string name)
        {
            Name = name;
        }

        public override string Name { get; }

        public override object[] GetCustomAttributes(bool inherit)
            => throw new NotSupportedException();

        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
            => throw new NotSupportedException();

        public override bool IsDefined(Type attributeType, bool inherit)
            => throw new NotSupportedException();

        public override Type? DeclaringType
            => throw new NotSupportedException();

        public override MemberTypes MemberType
            => throw new NotSupportedException();

        public override Type? ReflectedType
            => throw new NotSupportedException();
    }
}
