// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Internal;
using Microsoft.Data.Entity.Query.Expressions;
using Microsoft.Data.Entity.Query.ExpressionVisitors;
using Microsoft.Data.Entity.Query.Internal;
using Microsoft.Data.Entity.Utilities;
using Remotion.Linq;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;

namespace Microsoft.Data.Entity.Query
{
    public class RelationalQueryModelVisitor : EntityQueryModelVisitor
    {
        private readonly Dictionary<IQuerySource, SelectExpression> _queriesBySource
            = new Dictionary<IQuerySource, SelectExpression>();

        private readonly Dictionary<IQuerySource, RelationalQueryModelVisitor> _subQueryModelVisitorsBySource
            = new Dictionary<IQuerySource, RelationalQueryModelVisitor>();

        private readonly IRelationalMetadataExtensionProvider _relationalMetadataExtensionProvider;
        private readonly IIncludeExpressionVisitorFactory _includeExpressionVisitorFactory;
        private readonly ISqlTranslatingExpressionVisitorFactory _sqlTranslatingExpressionVisitorFactory;
        private readonly ICompositePredicateExpressionVisitorFactory _compositePredicateExpressionVisitorFactory;
        private readonly IQueryFlatteningExpressionVisitorFactory _queryFlatteningExpressionVisitorFactory;
        private readonly IShapedQueryFindingExpressionVisitorFactory _shapedQueryFindingExpressionVisitorFactory;

        private bool _bindParentQueries;

        private bool _requiresClientFilter;
        private bool _requiresClientSelectMany;
        private bool _requiresClientJoin;
        private bool _requiresClientProjection;
        private bool _requiresClientResultOperator;
        private Dictionary<IncludeSpecification, List<int>> _navigationIndexMap = new Dictionary<IncludeSpecification, List<int>>();

        public RelationalQueryModelVisitor(
            [NotNull] IModel model,
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
            [NotNull] IRelationalMetadataExtensionProvider relationalMetadataExtensionProvider,
            [NotNull] IIncludeExpressionVisitorFactory includeExpressionVisitorFactory,
            [NotNull] ISqlTranslatingExpressionVisitorFactory sqlTranslatingExpressionVisitorFactory,
            [NotNull] ICompositePredicateExpressionVisitorFactory compositePredicateExpressionVisitorFactory,
            [NotNull] IQueryFlatteningExpressionVisitorFactory queryFlatteningExpressionVisitorFactory,
            [NotNull] IShapedQueryFindingExpressionVisitorFactory shapedQueryFindingExpressionVisitorFactory,
            [NotNull] RelationalQueryCompilationContext queryCompilationContext,
            [CanBeNull] RelationalQueryModelVisitor parentQueryModelVisitor)
            : base(
                  Check.NotNull(model, nameof(model)),
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
            Check.NotNull(relationalMetadataExtensionProvider, nameof(relationalMetadataExtensionProvider));
            Check.NotNull(includeExpressionVisitorFactory, nameof(includeExpressionVisitorFactory));
            Check.NotNull(sqlTranslatingExpressionVisitorFactory, nameof(sqlTranslatingExpressionVisitorFactory));
            Check.NotNull(compositePredicateExpressionVisitorFactory, nameof(compositePredicateExpressionVisitorFactory));
            Check.NotNull(queryFlatteningExpressionVisitorFactory, nameof(queryFlatteningExpressionVisitorFactory));
            Check.NotNull(shapedQueryFindingExpressionVisitorFactory, nameof(shapedQueryFindingExpressionVisitorFactory));

            _relationalMetadataExtensionProvider = relationalMetadataExtensionProvider;
            _includeExpressionVisitorFactory = includeExpressionVisitorFactory;
            _sqlTranslatingExpressionVisitorFactory = sqlTranslatingExpressionVisitorFactory;
            _compositePredicateExpressionVisitorFactory = compositePredicateExpressionVisitorFactory;
            _queryFlatteningExpressionVisitorFactory = queryFlatteningExpressionVisitorFactory;
            _shapedQueryFindingExpressionVisitorFactory = shapedQueryFindingExpressionVisitorFactory;
            ParentQueryModelVisitor = parentQueryModelVisitor;
        }

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

        public virtual ICollection<SelectExpression> Queries => _queriesBySource.Values;

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

            _queriesBySource.Add(querySource, selectExpression);
        }

        public virtual SelectExpression TryGetQuery([NotNull] IQuerySource querySource)
        {
            Check.NotNull(querySource, nameof(querySource));

            SelectExpression selectExpression;
            return (_queriesBySource.TryGetValue(querySource, out selectExpression)
                ? selectExpression
                : _queriesBySource.Values.SingleOrDefault(se => se.HandlesQuerySource(querySource)));
        }

        public override void VisitQueryModel(QueryModel queryModel)
        {
            Check.NotNull(queryModel, nameof(queryModel));

            base.VisitQueryModel(queryModel);

            var compositePredicateVisitor
                = _compositePredicateExpressionVisitorFactory.Create(
                    QueryCompilationContext
                        .GetCustomQueryAnnotations(RelationalQueryableExtensions.UseRelationalNullSemanticsMethodInfo)
                        .Any());

            foreach (var selectExpression in _queriesBySource.Values.Where(se => se.Predicate != null))
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
            LambdaExpression accessorLambda,
            bool querySourceRequiresTracking)
        {
            Check.NotNull(includeSpecification, nameof(includeSpecification));
            Check.NotNull(resultType, nameof(resultType));
            Check.NotNull(accessorLambda, nameof(accessorLambda));

            Expression
                = _includeExpressionVisitorFactory
                    .Create(
                        includeSpecification.QuerySource,
                        includeSpecification.NavigationPath,
                        QueryCompilationContext,
                        _navigationIndexMap[includeSpecification],
                        querySourceRequiresTracking)
                    .Visit(Expression);
        }

        protected override Expression CompileMainFromClauseExpression(
            MainFromClause mainFromClause, QueryModel queryModel)
        {
            Check.NotNull(mainFromClause, nameof(mainFromClause));
            Check.NotNull(queryModel, nameof(queryModel));

            var expression = base.CompileMainFromClauseExpression(mainFromClause, queryModel);

            return LiftSubQuery(mainFromClause, mainFromClause.FromExpression, queryModel, expression);
        }

        public override void VisitAdditionalFromClause(AdditionalFromClause fromClause, QueryModel queryModel, int index)
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
                        var readerOffset = previousSelectExpression.Projection.Count;

                        var correlated = selectExpression.IsCorrelated();

                        if (correlated)
                        {
                            if (!QueryCompilationContext.IsCrossApplySupported)
                            {
                                return;
                            }

                            previousSelectExpression
                                .AddCrossApply(selectExpression.Tables.First(), selectExpression.Projection);
                        }
                        else
                        {
                            previousSelectExpression
                                .AddCrossJoin(selectExpression.Tables.First(), selectExpression.Projection);
                        }

                        _queriesBySource.Remove(fromClause);

                        Expression
                            = _queryFlatteningExpressionVisitorFactory.Create(
                                previousQuerySource,
                                fromClause,
                                QueryCompilationContext,
                                readerOffset,
                                QueryCompilationContext.LinqOperatorProvider.SelectMany)
                                .Visit(Expression);

                        RequiresClientSelectMany = false;
                    }
                }
            }
        }

        protected override Expression CompileAdditionalFromClauseExpression(
            AdditionalFromClause additionalFromClause, QueryModel queryModel)
        {
            Check.NotNull(additionalFromClause, nameof(additionalFromClause));
            Check.NotNull(queryModel, nameof(queryModel));

            var expression = base.CompileAdditionalFromClauseExpression(additionalFromClause, queryModel);

            return LiftSubQuery(additionalFromClause, additionalFromClause.FromExpression, queryModel, expression);
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
                QueryCompilationContext.LinqOperatorProvider.Join);
        }

        protected override Expression CompileJoinClauseInnerSequenceExpression(JoinClause joinClause, QueryModel queryModel)
        {
            Check.NotNull(joinClause, nameof(joinClause));
            Check.NotNull(queryModel, nameof(queryModel));

            var expression = base.CompileJoinClauseInnerSequenceExpression(joinClause, queryModel);

            return LiftSubQuery(joinClause, joinClause.InnerSequence, queryModel, expression);
        }

        public override void VisitGroupJoinClause(GroupJoinClause groupJoinClause, QueryModel queryModel, int index)
        {
            Check.NotNull(groupJoinClause, nameof(groupJoinClause));
            Check.NotNull(queryModel, nameof(queryModel));

            OptimizeJoinClause(
                groupJoinClause.JoinClause,
                queryModel,
                index,
                () => base.VisitGroupJoinClause(groupJoinClause, queryModel, index),
                QueryCompilationContext.LinqOperatorProvider.GroupJoin,
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
                        _queriesBySource.Remove(joinClause);

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
                                : previousSelectExpression.AddOuterJoin(tableExpression, projection);

                        joinExpression.Predicate = predicate;

                        Expression
                            = _queryFlatteningExpressionVisitorFactory.Create(
                                previousQuerySource,
                                joinClause,
                                QueryCompilationContext,
                                previousSelectProjectionCount,
                                operatorToFlatten)
                                .Visit(Expression);

                        RequiresClientJoin = false;
                    }
                    else
                    {
                        previousSelectExpression
                            .RemoveRangeFromProjection(previousSelectProjectionCount);
                    }
                }
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

        protected override Expression CompileGroupJoinInnerSequenceExpression(GroupJoinClause groupJoinClause, QueryModel queryModel)
        {
            Check.NotNull(groupJoinClause, nameof(groupJoinClause));
            Check.NotNull(queryModel, nameof(queryModel));

            var expression = base.CompileGroupJoinInnerSequenceExpression(groupJoinClause, queryModel);

            return LiftSubQuery(groupJoinClause.JoinClause, groupJoinClause.JoinClause.InnerSequence, queryModel, expression);
        }

        private Expression LiftSubQuery(
            IQuerySource querySource, Expression itemsExpression, QueryModel queryModel, Expression expression)
        {
            var subQueryExpression = itemsExpression as SubQueryExpression;

            if (subQueryExpression == null)
            {
                return expression;
            }

            var subQueryModelVisitor
                = (RelationalQueryModelVisitor)QueryCompilationContext.CreateQueryModelVisitor(this);

            subQueryModelVisitor.VisitSubQueryModel(subQueryExpression.QueryModel);

            SelectExpression subSelectExpression = null;

            if (subQueryModelVisitor.Queries.Count == 1
                && !subQueryModelVisitor.RequiresClientFilter
                && !subQueryModelVisitor.RequiresClientSelectMany
                && !subQueryModelVisitor.RequiresClientProjection
                && !subQueryModelVisitor.RequiresClientResultOperator)
            {
                subSelectExpression = subQueryModelVisitor.Queries.First();
            }

            if (subSelectExpression != null
                && (!subSelectExpression.OrderBy.Any()
                    || subSelectExpression.Limit != null)
                && (QueryCompilationContext.IsCrossApplySupported
                    || (!subSelectExpression.IsCorrelated()
                        || !(querySource is AdditionalFromClause))))
            {
                subSelectExpression.PushDownSubquery().QuerySource = querySource;

                AddQuery(querySource, subSelectExpression);

                var shapedQueryMethodExpression
                    = _shapedQueryFindingExpressionVisitorFactory
                        .Create(QueryCompilationContext)
                        .Find(subQueryModelVisitor.Expression);

                var shaperLambda = (LambdaExpression)shapedQueryMethodExpression.Arguments[2];
                var shaperMethodCall = (MethodCallExpression)shaperLambda.Body;

                var shaperMethod = shaperMethodCall.Method;
                var shaperMethodArgs = shaperMethodCall.Arguments.ToList();

                if (!QueryCompilationContext.QuerySourceRequiresMaterialization(querySource)
                    && shaperMethod.MethodIsClosedFormOf(RelationalEntityQueryableExpressionVisitor.CreateEntityMethodInfo))
                {
                    shaperMethod = RelationalEntityQueryableExpressionVisitor.CreateValueBufferMethodInfo;
                    shaperMethodArgs.RemoveRange(5, 5);
                }
                else
                {
                    subSelectExpression.ExplodeStarProjection();
                }

                var innerQuerySource = (IQuerySource)((ConstantExpression)shaperMethodArgs[0]).Value;

                foreach (var queryAnnotation
                    in QueryCompilationContext.QueryAnnotations
                        .Where(qa => qa.QuerySource == innerQuerySource))
                {
                    queryAnnotation.QuerySource = querySource;
                }

                shaperMethodArgs[0] = Expression.Constant(querySource);

                var querySourceReferenceExpression
                    = queryModel.SelectClause.Selector as QuerySourceReferenceExpression;

                if (querySourceReferenceExpression != null
                    && querySourceReferenceExpression.ReferencedQuerySource == querySource)
                {
                    if (subQueryExpression.QueryModel != null)
                    {
                        queryModel.SelectClause.Selector = subQueryExpression.QueryModel.SelectClause.Selector;

                        QueryCompilationContext.QuerySourceMapping
                            .ReplaceMapping(
                                subQueryExpression.QueryModel.MainFromClause,
                                QueryResultScope.GetResult(
                                    QueryResultScopeParameter,
                                    querySource,
                                    shaperMethod.ReturnType.GenericTypeArguments[0]));
                    }
                }

                return Expression.Call(
                    QueryCompilationContext.QueryMethodProvider.ShapedQueryMethod
                        .MakeGenericMethod(shaperMethod.ReturnType),
                    shapedQueryMethodExpression.Arguments[0],
                    shapedQueryMethodExpression.Arguments[1],
                    Expression.Lambda(
                        Expression.Call(shaperMethod, shaperMethodArgs),
                        shaperLambda.Parameters[0]));
            }

            return expression;
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

            if (RequiresClientEval || requiresClientOrderBy)
            {
                base.VisitOrderByClause(orderByClause, queryModel, index);
            }
        }

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
                        selectExpression = subQueryModelVisitor.Queries.Single();

                        selectExpression
                            .AddToProjection(
                                _relationalMetadataExtensionProvider.For(property).ColumnName,
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
                    _relationalMetadataExtensionProvider.For(property).ColumnName,
                    property,
                    querySource);
            }

            return default(TResult);
        }
    }
}
