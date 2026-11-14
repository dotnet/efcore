// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Sqlite.Internal;

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
    private readonly SqlAliasManager _sqlAliasManager;
    private readonly bool _areJsonFunctionsSupported;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal] // https://www.sqlite.org/json1.html#jeach
    public const string JsonEachKeyColumnName = "key";

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal] // https://www.sqlite.org/json1.html#jeach
    public const string JsonEachValueColumnName = "value";

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SqliteQueryableMethodTranslatingExpressionVisitor(
        QueryableMethodTranslatingExpressionVisitorDependencies dependencies,
        RelationalQueryableMethodTranslatingExpressionVisitorDependencies relationalDependencies,
        RelationalQueryCompilationContext queryCompilationContext)
        : base(dependencies, relationalDependencies, queryCompilationContext)
    {
        _typeMappingSource = relationalDependencies.TypeMappingSource;
        _sqlExpressionFactory = (SqliteSqlExpressionFactory)relationalDependencies.SqlExpressionFactory;
        _sqlAliasManager = queryCompilationContext.SqlAliasManager;

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
        _sqlAliasManager = parentVisitor._sqlAliasManager;

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

    /// <inheritdoc />
    protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
        => TryTranslateMinMaxBy(methodCallExpression) ?? base.VisitMethodCall(methodCallExpression);

    private ShapedQueryExpression? TryTranslateMinMaxBy(MethodCallExpression methodCallExpression)
    {
        if (!IsMethod(methodCallExpression, QueryableMethods.FirstOrDefaultWithoutPredicate)
            || methodCallExpression.Arguments[0] is not MethodCallExpression orderBy
            || !IsMethod(orderBy, QueryableMethods.OrderBy)
            || orderBy.Arguments[1].UnwrapLambdaFromQuote() is not
            {
                Body: BinaryExpression { NodeType: ExpressionType.Equal, Right: ConstantExpression { Value: null } } nullTest
            } ordering
            || orderBy.Arguments[0] is not MethodCallExpression concat
            || !IsMethod(concat, QueryableMethods.Concat)
            || concat.Arguments[0] is not MethodCallExpression preferred
            || !IsMethod(preferred, QueryableMethods.Take)
            || preferred.Arguments[1] is not ConstantExpression { Value: 1 }
            || concat.Arguments[1] is not MethodCallExpression fallback
            || !IsMethod(fallback, QueryableMethods.Take)
            || fallback.Arguments[1] is not ConstantExpression { Value: 1 }
            || fallback.Arguments[0] is not MethodCallExpression nullRows
            || !IsMethod(nullRows, QueryableMethods.Where)
            || preferred.Arguments[0] is not MethodCallExpression preferredRows
            || !IsMethod(preferredRows, QueryableMethods.Where)
            || preferredRows.Arguments[1].UnwrapLambdaFromQuote() is not
            { Body: BinaryExpression { NodeType: ExpressionType.Equal, Right: MethodCallExpression extremalKey } equality } predicate
            || preferredRows.Arguments[0] is not MethodCallExpression nonNullRows
            || !IsMethod(nonNullRows, QueryableMethods.Where)
            || !IsMethod(extremalKey, QueryableMethods.FirstOrDefaultWithoutPredicate)
            || extremalKey.Arguments[0] is not MethodCallExpression selectKey
            || !IsMethod(selectKey, QueryableMethods.Select)
            || selectKey.Arguments[0] is not MethodCallExpression keyOrdering
            || (!IsMethod(keyOrdering, QueryableMethods.OrderBy) && !IsMethod(keyOrdering, QueryableMethods.OrderByDescending)))
        {
            return null;
        }

        var source = nullRows.Arguments[0];
        var selector = Expression.Lambda(nullTest.Left, ordering.Parameters);
        if (!selector.ReturnType.IsNullableType()
            || selector.Body is not MemberExpression { Expression: ParameterExpression parameter }
            || parameter != selector.Parameters[0])
        {
            return null;
        }

        var nonNullTest = Expression.Lambda(
            Expression.NotEqual(selector.Body, Expression.Constant(null, selector.ReturnType)), selector.Parameters);
        // Require both partitions and the extremal key to refer to exactly the same ordered source and key.
        if (!MatchesSource(source, nonNullRows.Arguments[0])
            || !MatchesSource(nonNullRows, keyOrdering.Arguments[0])
            || !MatchesSource(
                equality.Left, ReplacingExpressionVisitor.Replace(selector.Parameters[0], predicate.Parameters[0], selector.Body))
            || !MatchesLambda(nullRows.Arguments[1].UnwrapLambdaFromQuote(), ordering)
            || !MatchesLambda(nonNullRows.Arguments[1].UnwrapLambdaFromQuote(), nonNullTest)
            || !MatchesLambda(selectKey.Arguments[1].UnwrapLambdaFromQuote(), selector)
            || !MatchesLambda(keyOrdering.Arguments[1].UnwrapLambdaFromQuote(), selector)
            || Visit(source) is not ShapedQueryExpression translatedSource
            || TranslateLambdaExpression(translatedSource, selector) is not ColumnExpression key
            || TranslateExpression(extremalKey) is not ScalarSubqueryExpression extremum)
        {
            return null;
        }

        ((SelectExpression)translatedSource.QueryExpression).ApplyPredicate(
            new SqliteIsExpression(key, extremum, _typeMappingSource.FindMapping(typeof(bool))!));
        return TranslateFirstOrDefault(
            translatedSource.UpdateResultCardinality(ResultCardinality.SingleOrDefault), null, methodCallExpression.Type, returnDefault: true);
    }

    private static bool IsMethod(MethodCallExpression expression, MethodInfo method)
        => expression.Method.IsGenericMethod && expression.Method.GetGenericMethodDefinition() == method;

    private static bool MatchesLambda(LambdaExpression expression, LambdaExpression expected)
        => expression.Parameters.Count == 1
            && expression.Parameters[0].Type == expected.Parameters[0].Type
            && MatchesSource(
                expression.Body, ReplacingExpressionVisitor.Replace(expected.Parameters[0], expression.Parameters[0], expected.Body));

    private static bool MatchesSource(Expression left, Expression right)
    {
        var freeParameters = new Dictionary<ParameterExpression, ParameterExpression>();
        var leftVisitor = new ScopedParameterNormalizingVisitor(freeParameters);
        var rightVisitor = new ScopedParameterNormalizingVisitor(freeParameters);
        var normalizedLeft = leftVisitor.Visit(left);
        var normalizedRight = rightVisitor.Visit(right);
        if (!leftVisitor.SupportsOptimization || !rightVisitor.SupportsOptimization)
        {
            return false;
        }

        return ExpressionEqualityComparer.Instance.Equals(
            normalizedLeft, normalizedRight);
    }

    private sealed class ScopedParameterNormalizingVisitor(Dictionary<ParameterExpression, ParameterExpression> freeParameters)
        : ExpressionVisitor
    {
        private readonly Dictionary<ParameterExpression, ParameterExpression> _scope = [];
        private int _nextParameter;

        public bool SupportsOptimization { get; private set; } = true;

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (!IsMethod(node, QueryableMethods.Where)
                && !IsMethod(node, QueryableMethods.OrderBy)
                && !IsMethod(node, QueryableMethods.OrderByDescending)
                && !IsMethod(node, QueryableMethods.ThenBy)
                && !IsMethod(node, QueryableMethods.ThenByDescending)
                && !IsMethod(node, QueryableMethods.Skip)
                && !IsMethod(node, QueryableMethods.Take)
                && !(node.Method.DeclaringType == typeof(EF) && node.Method.Name == nameof(EF.Property)))
            {
                SupportsOptimization = false;
            }

            return base.VisitMethodCall(node);
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            if (node.Expression is null)
            {
                SupportsOptimization = false;
            }

            return base.VisitMember(node);
        }

        protected override Expression VisitInvocation(InvocationExpression node)
        {
            SupportsOptimization = false;
            return base.VisitInvocation(node);
        }

        protected override Expression VisitExtension(Expression node)
        {
            if (node is EntityQueryRootExpression root
                && (node.GetType() != typeof(EntityQueryRootExpression)
                    || root.EntityType.GetViewName() != null
                    || root.EntityType.GetSqlQuery() != null
                    || root.EntityType.GetFunctionName() != null))
            {
                SupportsOptimization = false;
            }

            return base.VisitExtension(node);
        }

        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            var previous = node.Parameters.Select(p => _scope.GetValueOrDefault(p)).ToArray();
            var parameters = node.Parameters.Select(p => Expression.Parameter(p.Type, "bound" + _nextParameter++)).ToArray();
            for (var i = 0; i < parameters.Length; i++)
            {
                _scope[node.Parameters[i]] = parameters[i];
            }

            var body = Visit(node.Body);
            for (var i = 0; i < parameters.Length; i++)
            {
                if (previous[i] is { } parameter)
                {
                    _scope[node.Parameters[i]] = parameter;
                }
                else
                {
                    _scope.Remove(node.Parameters[i]);
                }
            }

            return node.Update(body, parameters);
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            if (_scope.TryGetValue(node, out var parameter))
            {
                return parameter;
            }

            if (!freeParameters.TryGetValue(node, out parameter))
            {
                freeParameters[node] = parameter = Expression.Parameter(node.Type, "free" + freeParameters.Count);
            }

            return parameter;
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override UpdateExpression TranslateExecuteUpdate(ShapedQueryExpression source, IReadOnlyList<ExecuteUpdateSetter> setters)
    {
        var updateExpression = base.TranslateExecuteUpdate(source, setters);

        // SQLite doesn't support referencing the UPDATE target table alias in JOIN ON clauses.
        // Detect such joins and replace them with cross joins, lifting their predicates to WHERE.
        var selectExpression = updateExpression.SelectExpression;
        if (selectExpression.Tables.Count <= 1)
        {
            return updateExpression;
        }

        var targetAlias = updateExpression.Table.Alias;
        var tables = selectExpression.Tables.ToList();
        var predicate = selectExpression.Predicate;
        var changed = false;

        for (var i = 0; i < tables.Count; i++)
        {
            if (tables[i] is InnerJoinExpression innerJoin
                && ContainsTargetReference(innerJoin.JoinPredicate, targetAlias))
            {
                predicate = predicate == null
                    ? innerJoin.JoinPredicate
                    : _sqlExpressionFactory.AndAlso(predicate, innerJoin.JoinPredicate);

                tables[i] = new CrossJoinExpression(innerJoin.Table);
                changed = true;
            }
        }

        if (!changed)
        {
            return updateExpression;
        }

        var newSelect = selectExpression.Update(
            tables,
            predicate,
            selectExpression.GroupBy,
            selectExpression.Having,
            selectExpression.Projection,
            selectExpression.Orderings,
            selectExpression.Offset,
            selectExpression.Limit);

        return updateExpression.Update(newSelect, updateExpression.ColumnValueSetters);

        static bool ContainsTargetReference(SqlExpression expression, string targetAlias)
        {
            var visitor = new TargetReferenceCheckingVisitor(targetAlias);
            visitor.Visit(expression);
            return visitor.Found;
        }
    }

    private sealed class TargetReferenceCheckingVisitor(string targetAlias) : ExpressionVisitor
    {
        public bool Found { get; private set; }

        [return: NotNullIfNotNull(nameof(node))]
        public override Expression? Visit(Expression? node)
        {
            if (Found)
            {
                return node;
            }

            if (node is ColumnExpression col && col.TableAlias == targetAlias)
            {
                Found = true;
                return node;
            }

            return base.Visit(node);
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [Experimental(EFDiagnostics.ExecuteMergeExperimental)]
    protected override Expression TranslateExecuteMerge(
        ShapedQueryExpression source,
        QueryParameterExpression sourceRows,
        IReadOnlyList<ExecuteMergeMatch> matches,
        IReadOnlyList<ExecuteUpdateSetter>? whenMatchedSetters,
        IReadOnlyList<ExecuteUpdateSetter>? whenNotMatchedSetters,
        LambdaExpression? returningSelector)
    {
        var translation = base.TranslateExecuteMerge(
            source, sourceRows, matches, whenMatchedSetters, whenNotMatchedSetters, returningSelector);

        // As in SaveChanges, RETURNING can only be used when SQLite supports it and the table hasn't opted out of it (e.g. because of
        // triggers).
        if (translation is MergeExpression { Returning: not null, Table.Table: ITable table })
        {
            var sqliteVersion = new SqliteConnection().ServerVersion;
            if (new Version(sqliteVersion) < new Version(3, 35))
            {
                throw new InvalidOperationException(SqliteStrings.ExecuteMergeReturningNotSupported(sqliteVersion));
            }

            if (!table.IsSqlReturningClauseUsed())
            {
                throw new InvalidOperationException(SqliteStrings.ExecuteMergeReturningWithoutReturningClause(table.SchemaQualifiedName));
            }
        }

        return translation;
    }

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
                Tables: [JsonEachExpression jsonEach],
                Predicate: null,
                GroupBy: [],
                Having: null,
                IsDistinct: false,
                Limit: null,
                Offset: null
            })
        {
            var translation =
                _sqlExpressionFactory.GreaterThan(
                    _sqlExpressionFactory.Function(
                        "json_array_length",
                        [jsonEach.Json],
                        nullable: true,
                        argumentsPropagateNullability: Statics.TrueArrays[1],
                        typeof(int)),
                    _sqlExpressionFactory.Constant(0));

#pragma warning disable EF1001
            return source.UpdateQueryExpression(new SelectExpression(translation, _sqlAliasManager));
#pragma warning restore EF1001
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
        var translation = TranslateLambdaExpression(source, keySelector);
        if (translation == null)
        {
            return null;
        }

        var orderingExpressionType = GetProviderType(translation);
        if (orderingExpressionType == typeof(DateTimeOffset)
            || orderingExpressionType == typeof(TimeSpan)
            || orderingExpressionType == typeof(ulong))
        {
            throw new NotSupportedException(
                SqliteStrings.OrderByNotSupported(orderingExpressionType.ShortDisplayName()));
        }

        if (orderingExpressionType == typeof(decimal))
        {
            translation = new CollateExpression(translation, "EF_DECIMAL");
        }

        ((SelectExpression)source.QueryExpression).ApplyOrdering(new OrderingExpression(translation, ascending));

        return source;
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
        var translation = TranslateLambdaExpression(source, keySelector);
        if (translation == null)
        {
            return null;
        }

        var orderingExpressionType = GetProviderType(translation);
        if (orderingExpressionType == typeof(DateTimeOffset)
            || orderingExpressionType == typeof(TimeSpan)
            || orderingExpressionType == typeof(ulong))
        {
            throw new NotSupportedException(
                SqliteStrings.OrderByNotSupported(orderingExpressionType.ShortDisplayName()));
        }

        if (orderingExpressionType == typeof(decimal))
        {
            translation = new CollateExpression(translation, "EF_DECIMAL");
        }

        ((SelectExpression)source.QueryExpression).AppendOrdering(new OrderingExpression(translation, ascending));

        return source;
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
                Tables: [JsonEachExpression jsonEach],
                Predicate: null,
                GroupBy: [],
                Having: null,
                IsDistinct: false,
                Limit: null,
                Offset: null
            })
        {
            var translation = _sqlExpressionFactory.Function(
                "json_array_length",
                [jsonEach.Json],
                nullable: true,
                argumentsPropagateNullability: Statics.TrueArrays[1],
                typeof(int));

#pragma warning disable EF1001
            return source.UpdateQueryExpression(new SelectExpression(translation, _sqlAliasManager));
#pragma warning restore EF1001
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

        var keyColumnTypeMapping = _typeMappingSource.FindMapping(typeof(int))!;

#pragma warning disable EF1001 // Internal EF Core API usage.
        var selectExpression = new SelectExpression(
            [jsonEachExpression],
            new ColumnExpression(
                JsonEachValueColumnName,
                tableAlias,
                elementClrType.UnwrapNullableType(),
                elementTypeMapping,
                isElementNullable ?? elementClrType.IsNullableType()),
            identifier:
            [
                (new ColumnExpression(JsonEachKeyColumnName, tableAlias, typeof(int), keyColumnTypeMapping, nullable: false),
                    keyColumnTypeMapping.Comparer)
            ],
            _sqlAliasManager);
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
                    JsonEachKeyColumnName,
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
        var structuralType = jsonQueryExpression.StructuralType;
        var textTypeMapping = _typeMappingSource.FindMapping(typeof(string));

        // TODO: Refactor this out
        // Calculate the table alias for the json_each expression based on the last named path segment
        // (or the JSON column name if there are none)
        var lastNamedPathSegment = jsonQueryExpression.Path.LastOrDefault(ps => ps.PropertyName is not null);
        var tableAlias = _sqlAliasManager.GenerateTableAlias(lastNamedPathSegment.PropertyName ?? jsonQueryExpression.JsonColumn.Name);

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
        var selectExpression = CreateSelect(
            jsonQueryExpression,
            jsonEachExpression,
            JsonEachKeyColumnName,
            typeof(int),
            _typeMappingSource.FindMapping(typeof(int))!);
#pragma warning restore EF1001 // Internal EF Core API usage.

        selectExpression.AppendOrdering(
            new OrderingExpression(
                selectExpression.CreateColumnExpression(
                    jsonEachExpression,
                    JsonEachKeyColumnName,
                    typeof(int),
                    typeMapping: _typeMappingSource.FindMapping(typeof(int)),
                    columnNullable: false),
                ascending: true));

        var propertyJsonScalarExpression = new Dictionary<ProjectionMember, Expression>();

        var jsonColumn = selectExpression.CreateColumnExpression(
            jsonEachExpression, JsonEachValueColumnName, typeof(string), _typeMappingSource.FindMapping(typeof(string))); // TODO: nullable?

        // First step: build a SelectExpression that will execute json_each and project all properties and navigations out, e.g.
        // (SELECT value ->> 'a' AS a, value ->> 'b' AS b FROM json_each(c."JsonColumn", '$.Something.SomeCollection')

        var jsonElements = new List<(string JsonPropertyName, JsonScalarExpression Expression)>();

        // We're only interested in properties which actually exist in the JSON, filter out uninteresting synthetic keys
        foreach (var property in structuralType.GetPropertiesInHierarchy())
        {
            if (jsonQueryExpression.FindJsonElement(property) is { PropertyName: { } jsonPropertyName } element)
            {
                jsonElements.Add((jsonPropertyName, new JsonScalarExpression(
                    jsonColumn,
                    [new PathSegment(jsonPropertyName)],
                    property.ClrType.UnwrapNullableType(),
                    element.StoreTypeMapping!,
                    property.IsNullable)));
            }
        }

        if (structuralType is IEntityType entityType)
        {
            foreach (var navigation in entityType.GetNavigationsInHierarchy()
                         .Where(n => n.ForeignKey.IsOwnership
                             && n.TargetEntityType.IsMappedToJson()
                             && n.ForeignKey.PrincipalToDependent == n))
            {
                var jsonNavigationName = jsonQueryExpression.GetJsonElement(navigation).PropertyName;
                Check.DebugAssert(jsonNavigationName is not null, "Invalid navigation found on JSON-mapped entity");

                jsonElements.Add((jsonNavigationName, new JsonScalarExpression(
                    jsonColumn,
                    [new PathSegment(jsonNavigationName)],
                    typeof(string),
                    textTypeMapping,
                    !navigation.ForeignKey.IsRequiredDependent)));
            }
        }

        foreach (var complexProperty in structuralType.GetComplexProperties())
        {
            var jsonNavigationName = jsonQueryExpression.GetJsonElement(complexProperty).PropertyName;
            Check.DebugAssert(jsonNavigationName is not null, "Invalid complex property found on JSON-mapped structural type");

            jsonElements.Add((jsonNavigationName, new JsonScalarExpression(
                jsonColumn,
                [new PathSegment(jsonNavigationName)],
                typeof(string),
                textTypeMapping,
                jsonQueryExpression.IsNullable || complexProperty.IsNullable)));
        }

        // json_each's key column (the index of the element in the array, which orders the elements) is projected out of the subquery
        // next to the JSON properties, and aliases are unique case-insensitively, so a JSON property named "key" in any casing needs
        // another alias (see GetJsonElementColumnName).
        var keyJsonPropertyAlias = JsonEachKeyColumnName;
        for (var i = 0;
             jsonElements.Any(e => string.Equals(e.JsonPropertyName, keyJsonPropertyAlias, StringComparison.OrdinalIgnoreCase));
             i++)
        {
            keyJsonPropertyAlias = JsonEachKeyColumnName + i;
        }

        foreach (var (jsonPropertyName, expression) in jsonElements)
        {
            // HACK: currently the only way to project multiple values from a SelectExpression is to simulate a Select out to an anonymous
            // type; this requires the MethodInfos of the anonymous type properties, from which the projection alias gets taken.
            // So we create fake members to hold the JSON property name for the alias.
            var alias = string.Equals(jsonPropertyName, JsonEachKeyColumnName, StringComparison.OrdinalIgnoreCase)
                ? keyJsonPropertyAlias
                : jsonPropertyName;

            propertyJsonScalarExpression[new ProjectionMember().Append(new FakeMemberInfo(alias))] = expression;
        }

        selectExpression.ReplaceProjection(propertyJsonScalarExpression);

        // Second step: push the above SelectExpression down to a subquery, and project an entity projection from the outer
        // SelectExpression, i.e.
        // SELECT "t"."a", "t"."b"
        // FROM (SELECT value ->> 'a' ... FROM json_each(...))

        selectExpression.PushdownIntoSubquery();
        var subquery = selectExpression.Tables[0];

#pragma warning disable EF1001 // Internal EF Core API usage.
        var newOuterSelectExpression = CreateSelect(
            jsonQueryExpression,
            subquery,
            JsonEachKeyColumnName,
            typeof(int),
            _typeMappingSource.FindMapping(typeof(int))!);
#pragma warning restore EF1001 // Internal EF Core API usage.

        newOuterSelectExpression.AppendOrdering(
            new OrderingExpression(
                selectExpression.CreateColumnExpression(
                    subquery,
                    JsonEachKeyColumnName,
                    typeof(int),
                    typeMapping: _typeMappingSource.FindMapping(typeof(int)),
                    columnNullable: false),
                ascending: true));

        return new ShapedQueryExpression(
            newOuterSelectExpression,
            new RelationalStructuralTypeShaperExpression(
                jsonQueryExpression.StructuralType,
                new ProjectionBindingExpression(
                    newOuterSelectExpression,
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
    protected override string GetJsonElementColumnName(TableExpressionBase tableExpressionBase, string jsonPropertyName)
        => tableExpressionBase is SelectExpression subquery
            && subquery.Projection.FirstOrDefault(p => p.Expression is JsonScalarExpression { Path: [{ PropertyName: var propertyName }] }
                && propertyName == jsonPropertyName) is { } projection
                ? projection.Alias
                : jsonPropertyName;

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
                    && TryTranslate(selectExpression, valuesParameter, translatedIndex, out var result):
                    return result;

                // Index on JSON array
                case SelectExpression
                {
                    Tables: [JsonEachExpression jsonEach],
                    Predicate: null,
                    GroupBy: [],
                    Having: null,
                    IsDistinct: false,
                    Orderings: [{ Expression: ColumnExpression { Name: JsonEachKeyColumnName } orderingColumn, IsAscending: true }],
                    Limit: null,
                    Offset: null
                } selectExpression
                    when orderingColumn.TableAlias == jsonEach.Alias
                    && TranslateExpression(index) is { } translatedIndex
                    && TryTranslate(selectExpression, jsonEach.Json, translatedIndex, out var result):
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
                // If the inner expression happens to itself be a JsonScalarExpression, simply append the two paths to avoid creating
                // JSON_VALUE within JSON_VALUE.
                var (json, path) = jsonArrayColumn is JsonScalarExpression innerJsonScalarExpression
                    ? (innerJsonScalarExpression.Json,
                        innerJsonScalarExpression.Path.Append(new PathSegment(translatedIndex)).ToArray())
                    : (jsonArrayColumn, [new PathSegment(translatedIndex)]);

                SqlExpression translation = new JsonScalarExpression(
                    json,
                    path,
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

#pragma warning disable EF1001
                result = source.UpdateQueryExpression(new SelectExpression(translation, _sqlAliasManager));
#pragma warning restore EF1001
                return true;
            }

            result = default;
            return false;
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override bool TrySerializeScalarToJson(
        JsonScalarExpression target,
        SqlExpression value,
        [NotNullWhen(true)] out SqlExpression? jsonValue)
    {
        var providerClrType = (value.TypeMapping!.Converter?.ProviderClrType ?? value.Type).UnwrapNullableType();

        // SQLite has no bool type, so if we simply sent the bool as-is, we'd get 1/0 in the JSON document.
        // To get an actual unquoted true/false value, we pass "true"/"false" string through the json() minifier, which does this.
        // See https://sqlite.org/forum/info/91d09974c3754ea6.
        if (providerClrType == typeof(bool))
        {
            jsonValue = _sqlExpressionFactory.Function(
                "json",
                [
                    value is SqlConstantExpression { Value: bool constant }
                        ? _sqlExpressionFactory.Constant(constant ? "true" : "false")
                        : _sqlExpressionFactory.Case(
                            [
                                new CaseWhenClause(
                                    _sqlExpressionFactory.Equal(value, _sqlExpressionFactory.Constant(true)),
                                    _sqlExpressionFactory.Constant("true")),
                                new CaseWhenClause(
                                    _sqlExpressionFactory.Equal(value, _sqlExpressionFactory.Constant(false)),
                                    _sqlExpressionFactory.Constant("false"))
                            ],
                            elseResult: _sqlExpressionFactory.Constant("null"))
                ],
                nullable: true,
                argumentsPropagateNullability: [true],
                typeof(string),
                _typeMappingSource.FindMapping(typeof(string)));

            return true;
        }

        if (providerClrType == typeof(ulong))
        {
            // See #36689
            throw new InvalidOperationException(SqliteStrings.ExecuteUpdateJsonPartialUpdateDoesNotSupportUlong);
        }

#pragma warning disable EF9002 // TrySerializeScalarToJson is experimental
        return base.TrySerializeScalarToJson(target, value, out jsonValue);
#pragma warning restore EF9002
    }

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
        var (jsonColumn, path, isJsonScalar) = target switch
        {
            JsonScalarExpression { TypeMapping.ElementTypeMapping: null } j => ((ColumnExpression)j.Json, j.Path, true),
            JsonScalarExpression { TypeMapping.ElementTypeMapping: not null } j => ((ColumnExpression)j.Json, j.Path, false),
            JsonQueryExpression j => (j.JsonColumn, j.Path, false),

            _ => throw new UnreachableException(),
        };

        var jsonSet = _sqlExpressionFactory.Function(
            "json_set",
            arguments:
            [
                existingSetterValue ?? jsonColumn,
                // Hack: Rendering of JSONPATH strings happens in value generation. We can have a special expression for modify to hold the
                // IReadOnlyList<PathSegment> (just like Json{Scalar,Query}Expression), but instead we do the slight hack of packaging it
                // as a constant argument; it will be unpacked and handled in SQL generation.
                _sqlExpressionFactory.Constant(path, RelationalTypeMapping.NullMapping),
                // json_set by default assumes text and escapes it.
                // In order to set a JSON fragment (for nested JSON objects), we need to wrap the JSON text with json(), which makes
                // json_set understand that it's JSON content and prevents escaping.
                isJsonScalar
                    ? value
                    : _sqlExpressionFactory.Function(
                        "json", [value], nullable: true, argumentsPropagateNullability: [true], typeof(string), value.TypeMapping)
            ],
            nullable: true,
            argumentsPropagateNullability: [true, true, true],
            typeof(string),
            jsonColumn.TypeMapping);

        if (existingSetterValue is null)
        {
            return jsonSet;
        }

        existingSetterValue = jsonSet;
        return null;
    }

#pragma warning disable EF9001 // Window function support is experimental
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override bool CanTranslateAggregateAsWindowFunction(SqlFunctionExpression function)
        // The decimal aggregates are user-defined (see SqliteRelationalConnection), and SQLite only accepts an OVER clause on a
        // user-defined aggregate registered as a window function.
        => function.Name is not ("ef_sum" or "ef_avg" or "ef_min" or "ef_max");
#pragma warning restore EF9001

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
                    Expression: ColumnExpression { Name: JsonEachKeyColumnName } orderingColumn,
                    IsAscending: true
                }
                ]
        }
            && orderingColumn.TableAlias == mainTable.Alias
            && IsJsonEachKeyColumn(selectExpression, orderingColumn);

        bool IsJsonEachKeyColumn(SelectExpression selectExpression, ColumnExpression orderingColumn)
            => selectExpression.Tables.FirstOrDefault(t => t.Alias == orderingColumn.TableAlias)?.UnwrapJoin() is { } table
                && (table is JsonEachExpression
                    || (table is SelectExpression subquery
                        && subquery.Projection.FirstOrDefault(p => p.Alias == JsonEachKeyColumnName)?.Expression is ColumnExpression
                            projectedColumn
                        && IsJsonEachKeyColumn(subquery, projectedColumn)));
    }

    private static Type GetProviderType(SqlExpression expression)
        => expression.TypeMapping?.Converter?.ProviderClrType
            ?? expression.TypeMapping?.ClrType
            ?? expression.Type;

    /// <summary>
    ///     Wraps the given expression with any SQL logic necessary to convert a value coming out of a JSON document into the relational value
    ///     represented by the given type mapping.
    /// </summary>
    /// <remarks>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </remarks>
    [EntityFrameworkInternal]
    public static SqlExpression ApplyJsonSqlConversion(
        SqlExpression expression,
        SqliteSqlExpressionFactory sqlExpressionFactory,
        RelationalTypeMapping typeMapping,
        bool isNullable)
        => typeMapping switch
        {
            // In general unhex returns NULL whenever the decoding fails.
            // In this case, we assume that the decoding cannot fail, because we
            // rely on the user to correctly model the database schema and
            // contents. Under this assumption, `expression` can only be a valid
            // hex string or NULL, hence unhex simply propagates the nullability
            // from its argument.

            ByteArrayTypeMapping
                => sqlExpressionFactory.Function(
                    "unhex",
                    [expression],
                    nullable: true,
                    argumentsPropagateNullability: Statics.TrueArrays[1],
                    typeof(byte[]),
                    typeMapping),

            _ => expression
        };

    private sealed class FakeMemberInfo(string name) : MemberInfo
    {
        public override string Name { get; } = name;

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
