// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.Logging;
using Remotion.Linq;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Parsing;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class RelationalQueryModelVisitor : EntityQueryModelVisitor
    {
        protected virtual Dictionary<IQuerySource, SelectExpression> QueriesBySource { get; }
            = new Dictionary<IQuerySource, SelectExpression>();

        private readonly Dictionary<IQuerySource, RelationalQueryModelVisitor> _subQueryModelVisitorsBySource
            = new Dictionary<IQuerySource, RelationalQueryModelVisitor>();

        private readonly IRelationalAnnotationProvider _relationalAnnotationProvider;
        private readonly IIncludeExpressionVisitorFactory _includeExpressionVisitorFactory;
        private readonly ISqlTranslatingExpressionVisitorFactory _sqlTranslatingExpressionVisitorFactory;
        private readonly ICompositePredicateExpressionVisitorFactory _compositePredicateExpressionVisitorFactory;
        private readonly IConditionalRemovingExpressionVisitorFactory _conditionalRemovingExpressionVisitorFactory;
        private readonly IQueryFlattenerFactory _queryFlattenerFactory;

        private bool _bindParentQueries;

        private bool _requiresClientSelectMany;
        private bool _requiresClientJoin;
        private bool _requiresClientFilter;
        private bool _requiresClientProjection;
        private bool _requiresClientOrderBy;
        private bool _requiresClientResultOperator;

        private Dictionary<IncludeSpecification, List<int>> _navigationIndexMap = new Dictionary<IncludeSpecification, List<int>>();

        public RelationalQueryModelVisitor(
            [NotNull] IQueryOptimizer queryOptimizer,
            [NotNull] INavigationRewritingExpressionVisitorFactory navigationRewritingExpressionVisitorFactory,
            [NotNull] ISubQueryMemberPushDownExpressionVisitor subQueryMemberPushDownExpressionVisitor,
            [NotNull] IQuerySourceTracingExpressionVisitorFactory querySourceTracingExpressionVisitorFactory,
            [NotNull] IEntityResultFindingExpressionVisitorFactory entityResultFindingExpressionVisitorFactory,
            [NotNull] ITaskBlockingExpressionVisitor taskBlockingExpressionVisitor,
            [NotNull] IMemberAccessBindingExpressionVisitorFactory memberAccessBindingExpressionVisitorFactory,
            [NotNull] IOrderingExpressionVisitorFactory orderingExpressionVisitorFactory,
            [NotNull] IProjectionExpressionVisitorFactory projectionExpressionVisitorFactory,
            [NotNull] IEntityQueryableExpressionVisitorFactory entityQueryableExpressionVisitorFactory,
            [NotNull] IQueryAnnotationExtractor queryAnnotationExtractor,
            [NotNull] IResultOperatorHandler resultOperatorHandler,
            [NotNull] IEntityMaterializerSource entityMaterializerSource,
            [NotNull] IExpressionPrinter expressionPrinter,
            [NotNull] IRelationalAnnotationProvider relationalAnnotationProvider,
            [NotNull] IIncludeExpressionVisitorFactory includeExpressionVisitorFactory,
            [NotNull] ISqlTranslatingExpressionVisitorFactory sqlTranslatingExpressionVisitorFactory,
            [NotNull] ICompositePredicateExpressionVisitorFactory compositePredicateExpressionVisitorFactory,
            [NotNull] IConditionalRemovingExpressionVisitorFactory conditionalRemovingExpressionVisitorFactory,
            [NotNull] IQueryFlattenerFactory queryFlattenerFactory,
            [NotNull] IDbContextOptions contextOptions,
            [NotNull] RelationalQueryCompilationContext queryCompilationContext,
            [CanBeNull] RelationalQueryModelVisitor parentQueryModelVisitor)
            : base(
                Check.NotNull(queryOptimizer, nameof(queryOptimizer)),
                Check.NotNull(navigationRewritingExpressionVisitorFactory, nameof(navigationRewritingExpressionVisitorFactory)),
                Check.NotNull(subQueryMemberPushDownExpressionVisitor, nameof(subQueryMemberPushDownExpressionVisitor)),
                Check.NotNull(querySourceTracingExpressionVisitorFactory, nameof(querySourceTracingExpressionVisitorFactory)),
                Check.NotNull(entityResultFindingExpressionVisitorFactory, nameof(entityResultFindingExpressionVisitorFactory)),
                Check.NotNull(taskBlockingExpressionVisitor, nameof(taskBlockingExpressionVisitor)),
                Check.NotNull(memberAccessBindingExpressionVisitorFactory, nameof(memberAccessBindingExpressionVisitorFactory)),
                Check.NotNull(orderingExpressionVisitorFactory, nameof(orderingExpressionVisitorFactory)),
                Check.NotNull(projectionExpressionVisitorFactory, nameof(projectionExpressionVisitorFactory)),
                Check.NotNull(entityQueryableExpressionVisitorFactory, nameof(entityQueryableExpressionVisitorFactory)),
                Check.NotNull(queryAnnotationExtractor, nameof(queryAnnotationExtractor)),
                Check.NotNull(resultOperatorHandler, nameof(resultOperatorHandler)),
                Check.NotNull(entityMaterializerSource, nameof(entityMaterializerSource)),
                Check.NotNull(expressionPrinter, nameof(expressionPrinter)),
                Check.NotNull(queryCompilationContext, nameof(queryCompilationContext)))
        {
            Check.NotNull(relationalAnnotationProvider, nameof(relationalAnnotationProvider));
            Check.NotNull(includeExpressionVisitorFactory, nameof(includeExpressionVisitorFactory));
            Check.NotNull(sqlTranslatingExpressionVisitorFactory, nameof(sqlTranslatingExpressionVisitorFactory));
            Check.NotNull(compositePredicateExpressionVisitorFactory, nameof(compositePredicateExpressionVisitorFactory));
            Check.NotNull(conditionalRemovingExpressionVisitorFactory, nameof(conditionalRemovingExpressionVisitorFactory));
            Check.NotNull(queryFlattenerFactory, nameof(queryFlattenerFactory));
            Check.NotNull(contextOptions, nameof(contextOptions));

            _relationalAnnotationProvider = relationalAnnotationProvider;
            _includeExpressionVisitorFactory = includeExpressionVisitorFactory;
            _sqlTranslatingExpressionVisitorFactory = sqlTranslatingExpressionVisitorFactory;
            _compositePredicateExpressionVisitorFactory = compositePredicateExpressionVisitorFactory;
            _conditionalRemovingExpressionVisitorFactory = conditionalRemovingExpressionVisitorFactory;
            _queryFlattenerFactory = queryFlattenerFactory;

            ContextOptions = contextOptions;
            ParentQueryModelVisitor = parentQueryModelVisitor;
        }

        protected virtual IDbContextOptions ContextOptions { get; }

        public virtual bool RequiresClientEval { get; set; }

        public virtual bool RequiresClientSelectMany
        {
            get { return _requiresClientSelectMany || RequiresClientEval; }
            set { _requiresClientSelectMany = value; }
        }

        public virtual bool RequiresClientJoin
        {
            get { return _requiresClientJoin || RequiresClientEval; }
            set { _requiresClientJoin = value; }
        }

        public virtual bool RequiresClientFilter
        {
            get { return _requiresClientFilter || RequiresClientEval; }
            set { _requiresClientFilter = value; }
        }

        public virtual bool RequiresClientOrderBy
        {
            get { return _requiresClientOrderBy || RequiresClientEval; }
            set { _requiresClientOrderBy = value; }
        }

        public virtual bool RequiresClientProjection
        {
            get { return _requiresClientProjection || RequiresClientEval; }
            set { _requiresClientProjection = value; }
        }

        public virtual bool RequiresClientResultOperator
        {
            get { return _requiresClientResultOperator || RequiresClientEval; }
            set { _requiresClientResultOperator = value; }
        }

        public new virtual RelationalQueryCompilationContext QueryCompilationContext
            => (RelationalQueryCompilationContext)base.QueryCompilationContext;

        public virtual ICollection<SelectExpression> Queries => QueriesBySource.Values;

        public virtual RelationalQueryModelVisitor ParentQueryModelVisitor { get; }

        public virtual void RegisterSubQueryVisitor(
            [NotNull] IQuerySource querySource, [NotNull] RelationalQueryModelVisitor queryModelVisitor)
        {
            Check.NotNull(querySource, nameof(querySource));
            Check.NotNull(queryModelVisitor, nameof(queryModelVisitor));

            _subQueryModelVisitorsBySource.Add(querySource, queryModelVisitor);
        }

        public virtual void AddQuery([NotNull] IQuerySource querySource, [NotNull] SelectExpression selectExpression)
        {
            Check.NotNull(querySource, nameof(querySource));
            Check.NotNull(selectExpression, nameof(selectExpression));

            QueriesBySource.Add(querySource, selectExpression);
        }

        public virtual SelectExpression TryGetQuery([NotNull] IQuerySource querySource)
        {
            Check.NotNull(querySource, nameof(querySource));

            SelectExpression selectExpression;
            return QueriesBySource.TryGetValue(querySource, out selectExpression)
                ? selectExpression
                : QueriesBySource.Values.SingleOrDefault(se => se.HandlesQuerySource(querySource));
        }

        protected override void IncludeNavigations(
            QueryModel queryModel,
            IReadOnlyCollection<IncludeSpecification> includeSpecifications)
        {
            Check.NotNull(queryModel, nameof(queryModel));
            Check.NotNull(includeSpecifications, nameof(includeSpecifications));

            _navigationIndexMap = BuildNavigationIndexMap(includeSpecifications);

            base.IncludeNavigations(queryModel, includeSpecifications);
        }

        private static Dictionary<IncludeSpecification, List<int>> BuildNavigationIndexMap(
            IEnumerable<IncludeSpecification> includeSpecifications)
        {
            var openedReaderCount = 0;
            var navigationIndexMap = new Dictionary<IncludeSpecification, List<int>>();

            foreach (var includeSpecification in includeSpecifications.Reverse())
            {
                var indexes = new List<int>();
                var openedNewReader = false;

                foreach (var navigation in includeSpecification.NavigationPath)
                {
                    if (navigation.IsCollection())
                    {
                        openedNewReader = true;
                        openedReaderCount++;
                        indexes.Add(openedReaderCount);
                    }
                    else
                    {
                        var index = openedNewReader ? openedReaderCount : 0;
                        indexes.Add(index);
                    }
                }

                navigationIndexMap.Add(includeSpecification, indexes);
            }

            return navigationIndexMap;
        }

        protected override void IncludeNavigations(
            IncludeSpecification includeSpecification,
            Type resultType,
            Expression accessorExpression,
            bool querySourceRequiresTracking)
        {
            Check.NotNull(includeSpecification, nameof(includeSpecification));
            Check.NotNull(resultType, nameof(resultType));

            var includeExpressionVisitor = _includeExpressionVisitorFactory.Create(
                includeSpecification.QuerySource,
                includeSpecification.NavigationPath,
                QueryCompilationContext,
                _navigationIndexMap[includeSpecification],
                querySourceRequiresTracking);

            Expression = includeExpressionVisitor.Visit(Expression);
        }

        public override void VisitQueryModel(QueryModel queryModel)
        {
            Check.NotNull(queryModel, nameof(queryModel));

            var typeIsExpressionTranslatingVisitor = new TypeIsExpressionTranslatingVisitor(QueryCompilationContext.Model, _relationalAnnotationProvider);
            queryModel.TransformExpressions(typeIsExpressionTranslatingVisitor.Visit);

            base.VisitQueryModel(queryModel);

            var compositePredicateVisitor = _compositePredicateExpressionVisitorFactory.Create();

            foreach (var selectExpression in QueriesBySource.Values)
            {
                selectExpression.Predicate
                    = compositePredicateVisitor.Visit(selectExpression.Predicate);
            }
        }

        public virtual void VisitSubQueryModel([NotNull] QueryModel queryModel)
        {
            _bindParentQueries = true;

            VisitQueryModel(queryModel);
        }

        protected override Expression CompileMainFromClauseExpression(
            MainFromClause mainFromClause, QueryModel queryModel)
        {
            Check.NotNull(mainFromClause, nameof(mainFromClause));
            Check.NotNull(queryModel, nameof(queryModel));

            var expression = base.CompileMainFromClauseExpression(mainFromClause, queryModel);

            return LiftSubQuery(mainFromClause, mainFromClause.FromExpression, expression);
        }

        public override void VisitAdditionalFromClause(
            AdditionalFromClause fromClause, QueryModel queryModel, int index)
        {
            Check.NotNull(fromClause, nameof(fromClause));
            Check.NotNull(queryModel, nameof(queryModel));

            base.VisitAdditionalFromClause(fromClause, queryModel, index);

            RequiresClientSelectMany = true;

            var selectExpression = TryGetQuery(fromClause);

            if (selectExpression != null
                && selectExpression.Tables.Count == 1)
            {
                var previousQuerySource = FindPreviousQuerySource(queryModel, index);

                if (previousQuerySource != null
                    && !RequiresClientJoin)
                {
                    var previousSelectExpression = TryGetQuery(previousQuerySource);

                    if (previousSelectExpression != null)
                    {
                        if (!QueryCompilationContext.QuerySourceRequiresMaterialization(previousQuerySource))
                        {
                            previousSelectExpression.ClearProjection();
                            previousSelectExpression.IsProjectStar = false;
                        }

                        var readerOffset = previousSelectExpression.Projection.Count;

                        var correlated = selectExpression.IsCorrelated();

                        if (correlated)
                        {
                            if (!QueryCompilationContext.IsLateralJoinSupported)
                            {
                                return;
                            }

                            previousSelectExpression
                                .AddLateralJoin(selectExpression.Tables.First(), selectExpression.Projection);
                        }
                        else
                        {
                            previousSelectExpression
                                .AddCrossJoin(selectExpression.Tables.First(), selectExpression.Projection);
                        }

                        QueriesBySource.Remove(fromClause);

                        Expression
                            = _queryFlattenerFactory
                                .Create(
                                    fromClause,
                                    QueryCompilationContext,
                                    LinqOperatorProvider.SelectMany,
                                    readerOffset)
                                .Flatten((MethodCallExpression)Expression);

                        RequiresClientSelectMany = false;
                    }
                }
            }

            if (RequiresClientSelectMany)
            {
                CheckClientEval(fromClause);
            }
        }

        protected override Expression CompileAdditionalFromClauseExpression(
            AdditionalFromClause additionalFromClause, QueryModel queryModel)
        {
            Check.NotNull(additionalFromClause, nameof(additionalFromClause));
            Check.NotNull(queryModel, nameof(queryModel));

            var expression = base.CompileAdditionalFromClauseExpression(additionalFromClause, queryModel);

            return LiftSubQuery(additionalFromClause, additionalFromClause.FromExpression, expression);
        }

        public override void VisitJoinClause(JoinClause joinClause, QueryModel queryModel, int index)
        {
            Check.NotNull(joinClause, nameof(joinClause));
            Check.NotNull(queryModel, nameof(queryModel));

            OptimizeJoinClause(
                joinClause,
                queryModel,
                index,
                () => base.VisitJoinClause(joinClause, queryModel, index),
                LinqOperatorProvider.Join);
        }

        protected override Expression CompileJoinClauseInnerSequenceExpression(
            JoinClause joinClause, QueryModel queryModel)
        {
            Check.NotNull(joinClause, nameof(joinClause));
            Check.NotNull(queryModel, nameof(queryModel));

            var expression = base.CompileJoinClauseInnerSequenceExpression(joinClause, queryModel);

            return LiftSubQuery(joinClause, joinClause.InnerSequence, expression);
        }

        public override void VisitGroupJoinClause(
            GroupJoinClause groupJoinClause, QueryModel queryModel, int index)
        {
            Check.NotNull(groupJoinClause, nameof(groupJoinClause));
            Check.NotNull(queryModel, nameof(queryModel));

            OptimizeJoinClause(
                groupJoinClause.JoinClause,
                queryModel,
                index,
                () => base.VisitGroupJoinClause(groupJoinClause, queryModel, index),
                LinqOperatorProvider.GroupJoin,
                outerJoin: true);
        }

        protected virtual void OptimizeJoinClause(
            [NotNull] JoinClause joinClause,
            [NotNull] QueryModel queryModel,
            int index,
            [NotNull] Action baseVisitAction,
            [NotNull] MethodInfo operatorToFlatten,
            bool outerJoin = false)
        {
            Check.NotNull(joinClause, nameof(joinClause));
            Check.NotNull(queryModel, nameof(queryModel));
            Check.NotNull(baseVisitAction, nameof(baseVisitAction));
            Check.NotNull(operatorToFlatten, nameof(operatorToFlatten));

            RequiresClientJoin = true;

            var previousQuerySource = FindPreviousQuerySource(queryModel, index);

            var previousSelectExpression
                = previousQuerySource != null
                    ? TryGetQuery(previousQuerySource)
                    : null;

            var previousSelectProjectionCount
                = previousSelectExpression?.Projection.Count ?? -1;

            baseVisitAction();

            if (previousSelectExpression != null)
            {
                var selectExpression = TryGetQuery(joinClause);

                if (selectExpression != null)
                {
                    var sqlTranslatingExpressionVisitor
                        = _sqlTranslatingExpressionVisitorFactory.Create(this);

                    var predicate
                        = sqlTranslatingExpressionVisitor
                            .Visit(
                                Expression.Equal(
                                    joinClause.OuterKeySelector,
                                    joinClause.InnerKeySelector));

                    if (predicate != null)
                    {
                        QueriesBySource.Remove(joinClause);

                        previousSelectExpression.RemoveRangeFromProjection(previousSelectProjectionCount);

                        var tableExpression = selectExpression.Tables.Single();

                        var projection
                            = QueryCompilationContext
                                .QuerySourceRequiresMaterialization(joinClause)
                                ? selectExpression.Projection
                                : Enumerable.Empty<Expression>();

                        var joinExpression
                            = !outerJoin
                                ? previousSelectExpression.AddInnerJoin(tableExpression, projection)
                                : previousSelectExpression.AddLeftOuterJoin(tableExpression, projection);

                        joinExpression.Predicate = predicate;

                        if (outerJoin)
                        {
                            var outerJoinOrderingExtractor = new OuterJoinOrderingExtractor();

                            outerJoinOrderingExtractor.Visit(predicate);

                            foreach (var expression in outerJoinOrderingExtractor.Expressions)
                            {
                                previousSelectExpression
                                    .AddToOrderBy(new Ordering(expression, OrderingDirection.Asc));
                            }
                        }

                        Expression
                            = _queryFlattenerFactory
                                .Create(
                                    joinClause,
                                    QueryCompilationContext,
                                    operatorToFlatten,
                                    previousSelectProjectionCount)
                                .Flatten((MethodCallExpression)Expression);

                        RequiresClientJoin = false;
                    }
                }
            }

            if (RequiresClientJoin)
            {
                CheckClientEval(joinClause);
            }
        }

        private class OuterJoinOrderingExtractor : ExpressionVisitor
        {
            private readonly List<Expression> _expressions = new List<Expression>();

            public IEnumerable<Expression> Expressions => _expressions;

            public override Expression Visit(Expression expression)
            {
                var binaryExpression = expression as BinaryExpression;

                if (binaryExpression != null)
                {
                    switch (binaryExpression.NodeType)
                    {
                        case ExpressionType.Equal:
                            _expressions.Add(binaryExpression.Left.RemoveConvert());
                            return expression;
                        case ExpressionType.AndAlso:
                            return VisitBinary(binaryExpression);
                    }
                }

                return expression;
            }
        }

        private static IQuerySource FindPreviousQuerySource(QueryModel queryModel, int index)
        {
            for (var i = index; i >= 0; i--)
            {
                var candidate = i == 0
                    ? queryModel.MainFromClause
                    : queryModel.BodyClauses[i - 1] as IQuerySource;

                if (candidate != null)
                {
                    return candidate;
                }
            }

            return null;
        }

        protected override Expression CompileGroupJoinInnerSequenceExpression(
            GroupJoinClause groupJoinClause, QueryModel queryModel)
        {
            Check.NotNull(groupJoinClause, nameof(groupJoinClause));
            Check.NotNull(queryModel, nameof(queryModel));

            var expression = base.CompileGroupJoinInnerSequenceExpression(groupJoinClause, queryModel);

            return LiftSubQuery(groupJoinClause.JoinClause, groupJoinClause.JoinClause.InnerSequence, expression);
        }

        private Expression LiftSubQuery(
            IQuerySource querySource, Expression itemsExpression, Expression expression)
        {
            var subQueryExpression = itemsExpression as SubQueryExpression;

            if (subQueryExpression == null)
            {
                return expression;
            }

            var subQueryModelVisitor
                = (RelationalQueryModelVisitor)QueryCompilationContext
                    .CreateQueryModelVisitor(this);

            subQueryModelVisitor.VisitSubQueryModel(subQueryExpression.QueryModel);

            if (subQueryModelVisitor.Queries.Count == 1
                && !subQueryModelVisitor.RequiresClientEval
                && !subQueryModelVisitor.RequiresClientSelectMany
                && !subQueryModelVisitor.RequiresClientJoin
                && !subQueryModelVisitor.RequiresClientFilter
                && !subQueryModelVisitor.RequiresClientProjection
                && !subQueryModelVisitor.RequiresClientOrderBy
                && !subQueryModelVisitor.RequiresClientResultOperator)
            {
                var subSelectExpression = subQueryModelVisitor.Queries.First();

                if ((!subSelectExpression.OrderBy.Any()
                     || subSelectExpression.Limit != null)
                    && (QueryCompilationContext.IsLateralJoinSupported
                        || !subSelectExpression.IsCorrelated()
                        || !(querySource is AdditionalFromClause)))
                {
                    subSelectExpression.PushDownSubquery().QuerySource = querySource;

                    AddQuery(querySource, subSelectExpression);

                    var newExpression
                        = new QuerySourceUpdater(
                            querySource,
                            QueryCompilationContext,
                            LinqOperatorProvider,
                            subSelectExpression)
                            .Visit(subQueryModelVisitor.Expression);

                    return newExpression;
                }
            }

            return expression;
        }

        private sealed class QuerySourceUpdater : ExpressionVisitorBase
        {
            private readonly IQuerySource _querySource;
            private readonly RelationalQueryCompilationContext _relationalQueryCompilationContext;
            private readonly ILinqOperatorProvider _linqOperatorProvider;
            private readonly SelectExpression _selectExpression;

            public QuerySourceUpdater(
                IQuerySource querySource,
                RelationalQueryCompilationContext relationalQueryCompilationContext,
                ILinqOperatorProvider linqOperatorProvider,
                SelectExpression selectExpression)
            {
                _querySource = querySource;
                _relationalQueryCompilationContext = relationalQueryCompilationContext;
                _linqOperatorProvider = linqOperatorProvider;
                _selectExpression = selectExpression;
            }

            protected override Expression VisitConstant(ConstantExpression constantExpression)
            {
                var shaper = constantExpression.Value as Shaper;

                if (shaper != null)
                {
                    foreach (var queryAnnotation
                        in _relationalQueryCompilationContext.QueryAnnotations
                            .Where(qa => shaper.IsShaperForQuerySource(qa.QuerySource)))
                    {
                        queryAnnotation.QuerySource = _querySource;
                    }

                    if (!_relationalQueryCompilationContext.QuerySourceRequiresMaterialization(_querySource)
                        && shaper is EntityShaper)
                    {
                        return Expression.Constant(new ValueBufferShaper(_querySource));
                    }

                    shaper.UpdateQuerySource(_querySource);

                    _selectExpression.ExplodeStarProjection();
                }

                return base.VisitConstant(constantExpression);
            }

            protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
            {
                var arguments = VisitAndConvert(methodCallExpression.Arguments, "VisitMethodCall");

                if (arguments != methodCallExpression.Arguments)
                {
                    if (methodCallExpression.Method.MethodIsClosedFormOf(
                        _relationalQueryCompilationContext.QueryMethodProvider.ShapedQueryMethod))
                    {
                        return Expression.Call(
                            _relationalQueryCompilationContext.QueryMethodProvider.ShapedQueryMethod
                                .MakeGenericMethod(((Shaper)((ConstantExpression)arguments[2]).Value).Type),
                            arguments);
                    }

                    if (methodCallExpression.Method.MethodIsClosedFormOf(
                        _linqOperatorProvider.Cast)
                        && arguments[0].Type.GetSequenceType() == typeof(ValueBuffer))
                    {
                        return arguments[0];
                    }
                }

                return base.VisitMethodCall(methodCallExpression);
            }

            protected override Expression VisitLambda<T>(Expression<T> lambdaExpression)
            {
                Check.NotNull(lambdaExpression, nameof(lambdaExpression));

                var newBodyExpression = Visit(lambdaExpression.Body);

                return newBodyExpression != lambdaExpression.Body
                    ? Expression.Lambda(newBodyExpression, lambdaExpression.Parameters)
                    : lambdaExpression;
            }
        }

        public override void VisitWhereClause(WhereClause whereClause, QueryModel queryModel, int index)
        {
            Check.NotNull(whereClause, nameof(whereClause));
            Check.NotNull(queryModel, nameof(queryModel));

            var selectExpression = TryGetQuery(queryModel.MainFromClause);
            var requiresClientFilter = selectExpression == null;

            if (!requiresClientFilter)
            {
                var sqlTranslatingExpressionVisitor
                    = _sqlTranslatingExpressionVisitorFactory
                        .Create(this, selectExpression, whereClause.Predicate, _bindParentQueries);

                var sqlPredicateExpression = sqlTranslatingExpressionVisitor.Visit(whereClause.Predicate);

                if (sqlPredicateExpression != null)
                {
                    sqlPredicateExpression =
                        _conditionalRemovingExpressionVisitorFactory
                            .Create()
                            .Visit(sqlPredicateExpression);

                    selectExpression.Predicate
                        = selectExpression.Predicate == null
                            ? sqlPredicateExpression
                            : Expression.AndAlso(selectExpression.Predicate, sqlPredicateExpression);
                }
                else
                {
                    requiresClientFilter = true;
                }

                if (sqlTranslatingExpressionVisitor.ClientEvalPredicate != null
                    && selectExpression.Predicate != null)
                {
                    requiresClientFilter = true;
                    whereClause = new WhereClause(sqlTranslatingExpressionVisitor.ClientEvalPredicate);
                }
            }

            RequiresClientFilter |= requiresClientFilter;

            if (RequiresClientFilter)
            {
                CheckClientEval(whereClause.Predicate);

                base.VisitWhereClause(whereClause, queryModel, index);
            }
        }

        public override void VisitOrderByClause(OrderByClause orderByClause, QueryModel queryModel, int index)
        {
            Check.NotNull(orderByClause, nameof(orderByClause));
            Check.NotNull(queryModel, nameof(queryModel));

            var selectExpression = TryGetQuery(queryModel.MainFromClause);
            var requiresClientOrderBy = selectExpression == null;

            if (!requiresClientOrderBy)
            {
                var sqlTranslatingExpressionVisitor
                    = _sqlTranslatingExpressionVisitorFactory
                        .Create(this, selectExpression, bindParentQueries: _bindParentQueries);

                var orderings = new List<Ordering>();

                foreach (var ordering in orderByClause.Orderings)
                {
                    var sqlOrderingExpression
                        = sqlTranslatingExpressionVisitor
                            .Visit(ordering.Expression);

                    if (sqlOrderingExpression == null)
                    {
                        break;
                    }

                    if (sqlOrderingExpression.IsComparisonOperation()
                        || sqlOrderingExpression.IsLogicalOperation())
                    {
                        sqlOrderingExpression = Expression.Condition(
                            sqlOrderingExpression,
                            Expression.Constant(true, typeof(bool)),
                            Expression.Constant(false, typeof(bool)));
                    }

                    orderings.Add(
                        new Ordering(
                            sqlOrderingExpression,
                            ordering.OrderingDirection));
                }

                if (orderings.Count == orderByClause.Orderings.Count)
                {
                    selectExpression.PrependToOrderBy(orderings);
                }
                else
                {
                    requiresClientOrderBy = true;
                }
            }

            RequiresClientOrderBy |= requiresClientOrderBy;

            if (RequiresClientOrderBy)
            {
                CheckClientEval(orderByClause);

                base.VisitOrderByClause(orderByClause, queryModel, index);
            }
        }

        public override void VisitResultOperator(ResultOperatorBase resultOperator, QueryModel queryModel, int index)
        {
            base.VisitResultOperator(resultOperator, queryModel, index);

            if (RequiresClientResultOperator)
            {
                CheckClientEval(resultOperator);
            }
        }

        protected virtual void CheckClientEval([NotNull] object expression)
        {
            Check.NotNull(expression, nameof(expression));

            var relationalOptionsExtension = RelationalOptionsExtension.Extract(ContextOptions);

            switch (relationalOptionsExtension.QueryClientEvaluationBehavior)
            {
                case QueryClientEvaluationBehavior.Throw:
                    throw new InvalidOperationException(RelationalStrings.ClientEvalDisabled(expression));
                case QueryClientEvaluationBehavior.Warn:
                    QueryCompilationContext.Logger.LogWarning(
                        RelationalLoggingEventId.ClientEvalWarning,
                        () => RelationalStrings.ClientEvalWarning(expression));
                    break;
            }
        }

        private class TypeIsExpressionTranslatingVisitor : ExpressionVisitorBase
        {
            private readonly IModel _model;
            private readonly IRelationalAnnotationProvider _relationalAnnotationProvider;

            public TypeIsExpressionTranslatingVisitor(IModel model, IRelationalAnnotationProvider relationalAnnotationProvider)
            {
                _model = model;
                _relationalAnnotationProvider = relationalAnnotationProvider;
            }

            protected override Expression VisitTypeBinary(TypeBinaryExpression typeBinaryExpression)
            {
                if (typeBinaryExpression.NodeType != ExpressionType.TypeIs)
                {
                    return base.VisitTypeBinary(typeBinaryExpression);
                }

                var entityType = _model.FindEntityType(typeBinaryExpression.TypeOperand);

                if (entityType == null)
                {
                    return base.VisitTypeBinary(typeBinaryExpression);
                }

                var concreteEntityTypes
                    = entityType.GetConcreteTypesInHierarchy().ToArray();

                if (concreteEntityTypes.Length != 1
                    || concreteEntityTypes[0].RootType() != concreteEntityTypes[0])
                {
                    var discriminatorProperty
                        = _relationalAnnotationProvider.For(concreteEntityTypes[0]).DiscriminatorProperty;

                    var discriminatorPropertyExpression = CreatePropertyExpression(typeBinaryExpression.Expression, discriminatorProperty);

                    var discriminatorPredicate
                        = concreteEntityTypes
                            .Select(concreteEntityType =>
                                Expression.Equal(
                                    discriminatorPropertyExpression,
                                    Expression.Constant(_relationalAnnotationProvider.For(concreteEntityType).DiscriminatorValue)))
                            .Aggregate((current, next) => Expression.OrElse(next, current));

                    return discriminatorPredicate;
                }

                return Expression.Constant(true, typeof(bool));
            }
        }

        #region Binding

        public override Expression BindMemberToValueBuffer(MemberExpression memberExpression, Expression expression)
        {
            Check.NotNull(memberExpression, nameof(memberExpression));
            Check.NotNull(expression, nameof(expression));

            return BindMemberExpression(
                memberExpression,
                (property, querySource, selectExpression) =>
                    {
                        var projectionIndex = selectExpression.GetProjectionIndex(property, querySource);

                        Debug.Assert(projectionIndex > -1);

                        return BindReadValueMethod(memberExpression.Type, expression, projectionIndex);
                    },
                bindSubQueries: true);
        }

        public override Expression BindMethodCallToValueBuffer(
            MethodCallExpression methodCallExpression, Expression expression)
        {
            Check.NotNull(methodCallExpression, nameof(methodCallExpression));
            Check.NotNull(expression, nameof(expression));

            return BindMethodCallExpression(
                methodCallExpression,
                (property, querySource, selectExpression) =>
                    {
                        var projectionIndex = selectExpression.GetProjectionIndex(property, querySource);

                        Debug.Assert(projectionIndex > -1);

                        return BindReadValueMethod(methodCallExpression.Type, expression, projectionIndex);
                    },
                bindSubQueries: true)
                   ?? ParentQueryModelVisitor?
                       .BindMethodCallToValueBuffer(methodCallExpression, expression);
        }

        public virtual TResult BindMemberExpression<TResult>(
            [NotNull] MemberExpression memberExpression,
            [NotNull] Func<IProperty, IQuerySource, SelectExpression, TResult> memberBinder,
            bool bindSubQueries = false)
        {
            Check.NotNull(memberExpression, nameof(memberExpression));
            Check.NotNull(memberBinder, nameof(memberBinder));

            return BindMemberExpression(memberExpression, null, memberBinder, bindSubQueries);
        }

        private TResult BindMemberExpression<TResult>(
            [NotNull] MemberExpression memberExpression,
            [CanBeNull] IQuerySource querySource,
            Func<IProperty, IQuerySource, SelectExpression, TResult> memberBinder,
            bool bindSubQueries)
        {
            Check.NotNull(memberExpression, nameof(memberExpression));
            Check.NotNull(memberBinder, nameof(memberBinder));

            return base.BindMemberExpression(memberExpression, querySource,
                (property, qs) => BindMemberOrMethod(memberBinder, qs, property, bindSubQueries));
        }

        public virtual TResult BindMethodCallExpression<TResult>(
            [NotNull] MethodCallExpression methodCallExpression,
            [NotNull] Func<IProperty, IQuerySource, SelectExpression, TResult> memberBinder,
            bool bindSubQueries = false)
        {
            Check.NotNull(methodCallExpression, nameof(methodCallExpression));
            Check.NotNull(memberBinder, nameof(memberBinder));

            return BindMethodCallExpression(methodCallExpression, null, memberBinder, bindSubQueries);
        }

        private TResult BindMethodCallExpression<TResult>(
            MethodCallExpression methodCallExpression,
            IQuerySource querySource,
            Func<IProperty, IQuerySource, SelectExpression, TResult> memberBinder,
            bool bindSubQueries)
        {
            return base.BindMethodCallExpression(methodCallExpression, querySource,
                (property, qs) => BindMemberOrMethod(memberBinder, qs, property, bindSubQueries));
        }

        public virtual Expression BindLocalMethodCallExpression(
            [NotNull] MethodCallExpression methodCallExpression)
        {
            Check.NotNull(methodCallExpression, nameof(methodCallExpression));

            return base.BindMethodCallExpression<Expression>(methodCallExpression, null,
                (property, qs) =>
                    {
                        var parameterExpression = methodCallExpression.Arguments[0] as ParameterExpression;

                        if (parameterExpression != null)
                        {
                            return new PropertyParameterExpression(parameterExpression.Name, property);
                        }

                        var constantExpression = methodCallExpression.Arguments[0] as ConstantExpression;

                        if (constantExpression != null)
                        {
                            return Expression.Constant(
                                property.GetGetter().GetClrValue(constantExpression.Value),
                                methodCallExpression.Method.GetGenericArguments()[0]);
                        }

                        return null;
                    });
        }

        private TResult BindMemberOrMethod<TResult>(
            Func<IProperty, IQuerySource, SelectExpression, TResult> memberBinder,
            IQuerySource querySource,
            IProperty property,
            bool bindSubQueries)
        {
            if (querySource != null)
            {
                var selectExpression = TryGetQuery(querySource);

                if (selectExpression == null
                    && bindSubQueries)
                {
                    RelationalQueryModelVisitor subQueryModelVisitor;
                    if (_subQueryModelVisitorsBySource.TryGetValue(querySource, out subQueryModelVisitor))
                    {
                        selectExpression = subQueryModelVisitor.Queries.SingleOrDefault();

                        selectExpression?
                            .AddToProjection(
                                _relationalAnnotationProvider.For(property).ColumnName,
                                property,
                                querySource);
                    }
                }

                if (selectExpression != null)
                {
                    return memberBinder(property, querySource, selectExpression);
                }

                selectExpression
                    = ParentQueryModelVisitor?.TryGetQuery(querySource);

                selectExpression?.AddToProjection(
                    _relationalAnnotationProvider.For(property).ColumnName,
                    property,
                    querySource);
            }

            return default(TResult);
        }

        #endregion
    }
}
