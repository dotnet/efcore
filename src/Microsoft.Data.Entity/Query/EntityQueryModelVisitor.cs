// Copyright (c) Microsoft Open Technologies, Inc.
// All Rights Reserved
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// THIS CODE IS PROVIDED *AS IS* BASIS, WITHOUT WARRANTIES OR
// CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING
// WITHOUT LIMITATION ANY IMPLIED WARRANTIES OR CONDITIONS OF
// TITLE, FITNESS FOR A PARTICULAR PURPOSE, MERCHANTABLITY OR
// NON-INFRINGEMENT.
// See the Apache 2 License for the specific language governing
// permissions and limitations under the License.

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
        protected static readonly ParameterExpression _queryContextParameter
            = Expression.Parameter(typeof(QueryContext));

        private static readonly ParameterExpression _querySourceScopeParameter
            = Expression.Parameter(typeof(QuerySourceScope));

        private readonly QuerySourceMapping _querySourceMapping = new QuerySourceMapping();

        private readonly ILinqOperatorProvider _linqOperatorProvider;
        private readonly EntityQueryModelVisitor _parentQueryModelVisitor;

        protected Expression _expression;
        protected StreamedSequenceInfo _streamedSequenceInfo;

        protected EntityQueryModelVisitor(EntityQueryModelVisitor parentQueryModelVisitor)
            : this(new LinqOperatorProvider(), parentQueryModelVisitor)
        {
        }

        protected EntityQueryModelVisitor(
            ILinqOperatorProvider linqOperatorProvider,
            EntityQueryModelVisitor parentQueryModelVisitor)
        {
            _linqOperatorProvider = linqOperatorProvider;
            _parentQueryModelVisitor = parentQueryModelVisitor;
        }

        protected abstract ExpressionTreeVisitor CreateQueryingExpressionTreeVisitor(EntityQueryModelVisitor parentQueryModelVisitor);
        protected abstract ExpressionTreeVisitor CreateProjectionExpressionTreeVisitor(EntityQueryModelVisitor parentQueryModelVisitor);

        public Func<QueryContext, IEnumerable<TResult>> CreateQueryExecutor<TResult>([NotNull] QueryModel queryModel)
        {
            Check.NotNull(queryModel, "queryModel");

            VisitQueryModel(queryModel);

            if (_streamedSequenceInfo == null)
            {
                _expression
                    = Expression.Call(
                        _linqOperatorProvider.ToSequence
                            .MakeGenericMethod(typeof(TResult)),
                        _expression);
            }

            return Expression
                .Lambda<Func<QueryContext, IEnumerable<TResult>>>(_expression, _queryContextParameter)
                .Compile();
        }

        public override void VisitMainFromClause(MainFromClause fromClause, QueryModel queryModel)
        {
            _expression
                = ReplaceClauseReferences(
                    CreateQueryingExpressionTreeVisitor(this)
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
                    _linqOperatorProvider.SelectMany
                        .MakeGenericMethod(typeof(QuerySourceScope), typeof(QuerySourceScope)),
                    Expression.Call(
                        _linqOperatorProvider.ToSequence.MakeGenericMethod(typeof(QuerySourceScope)),
                        parentScopeExpression),
                    Expression.Lambda(
                        Expression.Call(
                            _linqOperatorProvider.Select
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
                    CreateQueryingExpressionTreeVisitor(this)
                        .VisitExpression(fromClause.FromExpression));

            var itemParameter
                = Expression.Parameter(fromClause.ItemType);

            var scopeCreatorExpression
                = QuerySourceScope
                    .Create(fromClause, itemParameter, _querySourceScopeParameter);

            _expression
                = Expression.Call(
                    _linqOperatorProvider.SelectMany
                        .MakeGenericMethod(typeof(QuerySourceScope), typeof(QuerySourceScope)),
                    _expression,
                    Expression.Lambda(
                        Expression.Call(
                            _linqOperatorProvider.Select
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

        public override void VisitJoinClause(JoinClause joinClause, QueryModel queryModel, int index)
        {
            var itemParameter
                = Expression.Parameter(joinClause.ItemType);

            _querySourceMapping.AddMapping(joinClause, itemParameter);

            var innerSequence
                = ReplaceClauseReferences(
                    CreateQueryingExpressionTreeVisitor(null)
                        .VisitExpression(joinClause.InnerSequence));

            var outerKeySelector
                = ReplaceClauseReferences(
                    CreateQueryingExpressionTreeVisitor(this)
                        .VisitExpression(joinClause.OuterKeySelector));

            var innerKeySelector
                = ReplaceClauseReferences(
                    CreateQueryingExpressionTreeVisitor(this)
                        .VisitExpression(joinClause.InnerKeySelector));

            var scopeCreatorExpression
                = QuerySourceScope
                    .Create(joinClause, itemParameter, _querySourceScopeParameter);

            _expression
                = Expression.Call(
                    _linqOperatorProvider.Join.MakeGenericMethod(
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

        public override void VisitGroupJoinClause(GroupJoinClause groupJoinClause, QueryModel queryModel, int index)
        {
            var itemParameter
                = Expression.Parameter(groupJoinClause.JoinClause.ItemType);

            _querySourceMapping.AddMapping(groupJoinClause.JoinClause, itemParameter);

            var innerSequence
                = ReplaceClauseReferences(
                    CreateQueryingExpressionTreeVisitor(null)
                        .VisitExpression(groupJoinClause.JoinClause.InnerSequence));

            var outerKeySelector
                = ReplaceClauseReferences(
                    CreateQueryingExpressionTreeVisitor(this)
                        .VisitExpression(groupJoinClause.JoinClause.OuterKeySelector));

            var innerKeySelector
                = ReplaceClauseReferences(
                    CreateQueryingExpressionTreeVisitor(this)
                        .VisitExpression(groupJoinClause.JoinClause.InnerKeySelector));

            var itemsParameter
                = Expression.Parameter(groupJoinClause.ItemType);

            var scopeCreatorExpression
                = QuerySourceScope
                    .Create(groupJoinClause, itemsParameter, _querySourceScopeParameter);

            _expression
                = Expression.Call(
                    _linqOperatorProvider.GroupJoin.MakeGenericMethod(
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

        public override void VisitWhereClause(WhereClause whereClause, QueryModel queryModel, int index)
        {
            var predicate
                = ReplaceClauseReferences(
                    CreateQueryingExpressionTreeVisitor(this)
                        .VisitExpression(whereClause.Predicate));

            _expression
                = Expression.Call(
                    _linqOperatorProvider.Where.MakeGenericMethod(typeof(QuerySourceScope)),
                    _expression,
                    Expression.Lambda(predicate, _querySourceScopeParameter));
        }

        public override void VisitSelectClause(SelectClause selectClause, QueryModel queryModel)
        {
            if (_streamedSequenceInfo != null)
            {
                return;
            }

            var selector
                = ReplaceClauseReferences(
                    CreateProjectionExpressionTreeVisitor(this)
                        .VisitExpression(selectClause.Selector));

            _expression
                = Expression.Call(
                    _linqOperatorProvider.Select.MakeGenericMethod(typeof(QuerySourceScope), selector.Type),
                    _expression,
                    Expression.Lambda(selector, _querySourceScopeParameter));

            _streamedSequenceInfo
                = (StreamedSequenceInfo)selectClause.GetOutputDataInfo()
                    .AdjustDataType(typeof(IEnumerable<>));
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
                        CreateQueryingExpressionTreeVisitor(this)
                            .VisitExpression(ordering.Expression));

                _expression
                    = Expression.Call(
                        (index == 0
                            ? _linqOperatorProvider.OrderBy
                            : _linqOperatorProvider.ThenBy)
                            .MakeGenericMethod(_streamedSequenceInfo.ResultItemType, expression.Type),
                        _expression,
                        Expression.Lambda(expression, parameterExpression),
                        Expression.Constant(ordering.OrderingDirection));
            }
            else
            {
                var expression
                    = ReplaceClauseReferences(
                        CreateQueryingExpressionTreeVisitor(this)
                            .VisitExpression(ordering.Expression));

                _expression
                    = Expression.Call(
                        (index == 0
                            ? _linqOperatorProvider.OrderBy
                            : _linqOperatorProvider.ThenBy)
                            .MakeGenericMethod(typeof(QuerySourceScope), expression.Type),
                        _expression,
                        Expression.Lambda(expression, _querySourceScopeParameter),
                        Expression.Constant(ordering.OrderingDirection));
            }
        }

        protected abstract class QueryingExpressionTreeVisitor : ExpressionTreeVisitor
        {
            protected readonly EntityQueryModelVisitor _parentQueryModelVisitor;

            protected QueryingExpressionTreeVisitor(EntityQueryModelVisitor parentQueryModelVisitor)
            {
                _parentQueryModelVisitor = parentQueryModelVisitor;
            }

            protected override Expression VisitConstantExpression(ConstantExpression expression)
            {
                if (expression.Type.GetTypeInfo().IsGenericType
                    && expression.Type.GetGenericTypeDefinition() == typeof(EntityQueryable<>))
                {
                    return VisitEntityQueryable(((IQueryable)expression.Value).ElementType);
                }

                return expression;
            }

            protected abstract Expression VisitEntityQueryable(Type elementType);

            protected Expression VisitProjectionSubQuery(
                SubQueryExpression expression, EntityQueryModelVisitor queryModelVisitor)
            {
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

            protected static readonly MethodInfo _asQueryableShim
                = typeof(QueryingExpressionTreeVisitor)
                    .GetTypeInfo().GetDeclaredMethod("AsQueryableShim");

            [UsedImplicitly]
            private static IOrderedQueryable<TSource> AsQueryableShim<TSource>(IEnumerable<TSource> source)
            {
                return new EnumerableQuery<TSource>(source);
            }
        }

        public override void VisitResultOperator(ResultOperatorBase resultOperator, QueryModel queryModel, int index)
        {
            // TODO: sub-queries in result op. expressions
            //                resultOperator
            //                    .TransformExpressions(e =>
            //                        ReplaceClauseReferences(new QueryingExpressionTreeVisitor(this)
            //                            .VisitExpression(e)));

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
            = typeof(EntityQueryModelVisitor)
                .GetTypeInfo().GetDeclaredMethod("ExecuteResultOperator");

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
