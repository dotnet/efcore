// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
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
        protected static readonly ParameterExpression QueryContextParameter
            = Expression.Parameter(typeof(QueryContext));

        private static readonly ParameterExpression _querySourceScopeParameter
            = Expression.Parameter(typeof(QuerySourceScope));

        private readonly QuerySourceMapping _querySourceMapping = new QuerySourceMapping();
        private readonly QueryCompilationContext _queryCompilationContext;

        private Expression _expression;
        private StreamedSequenceInfo _streamedSequenceInfo;

        private ISet<IQuerySource> _querySourcesRequiringMaterialization;

        protected EntityQueryModelVisitor(
            [NotNull] QueryCompilationContext queryCompilationContext)
        {
            Check.NotNull(queryCompilationContext, "queryCompilationContext");

            _queryCompilationContext = queryCompilationContext;
        }

        public virtual Expression Expression
        {
            get { return _expression; }
        }

        public QueryCompilationContext QueryCompilationContext
        {
            get { return _queryCompilationContext; }
        }

        public StreamedSequenceInfo StreamedSequenceInfo
        {
            get { return _streamedSequenceInfo; }
        }

        protected abstract ExpressionTreeVisitor CreateQueryingExpressionTreeVisitor([CanBeNull] IQuerySource querySource);

        protected virtual ExpressionTreeVisitor CreateProjectionExpressionTreeVisitor()
        {
            return new ProjectionExpressionTreeVisitor(_queryCompilationContext);
        }

        protected virtual ExpressionTreeVisitor CreateOrderingExpressionTreeVisitor(Ordering ordering)
        {
            return new DefaultExpressionTreeVisitor(_queryCompilationContext);
        }

        public Func<QueryContext, QuerySourceScope, IEnumerable<TResult>> CreateQueryExecutor<TResult>([NotNull] QueryModel queryModel)
        {
            Check.NotNull(queryModel, "queryModel");

            VisitQueryModel(queryModel);

            if (_streamedSequenceInfo == null)
            {
                _expression
                    = Expression.Call(
                        _queryCompilationContext.LinqOperatorProvider.ToSequence
                            .MakeGenericMethod(typeof(TResult)),
                        _expression);
            }

            return Expression
                .Lambda<Func<QueryContext, QuerySourceScope, IEnumerable<TResult>>>(
                    _expression, QueryContextParameter, _querySourceScopeParameter)
                .Compile();
        }

        public Func<QueryContext, QuerySourceScope, IAsyncEnumerable<TResult>> CreateAsyncQueryExecutor<TResult>([NotNull] QueryModel queryModel)
        {
            Check.NotNull(queryModel, "queryModel");

            VisitQueryModel(queryModel);

            if (_streamedSequenceInfo == null)
            {
                _expression
                    = Expression.Call(
                        _taskToSequenceShim.MakeGenericMethod(typeof(TResult)),
                        _expression);
            }

            return Expression
                .Lambda<Func<QueryContext, QuerySourceScope, IAsyncEnumerable<TResult>>>(
                    _expression, QueryContextParameter, _querySourceScopeParameter)
                .Compile();
        }

        private static readonly MethodInfo _taskToSequenceShim
            = typeof(EntityQueryModelVisitor)
                .GetTypeInfo().GetDeclaredMethod("TaskToSequenceShim");

        [UsedImplicitly]
        private static IAsyncEnumerable<T> TaskToSequenceShim<T>(Task<T> task)
        {
            return new TaskResultAsyncEnumerable<T>(task);
        }

        protected bool QuerySourceRequiresMaterialization([NotNull] IQuerySource querySource)
        {
            Check.NotNull(querySource, "querySource");

            return _querySourcesRequiringMaterialization.Contains(querySource);
        }

        public override void VisitQueryModel([NotNull] QueryModel queryModel)
        {
            Check.NotNull(queryModel, "queryModel");

            var requiresEntityMaterializationExpressionTreeVisitor
                = new RequiresEntityMaterializationExpressionTreeVisitor(_queryCompilationContext.Model);

            queryModel.TransformExpressions(requiresEntityMaterializationExpressionTreeVisitor.VisitExpression);

            _querySourcesRequiringMaterialization
                = requiresEntityMaterializationExpressionTreeVisitor.QuerySourcesRequiringMaterialization;

            foreach (var groupJoinClause in queryModel.BodyClauses.OfType<GroupJoinClause>())
            {
                _querySourcesRequiringMaterialization.Add(groupJoinClause.JoinClause);
            }

            base.VisitQueryModel(queryModel);
        }

        private class RequiresEntityMaterializationExpressionTreeVisitor : ExpressionTreeVisitor
        {
            private readonly Dictionary<IQuerySource, int> _querySources = new Dictionary<IQuerySource, int>();
            private readonly IModel _model;

            public RequiresEntityMaterializationExpressionTreeVisitor(IModel model)
            {
                _model = model;
            }

            public ISet<IQuerySource> QuerySourcesRequiringMaterialization
            {
                get { return new HashSet<IQuerySource>(_querySources.Where(kv => kv.Value > 0).Select(kv => kv.Key)); }
            }

            protected override Expression VisitQuerySourceReferenceExpression(QuerySourceReferenceExpression expression)
            {
                if (!_querySources.ContainsKey(expression.ReferencedQuerySource))
                {
                    _querySources.Add(expression.ReferencedQuerySource, 0);
                }

                _querySources[expression.ReferencedQuerySource]++;

                return base.VisitQuerySourceReferenceExpression(expression);
            }

            protected override Expression VisitMemberExpression(MemberExpression expression)
            {
                var newExpression = base.VisitMemberExpression(expression);

                var querySourceReferenceExpression
                    = expression.Expression as QuerySourceReferenceExpression;

                if (querySourceReferenceExpression != null)
                {
                    var entityType
                        = _model.TryGetEntityType(querySourceReferenceExpression.ReferencedQuerySource.ItemType);

                    if (entityType != null
                        && entityType.TryGetProperty(expression.Member.Name) != null)
                    {
                        _querySources[querySourceReferenceExpression.ReferencedQuerySource]--;
                    }
                }

                return newExpression;
            }
        }

        public override void VisitMainFromClause(
            [NotNull] MainFromClause fromClause, [NotNull] QueryModel queryModel)
        {
            Check.NotNull(fromClause, "fromClause");
            Check.NotNull(queryModel, "queryModel");

            _expression
                = ReplaceClauseReferences(
                    CreateQueryingExpressionTreeVisitor(fromClause)
                        .VisitExpression(fromClause.FromExpression));

            var elementType = _expression.Type.GetSequenceType();

            var itemParameter
                = Expression.Parameter(elementType);

            var scopeCreatorExpression
                = QuerySourceScope
                    .Create(fromClause, itemParameter, _querySourceScopeParameter);

            _expression
                = Expression.Call(
                    _queryCompilationContext.LinqOperatorProvider.SelectMany
                        .MakeGenericMethod(typeof(QuerySourceScope), typeof(QuerySourceScope)),
                    Expression.Call(
                        _queryCompilationContext.LinqOperatorProvider.ToSequence
                            .MakeGenericMethod(typeof(QuerySourceScope)),
                        _querySourceScopeParameter),
                    Expression.Lambda(
                        Expression.Call(
                            _queryCompilationContext.LinqOperatorProvider.Select
                                .MakeGenericMethod(elementType, typeof(QuerySourceScope)),
                            _expression,
                            Expression.Lambda(
                                scopeCreatorExpression,
                                new[] { itemParameter })),
                        new[] { _querySourceScopeParameter }));

            _querySourceMapping.AddMapping(
                fromClause,
                QuerySourceScope.GetResult(_querySourceScopeParameter, fromClause, elementType));
        }

        public override void VisitAdditionalFromClause(
            [NotNull] AdditionalFromClause fromClause, [NotNull] QueryModel queryModel, int index)
        {
            Check.NotNull(fromClause, "fromClause");
            Check.NotNull(queryModel, "queryModel");

            var innerExpression
                = ReplaceClauseReferences(
                    CreateQueryingExpressionTreeVisitor(fromClause)
                        .VisitExpression(fromClause.FromExpression));

            var innerElementType = innerExpression.Type.GetSequenceType();

            var itemParameter
                = Expression.Parameter(innerElementType);

            var scopeCreatorExpression
                = QuerySourceScope
                    .Create(fromClause, itemParameter, _querySourceScopeParameter);

            _expression
                = Expression.Call(
                    _queryCompilationContext.LinqOperatorProvider.SelectMany
                        .MakeGenericMethod(typeof(QuerySourceScope), typeof(QuerySourceScope)),
                    _expression,
                    Expression.Lambda(
                        Expression.Call(
                            _queryCompilationContext.LinqOperatorProvider.Select
                                .MakeGenericMethod(innerElementType, typeof(QuerySourceScope)),
                            innerExpression,
                            Expression.Lambda(
                                scopeCreatorExpression,
                                new[] { itemParameter })),
                        new[] { _querySourceScopeParameter }));

            _querySourceMapping.AddMapping(
                fromClause,
                QuerySourceScope.GetResult(_querySourceScopeParameter, fromClause, innerElementType));
        }

        public override void VisitJoinClause(
            [NotNull] JoinClause joinClause, [NotNull] QueryModel queryModel, int index)
        {
            Check.NotNull(joinClause, "joinClause");
            Check.NotNull(queryModel, "queryModel");

            var innerSequenceExpression
                = ReplaceClauseReferences(
                    CreateQueryingExpressionTreeVisitor(joinClause)
                        .VisitExpression(joinClause.InnerSequence));

            var innerElementType
                = innerSequenceExpression.Type.GetSequenceType();

            var itemParameter
                = Expression.Parameter(innerElementType);

            _querySourceMapping.AddMapping(joinClause, itemParameter);

            var outerKeySelector
                = ReplaceClauseReferences(
                    CreateQueryingExpressionTreeVisitor(joinClause)
                        .VisitExpression(joinClause.OuterKeySelector));

            var innerKeySelector
                = ReplaceClauseReferences(
                    CreateQueryingExpressionTreeVisitor(joinClause)
                        .VisitExpression(joinClause.InnerKeySelector));

            var scopeCreatorExpression
                = QuerySourceScope
                    .Create(joinClause, itemParameter, _querySourceScopeParameter);

            _expression
                = Expression.Call(
                    _queryCompilationContext.LinqOperatorProvider.Join.MakeGenericMethod(
                        typeof(QuerySourceScope),
                        innerElementType,
                        outerKeySelector.Type,
                        typeof(QuerySourceScope)),
                    _expression,
                    innerSequenceExpression,
                    Expression.Lambda(outerKeySelector, _querySourceScopeParameter),
                    Expression.Lambda(innerKeySelector, itemParameter),
                    Expression.Lambda(
                        scopeCreatorExpression,
                        new[] { _querySourceScopeParameter, itemParameter }));

            _querySourceMapping.ReplaceMapping(
                joinClause,
                QuerySourceScope.GetResult(_querySourceScopeParameter, joinClause, innerElementType));
        }

        public override void VisitGroupJoinClause(
            [NotNull] GroupJoinClause groupJoinClause, [NotNull] QueryModel queryModel, int index)
        {
            Check.NotNull(groupJoinClause, "groupJoinClause");
            Check.NotNull(queryModel, "queryModel");

            var innerSequenceExpression
                = ReplaceClauseReferences(
                    CreateQueryingExpressionTreeVisitor(groupJoinClause.JoinClause)
                        .VisitExpression(groupJoinClause.JoinClause.InnerSequence));

            var innerElementType
                = innerSequenceExpression.Type.GetSequenceType();

            var itemParameter
                = Expression.Parameter(innerElementType);

            _querySourceMapping.AddMapping(groupJoinClause.JoinClause, itemParameter);

            var outerKeySelector
                = ReplaceClauseReferences(
                    CreateQueryingExpressionTreeVisitor(groupJoinClause)
                        .VisitExpression(groupJoinClause.JoinClause.OuterKeySelector));

            var innerKeySelector
                = ReplaceClauseReferences(
                    CreateQueryingExpressionTreeVisitor(groupJoinClause)
                        .VisitExpression(groupJoinClause.JoinClause.InnerKeySelector));

            var itemsParameter
                = Expression.Parameter(innerSequenceExpression.Type);

            var scopeCreatorExpression
                = QuerySourceScope
                    .Create(groupJoinClause, itemsParameter, _querySourceScopeParameter);

            _expression
                = Expression.Call(
                    _queryCompilationContext.LinqOperatorProvider.GroupJoin.MakeGenericMethod(
                        typeof(QuerySourceScope),
                        innerElementType,
                        outerKeySelector.Type,
                        typeof(QuerySourceScope)),
                    _expression,
                    innerSequenceExpression,
                    Expression.Lambda(outerKeySelector, _querySourceScopeParameter),
                    Expression.Lambda(innerKeySelector, itemParameter),
                    Expression.Lambda(
                        scopeCreatorExpression,
                        new[] { _querySourceScopeParameter, itemsParameter }));

            _querySourceMapping.AddMapping(
                groupJoinClause,
                QuerySourceScope.GetResult(_querySourceScopeParameter, groupJoinClause, innerSequenceExpression.Type));
        }

        public override void VisitWhereClause(
            [NotNull] WhereClause whereClause, [NotNull] QueryModel queryModel, int index)
        {
            Check.NotNull(whereClause, "whereClause");
            Check.NotNull(queryModel, "queryModel");

            var predicate
                = ReplaceClauseReferences(
                    CreateQueryingExpressionTreeVisitor(queryModel.MainFromClause)
                        .VisitExpression(whereClause.Predicate));

            _expression
                = Expression.Call(
                    _queryCompilationContext.LinqOperatorProvider.Where.MakeGenericMethod(typeof(QuerySourceScope)),
                    _expression,
                    Expression.Lambda(predicate, _querySourceScopeParameter));
        }

        public override void VisitSelectClause(
            [NotNull] SelectClause selectClause, [NotNull] QueryModel queryModel)
        {
            Check.NotNull(selectClause, "selectClause");
            Check.NotNull(queryModel, "queryModel");

            if (_streamedSequenceInfo != null)
            {
                return;
            }

            var selector
                = ReplaceClauseReferences(
                    CreateProjectionExpressionTreeVisitor()
                        .VisitExpression(selectClause.Selector));

            _expression
                = Expression.Call(
                    _queryCompilationContext.LinqOperatorProvider.Select
                        .MakeGenericMethod(typeof(QuerySourceScope), selector.Type),
                    _expression,
                    Expression.Lambda(selector, _querySourceScopeParameter));

            _streamedSequenceInfo
                = (StreamedSequenceInfo)selectClause.GetOutputDataInfo()
                    .AdjustDataType(typeof(IEnumerable<>));
        }

        public override void VisitOrdering(
            [NotNull] Ordering ordering, [NotNull] QueryModel queryModel, [NotNull] OrderByClause orderByClause, int index)
        {
            Check.NotNull(ordering, "ordering");
            Check.NotNull(queryModel, "queryModel");
            Check.NotNull(orderByClause, "orderByClause");

            var elementType = _expression.Type.GetSequenceType();
            var parameterExpression = _querySourceScopeParameter;
            var resultType = queryModel.GetResultType();

            if (resultType.GetTypeInfo().IsGenericType
                && resultType.GetGenericTypeDefinition() == typeof(IOrderedEnumerable<>))
            {
                VisitSelectClause(queryModel.SelectClause, queryModel);

                parameterExpression
                    = Expression.Parameter(_streamedSequenceInfo.ResultItemType);

                _querySourceMapping
                    .ReplaceMapping(
                        queryModel.MainFromClause, parameterExpression);

                elementType = _streamedSequenceInfo.ResultItemType;
            }

            var expression
                = CreateOrderingExpressionTreeVisitor(ordering)
                    .VisitExpression(ordering.Expression);

            if (expression != null)
            {
                expression = ReplaceClauseReferences(expression);

                _expression
                    = Expression.Call(
                        (index == 0
                            ? _queryCompilationContext.LinqOperatorProvider.OrderBy
                            : _queryCompilationContext.LinqOperatorProvider.ThenBy)
                            .MakeGenericMethod(elementType, expression.Type),
                        _expression,
                        Expression.Lambda(expression, parameterExpression),
                        Expression.Constant(ordering.OrderingDirection));
            }
        }

        public override void VisitResultOperator(
            [NotNull] ResultOperatorBase resultOperator, [NotNull] QueryModel queryModel, int index)
        {
            Check.NotNull(resultOperator, "resultOperator");
            Check.NotNull(queryModel, "queryModel");

            // TODO: sub-queries in result op. expressions

            var streamedDataInfo
                = resultOperator.GetOutputDataInfo(_streamedSequenceInfo);

            var expression
               = _queryCompilationContext.ResultOperatorHandler
                   .HandleResultOperator(this, streamedDataInfo, resultOperator, queryModel);

            if (expression != null)
            {
                _expression = expression;

                _streamedSequenceInfo = streamedDataInfo as StreamedSequenceInfo;
            }
        }

        private Expression ReplaceClauseReferences(Expression expression)
        {
            return ReplaceClauseReferences(expression, _querySourceMapping);
        }

        protected virtual Expression ReplaceClauseReferences(
            [NotNull] Expression expression, [NotNull] QuerySourceMapping querySourceMapping)
        {
            Check.NotNull(expression, "expression");
            Check.NotNull(querySourceMapping, "querySourceMapping");

            return ReferenceReplacingExpressionTreeVisitor
                .ReplaceClauseReferences(expression, querySourceMapping, throwOnUnmappedReferences: false);
        }

        protected class DefaultExpressionTreeVisitor : ExpressionTreeVisitor
        {
            private readonly QueryCompilationContext _queryCompilationContext;

            public DefaultExpressionTreeVisitor([NotNull] QueryCompilationContext queryCompilationContext)
            {
                Check.NotNull(queryCompilationContext, "queryCompilationContext");

                _queryCompilationContext = queryCompilationContext;
            }

            public QueryCompilationContext QueryCompilationContext
            {
                get { return _queryCompilationContext; }
            }

            protected override Expression VisitSubQueryExpression(SubQueryExpression expression)
            {
                var queryModelVisitor = QueryCompilationContext.CreateVisitor();

                queryModelVisitor.VisitQueryModel(expression.QueryModel);

                return queryModelVisitor.Expression;
            }
        }

        protected abstract class QueryingExpressionTreeVisitor : DefaultExpressionTreeVisitor
        {
            protected QueryingExpressionTreeVisitor([NotNull] QueryCompilationContext queryCompilationContext)
                : base(queryCompilationContext)
            {
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

            protected abstract Expression VisitEntityQueryable([NotNull] Type elementType);
        }

        protected class ProjectionExpressionTreeVisitor : DefaultExpressionTreeVisitor
        {
            public ProjectionExpressionTreeVisitor([NotNull] QueryCompilationContext queryCompilationContext)
                : base(queryCompilationContext)
            {
            }

            protected override Expression VisitSubQueryExpression(SubQueryExpression expression)
            {
                var queryModelVisitor = QueryCompilationContext.CreateVisitor();

                queryModelVisitor.VisitQueryModel(expression.QueryModel);

                var subExpression = queryModelVisitor.Expression;

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
                = typeof(ProjectionExpressionTreeVisitor)
                    .GetTypeInfo().GetDeclaredMethod("AsQueryableShim");

            [UsedImplicitly]
            private static IOrderedQueryable<TSource> AsQueryableShim<TSource>(IEnumerable<TSource> source)
            {
                return new EnumerableQuery<TSource>(source);
            }
        }
    }
}
