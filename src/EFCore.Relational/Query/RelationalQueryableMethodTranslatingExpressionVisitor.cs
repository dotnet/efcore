// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.Query;

/// <inheritdoc />
public class RelationalQueryableMethodTranslatingExpressionVisitor : QueryableMethodTranslatingExpressionVisitor
{
    private readonly RelationalSqlTranslatingExpressionVisitor _sqlTranslator;
    private readonly SharedTypeEntityExpandingExpressionVisitor _sharedTypeEntityExpandingExpressionVisitor;
    private readonly RelationalProjectionBindingExpressionVisitor _projectionBindingExpressionVisitor;
    private readonly QueryCompilationContext _queryCompilationContext;
    private readonly ISqlExpressionFactory _sqlExpressionFactory;
    private readonly bool _subquery;
    private SqlExpression? _groupingElementCorrelationalPredicate;

    /// <summary>
    ///     Creates a new instance of the <see cref="QueryableMethodTranslatingExpressionVisitor" /> class.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this class.</param>
    /// <param name="relationalDependencies">Parameter object containing relational dependencies for this class.</param>
    /// <param name="queryCompilationContext">The query compilation context object to use.</param>
    public RelationalQueryableMethodTranslatingExpressionVisitor(
        QueryableMethodTranslatingExpressionVisitorDependencies dependencies,
        RelationalQueryableMethodTranslatingExpressionVisitorDependencies relationalDependencies,
        QueryCompilationContext queryCompilationContext)
        : base(dependencies, queryCompilationContext, subquery: false)
    {
        RelationalDependencies = relationalDependencies;

        var sqlExpressionFactory = relationalDependencies.SqlExpressionFactory;
        _queryCompilationContext = queryCompilationContext;
        _sqlTranslator = relationalDependencies.RelationalSqlTranslatingExpressionVisitorFactory.Create(queryCompilationContext, this);
        _sharedTypeEntityExpandingExpressionVisitor =
            new SharedTypeEntityExpandingExpressionVisitor(_sqlTranslator, sqlExpressionFactory);
        _projectionBindingExpressionVisitor = new RelationalProjectionBindingExpressionVisitor(this, _sqlTranslator);
        _sqlExpressionFactory = sqlExpressionFactory;
        _subquery = false;
    }

    /// <summary>
    ///     Relational provider-specific dependencies for this service.
    /// </summary>
    protected virtual RelationalQueryableMethodTranslatingExpressionVisitorDependencies RelationalDependencies { get; }

    /// <summary>
    ///     Creates a new instance of the <see cref="QueryableMethodTranslatingExpressionVisitor" /> class.
    /// </summary>
    /// <param name="parentVisitor">A parent visitor to create subquery visitor for.</param>
    protected RelationalQueryableMethodTranslatingExpressionVisitor(
        RelationalQueryableMethodTranslatingExpressionVisitor parentVisitor)
        : base(parentVisitor.Dependencies, parentVisitor.QueryCompilationContext, subquery: true)
    {
        RelationalDependencies = parentVisitor.RelationalDependencies;
        _queryCompilationContext = parentVisitor._queryCompilationContext;
        _sqlTranslator = RelationalDependencies.RelationalSqlTranslatingExpressionVisitorFactory.Create(
            parentVisitor._queryCompilationContext, parentVisitor);
        _sharedTypeEntityExpandingExpressionVisitor =
            new SharedTypeEntityExpandingExpressionVisitor(_sqlTranslator, parentVisitor._sqlExpressionFactory);
        _projectionBindingExpressionVisitor = new RelationalProjectionBindingExpressionVisitor(this, _sqlTranslator);
        _sqlExpressionFactory = parentVisitor._sqlExpressionFactory;
        _subquery = true;
    }

    /// <inheritdoc />
    protected override Expression VisitExtension(Expression extensionExpression)
    {
        switch (extensionExpression)
        {
            case FromSqlQueryRootExpression fromSqlQueryRootExpression:
                return CreateShapedQueryExpression(
                    fromSqlQueryRootExpression.EntityType,
                    _sqlExpressionFactory.Select(
                        fromSqlQueryRootExpression.EntityType,
                        new FromSqlExpression(
                            fromSqlQueryRootExpression.EntityType.GetDefaultMappings().Single().Table.Name[..1]
                                .ToLowerInvariant(),
                            fromSqlQueryRootExpression.Sql,
                            fromSqlQueryRootExpression.Argument)));

            case TableValuedFunctionQueryRootExpression tableValuedFunctionQueryRootExpression:
                var function = tableValuedFunctionQueryRootExpression.Function;
                var arguments = new List<SqlExpression>();
                foreach (var arg in tableValuedFunctionQueryRootExpression.Arguments)
                {
                    var sqlArgument = TranslateExpression(arg);
                    if (sqlArgument == null)
                    {
                        string call;
                        var methodInfo = function.DbFunctions.Last().MethodInfo;
                        if (methodInfo != null)
                        {
                            var methodCall = Expression.Call(
                                // Declaring types would be derived db context.
                                Expression.Constant(null, methodInfo.DeclaringType!),
                                methodInfo,
                                tableValuedFunctionQueryRootExpression.Arguments);

                            call = methodCall.Print();
                        }
                        else
                        {
                            call = $"{function.DbFunctions.Last().Name}()";
                        }

                        throw new InvalidOperationException(
                            TranslationErrorDetails == null
                                ? CoreStrings.TranslationFailed(call)
                                : CoreStrings.TranslationFailedWithDetails(call, TranslationErrorDetails));
                    }

                    arguments.Add(sqlArgument);
                }

                var entityType = tableValuedFunctionQueryRootExpression.EntityType;

                var translation = new TableValuedFunctionExpression(function, arguments);
                var queryExpression = _sqlExpressionFactory.Select(entityType, translation);

                return CreateShapedQueryExpression(entityType, queryExpression);

            case QueryRootExpression queryRootExpression
                when queryRootExpression.GetType() == typeof(QueryRootExpression)
                && queryRootExpression.EntityType.GetSqlQueryMappings().FirstOrDefault(m => m.IsDefaultSqlQueryMapping)?.SqlQuery is
                    ISqlQuery sqlQuery:
                return Visit(
                    new FromSqlQueryRootExpression(
                        queryRootExpression.EntityType, sqlQuery.Sql, Expression.Constant(Array.Empty<object>(), typeof(object[]))));

            case GroupByShaperExpression groupByShaperExpression:
                var groupShapedQueryExpression = groupByShaperExpression.GroupingEnumerable;
                var groupClonedSelectExpression = ((SelectExpression)groupShapedQueryExpression.QueryExpression).Clone();
                _groupingElementCorrelationalPredicate = groupClonedSelectExpression.Predicate;
                return new ShapedQueryExpression(
                    groupClonedSelectExpression,
                    new QueryExpressionReplacingExpressionVisitor(
                            groupShapedQueryExpression.QueryExpression, groupClonedSelectExpression)
                        .Visit(groupShapedQueryExpression.ShaperExpression));

            case ShapedQueryExpression shapedQueryExpression:
                var clonedSelectExpression = ((SelectExpression)shapedQueryExpression.QueryExpression).Clone();
                return new ShapedQueryExpression(
                    clonedSelectExpression,
                    new QueryExpressionReplacingExpressionVisitor(shapedQueryExpression.QueryExpression, clonedSelectExpression)
                        .Visit(shapedQueryExpression.ShaperExpression));

            default:
                return base.VisitExtension(extensionExpression);
        }
    }

    /// <inheritdoc />
    protected override QueryableMethodTranslatingExpressionVisitor CreateSubqueryVisitor()
        => new RelationalQueryableMethodTranslatingExpressionVisitor(this);

    /// <inheritdoc />
    protected override ShapedQueryExpression CreateShapedQueryExpression(IEntityType entityType)
        => CreateShapedQueryExpression(entityType, _sqlExpressionFactory.Select(entityType));

    private static ShapedQueryExpression CreateShapedQueryExpression(IEntityType entityType, SelectExpression selectExpression)
        => new(
            selectExpression,
            new RelationalEntityShaperExpression(
                entityType,
                new ProjectionBindingExpression(
                    selectExpression,
                    new ProjectionMember(),
                    typeof(ValueBuffer)),
                false));

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateAll(ShapedQueryExpression source, LambdaExpression predicate)
    {
        var translation = TranslateLambdaExpression(source, predicate);
        if (translation == null)
        {
            return null;
        }

        var selectExpression = (SelectExpression)source.QueryExpression;
        selectExpression.ApplyPredicate(_sqlExpressionFactory.Not(translation));
        selectExpression.ReplaceProjection(new Dictionary<ProjectionMember, Expression>());
        if (selectExpression.Limit == null
            && selectExpression.Offset == null)
        {
            selectExpression.ClearOrdering();
        }

        translation = _sqlExpressionFactory.Exists(selectExpression, true);

        return source.Update(
            _sqlExpressionFactory.Select(translation),
            Expression.Convert(
                new ProjectionBindingExpression(source.QueryExpression, new ProjectionMember(), typeof(bool?)),
                typeof(bool)));
    }

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateAny(ShapedQueryExpression source, LambdaExpression? predicate)
    {
        if (predicate != null)
        {
            var translatedSource = TranslateWhere(source, predicate);
            if (translatedSource == null)
            {
                return null;
            }

            source = translatedSource;
        }

        var selectExpression = (SelectExpression)source.QueryExpression;
        selectExpression.ReplaceProjection(new Dictionary<ProjectionMember, Expression>());
        if (selectExpression.Limit == null
            && selectExpression.Offset == null)
        {
            selectExpression.ClearOrdering();
        }

        var translation = _sqlExpressionFactory.Exists(selectExpression, false);

        return source.Update(
            _sqlExpressionFactory.Select(translation),
            Expression.Convert(
                new ProjectionBindingExpression(source.QueryExpression, new ProjectionMember(), typeof(bool?)),
                typeof(bool)));
    }

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateAverage(
        ShapedQueryExpression source,
        LambdaExpression? selector,
        Type resultType)
        => TranslateAggregateWithSelector(
            source, selector, e => _sqlTranslator.TranslateAverage(e), throwWhenEmpty: true, resultType);

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateCast(ShapedQueryExpression source, Type resultType)
        => source.ShaperExpression.Type != resultType
            ? source.UpdateShaperExpression(Expression.Convert(source.ShaperExpression, resultType))
            : source;

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateConcat(ShapedQueryExpression source1, ShapedQueryExpression source2)
    {
        ((SelectExpression)source1.QueryExpression).ApplyUnion((SelectExpression)source2.QueryExpression, distinct: false);

        return source1.UpdateShaperExpression(
            MatchShaperNullabilityForSetOperation(source1.ShaperExpression, source2.ShaperExpression, makeNullable: true));
    }

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateContains(ShapedQueryExpression source, Expression item)
    {
        var selectExpression = (SelectExpression)source.QueryExpression;
        var translation = TranslateExpression(item);
        if (translation == null)
        {
            return null;
        }

        if (selectExpression.Limit == null
            && selectExpression.Offset == null)
        {
            selectExpression.ClearOrdering();
        }

        var shaperExpression = source.ShaperExpression;
        // No need to check ConvertChecked since this is convert node which we may have added during projection
        if (shaperExpression is UnaryExpression { NodeType: ExpressionType.Convert } unaryExpression
            && unaryExpression.Operand.Type.IsNullableType()
            && unaryExpression.Operand.Type.UnwrapNullableType() == unaryExpression.Type)
        {
            shaperExpression = unaryExpression.Operand;
        }

        if (shaperExpression is ProjectionBindingExpression projectionBindingExpression)
        {
            var projection = selectExpression.GetProjection(projectionBindingExpression);
            if (projection is SqlExpression sqlExpression)
            {
                selectExpression.ReplaceProjection(new List<Expression>());
                selectExpression.AddToProjection(sqlExpression);

                translation = _sqlExpressionFactory.In(translation, selectExpression, false);

                return source.Update(
                    _sqlExpressionFactory.Select(translation),
                    Expression.Convert(
                        new ProjectionBindingExpression(source.QueryExpression, new ProjectionMember(), typeof(bool?)),
                        typeof(bool)));
            }
        }

        return null;
    }

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateCount(ShapedQueryExpression source, LambdaExpression? predicate)
        => TranslateAggregateWithPredicate(source, predicate, e => _sqlTranslator.TranslateCount(e), typeof(int));

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateDefaultIfEmpty(ShapedQueryExpression source, Expression? defaultValue)
    {
        if (defaultValue == null)
        {
            ((SelectExpression)source.QueryExpression).ApplyDefaultIfEmpty(_sqlExpressionFactory);
            return source.UpdateShaperExpression(MarkShaperNullable(source.ShaperExpression));
        }

        return null;
    }

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateDistinct(ShapedQueryExpression source)
    {
        var selectExpression = (SelectExpression)source.QueryExpression;
        if (selectExpression.Orderings.Count > 0
            && selectExpression.Limit == null
            && selectExpression.Offset == null)
        {
            _queryCompilationContext.Logger.DistinctAfterOrderByWithoutRowLimitingOperatorWarning();
        }

        selectExpression.ApplyDistinct();
        return source;
    }

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateElementAtOrDefault(
        ShapedQueryExpression source,
        Expression index,
        bool returnDefault)
        => null;

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateExcept(ShapedQueryExpression source1, ShapedQueryExpression source2)
    {
        ((SelectExpression)source1.QueryExpression).ApplyExcept((SelectExpression)source2.QueryExpression, distinct: true);

        // Since except has result from source1, we don't need to change shaper
        return source1;
    }

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateFirstOrDefault(
        ShapedQueryExpression source,
        LambdaExpression? predicate,
        Type returnType,
        bool returnDefault)
    {
        if (predicate != null)
        {
            var translatedSource = TranslateWhere(source, predicate);
            if (translatedSource == null)
            {
                return null;
            }

            source = translatedSource;
        }

        var selectExpression = (SelectExpression)source.QueryExpression;
        if (selectExpression.Predicate == null
            && selectExpression.Orderings.Count == 0)
        {
            _queryCompilationContext.Logger.FirstWithoutOrderByAndFilterWarning();
        }

        selectExpression.ApplyLimit(TranslateExpression(Expression.Constant(1))!);

        return source.ShaperExpression.Type != returnType
            ? source.UpdateShaperExpression(Expression.Convert(source.ShaperExpression, returnType))
            : source;
    }

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateGroupBy(
        ShapedQueryExpression source,
        LambdaExpression keySelector,
        LambdaExpression? elementSelector,
        LambdaExpression? resultSelector)
    {
        var selectExpression = (SelectExpression)source.QueryExpression;
        // This has it's own set of condition since it is different scenario from below.
        // Aggregate operators need pushdown for skip/limit/offset covered by selectExpression.PrepareForAggregate.
        // Aggregate operators need special processing beyond pushdown when applying over group by for client eval.
        if (selectExpression.Limit != null
            || selectExpression.Offset != null
            || selectExpression.IsDistinct
            || selectExpression.GroupBy.Count > 0)
        {
            selectExpression.PushdownIntoSubquery();
        }

        var remappedKeySelector = RemapLambdaBody(source, keySelector);
        var translatedKey = TranslateGroupingKey(remappedKeySelector);
        if (translatedKey == null)
        {
            return null;
        }

        if (elementSelector != null)
        {
            source = TranslateSelect(source, elementSelector);
        }

        var groupByShaper = selectExpression.ApplyGrouping(translatedKey, source.ShaperExpression, _sqlExpressionFactory);
        if (resultSelector == null)
        {
            return source.UpdateShaperExpression(groupByShaper);
        }

        var original1 = resultSelector.Parameters[0];
        var original2 = resultSelector.Parameters[1];

        var newResultSelectorBody = new ReplacingExpressionVisitor(
                new Expression[] { original1, original2 },
                new[] { translatedKey, groupByShaper })
            .Visit(resultSelector.Body);

        newResultSelectorBody = ExpandSharedTypeEntities(selectExpression, newResultSelectorBody);

        return source.UpdateShaperExpression(
            _projectionBindingExpressionVisitor.Translate(selectExpression, newResultSelectorBody));
    }

    private Expression? TranslateGroupingKey(Expression expression)
    {
        switch (expression)
        {
            case NewExpression newExpression:
                if (newExpression.Arguments.Count == 0)
                {
                    return newExpression;
                }

                var newArguments = new Expression[newExpression.Arguments.Count];
                for (var i = 0; i < newArguments.Length; i++)
                {
                    var key = TranslateGroupingKey(newExpression.Arguments[i]);
                    if (key == null)
                    {
                        return null;
                    }

                    newArguments[i] = key;
                }

                return newExpression.Update(newArguments);

            case MemberInitExpression memberInitExpression:
                var updatedNewExpression = (NewExpression?)TranslateGroupingKey(memberInitExpression.NewExpression);
                if (updatedNewExpression == null)
                {
                    return null;
                }

                var newBindings = new MemberAssignment[memberInitExpression.Bindings.Count];
                for (var i = 0; i < newBindings.Length; i++)
                {
                    var memberAssignment = (MemberAssignment)memberInitExpression.Bindings[i];
                    var visitedExpression = TranslateGroupingKey(memberAssignment.Expression);
                    if (visitedExpression == null)
                    {
                        return null;
                    }

                    newBindings[i] = memberAssignment.Update(visitedExpression);
                }

                return memberInitExpression.Update(updatedNewExpression, newBindings);

            default:
                var translation = TranslateExpression(expression);
                if (translation == null)
                {
                    return null;
                }

                return translation.Type == expression.Type
                    ? translation
                    : Expression.Convert(translation, expression.Type);
        }
    }

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateGroupJoin(
        ShapedQueryExpression outer,
        ShapedQueryExpression inner,
        LambdaExpression outerKeySelector,
        LambdaExpression innerKeySelector,
        LambdaExpression resultSelector)
        => null;

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateIntersect(ShapedQueryExpression source1, ShapedQueryExpression source2)
    {
        ((SelectExpression)source1.QueryExpression).ApplyIntersect((SelectExpression)source2.QueryExpression, distinct: true);

        // For intersect since result comes from both sides, if one of them is non-nullable then both are non-nullable
        return source1.UpdateShaperExpression(
            MatchShaperNullabilityForSetOperation(source1.ShaperExpression, source2.ShaperExpression, makeNullable: false));
    }

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateJoin(
        ShapedQueryExpression outer,
        ShapedQueryExpression inner,
        LambdaExpression outerKeySelector,
        LambdaExpression innerKeySelector,
        LambdaExpression resultSelector)
    {
        var joinPredicate = CreateJoinPredicate(outer, outerKeySelector, inner, innerKeySelector);
        if (joinPredicate != null)
        {
            var outerSelectExpression = (SelectExpression)outer.QueryExpression;
            var outerShaperExpression = outerSelectExpression.AddInnerJoin(inner, joinPredicate, outer.ShaperExpression);
            outer = outer.UpdateShaperExpression(outerShaperExpression);

            return TranslateTwoParameterSelector(outer, resultSelector);
        }

        return null;
    }

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateLeftJoin(
        ShapedQueryExpression outer,
        ShapedQueryExpression inner,
        LambdaExpression outerKeySelector,
        LambdaExpression innerKeySelector,
        LambdaExpression resultSelector)
    {
        var joinPredicate = CreateJoinPredicate(outer, outerKeySelector, inner, innerKeySelector);
        if (joinPredicate != null)
        {
            var outerSelectExpression = (SelectExpression)outer.QueryExpression;
            var outerShaperExpression = outerSelectExpression.AddLeftJoin(inner, joinPredicate, outer.ShaperExpression);
            outer = outer.UpdateShaperExpression(outerShaperExpression);

            return TranslateTwoParameterSelector(outer, resultSelector);
        }

        return null;
    }

    private SqlExpression CreateJoinPredicate(
        ShapedQueryExpression outer,
        LambdaExpression outerKeySelector,
        ShapedQueryExpression inner,
        LambdaExpression innerKeySelector)
    {
        var outerKey = RemapLambdaBody(outer, outerKeySelector);
        var innerKey = RemapLambdaBody(inner, innerKeySelector);

        if (outerKey is NewExpression outerNew
            && outerNew.Arguments.Count > 0)
        {
            var innerNew = (NewExpression)innerKey;

            SqlExpression? result = null;
            for (var i = 0; i < outerNew.Arguments.Count; i++)
            {
                var joinPredicate = CreateJoinPredicate(outerNew.Arguments[i], innerNew.Arguments[i]);
                result = result == null
                    ? joinPredicate
                    : _sqlExpressionFactory.AndAlso(result, joinPredicate);
            }

            if (outerNew.Arguments.Count == 1)
            {
                result = _sqlExpressionFactory.AndAlso(
                    result!,
                    CreateJoinPredicate(Expression.Constant(true), Expression.Constant(true)));
            }

            return result!;
        }

        return CreateJoinPredicate(outerKey, innerKey);
    }

    private SqlExpression CreateJoinPredicate(Expression outerKey, Expression innerKey)
        => TranslateExpression(Expression.Equal(outerKey, innerKey))!;

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateLastOrDefault(
        ShapedQueryExpression source,
        LambdaExpression? predicate,
        Type returnType,
        bool returnDefault)
    {
        var selectExpression = (SelectExpression)source.QueryExpression;
        if (selectExpression.Orderings.Count == 0)
        {
            throw new InvalidOperationException(
                RelationalStrings.LastUsedWithoutOrderBy(returnDefault ? nameof(Queryable.LastOrDefault) : nameof(Queryable.Last)));
        }

        if (predicate != null)
        {
            var translatedSource = TranslateWhere(source, predicate);
            if (translatedSource == null)
            {
                return null;
            }

            source = translatedSource;
        }

        selectExpression.ReverseOrderings();
        selectExpression.ApplyLimit(TranslateExpression(Expression.Constant(1))!);

        return source.ShaperExpression.Type != returnType
            ? source.UpdateShaperExpression(Expression.Convert(source.ShaperExpression, returnType))
            : source;
    }

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateLongCount(ShapedQueryExpression source, LambdaExpression? predicate)
        => TranslateAggregateWithPredicate(source, predicate, e => _sqlTranslator.TranslateLongCount(e), typeof(long));

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateMax(ShapedQueryExpression source, LambdaExpression? selector, Type resultType)
        => TranslateAggregateWithSelector(source, selector, e => _sqlTranslator.TranslateMax(e), throwWhenEmpty: true, resultType);

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateMin(ShapedQueryExpression source, LambdaExpression? selector, Type resultType)
        => TranslateAggregateWithSelector(source, selector, e => _sqlTranslator.TranslateMin(e), throwWhenEmpty: true, resultType);

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateOfType(ShapedQueryExpression source, Type resultType)
    {
        if (source.ShaperExpression is EntityShaperExpression entityShaperExpression)
        {
            var entityType = entityShaperExpression.EntityType;
            if (entityType.ClrType == resultType)
            {
                return source;
            }

            var parameterExpression = Expression.Parameter(entityShaperExpression.Type);
            var predicate = Expression.Lambda(Expression.TypeIs(parameterExpression, resultType), parameterExpression);
            var translation = TranslateLambdaExpression(source, predicate);
            if (translation == null)
            {
                // EntityType is not part of hierarchy
                return null;
            }

            var selectExpression = (SelectExpression)source.QueryExpression;
            if (!(translation is SqlConstantExpression sqlConstantExpression
                    && sqlConstantExpression.Value is bool constantValue
                    && constantValue))
            {
                selectExpression.ApplyPredicate(translation);
            }

            var baseType = entityType.GetAllBaseTypes().SingleOrDefault(et => et.ClrType == resultType);
            if (baseType != null)
            {
                return source.UpdateShaperExpression(entityShaperExpression.WithEntityType(baseType));
            }

            var derivedType = entityType.GetDerivedTypes().Single(et => et.ClrType == resultType);
            var projectionBindingExpression = (ProjectionBindingExpression)entityShaperExpression.ValueBufferExpression;

            var projectionMember = projectionBindingExpression.ProjectionMember;
            Check.DebugAssert(new ProjectionMember().Equals(projectionMember), "Invalid ProjectionMember when processing OfType");

            var entityProjectionExpression = (EntityProjectionExpression)selectExpression.GetProjection(projectionBindingExpression);
            selectExpression.ReplaceProjection(
                new Dictionary<ProjectionMember, Expression>
                {
                    { projectionMember, entityProjectionExpression.UpdateEntityType(derivedType) }
                });

            return source.UpdateShaperExpression(entityShaperExpression.WithEntityType(derivedType));
        }

        return null;
    }

    /// <inheritdoc />
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

        ((SelectExpression)source.QueryExpression).ApplyOrdering(new OrderingExpression(translation, ascending));

        return source;
    }

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateReverse(ShapedQueryExpression source)
    {
        var selectExpression = (SelectExpression)source.QueryExpression;
        if (selectExpression.Orderings.Count == 0)
        {
            AddTranslationErrorDetails(RelationalStrings.MissingOrderingInSelectExpression);
            return null;
        }

        selectExpression.ReverseOrderings();

        return source;
    }

    /// <inheritdoc />
    protected override ShapedQueryExpression TranslateSelect(ShapedQueryExpression source, LambdaExpression selector)
    {
        if (selector.Body == selector.Parameters[0])
        {
            return source;
        }

        var selectExpression = (SelectExpression)source.QueryExpression;
        if (selectExpression.IsDistinct)
        {
            selectExpression.PushdownIntoSubquery();
        }

        var newSelectorBody = RemapLambdaBody(source, selector);

        return source.UpdateShaperExpression(_projectionBindingExpressionVisitor.Translate(selectExpression, newSelectorBody));
    }

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateSelectMany(
        ShapedQueryExpression source,
        LambdaExpression collectionSelector,
        LambdaExpression resultSelector)
    {
        var (newCollectionSelector, correlated, defaultIfEmpty)
            = new CorrelationFindingExpressionVisitor().IsCorrelated(collectionSelector);
        if (correlated)
        {
            var collectionSelectorBody = RemapLambdaBody(source, newCollectionSelector);
            if (Visit(collectionSelectorBody) is ShapedQueryExpression inner)
            {
                var innerSelectExpression = (SelectExpression)source.QueryExpression;
                var shaper = defaultIfEmpty
                    ? innerSelectExpression.AddOuterApply(inner, source.ShaperExpression)
                    : innerSelectExpression.AddCrossApply(inner, source.ShaperExpression);

                return TranslateTwoParameterSelector(source.UpdateShaperExpression(shaper), resultSelector);
            }
        }
        else
        {
            if (Visit(newCollectionSelector.Body) is ShapedQueryExpression inner)
            {
                if (defaultIfEmpty)
                {
                    var translatedInner = TranslateDefaultIfEmpty(inner, null);
                    if (translatedInner == null)
                    {
                        return null;
                    }

                    inner = translatedInner;
                }

                var innerSelectExpression = (SelectExpression)source.QueryExpression;
                var shaper = innerSelectExpression.AddCrossJoin(inner, source.ShaperExpression);

                return TranslateTwoParameterSelector(source.UpdateShaperExpression(shaper), resultSelector);
            }
        }

        return null;
    }

    private sealed class CorrelationFindingExpressionVisitor : ExpressionVisitor
    {
        private ParameterExpression? _outerParameter;
        private bool _correlated;
        private bool _defaultIfEmpty;

        public (LambdaExpression, bool, bool) IsCorrelated(LambdaExpression lambdaExpression)
        {
            Check.DebugAssert(
                lambdaExpression.Parameters.Count == 1, "Multiparameter lambda passed to CorrelationFindingExpressionVisitor");

            _correlated = false;
            _defaultIfEmpty = false;
            _outerParameter = lambdaExpression.Parameters[0];

            var result = Visit(lambdaExpression.Body);

            return (Expression.Lambda(result, _outerParameter), _correlated, _defaultIfEmpty);
        }

        protected override Expression VisitParameter(ParameterExpression parameterExpression)
        {
            if (parameterExpression == _outerParameter)
            {
                _correlated = true;
            }

            return base.VisitParameter(parameterExpression);
        }

        protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
        {
            if (methodCallExpression.Method.IsGenericMethod
                && methodCallExpression.Method.GetGenericMethodDefinition() == QueryableMethods.DefaultIfEmptyWithoutArgument)
            {
                _defaultIfEmpty = true;
                return Visit(methodCallExpression.Arguments[0]);
            }

            return base.VisitMethodCall(methodCallExpression);
        }
    }

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateSelectMany(ShapedQueryExpression source, LambdaExpression selector)
    {
        var innerParameter = Expression.Parameter(selector.ReturnType.GetSequenceType(), "i");
        var resultSelector = Expression.Lambda(
            innerParameter, Expression.Parameter(source.Type.GetSequenceType()), innerParameter);

        return TranslateSelectMany(source, selector, resultSelector);
    }

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateSingleOrDefault(
        ShapedQueryExpression source,
        LambdaExpression? predicate,
        Type returnType,
        bool returnDefault)
    {
        if (predicate != null)
        {
            var translatedSource = TranslateWhere(source, predicate);
            if (translatedSource == null)
            {
                return null;
            }

            source = translatedSource;
        }

        var selectExpression = (SelectExpression)source.QueryExpression;
        selectExpression.ApplyLimit(TranslateExpression(Expression.Constant(_subquery ? 1 : 2))!);

        return source.ShaperExpression.Type != returnType
            ? source.UpdateShaperExpression(Expression.Convert(source.ShaperExpression, returnType))
            : source;
    }

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateSkip(ShapedQueryExpression source, Expression count)
    {
        var selectExpression = (SelectExpression)source.QueryExpression;
        var translation = TranslateExpression(count);
        if (translation == null)
        {
            return null;
        }

        if (selectExpression.Orderings.Count == 0)
        {
            _queryCompilationContext.Logger.RowLimitingOperationWithoutOrderByWarning();
        }

        selectExpression.ApplyOffset(translation);

        return source;
    }

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateSkipWhile(ShapedQueryExpression source, LambdaExpression predicate)
        => null;

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateSum(ShapedQueryExpression source, LambdaExpression? selector, Type resultType)
        => TranslateAggregateWithSelector(source, selector, e => _sqlTranslator.TranslateSum(e), throwWhenEmpty: false, resultType);

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateTake(ShapedQueryExpression source, Expression count)
    {
        var selectExpression = (SelectExpression)source.QueryExpression;
        var translation = TranslateExpression(count);
        if (translation == null)
        {
            return null;
        }

        if (selectExpression.Orderings.Count == 0)
        {
            _queryCompilationContext.Logger.RowLimitingOperationWithoutOrderByWarning();
        }

        selectExpression.ApplyLimit(translation);

        return source;
    }

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateTakeWhile(ShapedQueryExpression source, LambdaExpression predicate)
        => null;

    /// <inheritdoc />
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

        ((SelectExpression)source.QueryExpression).AppendOrdering(new OrderingExpression(translation, ascending));

        return source;
    }

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateUnion(ShapedQueryExpression source1, ShapedQueryExpression source2)
    {
        ((SelectExpression)source1.QueryExpression).ApplyUnion((SelectExpression)source2.QueryExpression, distinct: true);

        return source1.UpdateShaperExpression(
            MatchShaperNullabilityForSetOperation(source1.ShaperExpression, source2.ShaperExpression, makeNullable: true));
    }

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateWhere(ShapedQueryExpression source, LambdaExpression predicate)
    {
        var translation = TranslateLambdaExpression(source, predicate);
        if (translation == null)
        {
            return null;
        }

        ((SelectExpression)source.QueryExpression).ApplyPredicate(translation);

        return source;
    }

    private SqlExpression? TranslateExpression(Expression expression)
    {
        var translation = _sqlTranslator.Translate(expression);
        if (translation == null && _sqlTranslator.TranslationErrorDetails != null)
        {
            AddTranslationErrorDetails(_sqlTranslator.TranslationErrorDetails);
        }

        return translation;
    }

    private SqlExpression? TranslateLambdaExpression(
        ShapedQueryExpression shapedQueryExpression,
        LambdaExpression lambdaExpression)
        => TranslateExpression(RemapLambdaBody(shapedQueryExpression, lambdaExpression));

    private Expression RemapLambdaBody(ShapedQueryExpression shapedQueryExpression, LambdaExpression lambdaExpression)
    {
        var lambdaBody = ReplacingExpressionVisitor.Replace(
            lambdaExpression.Parameters.Single(), shapedQueryExpression.ShaperExpression, lambdaExpression.Body);

        return ExpandSharedTypeEntities((SelectExpression)shapedQueryExpression.QueryExpression, lambdaBody);
    }

    private Expression ExpandSharedTypeEntities(SelectExpression selectExpression, Expression lambdaBody)
        => _sharedTypeEntityExpandingExpressionVisitor.Expand(selectExpression, lambdaBody);

    private sealed class SharedTypeEntityExpandingExpressionVisitor : ExpressionVisitor
    {
        private static readonly MethodInfo ObjectEqualsMethodInfo
            = typeof(object).GetRuntimeMethod(nameof(object.Equals), new[] { typeof(object), typeof(object) })!;

        private readonly RelationalSqlTranslatingExpressionVisitor _sqlTranslator;
        private readonly ISqlExpressionFactory _sqlExpressionFactory;

        private SelectExpression _selectExpression;
        private DeferredOwnedExpansionRemovingVisitor _deferredOwnedExpansionRemover;

        public SharedTypeEntityExpandingExpressionVisitor(
            RelationalSqlTranslatingExpressionVisitor sqlTranslator,
            ISqlExpressionFactory sqlExpressionFactory)
        {
            _sqlTranslator = sqlTranslator;
            _sqlExpressionFactory = sqlExpressionFactory;
            _selectExpression = null!;
            _deferredOwnedExpansionRemover = null!;
        }

        public Expression Expand(SelectExpression selectExpression, Expression lambdaBody)
        {
            _selectExpression = selectExpression;
            _deferredOwnedExpansionRemover = new DeferredOwnedExpansionRemovingVisitor(_selectExpression);

            return _deferredOwnedExpansionRemover.Visit(Visit(lambdaBody));
        }

        protected override Expression VisitMember(MemberExpression memberExpression)
        {
            var innerExpression = Visit(memberExpression.Expression);

            return TryExpand(innerExpression, MemberIdentity.Create(memberExpression.Member))
                ?? memberExpression.Update(innerExpression);
        }

        protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
        {
            if (methodCallExpression.TryGetEFPropertyArguments(out var source, out var navigationName))
            {
                source = Visit(source);

                return TryExpand(source, MemberIdentity.Create(navigationName))
                    ?? methodCallExpression.Update(null!, new[] { source, methodCallExpression.Arguments[1] });
            }

            return base.VisitMethodCall(methodCallExpression);
        }

        protected override Expression VisitExtension(Expression extensionExpression)
            => extensionExpression is EntityShaperExpression
                || extensionExpression is ShapedQueryExpression
                    ? extensionExpression
                    : base.VisitExtension(extensionExpression);

        private Expression? TryExpand(Expression? source, MemberIdentity member)
        {
            source = source.UnwrapTypeConversion(out var convertedType);
            var doee = source as DeferredOwnedExpansionExpression;
            if (doee is not null)
            {
                source = _deferredOwnedExpansionRemover.UnwrapDeferredEntityProjectionExpression(doee);
            }

            if (source is not EntityShaperExpression entityShaperExpression)
            {
                return null;
            }

            var entityType = entityShaperExpression.EntityType;
            if (convertedType != null)
            {
                entityType = entityType.GetRootType().GetDerivedTypesInclusive()
                    .FirstOrDefault(et => et.ClrType == convertedType);

                if (entityType == null)
                {
                    return null;
                }
            }

            var navigation = member.MemberInfo != null
                ? entityType.FindNavigation(member.MemberInfo)
                : entityType.FindNavigation(member.Name!);

            if (navigation == null)
            {
                return null;
            }

            var targetEntityType = navigation.TargetEntityType;
            if (targetEntityType == null
                || !targetEntityType.IsOwned())
            {
                return null;
            }

            var foreignKey = navigation.ForeignKey;
            if (navigation.IsCollection)
            {
                var innerShapedQuery = CreateShapedQueryExpression(
                    targetEntityType, _sqlExpressionFactory.Select(targetEntityType));

                var makeNullable = foreignKey.PrincipalKey.Properties
                    .Concat(foreignKey.Properties)
                    .Select(p => p.ClrType)
                    .Any(t => t.IsNullableType());

                var innerSequenceType = innerShapedQuery.Type.GetSequenceType();
                var correlationPredicateParameter = Expression.Parameter(innerSequenceType);

                var outerKey = entityShaperExpression.CreateKeyValuesExpression(
                    navigation.IsOnDependent
                        ? foreignKey.Properties
                        : foreignKey.PrincipalKey.Properties,
                    makeNullable);
                var innerKey = correlationPredicateParameter.CreateKeyValuesExpression(
                    navigation.IsOnDependent
                        ? foreignKey.PrincipalKey.Properties
                        : foreignKey.Properties,
                    makeNullable);

                var keyComparison = Expression.Call(
                    ObjectEqualsMethodInfo, AddConvertToObject(outerKey), AddConvertToObject(innerKey));

                var predicate = makeNullable
                    ? Expression.AndAlso(
                        outerKey is NewArrayExpression newArrayExpression
                            ? newArrayExpression.Expressions
                                .Select(
                                    e =>
                                    {
                                        var left = (e as UnaryExpression)?.Operand ?? e;

                                        return Expression.NotEqual(left, Expression.Constant(null, left.Type));
                                    })
                                .Aggregate((l, r) => Expression.AndAlso(l, r))
                            : Expression.NotEqual(outerKey, Expression.Constant(null, outerKey.Type)),
                        keyComparison)
                    : (Expression)keyComparison;

                var correlationPredicate = Expression.Lambda(predicate, correlationPredicateParameter);

                return Expression.Call(
                    QueryableMethods.Where.MakeGenericMethod(innerSequenceType),
                    innerShapedQuery,
                    Expression.Quote(correlationPredicate));
            }

            var entityProjectionExpression = GetEntityProjectionExpression(entityShaperExpression);
            var innerShaper = entityProjectionExpression.BindNavigation(navigation);
            if (innerShaper == null)
            {
                // Owned types don't support inheritance See https://github.com/dotnet/efcore/issues/9630
                // So there is no handling for dependent having TPT
                // If navigation is defined on derived type and entity type is part of TPT then we need to get ITableBase for derived type.
                // TODO: The following code should also handle Function and SqlQuery mappings
                var table = navigation.DeclaringEntityType.BaseType == null
                    || entityType.FindDiscriminatorProperty() != null
                        ? navigation.DeclaringEntityType.GetViewOrTableMappings().Single().Table
                        : navigation.DeclaringEntityType.GetViewOrTableMappings().Select(tm => tm.Table)
                            .Except(navigation.DeclaringEntityType.BaseType.GetViewOrTableMappings().Select(tm => tm.Table))
                            .Single();
                if (table.GetReferencingRowInternalForeignKeys(foreignKey.PrincipalEntityType)?.Contains(foreignKey) == true)
                {
                    // Mapped to same table
                    // We get identifying column to figure out tableExpression to pull columns from and nullability of most principal side
                    var identifyingColumn = entityProjectionExpression.BindProperty(entityType.FindPrimaryKey()!.Properties.First());
                    var principalNullable = identifyingColumn.IsNullable
                        // Also make nullable if navigation is on derived type and and principal is TPT
                        // Since identifying PK would be non-nullable but principal can still be null
                        // Derived owned navigation does not de-dupe the PK column which for principal is from base table
                        // and for dependent on derived table
                        || (entityType.FindDiscriminatorProperty() == null
                            && navigation.DeclaringEntityType.IsStrictlyDerivedFrom(entityShaperExpression.EntityType));

                    var entityProjection = _selectExpression.GenerateWeakEntityProjectionExpression(
                        targetEntityType, table, identifyingColumn.Name, identifyingColumn.Table, principalNullable);

                    if (entityProjection != null)
                    {
                        innerShaper = new RelationalEntityShaperExpression(targetEntityType, entityProjection, principalNullable);
                    }
                }

                if (innerShaper == null)
                {
                    // InnerShaper is still null if either it is not table sharing or we failed to find table to pick data from
                    // So we find the table it is mapped to and generate join with it.
                    // Owned types don't support inheritance See https://github.com/dotnet/efcore/issues/9630
                    // So there is no handling for dependent having TPT
                    table = targetEntityType.GetViewOrTableMappings().Single().Table;
                    var innerSelectExpression = _sqlExpressionFactory.Select(targetEntityType);
                    var innerShapedQuery = CreateShapedQueryExpression(targetEntityType, innerSelectExpression);

                    var makeNullable = foreignKey.PrincipalKey.Properties
                        .Concat(foreignKey.Properties)
                        .Select(p => p.ClrType)
                        .Any(t => t.IsNullableType());

                    var outerKey = entityShaperExpression.CreateKeyValuesExpression(
                        navigation.IsOnDependent
                            ? foreignKey.Properties
                            : foreignKey.PrincipalKey.Properties,
                        makeNullable);
                    var innerKey = innerShapedQuery.ShaperExpression.CreateKeyValuesExpression(
                        navigation.IsOnDependent
                            ? foreignKey.PrincipalKey.Properties
                            : foreignKey.Properties,
                        makeNullable);

                    var joinPredicate = _sqlTranslator.Translate(Expression.Equal(outerKey, innerKey))!;
                    // Following conditions should match conditions for pushdown on outer during SelectExpression.AddJoin method
                    var pushdownRequired = _selectExpression.Limit != null
                        || _selectExpression.Offset != null
                        || _selectExpression.IsDistinct
                        || _selectExpression.GroupBy.Count > 0;
                    _selectExpression.AddLeftJoin(innerSelectExpression, joinPredicate);

                    // If pushdown was required on SelectExpression then we need to fetch the updated entity projection
                    if (pushdownRequired)
                    {
                        if (doee is not null)
                        {
                            entityShaperExpression = _deferredOwnedExpansionRemover.UnwrapDeferredEntityProjectionExpression(doee);
                        }

                        entityProjectionExpression = GetEntityProjectionExpression(entityShaperExpression);
                    }

                    var leftJoinTable = _selectExpression.Tables.Last();

                    innerShaper = new RelationalEntityShaperExpression(
                        targetEntityType,
                        _selectExpression.GenerateWeakEntityProjectionExpression(
                            targetEntityType, table, null, leftJoinTable, nullable: true)!,
                        nullable: true);
                }

                entityProjectionExpression.AddNavigationBinding(navigation, innerShaper);
            }

            return doee is not null
                ? doee.AddNavigation(targetEntityType, navigation)
                : new DeferredOwnedExpansionExpression(
                    targetEntityType,
                    (ProjectionBindingExpression)entityShaperExpression.ValueBufferExpression,
                    navigation);
        }

        private static Expression AddConvertToObject(Expression expression)
            => expression.Type.IsValueType
                ? Expression.Convert(expression, typeof(object))
                : expression;

        private EntityProjectionExpression GetEntityProjectionExpression(EntityShaperExpression entityShaperExpression)
            => entityShaperExpression.ValueBufferExpression switch
            {
                ProjectionBindingExpression projectionBindingExpression
                    => (EntityProjectionExpression)_selectExpression.GetProjection(projectionBindingExpression),
                EntityProjectionExpression entityProjectionExpression => entityProjectionExpression,
                _ => throw new InvalidOperationException()
            };

        private sealed class DeferredOwnedExpansionExpression : Expression
        {
            private readonly IEntityType _entityType;

            public DeferredOwnedExpansionExpression(
                IEntityType entityType,
                ProjectionBindingExpression projectionBindingExpression,
                INavigation navigation)
            {
                _entityType = entityType;
                ProjectionBindingExpression = projectionBindingExpression;
                NavigationChain = new List<INavigation> { navigation };
            }

            private DeferredOwnedExpansionExpression(
                IEntityType entityType,
                ProjectionBindingExpression projectionBindingExpression,
                List<INavigation> navigationChain)
            {
                _entityType = entityType;
                ProjectionBindingExpression = projectionBindingExpression;
                NavigationChain = navigationChain;
            }

            public ProjectionBindingExpression ProjectionBindingExpression { get; }
            public List<INavigation> NavigationChain { get; }

            public DeferredOwnedExpansionExpression AddNavigation(IEntityType entityType, INavigation navigation)
            {
                var navigationChain = new List<INavigation>(NavigationChain.Count + 1);
                navigationChain.AddRange(NavigationChain);
                navigationChain.Add(navigation);

                return new DeferredOwnedExpansionExpression(
                    entityType,
                    ProjectionBindingExpression,
                    navigationChain);
            }

            public override Type Type
                => _entityType.ClrType;

            public override ExpressionType NodeType
                => ExpressionType.Extension;
        }

        private sealed class DeferredOwnedExpansionRemovingVisitor : ExpressionVisitor
        {
            private readonly SelectExpression _selectExpression;

            public DeferredOwnedExpansionRemovingVisitor(SelectExpression selectExpression)
            {
                _selectExpression = selectExpression;
            }

            [return: NotNullIfNotNull("expression")]
            public override Expression? Visit(Expression? expression)
                => expression switch
                {
                    DeferredOwnedExpansionExpression doee => UnwrapDeferredEntityProjectionExpression(doee),
                    // For the source entity shaper or owned collection expansion
                    EntityShaperExpression or ShapedQueryExpression => expression,
                    _ => base.Visit(expression)
                };

            public EntityShaperExpression UnwrapDeferredEntityProjectionExpression(DeferredOwnedExpansionExpression doee)
            {
                var entityProjection = (EntityProjectionExpression)_selectExpression.GetProjection(doee.ProjectionBindingExpression);
                var entityShaper = entityProjection.BindNavigation(doee.NavigationChain[0])!;

                for (var i = 1; i < doee.NavigationChain.Count; i++)
                {
                    entityProjection = (EntityProjectionExpression)entityShaper.ValueBufferExpression;
                    entityShaper = entityProjection.BindNavigation(doee.NavigationChain[i])!;
                }

                return entityShaper;
            }
        }
    }

    private ShapedQueryExpression TranslateTwoParameterSelector(ShapedQueryExpression source, LambdaExpression resultSelector)
    {
        var transparentIdentifierType = source.ShaperExpression.Type;
        var transparentIdentifierParameter = Expression.Parameter(transparentIdentifierType);

        Expression original1 = resultSelector.Parameters[0];
        var replacement1 = AccessField(transparentIdentifierType, transparentIdentifierParameter, "Outer");
        Expression original2 = resultSelector.Parameters[1];
        var replacement2 = AccessField(transparentIdentifierType, transparentIdentifierParameter, "Inner");
        var newResultSelector = Expression.Lambda(
            new ReplacingExpressionVisitor(
                    new[] { original1, original2 }, new[] { replacement1, replacement2 })
                .Visit(resultSelector.Body),
            transparentIdentifierParameter);

        return TranslateSelect(source, newResultSelector);
    }

    private static Expression AccessField(
        Type transparentIdentifierType,
        Expression targetExpression,
        string fieldName)
        => Expression.Field(targetExpression, transparentIdentifierType.GetTypeInfo().GetDeclaredField(fieldName)!);

    private static void HandleGroupByForAggregate(SelectExpression selectExpression, bool eraseProjection = false)
    {
        if (selectExpression.GroupBy.Count > 0)
        {
            if (eraseProjection)
            {
                selectExpression.ReplaceProjection(new Dictionary<ProjectionMember, Expression>());
            }

            selectExpression.PushdownIntoSubquery();
        }
    }

    private static Expression MatchShaperNullabilityForSetOperation(Expression shaper1, Expression shaper2, bool makeNullable)
    {
        switch (shaper1)
        {
            case EntityShaperExpression entityShaperExpression1
                when shaper2 is EntityShaperExpression entityShaperExpression2:
                return entityShaperExpression1.IsNullable != entityShaperExpression2.IsNullable
                    ? entityShaperExpression1.MakeNullable(makeNullable)
                    : entityShaperExpression1;

            case NewExpression newExpression1
                when shaper2 is NewExpression newExpression2:
                var newArguments = new Expression[newExpression1.Arguments.Count];
                for (var i = 0; i < newArguments.Length; i++)
                {
                    newArguments[i] = MatchShaperNullabilityForSetOperation(
                        newExpression1.Arguments[i], newExpression2.Arguments[i], makeNullable);
                }

                return newExpression1.Update(newArguments);

            case MemberInitExpression memberInitExpression1
                when shaper2 is MemberInitExpression memberInitExpression2:
                var newExpression = (NewExpression)MatchShaperNullabilityForSetOperation(
                    memberInitExpression1.NewExpression, memberInitExpression2.NewExpression, makeNullable);

                var memberBindings = new MemberBinding[memberInitExpression1.Bindings.Count];
                for (var i = 0; i < memberBindings.Length; i++)
                {
                    var memberAssignment = memberInitExpression1.Bindings[i] as MemberAssignment;
                    Check.DebugAssert(memberAssignment != null, "Only member assignment bindings are supported");

                    memberBindings[i] = memberAssignment.Update(
                        MatchShaperNullabilityForSetOperation(
                            memberAssignment.Expression, ((MemberAssignment)memberInitExpression2.Bindings[i]).Expression,
                            makeNullable));
                }

                return memberInitExpression1.Update(newExpression, memberBindings);

            default:
                return shaper1;
        }
    }

    private ShapedQueryExpression? TranslateAggregateWithPredicate(
        ShapedQueryExpression source,
        LambdaExpression? predicate,
        Func<SqlExpression, SqlExpression?> aggregateTranslator,
        Type resultType)
    {
        var selectExpression = (SelectExpression)source.QueryExpression;
        if (_groupingElementCorrelationalPredicate == null)
        {
            selectExpression.PrepareForAggregate();
        }

        if (predicate != null)
        {
            var translatedSource = TranslateWhere(source, predicate);
            if (translatedSource == null)
            {
                return null;
            }

            source = translatedSource;
        }

        SqlExpression sqlExpression = _sqlExpressionFactory.Fragment("*");

        if (_groupingElementCorrelationalPredicate != null)
        {
            if (selectExpression.IsDistinct)
            {
                var shaperExpression = source.ShaperExpression;
                if (shaperExpression is UnaryExpression unaryExpression
                    && unaryExpression.NodeType == ExpressionType.Convert)
                {
                    shaperExpression = unaryExpression.Operand;
                }

                if (shaperExpression is ProjectionBindingExpression projectionBindingExpression)
                {
                    sqlExpression = (SqlExpression)selectExpression.GetProjection(projectionBindingExpression);
                }
                else
                {
                    return null;
                }
            }

            sqlExpression = CombineGroupByAggregateTerms(selectExpression, sqlExpression);
        }
        else
        {
            HandleGroupByForAggregate(selectExpression, eraseProjection: true);
        }

        var translation = aggregateTranslator(sqlExpression);
        if (translation == null)
        {
            return null;
        }

        var projectionMapping = new Dictionary<ProjectionMember, Expression> { { new ProjectionMember(), translation } };

        selectExpression.ClearOrdering();
        selectExpression.ReplaceProjection(projectionMapping);

        return source.UpdateShaperExpression(
            Expression.Convert(
                new ProjectionBindingExpression(source.QueryExpression, new ProjectionMember(), resultType.MakeNullable()),
                resultType));
    }

    private ShapedQueryExpression? TranslateAggregateWithSelector(
        ShapedQueryExpression source,
        LambdaExpression? selector,
        Func<SqlExpression, SqlExpression?> aggregateTranslator,
        bool throwWhenEmpty,
        Type resultType)
    {
        var selectExpression = (SelectExpression)source.QueryExpression;
        if (_groupingElementCorrelationalPredicate == null)
        {
            selectExpression.PrepareForAggregate();
            HandleGroupByForAggregate(selectExpression);
        }

        SqlExpression translatedSelector;
        if (selector == null
            || selector.Body == selector.Parameters[0])
        {
            var shaperExpression = source.ShaperExpression;
            if (shaperExpression is UnaryExpression unaryExpression
                && unaryExpression.NodeType == ExpressionType.Convert)
            {
                shaperExpression = unaryExpression.Operand;
            }

            if (shaperExpression is ProjectionBindingExpression projectionBindingExpression)
            {
                translatedSelector = (SqlExpression)selectExpression.GetProjection(projectionBindingExpression);
            }
            else
            {
                return null;
            }
        }
        else
        {
            var newSelector = RemapLambdaBody(source, selector);
            if (TranslateExpression(newSelector) is SqlExpression sqlExpression)
            {
                translatedSelector = sqlExpression;
            }
            else
            {
                return null;
            }
        }

        if (_groupingElementCorrelationalPredicate != null)
        {
            translatedSelector = CombineGroupByAggregateTerms(selectExpression, translatedSelector);
        }

        var projection = aggregateTranslator(translatedSelector);
        if (projection == null)
        {
            return null;
        }

        selectExpression.ReplaceProjection(
            new Dictionary<ProjectionMember, Expression> { { new ProjectionMember(), projection } });

        selectExpression.ClearOrdering();
        Expression shaper;

        if (throwWhenEmpty)
        {
            // Avg/Max/Min case.
            // We always read nullable value
            // If resultType is nullable then we always return null. Only non-null result shows throwing behavior.
            // otherwise, if projection.Type is nullable then server result is passed through DefaultIfEmpty, hence we return default
            // otherwise, server would return null only if it is empty, and we throw
            var nullableResultType = resultType.MakeNullable();
            shaper = new ProjectionBindingExpression(source.QueryExpression, new ProjectionMember(), nullableResultType);
            var resultVariable = Expression.Variable(nullableResultType, "result");
            var returnValueForNull = resultType.IsNullableType()
                ? (Expression)Expression.Constant(null, resultType)
                : projection.Type.IsNullableType()
                    ? Expression.Default(resultType)
                    : Expression.Throw(
                        Expression.New(
                            typeof(InvalidOperationException).GetConstructors()
                                .Single(ci => ci.GetParameters().Length == 1),
                            Expression.Constant(CoreStrings.SequenceContainsNoElements)),
                        resultType);

            shaper = Expression.Block(
                new[] { resultVariable },
                Expression.Assign(resultVariable, shaper),
                Expression.Condition(
                    Expression.Equal(resultVariable, Expression.Default(nullableResultType)),
                    returnValueForNull,
                    resultType != resultVariable.Type
                        ? Expression.Convert(resultVariable, resultType)
                        : resultVariable));
        }
        else
        {
            // Sum case. Projection is always non-null. We read nullable value.
            shaper = new ProjectionBindingExpression(source.QueryExpression, new ProjectionMember(), projection.Type.MakeNullable());

            if (resultType != shaper.Type)
            {
                shaper = Expression.Convert(shaper, resultType);
            }
        }

        return source.UpdateShaperExpression(shaper);
    }

    private SqlExpression CombineGroupByAggregateTerms(SelectExpression selectExpression, SqlExpression selector)
    {
        if (selectExpression.Predicate != null
            && !selectExpression.Predicate.Equals(_groupingElementCorrelationalPredicate))
        {
            if (selector is SqlFragmentExpression { Sql: "*" })
            {
                selector = _sqlExpressionFactory.Constant(1);
            }

            var correlationTerms = new List<SqlExpression>();
            var predicateTerms = new List<SqlExpression>();
            PopulatePredicateTerms(_groupingElementCorrelationalPredicate!, correlationTerms);
            PopulatePredicateTerms(selectExpression.Predicate, predicateTerms);
            var predicate = predicateTerms.Skip(correlationTerms.Count)
                .Aggregate((l, r) => _sqlExpressionFactory.AndAlso(l, r));
            selector = _sqlExpressionFactory.Case(
                new List<CaseWhenClause> { new(predicate, selector) },
                elseResult: null);
        }

        if (selectExpression.IsDistinct)
        {
            if (selector is SqlFragmentExpression { Sql: "*" })
            {
                selector = _sqlExpressionFactory.Constant(1);
            }

            selector = new DistinctExpression(selector);
        }

        return selector;

        static void PopulatePredicateTerms(SqlExpression predicate, List<SqlExpression> terms)
        {
            if (predicate is SqlBinaryExpression { OperatorType: ExpressionType.AndAlso } sqlBinaryExpression)
            {
                PopulatePredicateTerms(sqlBinaryExpression.Left, terms);
                PopulatePredicateTerms(sqlBinaryExpression.Right, terms);
            }
            else
            {
                terms.Add(predicate);
            }
        }
    }
}
