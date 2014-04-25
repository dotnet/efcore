// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;
using Remotion.Linq;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Clauses.ExpressionTreeVisitors;
using Remotion.Linq.Clauses.StreamedData;
using Remotion.Linq.Parsing;

namespace Microsoft.Data.Entity.Query
{
    public abstract class EntityQueryModelVisitor : QueryModelVisitorBase
    {
        private static readonly ParameterExpression _queryContextParameter
            = Expression.Parameter(typeof(QueryContext));

        private static readonly ParameterExpression _querySourceScopeParameter
            = Expression.Parameter(typeof(QuerySourceScope));

        private readonly QuerySourceMapping _querySourceMapping = new QuerySourceMapping();
        private readonly EntityQueryModelVisitor _parentQueryModelVisitor;
        private readonly MethodInfo _entityScanMethodInfo;
        private readonly Func<EntityQueryModelVisitor, EntityQueryModelVisitor> _visitorFactory;

        private Expression _expression;
        private StreamedSequenceInfo _streamedSequenceInfo;

        protected EntityQueryModelVisitor(
            MethodInfo entityScanMethodInfo,
            Func<EntityQueryModelVisitor, EntityQueryModelVisitor> visitorFactory)
        {
            _entityScanMethodInfo = entityScanMethodInfo;
            _visitorFactory = visitorFactory;
        }

        protected EntityQueryModelVisitor(
            EntityQueryModelVisitor parentQueryModelVisitor,
            MethodInfo entityScanMethodInfo,
            Func<EntityQueryModelVisitor, EntityQueryModelVisitor> visitorFactory)
        {
            _parentQueryModelVisitor = parentQueryModelVisitor;
            _entityScanMethodInfo = entityScanMethodInfo;
            _visitorFactory = visitorFactory;
        }

        public Func<QueryContext, IEnumerable<TResult>> CreateQueryExecutor<TResult>([NotNull] QueryModel queryModel)
        {
            Check.NotNull(queryModel, "queryModel");

            VisitQueryModel(queryModel);

            if (_streamedSequenceInfo == null)
            {
                _expression
                    = Expression.NewArrayInit(typeof(TResult), _expression);
            }

            return Expression
                .Lambda<Func<QueryContext, IEnumerable<TResult>>>(_expression, _queryContextParameter)
                .Compile();
        }

        public override void VisitMainFromClause(MainFromClause fromClause, QueryModel queryModel)
        {
            _expression
                = ReplaceClauseReferences(
                    new QueryingExpressionTreeVisitor(this, _entityScanMethodInfo, _visitorFactory)
                        .VisitExpression(fromClause.FromExpression));

            var itemParameter
                = Expression.Parameter(fromClause.ItemType);

            var parentScopeExpression
                = _parentQueryModelVisitor == null
                    ? (Expression)Expression.Default(typeof(QuerySourceScope))
                    : _querySourceScopeParameter;

            var scopeCreatorExpression
                = QuerySourceScope
                    .Create(fromClause, itemParameter, parentScopeExpression);

            _expression
                = Expression.Call(
                    _selectManyShim
                        .MakeGenericMethod(typeof(QuerySourceScope), typeof(QuerySourceScope)),
                    Expression.NewArrayInit(typeof(QuerySourceScope), parentScopeExpression),
                    Expression.Lambda(
                        Expression.Call(
                            _selectShim
                                .MakeGenericMethod(fromClause.ItemType, typeof(QuerySourceScope)),
                            _expression,
                            Expression.Lambda(
                                scopeCreatorExpression,
                                new[] { itemParameter })),
                        new[] { _querySourceScopeParameter }));

            _querySourceMapping.AddMapping(
                fromClause,
                QuerySourceScope.GetResult(_querySourceScopeParameter, fromClause));
        }

        public override void VisitAdditionalFromClause(AdditionalFromClause fromClause, QueryModel queryModel, int index)
        {
            var innerExpression
                = ReplaceClauseReferences(
                    new QueryingExpressionTreeVisitor(this, _entityScanMethodInfo, _visitorFactory)
                        .VisitExpression(fromClause.FromExpression));

            var itemParameter
                = Expression.Parameter(fromClause.ItemType);

            var scopeCreatorExpression
                = QuerySourceScope
                    .Create(fromClause, itemParameter, _querySourceScopeParameter);

            _expression
                = Expression.Call(
                    _selectManyShim
                        .MakeGenericMethod(typeof(QuerySourceScope), typeof(QuerySourceScope)),
                    _expression,
                    Expression.Lambda(
                        Expression.Call(
                            _selectShim
                                .MakeGenericMethod(fromClause.ItemType, typeof(QuerySourceScope)),
                            innerExpression,
                            Expression.Lambda(
                                scopeCreatorExpression,
                                new[] { itemParameter })),
                        new[] { _querySourceScopeParameter }));

            _querySourceMapping.AddMapping(
                fromClause,
                QuerySourceScope.GetResult(_querySourceScopeParameter, fromClause));
        }

        private static readonly MethodInfo _selectManyShim
            = typeof(EntityQueryModelVisitor).GetTypeInfo().GetDeclaredMethod("SelectManyShim");

        [UsedImplicitly]
        private static IEnumerable<TResult> SelectManyShim<TSource, TResult>(
            IEnumerable<TSource> source, Func<TSource, IEnumerable<TResult>> selector)
        {
            return source.SelectMany(selector);
        }

        public override void VisitJoinClause(JoinClause joinClause, QueryModel queryModel, int index)
        {
            var itemParameter
                = Expression.Parameter(joinClause.ItemType);

            _querySourceMapping.AddMapping(joinClause, itemParameter);

            var innerSequence
                = ReplaceClauseReferences(
                    new QueryingExpressionTreeVisitor(null, _entityScanMethodInfo, _visitorFactory)
                        .VisitExpression(joinClause.InnerSequence));

            var outerKeySelector
                = ReplaceClauseReferences(
                    new QueryingExpressionTreeVisitor(this, _entityScanMethodInfo, _visitorFactory)
                        .VisitExpression(joinClause.OuterKeySelector));

            var innerKeySelector
                = ReplaceClauseReferences(
                    new QueryingExpressionTreeVisitor(this, _entityScanMethodInfo, _visitorFactory)
                        .VisitExpression(joinClause.InnerKeySelector));

            var scopeCreatorExpression
                = QuerySourceScope
                    .Create(joinClause, itemParameter, _querySourceScopeParameter);

            _expression
                = Expression.Call(
                    _joinShim.MakeGenericMethod(
                        typeof(QuerySourceScope),
                        joinClause.ItemType,
                        outerKeySelector.Type,
                        typeof(QuerySourceScope)),
                    _expression,
                    innerSequence,
                    Expression.Lambda(outerKeySelector, _querySourceScopeParameter),
                    Expression.Lambda(innerKeySelector, itemParameter),
                    Expression.Lambda(
                        scopeCreatorExpression,
                        new[] { _querySourceScopeParameter, itemParameter }));

            _querySourceMapping.ReplaceMapping(
                joinClause,
                QuerySourceScope.GetResult(_querySourceScopeParameter, joinClause));
        }

        private static readonly MethodInfo _joinShim
            = typeof(EntityQueryModelVisitor).GetTypeInfo().GetDeclaredMethod("JoinShim");

        [UsedImplicitly]
        private static IEnumerable<TResult> JoinShim<TOuter, TInner, TKey, TResult>(
            IEnumerable<TOuter> outer,
            IEnumerable<TInner> inner,
            Func<TOuter, TKey> outerKeySelector,
            Func<TInner, TKey> innerKeySelector,
            Func<TOuter, TInner, TResult> resultSelector)
        {
            return outer.Join(inner, outerKeySelector, innerKeySelector, resultSelector);
        }

        public override void VisitGroupJoinClause(GroupJoinClause groupJoinClause, QueryModel queryModel, int index)
        {
            var itemParameter
                = Expression.Parameter(groupJoinClause.JoinClause.ItemType);

            _querySourceMapping.AddMapping(groupJoinClause.JoinClause, itemParameter);

            var innerSequence
                = ReplaceClauseReferences(
                    new QueryingExpressionTreeVisitor(null, _entityScanMethodInfo, _visitorFactory)
                        .VisitExpression(groupJoinClause.JoinClause.InnerSequence));

            var outerKeySelector
                = ReplaceClauseReferences(
                    new QueryingExpressionTreeVisitor(this, _entityScanMethodInfo, _visitorFactory)
                        .VisitExpression(groupJoinClause.JoinClause.OuterKeySelector));

            var innerKeySelector
                = ReplaceClauseReferences(
                    new QueryingExpressionTreeVisitor(this, _entityScanMethodInfo, _visitorFactory)
                        .VisitExpression(groupJoinClause.JoinClause.InnerKeySelector));

            var itemsParameter
                = Expression.Parameter(groupJoinClause.ItemType);

            var scopeCreatorExpression
                = QuerySourceScope
                    .Create(groupJoinClause, itemsParameter, _querySourceScopeParameter);

            _expression
                = Expression.Call(
                    _groupJoinShim.MakeGenericMethod(
                        typeof(QuerySourceScope),
                        groupJoinClause.JoinClause.ItemType,
                        outerKeySelector.Type,
                        typeof(QuerySourceScope)),
                    _expression,
                    innerSequence,
                    Expression.Lambda(outerKeySelector, _querySourceScopeParameter),
                    Expression.Lambda(innerKeySelector, itemParameter),
                    Expression.Lambda(
                        scopeCreatorExpression,
                        new[] { _querySourceScopeParameter, itemsParameter }));

            _querySourceMapping.AddMapping(
                groupJoinClause,
                QuerySourceScope.GetResult(_querySourceScopeParameter, groupJoinClause));
        }

        private static readonly MethodInfo _groupJoinShim
            = typeof(EntityQueryModelVisitor).GetTypeInfo().GetDeclaredMethod("GroupJoinShim");

        [UsedImplicitly]
        private static IEnumerable<TResult> GroupJoinShim<TOuter, TInner, TKey, TResult>(
            IEnumerable<TOuter> outer,
            IEnumerable<TInner> inner,
            Func<TOuter, TKey> outerKeySelector,
            Func<TInner, TKey> innerKeySelector,
            Func<TOuter, IEnumerable<TInner>, TResult> resultSelector)
        {
            return outer.GroupJoin(inner, outerKeySelector, innerKeySelector, resultSelector);
        }

        public override void VisitWhereClause(WhereClause whereClause, QueryModel queryModel, int index)
        {
            var predicate
                = ReplaceClauseReferences(
                    new QueryingExpressionTreeVisitor(this, _entityScanMethodInfo, _visitorFactory)
                        .VisitExpression(whereClause.Predicate));

            _expression
                = Expression.Call(
                    _whereShim.MakeGenericMethod(typeof(QuerySourceScope)),
                    _expression,
                    Expression.Lambda(predicate, _querySourceScopeParameter));
        }

        private static readonly MethodInfo _whereShim
            = typeof(EntityQueryModelVisitor).GetTypeInfo().GetDeclaredMethod("WhereShim");

        [UsedImplicitly]
        private static IEnumerable<TSource> WhereShim<TSource>(
            IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            return source.Where(predicate);
        }

        private class ProjectionSubQueryExpressionTreeVisitor : QueryingExpressionTreeVisitor
        {
            public ProjectionSubQueryExpressionTreeVisitor(
                EntityQueryModelVisitor queryModelVisitor,
                MethodInfo entityScanMethodInfo,
                Func<EntityQueryModelVisitor, EntityQueryModelVisitor> visitorFactory)
                : base(queryModelVisitor, entityScanMethodInfo, visitorFactory)
            {
            }

            protected override Expression VisitSubQueryExpression(SubQueryExpression expression)
            {
                var queryModelVisitor = _visitorFactory(_queryModelVisitor);

                queryModelVisitor.VisitQueryModel(expression.QueryModel);

                var subExpression = queryModelVisitor._expression;

                if (queryModelVisitor._streamedSequenceInfo == null)
                {
                    return subExpression;
                }

                if (typeof(IQueryable).GetTypeInfo().IsAssignableFrom(expression.Type.GetTypeInfo()))
                {
                    subExpression
                        = Expression.Call(
                            _asQueryableShim.MakeGenericMethod(
                                queryModelVisitor._streamedSequenceInfo.ResultItemType),
                            subExpression);
                }

                return Expression.Convert(subExpression, expression.Type);
            }

            private static readonly MethodInfo _asQueryableShim
                = typeof(ProjectionSubQueryExpressionTreeVisitor).GetTypeInfo()
                    .GetDeclaredMethod("AsQueryableShim");

            [UsedImplicitly]
            private static IOrderedQueryable<TSource> AsQueryableShim<TSource>(IEnumerable<TSource> source)
            {
                return new EnumerableQuery<TSource>(source);
            }
        }

        public override void VisitSelectClause(SelectClause selectClause, QueryModel queryModel)
        {
            if (_streamedSequenceInfo != null)
            {
                return;
            }

            var selector
                = ReplaceClauseReferences(
                    new ProjectionSubQueryExpressionTreeVisitor(this, _entityScanMethodInfo, _visitorFactory)
                        .VisitExpression(selectClause.Selector));

            _expression
                = Expression.Call(
                    _selectShim.MakeGenericMethod(typeof(QuerySourceScope), selector.Type),
                    _expression,
                    Expression.Lambda(selector, _querySourceScopeParameter));

            _streamedSequenceInfo
                = (StreamedSequenceInfo)selectClause.GetOutputDataInfo()
                    .AdjustDataType(_expression.Type);
        }

        private static readonly MethodInfo _selectShim
            = typeof(EntityQueryModelVisitor).GetTypeInfo().GetDeclaredMethod("SelectShim");

        [UsedImplicitly]
        private static IEnumerable<TResult> SelectShim<TSource, TResult>(
            IEnumerable<TSource> source, Func<TSource, TResult> selector)
        {
            return source.Select(selector);
        }

        public override void VisitOrdering(Ordering ordering, QueryModel queryModel, OrderByClause orderByClause, int index)
        {
            var resultType = queryModel.GetResultType();

            if (resultType.GetTypeInfo().IsGenericType
                && resultType.GetGenericTypeDefinition() == typeof(IOrderedEnumerable<>))
            {
                VisitSelectClause(queryModel.SelectClause, queryModel);

                var parameterExpression
                    = Expression.Parameter(_streamedSequenceInfo.ResultItemType);

                _querySourceMapping
                    .ReplaceMapping(
                        queryModel.MainFromClause, parameterExpression);

                var expression
                    = ReplaceClauseReferences(
                        new QueryingExpressionTreeVisitor(this, _entityScanMethodInfo, _visitorFactory)
                            .VisitExpression(ordering.Expression));

                _expression
                    = Expression.Call(
                        (index == 0
                            ? _orderByShim
                            : _thenByShim)
                            .MakeGenericMethod(_streamedSequenceInfo.ResultItemType, expression.Type),
                        _expression,
                        Expression.Lambda(expression, parameterExpression),
                        Expression.Constant(ordering.OrderingDirection));
            }
            else
            {
                var expression
                    = ReplaceClauseReferences(
                        new QueryingExpressionTreeVisitor(this, _entityScanMethodInfo, _visitorFactory)
                            .VisitExpression(ordering.Expression));

                _expression
                    = Expression.Call(
                        (index == 0
                            ? _orderByShim
                            : _thenByShim)
                            .MakeGenericMethod(typeof(QuerySourceScope), expression.Type),
                        _expression,
                        Expression.Lambda(expression, _querySourceScopeParameter),
                        Expression.Constant(ordering.OrderingDirection));
            }
        }

        private static readonly MethodInfo _orderByShim
            = typeof(EntityQueryModelVisitor).GetTypeInfo().GetDeclaredMethod("OrderByShim");

        [UsedImplicitly]
        private static IOrderedEnumerable<TSource> OrderByShim<TSource, TKey>(
            IEnumerable<TSource> source, Func<TSource, TKey> expression, OrderingDirection orderingDirection)
        {
            return orderingDirection == OrderingDirection.Asc
                ? source.OrderBy(expression)
                : source.OrderByDescending(expression);
        }

        private static readonly MethodInfo _thenByShim
            = typeof(EntityQueryModelVisitor).GetTypeInfo().GetDeclaredMethod("ThenByShim");

        [UsedImplicitly]
        private static IOrderedEnumerable<TSource> ThenByShim<TSource, TKey>(
            IOrderedEnumerable<TSource> source, Func<TSource, TKey> expression, OrderingDirection orderingDirection)
        {
            return orderingDirection == OrderingDirection.Asc
                ? source.ThenBy(expression)
                : source.ThenByDescending(expression);
        }

        private class QueryingExpressionTreeVisitor : ExpressionTreeVisitor
        {
            protected readonly EntityQueryModelVisitor _queryModelVisitor;
            protected readonly Func<EntityQueryModelVisitor, EntityQueryModelVisitor> _visitorFactory;

            private readonly MethodInfo _entityScanMethodInfo;

            public QueryingExpressionTreeVisitor(
                EntityQueryModelVisitor queryModelVisitor,
                MethodInfo entityScanMethodInfo,
                Func<EntityQueryModelVisitor, EntityQueryModelVisitor> visitorFactory)
            {
                _queryModelVisitor = queryModelVisitor;
                _entityScanMethodInfo = entityScanMethodInfo;
                _visitorFactory = visitorFactory;
            }

            protected override Expression VisitSubQueryExpression(SubQueryExpression expression)
            {
                var queryModelVisitor = _visitorFactory(_queryModelVisitor);

                queryModelVisitor.VisitQueryModel(expression.QueryModel);

                return queryModelVisitor._expression;
            }

            protected override Expression VisitConstantExpression(ConstantExpression expression)
            {
                if (expression.Type.GetTypeInfo().IsGenericType
                    && expression.Type.GetGenericTypeDefinition() == typeof(EntityQueryable<>))
                {
                    return Expression.Call(
                        _entityScanMethodInfo.MakeGenericMethod(((IQueryable)expression.Value).ElementType),
                        _queryContextParameter);
                }

                return expression;
            }
        }

        public override void VisitResultOperator(ResultOperatorBase resultOperator, QueryModel queryModel, int index)
        {
            // TODO: sub-queries in result op. expressions
            //    resultOperator
            //        .TransformExpressions(e =>
            //            ReplaceClauseReferences(new QueryingExpressionTreeVisitor(this)
            //                .VisitExpression(e)));

            var streamedDataInfo
                = resultOperator.GetOutputDataInfo(_streamedSequenceInfo);

            _expression
                = Expression.Call(
                    _executeResultOperatorMethodInfo
                        .MakeGenericMethod(_streamedSequenceInfo.ResultItemType, streamedDataInfo.DataType),
                    _expression,
                    Expression.Constant(resultOperator),
                    Expression.Constant(_streamedSequenceInfo));

            _streamedSequenceInfo = streamedDataInfo as StreamedSequenceInfo;
        }

        private static readonly MethodInfo _executeResultOperatorMethodInfo
            = typeof(EntityQueryModelVisitor).GetTypeInfo().GetDeclaredMethod("ExecuteResultOperator");

        [UsedImplicitly]
        private static TResult ExecuteResultOperator<TSource, TResult>(
            IEnumerable<TSource> source, ResultOperatorBase resultOperator, StreamedSequenceInfo streamedSequenceInfo)
        {
            var streamedData
                = resultOperator.ExecuteInMemory(
                    new StreamedSequence(source, streamedSequenceInfo));

            return (TResult)streamedData.Value;
        }

        private Expression ReplaceClauseReferences(Expression expression)
        {
            var isNestedQuery = _parentQueryModelVisitor != null;

            var resultExpression
                = ReferenceReplacingExpressionTreeVisitor
                    .ReplaceClauseReferences(
                        expression,
                        _querySourceMapping,
                        throwOnUnmappedReferences: !isNestedQuery);

            if (isNestedQuery)
            {
                resultExpression
                    = _parentQueryModelVisitor.ReplaceClauseReferences(resultExpression);
            }

            return resultExpression;
        }
    }
}
