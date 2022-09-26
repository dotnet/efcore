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
                            fromSqlQueryRootExpression.EntityType.GetDefaultMappings().Single().Table,
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

            case EntityQueryRootExpression entityQueryRootExpression
                when entityQueryRootExpression.GetType() == typeof(EntityQueryRootExpression)
                && entityQueryRootExpression.EntityType.GetSqlQueryMappings().FirstOrDefault(m => m.IsDefaultSqlQueryMapping)?.SqlQuery is
                    ISqlQuery sqlQuery:
                return CreateShapedQueryExpression(
                    entityQueryRootExpression.EntityType,
                    _sqlExpressionFactory.Select(
                        entityQueryRootExpression.EntityType,
                        new FromSqlExpression(
                            entityQueryRootExpression.EntityType.GetDefaultMappings().Single().Table,
                            sqlQuery.Sql,
                            Expression.Constant(Array.Empty<object>(), typeof(object[])))));

            case GroupByShaperExpression groupByShaperExpression:
                var groupShapedQueryExpression = groupByShaperExpression.GroupingEnumerable;
                var groupClonedSelectExpression = ((SelectExpression)groupShapedQueryExpression.QueryExpression).Clone();
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

            case SqlQueryRootExpression sqlQueryRootExpression:
                var typeMapping = RelationalDependencies.TypeMappingSource.FindMapping(sqlQueryRootExpression.ElementType);
                if (typeMapping == null)
                {
                    throw new InvalidOperationException(
                        RelationalStrings.SqlQueryUnmappedType(sqlQueryRootExpression.ElementType.DisplayName()));
                }

                var selectExpression = new SelectExpression(
                    sqlQueryRootExpression.Type, typeMapping,
                    new FromSqlExpression("t", sqlQueryRootExpression.Sql, sqlQueryRootExpression.Argument));

                Expression shaperExpression = new ProjectionBindingExpression(
                    selectExpression, new ProjectionMember(), sqlQueryRootExpression.ElementType.MakeNullable());

                if (sqlQueryRootExpression.ElementType != shaperExpression.Type)
                {
                    Check.DebugAssert(
                        sqlQueryRootExpression.ElementType.MakeNullable() == shaperExpression.Type,
                        "expression.Type must be nullable of targetType");

                    shaperExpression = Expression.Convert(shaperExpression, sqlQueryRootExpression.ElementType);
                }

                return new ShapedQueryExpression(selectExpression, shaperExpression);

            default:
                return base.VisitExtension(extensionExpression);
        }
    }

    /// <inheritdoc />
    protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
    {
        var method = methodCallExpression.Method;
        if (method.DeclaringType == typeof(RelationalQueryableExtensions))
        {
            var source = Visit(methodCallExpression.Arguments[0]);
            if (source is ShapedQueryExpression shapedQueryExpression)
            {
                var genericMethod = method.IsGenericMethod ? method.GetGenericMethodDefinition() : null;
                switch (method.Name)
                {
                    case nameof(RelationalQueryableExtensions.ExecuteDelete)
                        when genericMethod == RelationalQueryableExtensions.ExecuteDeleteMethodInfo:
                        return TranslateExecuteDelete(shapedQueryExpression)
                            ?? throw new InvalidOperationException(
                                RelationalStrings.NonQueryTranslationFailedWithDetails(
                                    methodCallExpression.Print(), TranslationErrorDetails));

                    case nameof(RelationalQueryableExtensions.ExecuteUpdate)
                        when genericMethod == RelationalQueryableExtensions.ExecuteUpdateMethodInfo:
                        return TranslateExecuteUpdate(shapedQueryExpression, methodCallExpression.Arguments[1].UnwrapLambdaFromQuote())
                            ?? throw new InvalidOperationException(
                                RelationalStrings.NonQueryTranslationFailedWithDetails(
                                    methodCallExpression.Print(), TranslationErrorDetails));
                }
            }
        }

        return base.VisitMethodCall(methodCallExpression);
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
        selectExpression.ReplaceProjection(new List<Expression>());
        selectExpression.ApplyProjection();
        if (selectExpression.Limit == null
            && selectExpression.Offset == null)
        {
            selectExpression.ClearOrdering();
        }

        translation = _sqlExpressionFactory.Exists(selectExpression, true);
        selectExpression = _sqlExpressionFactory.Select(translation);

        return source.Update(
            selectExpression,
            Expression.Convert(new ProjectionBindingExpression(selectExpression, new ProjectionMember(), typeof(bool?)), typeof(bool)));
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
        selectExpression.ReplaceProjection(new List<Expression>());
        selectExpression.ApplyProjection();
        if (selectExpression.Limit == null
            && selectExpression.Offset == null)
        {
            selectExpression.ClearOrdering();
        }

        var translation = _sqlExpressionFactory.Exists(selectExpression, false);
        selectExpression = _sqlExpressionFactory.Select(translation);

        return source.Update(
            selectExpression,
            Expression.Convert(new ProjectionBindingExpression(selectExpression, new ProjectionMember(), typeof(bool?)), typeof(bool)));
    }

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateAverage(
        ShapedQueryExpression source,
        LambdaExpression? selector,
        Type resultType)
        => TranslateAggregateWithSelector(source, selector, QueryableMethods.GetAverageWithoutSelector, throwWhenEmpty: true, resultType);

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
                selectExpression.ReplaceProjection(new List<Expression> { sqlExpression });
                selectExpression.ApplyProjection();

                translation = _sqlExpressionFactory.In(translation, selectExpression, false);
                selectExpression = _sqlExpressionFactory.Select(translation);

                return source.Update(
                    selectExpression,
                    Expression.Convert(
                        new ProjectionBindingExpression(selectExpression, new ProjectionMember(), typeof(bool?)), typeof(bool)));
            }
        }

        return null;
    }

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateCount(ShapedQueryExpression source, LambdaExpression? predicate)
        => TranslateAggregateWithPredicate(source, predicate, QueryableMethods.CountWithoutPredicate);

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
        selectExpression.PrepareForAggregate();

        var remappedKeySelector = RemapLambdaBody(source, keySelector);
        var translatedKey = TranslateGroupingKey(remappedKeySelector);
        if (translatedKey == null)
        {
            // This could be group by entity type
            if (remappedKeySelector is not EntityShaperExpression
                { ValueBufferExpression: ProjectionBindingExpression pbe } ese)
            {
                // ValueBufferExpression can be JsonQuery, ProjectionBindingExpression, EntityProjection
                // We only allow ProjectionBindingExpression which represents a regular entity
                return null;
            }

            translatedKey = ese.Update(((SelectExpression)pbe.QueryExpression).GetProjection(pbe));
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
                new[] { groupByShaper.KeySelector, groupByShaper })
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
        => TranslateExpression(Infrastructure.ExpressionExtensions.CreateEqualsExpression(outerKey, innerKey))!;

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
        => TranslateAggregateWithPredicate(source, predicate, QueryableMethods.LongCountWithoutPredicate);

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateMax(ShapedQueryExpression source, LambdaExpression? selector, Type resultType)
        => TranslateAggregateWithSelector(
            source, selector, t => QueryableMethods.MaxWithoutSelector.MakeGenericMethod(t), throwWhenEmpty: true, resultType);

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateMin(ShapedQueryExpression source, LambdaExpression? selector, Type resultType)
        => TranslateAggregateWithSelector(
            source, selector, t => QueryableMethods.MinWithoutSelector.MakeGenericMethod(t), throwWhenEmpty: true, resultType);

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
        => TranslateAggregateWithSelector(source, selector, QueryableMethods.GetSumWithoutSelector, throwWhenEmpty: false, resultType);

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

    /// <summary>
    ///     Translates <see cref="RelationalQueryableExtensions.ExecuteDelete{TSource}(IQueryable{TSource})" /> method
    ///     over the given source.
    /// </summary>
    /// <param name="source">The shaped query on which the operator is applied.</param>
    /// <returns>The non query after translation.</returns>
    protected virtual NonQueryExpression? TranslateExecuteDelete(ShapedQueryExpression source)
    {
        if (source.ShaperExpression is IncludeExpression includeExpression)
        {
            source = source.UpdateShaperExpression(PruneOwnedIncludes(includeExpression));
        }

        if (source.ShaperExpression is not EntityShaperExpression entityShaperExpression)
        {
            AddTranslationErrorDetails(RelationalStrings.ExecuteDeleteOnNonEntityType);
            return null;
        }

        var entityType = entityShaperExpression.EntityType;
        var mappingStrategy = entityType.GetMappingStrategy();
        if (mappingStrategy == RelationalAnnotationNames.TptMappingStrategy)
        {
            AddTranslationErrorDetails(
                RelationalStrings.ExecuteOperationOnTPT(nameof(RelationalQueryableExtensions.ExecuteDelete), entityType.DisplayName()));
            return null;
        }

        if (mappingStrategy == RelationalAnnotationNames.TpcMappingStrategy
            && entityType.GetDirectlyDerivedTypes().Any())
        {
            // We allow TPC is it is leaf type
            AddTranslationErrorDetails(
                RelationalStrings.ExecuteOperationOnTPC(nameof(RelationalQueryableExtensions.ExecuteDelete), entityType.DisplayName()));
            return null;
        }

        if (entityType.GetViewOrTableMappings().Count() != 1)
        {
            AddTranslationErrorDetails(
                RelationalStrings.ExecuteOperationOnEntitySplitting(
                    nameof(RelationalQueryableExtensions.ExecuteDelete), entityType.DisplayName()));
            return null;
        }

        var selectExpression = (SelectExpression)source.QueryExpression;
        if (IsValidSelectExpressionForExecuteDelete(selectExpression, entityShaperExpression, out var tableExpression))
        {
            if (AreOtherNonOwnedEntityTypesInTheTable(entityType.GetRootType(), tableExpression.Table))
            {
                AddTranslationErrorDetails(
                    RelationalStrings.ExecuteDeleteOnTableSplitting(tableExpression.Table.SchemaQualifiedName));

                return null;
            }

            selectExpression.ReplaceProjection(new List<Expression>());
            selectExpression.ApplyProjection();

            return new NonQueryExpression(new DeleteExpression(tableExpression, selectExpression));

            static bool AreOtherNonOwnedEntityTypesInTheTable(IEntityType rootType, ITableBase table)
            {
                foreach (var entityTypeMapping in table.EntityTypeMappings)
                {
                    var entityType = entityTypeMapping.EntityType;
                    if ((entityTypeMapping.IsSharedTablePrincipal == true
                        && entityType != rootType)
                        || (entityTypeMapping.IsSharedTablePrincipal == false
                            && entityType.GetRootType() != rootType
                            && !entityType.IsOwned()))
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        // We need to convert to PK predicate
        var pk = entityType.FindPrimaryKey();
        if (pk == null)
        {
            AddTranslationErrorDetails(
                RelationalStrings.ExecuteOperationOnKeylessEntityTypeWithUnsupportedOperator(
                    nameof(RelationalQueryableExtensions.ExecuteDelete),
                    entityType.DisplayName()));
            return null;
        }

        var clrType = entityType.ClrType;
        var entityParameter = Expression.Parameter(clrType);
        Expression predicateBody;
        var innerParameter = Expression.Parameter(clrType);
        predicateBody = Expression.Call(
            QueryableMethods.AnyWithPredicate.MakeGenericMethod(clrType),
            source,
            Expression.Quote(
                Expression.Lambda(
                    Infrastructure.ExpressionExtensions.CreateEqualsExpression(innerParameter, entityParameter),
                    innerParameter)));

        var newSource = Expression.Call(
            QueryableMethods.Where.MakeGenericMethod(clrType),
            new EntityQueryRootExpression(entityType),
            Expression.Quote(Expression.Lambda(predicateBody, entityParameter)));

        return TranslateExecuteDelete((ShapedQueryExpression)Visit(newSource));

        static Expression PruneOwnedIncludes(IncludeExpression includeExpression)
        {
            if (includeExpression.Navigation is ISkipNavigation
                || includeExpression.Navigation is not INavigation navigation
                || !navigation.ForeignKey.IsOwnership)
            {
                return includeExpression;
            }

            return includeExpression.EntityExpression is IncludeExpression innerIncludeExpression
                ? PruneOwnedIncludes(innerIncludeExpression)
                : includeExpression.EntityExpression;
        }
    }

    /// <summary>
    ///     Translates
    ///     <see
    ///         cref="RelationalQueryableExtensions.ExecuteUpdate{TSource}(IQueryable{TSource}, Expression{Func{SetPropertyCalls{TSource}, SetPropertyCalls{TSource}}})" />
    ///     method
    ///     over the given source.
    /// </summary>
    /// <param name="source">The shaped query on which the operator is applied.</param>
    /// <param name="setPropertyCalls">
    ///     The lambda expression containing
    ///     <see
    ///         cref="SetPropertyCalls{TSource}.SetProperty{TProperty}(Func{TSource, TProperty}, Func{TSource, TProperty})" />
    ///     statements.
    /// </param>
    /// <returns>The non query after translation.</returns>
    protected virtual NonQueryExpression? TranslateExecuteUpdate(
        ShapedQueryExpression source,
        LambdaExpression setPropertyCalls)
    {
        var propertyValueLambdaExpressions = new List<(LambdaExpression, Expression)>();
        PopulateSetPropertyCalls(setPropertyCalls.Body, propertyValueLambdaExpressions, setPropertyCalls.Parameters[0]);
        if (TranslationErrorDetails != null)
        {
            return null;
        }

        if (propertyValueLambdaExpressions.Count == 0)
        {
            AddTranslationErrorDetails(RelationalStrings.NoSetPropertyInvocation);
            return null;
        }

        EntityShaperExpression? entityShaperExpression = null;
        var remappedUnwrappedLeftExpressions = new List<Expression>();
        foreach (var (propertyExpression, _) in propertyValueLambdaExpressions)
        {
            var left = RemapLambdaBody(source, propertyExpression);
            left = left.UnwrapTypeConversion(out _);
            if (!IsValidPropertyAccess(RelationalDependencies.Model, left, out var ese))
            {
                AddTranslationErrorDetails(RelationalStrings.InvalidPropertyInSetProperty(propertyExpression.Print()));
                return null;
            }

            if (entityShaperExpression is null)
            {
                entityShaperExpression = ese;
            }
            else if (!ReferenceEquals(ese, entityShaperExpression))
            {
                AddTranslationErrorDetails(
                    RelationalStrings.MultipleEntityPropertiesInSetProperty(
                        entityShaperExpression.EntityType.DisplayName(), ese.EntityType.DisplayName()));
                return null;
            }

            remappedUnwrappedLeftExpressions.Add(left);
        }

        Check.DebugAssert(entityShaperExpression != null, "EntityShaperExpression should have a value.");

        var entityType = entityShaperExpression.EntityType;
        var mappingStrategy = entityType.GetMappingStrategy();
        if (mappingStrategy == RelationalAnnotationNames.TptMappingStrategy)
        {
            AddTranslationErrorDetails(
                RelationalStrings.ExecuteOperationOnTPT(nameof(RelationalQueryableExtensions.ExecuteUpdate), entityType.DisplayName()));
            return null;
        }

        if (mappingStrategy == RelationalAnnotationNames.TpcMappingStrategy
            && entityType.GetDirectlyDerivedTypes().Any())
        {
            // We allow TPC is it is leaf type
            AddTranslationErrorDetails(
                RelationalStrings.ExecuteOperationOnTPC(nameof(RelationalQueryableExtensions.ExecuteUpdate), entityType.DisplayName()));
            return null;
        }

        if (entityType.GetViewOrTableMappings().Count() != 1)
        {
            AddTranslationErrorDetails(
                RelationalStrings.ExecuteOperationOnEntitySplitting(
                    nameof(RelationalQueryableExtensions.ExecuteUpdate), entityType.DisplayName()));
            return null;
        }

        var selectExpression = (SelectExpression)source.QueryExpression;
        if (IsValidSelectExpressionForExecuteUpdate(selectExpression, entityShaperExpression, out var tableExpression))
        {
            return TranslateSetPropertyExpressions(
                this, source, selectExpression, tableExpression,
                propertyValueLambdaExpressions, remappedUnwrappedLeftExpressions);
        }

        // We need to convert to join with original query using PK
        var pk = entityType.FindPrimaryKey();
        if (pk == null)
        {
            AddTranslationErrorDetails(
                RelationalStrings.ExecuteOperationOnKeylessEntityTypeWithUnsupportedOperator(
                    nameof(RelationalQueryableExtensions.ExecuteUpdate),
                    entityType.DisplayName()));
            return null;
        }

        var outer = (ShapedQueryExpression)Visit(new EntityQueryRootExpression(entityType));
        var inner = source;
        var outerParameter = Expression.Parameter(entityType.ClrType);
        var outerKeySelector = Expression.Lambda(outerParameter.CreateKeyValuesExpression(pk.Properties), outerParameter);
        var firstPropertyLambdaExpression = propertyValueLambdaExpressions[0].Item1;
        var entitySource = GetEntitySource(RelationalDependencies.Model, firstPropertyLambdaExpression.Body);
        var innerKeySelector = Expression.Lambda(
            entitySource.CreateKeyValuesExpression(pk.Properties), firstPropertyLambdaExpression.Parameters);

        var joinPredicate = CreateJoinPredicate(outer, outerKeySelector, inner, innerKeySelector);

        Check.DebugAssert(joinPredicate != null, "Join predicate shouldn't be null");

        var outerSelectExpression = (SelectExpression)outer.QueryExpression;
        var outerShaperExpression = outerSelectExpression.AddInnerJoin(inner, joinPredicate, outer.ShaperExpression);
        outer = outer.UpdateShaperExpression(outerShaperExpression);
        var transparentIdentifierType = outer.ShaperExpression.Type;
        var transparentIdentifierParameter = Expression.Parameter(transparentIdentifierType);

        var propertyReplacement = AccessField(transparentIdentifierType, transparentIdentifierParameter, "Outer");
        var valueReplacement = AccessField(transparentIdentifierType, transparentIdentifierParameter, "Inner");
        for (var i = 0; i < propertyValueLambdaExpressions.Count; i++)
        {
            var (propertyExpression, valueExpression) = propertyValueLambdaExpressions[i];
            propertyExpression = Expression.Lambda(
                ReplacingExpressionVisitor.Replace(
                    ReplacingExpressionVisitor.Replace(
                        firstPropertyLambdaExpression.Parameters[0],
                        propertyExpression.Parameters[0],
                        entitySource),
                    propertyReplacement, propertyExpression.Body),
                transparentIdentifierParameter);

            valueExpression = valueExpression is LambdaExpression lambdaExpression
                ? Expression.Lambda(
                    ReplacingExpressionVisitor.Replace(lambdaExpression.Parameters[0], valueReplacement, lambdaExpression.Body),
                    transparentIdentifierParameter)
                : valueExpression;

            propertyValueLambdaExpressions[i] = (propertyExpression, valueExpression);
        }

        tableExpression = (TableExpression)outerSelectExpression.Tables[0];

        return TranslateSetPropertyExpressions(this, outer, outerSelectExpression, tableExpression, propertyValueLambdaExpressions, null);

        static NonQueryExpression? TranslateSetPropertyExpressions(
            RelationalQueryableMethodTranslatingExpressionVisitor visitor,
            ShapedQueryExpression source,
            SelectExpression selectExpression,
            TableExpression tableExpression,
            List<(LambdaExpression, Expression)> propertyValueLambdaExpressions,
            List<Expression>? leftExpressions)
        {
            var columnValueSetters = new List<ColumnValueSetter>();
            for (var i = 0; i < propertyValueLambdaExpressions.Count; i++)
            {
                var (propertyExpression, valueExpression) = propertyValueLambdaExpressions[i];
                Expression left;
                if (leftExpressions != null)
                {
                    left = leftExpressions[i];
                }
                else
                {
                    left = visitor.RemapLambdaBody(source, propertyExpression);
                    left = left.UnwrapTypeConversion(out _);
                }

                var right = valueExpression is LambdaExpression lambdaExpression
                    ? visitor.RemapLambdaBody(source, lambdaExpression)
                    : valueExpression;

                if (right.Type != left.Type)
                {
                    right = Expression.Convert(right, left.Type);
                }

                // We generate equality between property = value while translating so that we infer the type mapping from property correctly.
                // Later we decompose it back into left/right components so that the equality is not in the tree which can get affected by
                // null semantics or other visitor.
                var setter = Infrastructure.ExpressionExtensions.CreateEqualsExpression(left, right);
                var translation = visitor._sqlTranslator.Translate(setter);
                if (translation is SqlBinaryExpression
                    {
                        OperatorType: ExpressionType.Equal, Left: ColumnExpression column
                    } sqlBinaryExpression)
                {
                    columnValueSetters.Add(new ColumnValueSetter(column, sqlBinaryExpression.Right));
                }
                else
                {
                    // We would reach here only if the property is unmapped or value fails to translate.
                    visitor.AddTranslationErrorDetails(
                        RelationalStrings.UnableToTranslateSetProperty(
                            propertyExpression.Print(), valueExpression.Print(), visitor._sqlTranslator.TranslationErrorDetails));
                    return null;
                }
            }

            selectExpression.ReplaceProjection(new List<Expression>());
            selectExpression.ApplyProjection();

            return new NonQueryExpression(new UpdateExpression(tableExpression, selectExpression, columnValueSetters));
        }

        void PopulateSetPropertyCalls(
            Expression expression,
            List<(LambdaExpression, Expression)> list,
            ParameterExpression parameter)
        {
            switch (expression)
            {
                case ParameterExpression p
                    when parameter == p:
                    break;

                case MethodCallExpression methodCallExpression
                    when methodCallExpression.Method.IsGenericMethod
                    && methodCallExpression.Method.Name == nameof(SetPropertyCalls<int>.SetProperty)
                    && methodCallExpression.Method.DeclaringType!.IsGenericType
                    && methodCallExpression.Method.DeclaringType.GetGenericTypeDefinition() == typeof(SetPropertyCalls<>):

                    list.Add(((LambdaExpression)methodCallExpression.Arguments[0], methodCallExpression.Arguments[1]));

                    PopulateSetPropertyCalls(methodCallExpression.Object!, list, parameter);

                    break;

                default:
                    AddTranslationErrorDetails(RelationalStrings.InvalidArgumentToExecuteUpdate);
                    break;
            }
        }

        static bool IsValidPropertyAccess(
            IModel model,
            Expression expression,
            [NotNullWhen(true)] out EntityShaperExpression? entityShaperExpression)
        {
            if (expression is MemberExpression { Expression: EntityShaperExpression ese })
            {
                entityShaperExpression = ese;
                return true;
            }

            if (expression is MethodCallExpression mce)
            {
                if (mce.TryGetEFPropertyArguments(out var source, out _)
                    && source is EntityShaperExpression ese1)
                {
                    entityShaperExpression = ese1;
                    return true;
                }

                if (mce.TryGetIndexerArguments(model, out var source2, out _)
                    && source2 is EntityShaperExpression ese2)
                {
                    entityShaperExpression = ese2;
                    return true;
                }
            }

            entityShaperExpression = null;
            return false;
        }

        static Expression GetEntitySource(IModel model, Expression propertyAccessExpression)
        {
            propertyAccessExpression = propertyAccessExpression.UnwrapTypeConversion(out _);
            if (propertyAccessExpression is MethodCallExpression mce)
            {
                if (mce.TryGetEFPropertyArguments(out var source, out _))
                {
                    return source;
                }

                if (mce.TryGetIndexerArguments(model, out var source2, out _))
                {
                    return source2;
                }
            }

            return ((MemberExpression)propertyAccessExpression).Expression!;
        }
    }

    /// <summary>
    ///     Checks weather the current select expression can be used as-is for execute a delete operation,
    ///     or whether it must be pushed down into a subquery.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         By default, only single-table select expressions are supported, and optionally with a predicate.
    ///     </para>
    ///     <para>
    ///         Providers can override this to allow more select expression features to be supported without pushing down into a subquery.
    ///         When doing this, VisitDelete must also be overridden in the provider's QuerySqlGenerator to add SQL generation support for
    ///         the feature.
    ///     </para>
    /// </remarks>
    /// <param name="selectExpression">The select expression to validate.</param>
    /// <param name="entityShaperExpression">The entity shaper expression on which the delete operation is being applied.</param>
    /// <param name="tableExpression">The table expression from which rows are being deleted.</param>
    /// <returns>Returns <see langword="true" /> if the current select expression can be used for delete as-is, <see langword="false" /> otherwise.</returns>
    protected virtual bool IsValidSelectExpressionForExecuteDelete(
        SelectExpression selectExpression,
        EntityShaperExpression entityShaperExpression,
        [NotNullWhen(true)] out TableExpression? tableExpression)
    {
        if (selectExpression.Offset == null
            && selectExpression.Limit == null
            // If entity type has primary key then Distinct is no-op
            && (!selectExpression.IsDistinct || entityShaperExpression.EntityType.FindPrimaryKey() != null)
            && selectExpression.GroupBy.Count == 0
            && selectExpression.Having == null
            && selectExpression.Orderings.Count == 0
            && selectExpression.Tables.Count == 1
            && selectExpression.Tables[0] is TableExpression expression)
        {
            tableExpression = expression;

            return true;
        }

        tableExpression = null;
        return false;
    }

    /// <summary>
    ///     Validates if the current select expression can be used for execute update operation or it requires to be joined as a subquery.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         By default, only muli-table select expressions are supported, and optionally with a predicate.
    ///     </para>
    ///     <para>
    ///         Providers can override this to allow more select expression features to be supported without pushing down into a subquery.
    ///         When doing this, VisitUpdate must also be overridden in the provider's QuerySqlGenerator to add SQL generation support for
    ///         the feature.
    ///     </para>
    /// </remarks>
    /// <param name="selectExpression">The select expression to validate.</param>
    /// <param name="entityShaperExpression">The entity shaper expression on which the update operation is being applied.</param>
    /// <param name="tableExpression">The table expression from which rows are being deleted.</param>
    /// <returns>Returns <see langword="true" /> if the current select expression can be used for update as-is, <see langword="false" /> otherwise.</returns>
    protected virtual bool IsValidSelectExpressionForExecuteUpdate(
        SelectExpression selectExpression,
        EntityShaperExpression entityShaperExpression,
        [NotNullWhen(true)] out TableExpression? tableExpression)
    {
        tableExpression = null;
        if (selectExpression.Offset == null
            && selectExpression.Limit == null
            // If entity type has primary key then Distinct is no-op
            && (!selectExpression.IsDistinct || entityShaperExpression.EntityType.FindPrimaryKey() != null)
            && selectExpression.GroupBy.Count == 0
            && selectExpression.Having == null
            && selectExpression.Orderings.Count == 0
            && selectExpression.Tables.Count > 0)
        {
            TableExpressionBase table;
            if (selectExpression.Tables.Count == 1)
            {
                table = selectExpression.Tables[0];
            }
            else
            {
                var projectionBindingExpression = (ProjectionBindingExpression)entityShaperExpression.ValueBufferExpression;
                var entityProjectionExpression = (EntityProjectionExpression)selectExpression.GetProjection(projectionBindingExpression);
                var column = entityProjectionExpression.BindProperty(entityShaperExpression.EntityType.GetProperties().First());
                table = column.Table;
                if (ReferenceEquals(selectExpression.Tables[0], table))
                {
                    // If the table we are looking for it first table, then we need to verify if we can lift the next table in FROM clause
                    var secondTable = selectExpression.Tables[1];
                    if (secondTable is not InnerJoinExpression and not CrossJoinExpression)
                    {
                        return false;
                    }
                }

                if (table is JoinExpressionBase joinExpressionBase)
                {
                    table = joinExpressionBase.Table;
                }
            }

            if (table is TableExpression te)
            {
                tableExpression = te;
                return true;
            }
        }

        return false;
    }

    /// <summary>
    ///     Translates the given expression into equivalent SQL representation.
    /// </summary>
    /// <param name="expression">An expression to translate.</param>
    /// <returns>A <see cref="SqlExpression" /> which is translation of given expression or <see langword="null" />.</returns>
    protected virtual SqlExpression? TranslateExpression(Expression expression)
    {
        var translation = _sqlTranslator.Translate(expression);
        if (translation == null && _sqlTranslator.TranslationErrorDetails != null)
        {
            AddTranslationErrorDetails(_sqlTranslator.TranslationErrorDetails);
        }

        return translation;
    }

    /// <summary>
    ///     Translates the given lambda expression for the <see cref="ShapedQueryExpression" /> source into equivalent SQL representation.
    /// </summary>
    /// <param name="shapedQueryExpression">A <see cref="ShapedQueryExpression" /> on which the lambda expression is being applied.</param>
    /// <param name="lambdaExpression">A <see cref="LambdaExpression" /> to translate into SQL.</param>
    /// <returns>A <see cref="SqlExpression" /> which is translation of given lambda expression or <see langword="null" />.</returns>
    protected virtual SqlExpression? TranslateLambdaExpression(
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
        private readonly RelationalSqlTranslatingExpressionVisitor _sqlTranslator;
        private readonly ISqlExpressionFactory _sqlExpressionFactory;

        private SelectExpression _selectExpression;

        public SharedTypeEntityExpandingExpressionVisitor(
            RelationalSqlTranslatingExpressionVisitor sqlTranslator,
            ISqlExpressionFactory sqlExpressionFactory)
        {
            _sqlTranslator = sqlTranslator;
            _sqlExpressionFactory = sqlExpressionFactory;
            _selectExpression = null!;
        }

        public Expression Expand(SelectExpression selectExpression, Expression lambdaBody)
        {
            _selectExpression = selectExpression;

            return Visit(lambdaBody);
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
                || extensionExpression is GroupByShaperExpression
                    ? extensionExpression
                    : base.VisitExtension(extensionExpression);

        private Expression? TryExpand(Expression? source, MemberIdentity member)
        {
            source = source.UnwrapTypeConversion(out var convertedType);
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

            if (TryGetJsonQueryExpression(entityShaperExpression, out var jsonQueryExpression))
            {
                var newJsonQueryExpression = jsonQueryExpression.BindNavigation(navigation);

                return navigation.IsCollection
                    ? newJsonQueryExpression
                    : new RelationalEntityShaperExpression(
                        navigation.TargetEntityType,
                        newJsonQueryExpression,
                        nullable: entityShaperExpression.IsNullable || !navigation.ForeignKey.IsRequired);
            }

            var entityProjectionExpression = GetEntityProjectionExpression(entityShaperExpression);
            var foreignKey = navigation.ForeignKey;

            if (targetEntityType.IsMappedToJson())
            {
                var innerShaper = entityProjectionExpression.BindNavigation(navigation)!;

                return navigation.IsCollection
                    ? (JsonQueryExpression)innerShaper.ValueBufferExpression
                    : innerShaper;
            }

            if (navigation.IsCollection)
            {
                // just need any column - we use it only to extract the table it originated from
                var sourceColumn = entityProjectionExpression
                    .BindProperty(
                        navigation.IsOnDependent
                            ? foreignKey.Properties[0]
                            : foreignKey.PrincipalKey.Properties[0]);

                var sourceTable = FindRootTableExpressionForColumn(sourceColumn);
                var innerSelectExpression = _sqlExpressionFactory.Select(targetEntityType);
                innerSelectExpression = (SelectExpression)new AnnotationApplyingExpressionVisitor(sourceTable.GetAnnotations().ToList())
                    .Visit(innerSelectExpression);

                var innerShapedQuery = CreateShapedQueryExpression(targetEntityType, innerSelectExpression);

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

                var keyComparison = Infrastructure.ExpressionExtensions.CreateEqualsExpression(outerKey, innerKey);

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
                    : keyComparison;

                var correlationPredicate = Expression.Lambda(predicate, correlationPredicateParameter);

                return Expression.Call(
                    QueryableMethods.Where.MakeGenericMethod(innerSequenceType),
                    innerShapedQuery,
                    Expression.Quote(correlationPredicate));
            }

            return entityProjectionExpression.BindNavigation(navigation)
                ?? _selectExpression.GenerateOwnedReferenceEntityProjectionExpression(
                    entityProjectionExpression, navigation, _sqlExpressionFactory);

            static TableExpressionBase FindRootTableExpressionForColumn(ColumnExpression column)
            {
                var table = column.Table;
                if (table is JoinExpressionBase joinExpressionBase)
                {
                    table = joinExpressionBase.Table;
                }
                else if (table is SetOperationBase setOperationBase)
                {
                    table = setOperationBase.Source1;
                }

                if (table is SelectExpression selectExpression)
                {
                    var matchingProjection =
                        (ColumnExpression)selectExpression.Projection.Where(p => p.Alias == column.Name).Single().Expression;

                    return FindRootTableExpressionForColumn(matchingProjection);
                }

                return table;
            }
        }

        private sealed class AnnotationApplyingExpressionVisitor : ExpressionVisitor
        {
            private readonly IReadOnlyList<IAnnotation> _annotations;

            public AnnotationApplyingExpressionVisitor(IReadOnlyList<IAnnotation> annotations)
            {
                _annotations = annotations;
            }

            [return: NotNullIfNotNull("expression")]
            public override Expression? Visit(Expression? expression)
            {
                if (expression is TableExpression te)
                {
                    TableExpressionBase ownedTable = te;
                    foreach (var annotation in _annotations)
                    {
                        ownedTable = ownedTable.AddAnnotation(annotation.Name, annotation.Value);
                    }

                    return ownedTable;
                }

                return base.Visit(expression);
            }
        }

        private bool TryGetJsonQueryExpression(
            EntityShaperExpression entityShaperExpression,
            [NotNullWhen(true)] out JsonQueryExpression? jsonQueryExpression)
        {
            switch (entityShaperExpression.ValueBufferExpression)
            {
                case ProjectionBindingExpression projectionBindingExpression:
                    jsonQueryExpression = _selectExpression.GetProjection(projectionBindingExpression) as JsonQueryExpression;
                    return jsonQueryExpression != null;

                case JsonQueryExpression jqe:
                    jsonQueryExpression = jqe;
                    return true;

                default:
                    jsonQueryExpression = null;
                    return false;
            }
        }

        private EntityProjectionExpression GetEntityProjectionExpression(EntityShaperExpression entityShaperExpression)
            => entityShaperExpression.ValueBufferExpression switch
            {
                ProjectionBindingExpression projectionBindingExpression
                    => (EntityProjectionExpression)_selectExpression.GetProjection(projectionBindingExpression),
                EntityProjectionExpression entityProjectionExpression => entityProjectionExpression,
                _ => throw new InvalidOperationException()
            };
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
                // Erasing client projections erase projectionMapping projections too
                selectExpression.ReplaceProjection(new List<Expression>());
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
        MethodInfo predicateLessMethodInfo)
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
        if (!selectExpression.IsDistinct)
        {
            selectExpression.ReplaceProjection(new List<Expression>());
        }

        selectExpression.PrepareForAggregate();
        var selector = _sqlExpressionFactory.Fragment("*");
        var methodCall = Expression.Call(
            predicateLessMethodInfo.MakeGenericMethod(selector.Type),
            Expression.Call(
                QueryableMethods.AsQueryable.MakeGenericMethod(selector.Type), new EnumerableExpression(selector)));
        var translation = TranslateExpression(methodCall);
        if (translation == null)
        {
            return null;
        }

        var projectionMapping = new Dictionary<ProjectionMember, Expression> { { new ProjectionMember(), translation } };

        selectExpression.ClearOrdering();
        selectExpression.ReplaceProjection(projectionMapping);
        var resultType = predicateLessMethodInfo.ReturnType;

        return source.UpdateShaperExpression(
            Expression.Convert(
                new ProjectionBindingExpression(source.QueryExpression, new ProjectionMember(), resultType.MakeNullable()),
                resultType));
    }

    private ShapedQueryExpression? TranslateAggregateWithSelector(
        ShapedQueryExpression source,
        LambdaExpression? selectorLambda,
        Func<Type, MethodInfo> methodGenerator,
        bool throwWhenEmpty,
        Type resultType)
    {
        var selectExpression = (SelectExpression)source.QueryExpression;
        selectExpression.PrepareForAggregate();

        Expression? selector = null;
        if (selectorLambda == null
            || selectorLambda.Body == selectorLambda.Parameters[0])
        {
            var shaperExpression = source.ShaperExpression;
            if (shaperExpression is UnaryExpression unaryExpression
                && unaryExpression.NodeType == ExpressionType.Convert)
            {
                shaperExpression = unaryExpression.Operand;
            }

            if (shaperExpression is ProjectionBindingExpression projectionBindingExpression)
            {
                selector = selectExpression.GetProjection(projectionBindingExpression);
            }
        }
        else
        {
            selector = RemapLambdaBody(source, selectorLambda);
        }

        if (selector == null
            || TranslateExpression(selector) is not SqlExpression translatedSelector)
        {
            return null;
        }

        var methodCall = Expression.Call(
            methodGenerator(translatedSelector.Type),
            Expression.Call(
                QueryableMethods.AsQueryable.MakeGenericMethod(translatedSelector.Type), new EnumerableExpression(translatedSelector)));
        var translation = _sqlTranslator.Translate(methodCall);
        if (translation == null)
        {
            return null;
        }

        selectExpression.ReplaceProjection(
            new Dictionary<ProjectionMember, Expression> { { new ProjectionMember(), translation } });

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
                : translation.Type.IsNullableType()
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
            shaper = new ProjectionBindingExpression(source.QueryExpression, new ProjectionMember(), translation.Type.MakeNullable());

            if (resultType != shaper.Type)
            {
                shaper = Expression.Convert(shaper, resultType);
            }
        }

        return source.UpdateShaperExpression(shaper);
    }
}
