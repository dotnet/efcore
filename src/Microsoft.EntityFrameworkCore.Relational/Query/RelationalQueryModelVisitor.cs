// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Extensions.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.EntityFrameworkCore.Utilities;
using Remotion.Linq;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Clauses.ExpressionVisitors;
using Remotion.Linq.Clauses.ResultOperators;

namespace Microsoft.EntityFrameworkCore.Query
{
    /// <summary>
    ///     The default relational <see cref="QueryModel" /> visitor.
    /// </summary>
    public class RelationalQueryModelVisitor : EntityQueryModelVisitor
    {
        /// <summary>
        ///     The SelectExpressions for this query, mapped by query source.
        /// </summary>
        /// <value>
        ///     A map of query source to select expression.
        /// </value>
        protected virtual Dictionary<IQuerySource, SelectExpression> QueriesBySource { get; } =
            new Dictionary<IQuerySource, SelectExpression>();

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
        private bool _requiresClientSingleColumnResultOperator;

        private Dictionary<IncludeSpecification, List<int>> _navigationIndexMap = new Dictionary<IncludeSpecification, List<int>>();

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
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

        /// <summary>
        ///     Gets the options for the target context.
        /// </summary>
        /// <value>
        ///     Options for the target context.
        /// </value>
        protected virtual IDbContextOptions ContextOptions { get; }

        /// <summary>
        ///     Gets or sets a value indicating whether the query requires client eval.
        /// </summary>
        /// <value>
        ///     true if the query requires client eval, false if not.
        /// </value>
        public virtual bool RequiresClientEval { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether the query requires client select many.
        /// </summary>
        /// <value>
        ///     true if the query requires client select many, false if not.
        /// </value>
        public virtual bool RequiresClientSelectMany
        {
            get { return _requiresClientSelectMany || RequiresClientEval; }
            set { _requiresClientSelectMany = value; }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether the query requires client join.
        /// </summary>
        /// <value>
        ///     true if the query requires client join, false if not.
        /// </value>
        public virtual bool RequiresClientJoin
        {
            get { return _requiresClientJoin || RequiresClientEval; }
            set { _requiresClientJoin = value; }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether the query requires client filter.
        /// </summary>
        /// <value>
        ///     true if the query requires client filter, false if not.
        /// </value>
        public virtual bool RequiresClientFilter
        {
            get { return _requiresClientFilter || RequiresClientEval; }
            set { _requiresClientFilter = value; }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether the query requires client order by.
        /// </summary>
        /// <value>
        ///     true if the query requires client order by, false if not.
        /// </value>
        public virtual bool RequiresClientOrderBy
        {
            get { return _requiresClientOrderBy || RequiresClientEval; }
            set { _requiresClientOrderBy = value; }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether the query requires client projection.
        /// </summary>
        /// <value>
        ///     true if the query requires client projection, false if not.
        /// </value>
        public virtual bool RequiresClientProjection
        {
            get { return _requiresClientProjection || RequiresClientEval; }
            set { _requiresClientProjection = value; }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether the query requires client result operator.
        /// </summary>
        /// <value>
        ///     true if the query requires client result operator, false if not.
        /// </value>
        public virtual bool RequiresClientResultOperator
        {
            get { return _requiresClientResultOperator || RequiresClientEval; }
            set { _requiresClientResultOperator = value; }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether the query requires client evaluation for result operators potentially apply to a subset of columns rather than entire row.
        /// </summary>
        /// <value>
        ///     true if the query requires client single column result operator, false if not.
        /// </value>
        internal virtual bool RequiresClientSingleColumnResultOperator
        {
            get { return _requiresClientSingleColumnResultOperator || _requiresClientResultOperator || RequiresClientEval; }
            set { _requiresClientSingleColumnResultOperator = value; }
        }

        /// <summary>
        ///     Context for the query compilation.
        /// </summary>
        public new virtual RelationalQueryCompilationContext QueryCompilationContext
            => (RelationalQueryCompilationContext)base.QueryCompilationContext;

        /// <summary>
        ///     The SelectExpressions active in the current query compilation.
        /// </summary>
        public virtual ICollection<SelectExpression> Queries => QueriesBySource.Values;

        /// <summary>
        ///     Gets the parent query model visitor, or null if there is no parent.
        /// </summary>
        /// <value>
        ///     The parent query model visitor, or null if there is no parent.
        /// </value>
        public virtual RelationalQueryModelVisitor ParentQueryModelVisitor { get; }

        /// <summary>
        ///     Registers a sub query visitor.
        /// </summary>
        /// <param name="querySource"> The query source. </param>
        /// <param name="queryModelVisitor"> The query model visitor. </param>
        public virtual void RegisterSubQueryVisitor(
            [NotNull] IQuerySource querySource, [NotNull] RelationalQueryModelVisitor queryModelVisitor)
        {
            Check.NotNull(querySource, nameof(querySource));
            Check.NotNull(queryModelVisitor, nameof(queryModelVisitor));

            _subQueryModelVisitorsBySource[querySource] = queryModelVisitor;
        }

        /// <summary>
        ///     Adds a SelectExpression to this query.
        /// </summary>
        /// <param name="querySource"> The query source. </param>
        /// <param name="selectExpression"> The select expression. </param>
        public virtual void AddQuery([NotNull] IQuerySource querySource, [NotNull] SelectExpression selectExpression)
        {
            Check.NotNull(querySource, nameof(querySource));
            Check.NotNull(selectExpression, nameof(selectExpression));

            QueriesBySource.Add(querySource, selectExpression);
        }

        /// <summary>
        ///     Try and get the active SelectExpression for a given query source.
        /// </summary>
        /// <param name="querySource"> The query source. </param>
        /// <returns>
        ///     A SelectExpression, or null.
        /// </returns>
        public virtual SelectExpression TryGetQuery([NotNull] IQuerySource querySource)
        {
            Check.NotNull(querySource, nameof(querySource));

            querySource
                = (querySource as GroupJoinClause)?.JoinClause ?? querySource;

            SelectExpression selectExpression;
            return QueriesBySource.TryGetValue(querySource, out selectExpression)
                ? selectExpression
                : QueriesBySource.Values.LastOrDefault(se => se.HandlesQuerySource(querySource));
        }

        /// <summary>
        ///     High-level method called to perform Include compilation.
        /// </summary>
        /// <param name="queryModel"> The query model. </param>
        /// <param name="includeSpecifications"> Related data to be included. </param>
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

        /// <summary>
        ///     High-level method called to perform Include compilation for a single Include.
        /// </summary>
        /// <param name="includeSpecification"> The navigation property to be included. </param>
        /// <param name="resultType"> The type of results returned by the query. </param>
        /// <param name="accessorExpression"> Expression for the navigation property to be included. </param>
        /// <param name="querySourceRequiresTracking"> A value indicating whether results of this query are to be tracked. </param>
        protected override void IncludeNavigations(
            IncludeSpecification includeSpecification,
            Type resultType,
            Expression accessorExpression,
            bool querySourceRequiresTracking)
        {
            Check.NotNull(includeSpecification, nameof(includeSpecification));
            Check.NotNull(resultType, nameof(resultType));

            var includeExpressionVisitor
                = _includeExpressionVisitorFactory.Create(
                    includeSpecification.QuerySource,
                    includeSpecification.NavigationPath,
                    QueryCompilationContext,
                    _navigationIndexMap[includeSpecification],
                    querySourceRequiresTracking);

            Expression = includeExpressionVisitor.Visit(Expression);
        }

        /// <summary>
        ///     Visit a query model.
        /// </summary>
        /// <param name="queryModel"> The query model. </param>
        public override void VisitQueryModel(QueryModel queryModel)
        {
            Check.NotNull(queryModel, nameof(queryModel));

            var typeIsExpressionTranslatingVisitor
                = new TypeIsExpressionTranslatingVisitor(QueryCompilationContext.Model, _relationalAnnotationProvider);

            queryModel.TransformExpressions(typeIsExpressionTranslatingVisitor.Visit);

            base.VisitQueryModel(queryModel);

            var compositePredicateVisitor = _compositePredicateExpressionVisitorFactory.Create();

            foreach (var selectExpression in QueriesBySource.Values)
            {
                selectExpression.Predicate
                    = compositePredicateVisitor.Visit(selectExpression.Predicate);
            }
        }

        /// <summary>
        ///     Visit a sub-query model.
        /// </summary>
        /// <param name="queryModel"> The sub-query model. </param>
        public virtual void VisitSubQueryModel([NotNull] QueryModel queryModel)
        {
            _bindParentQueries = true;

            VisitQueryModel(queryModel);
        }

        /// <summary>
        ///     Compile main from clause expression.
        /// </summary>
        /// <param name="mainFromClause"> The main from clause. </param>
        /// <param name="queryModel"> The query model. </param>
        /// <returns>
        ///     An Expression.
        /// </returns>
        protected override Expression CompileMainFromClauseExpression(
            MainFromClause mainFromClause, QueryModel queryModel)
        {
            Check.NotNull(mainFromClause, nameof(mainFromClause));
            Check.NotNull(queryModel, nameof(queryModel));

            var expression = base.CompileMainFromClauseExpression(mainFromClause, queryModel);

            return LiftSubQuery(mainFromClause, mainFromClause.FromExpression, expression);
        }

        /// <summary>
        ///     Visit an additional from clause.
        /// </summary>
        /// <param name="fromClause"> The from clause being visited. </param>
        /// <param name="queryModel"> The query model. </param>
        /// <param name="index"> Index of the node being visited. </param>
        public override void VisitAdditionalFromClause(
            AdditionalFromClause fromClause, QueryModel queryModel, int index)
        {
            Check.NotNull(fromClause, nameof(fromClause));
            Check.NotNull(queryModel, nameof(queryModel));

            var previousQuerySource = FindPreviousQuerySource(queryModel, index);

            var previousSelectExpression
                = previousQuerySource != null
                    ? TryGetQuery(previousQuerySource)
                    : null;

            var previousSelectProjectionCount
                = previousSelectExpression?.Projection.Count ?? -1;

            base.VisitAdditionalFromClause(fromClause, queryModel, index);

            var fromQuerySourceReferenceExpression
                = fromClause.FromExpression as QuerySourceReferenceExpression;

            if (fromQuerySourceReferenceExpression != null)
            {
                previousQuerySource = FindPreviousQuerySource(queryModel, index - 1);

                if (previousQuerySource != null
                    && !RequiresClientJoin)
                {
                    previousSelectExpression = TryGetQuery(previousQuerySource);

                    if (previousSelectExpression != null)
                    {
                        AddQuery(fromClause, previousSelectExpression);
                    }
                }

                return;
            }

            RequiresClientSelectMany = true;

            var selectExpression = TryGetQuery(fromClause);

            if (selectExpression != null
                && selectExpression.Tables.Count == 1)
            {
                if (previousSelectExpression != null
                    && !RequiresClientJoin
                    && CanFlattenSelectMany())
                {
                    if (!QueryCompilationContext.QuerySourceRequiresMaterialization(previousQuerySource))
                    {
                        previousSelectExpression.RemoveRangeFromProjection(previousSelectProjectionCount);
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

            if (RequiresClientSelectMany)
            {
                WarnClientEval(fromClause);
            }
        }

        private bool CanFlattenSelectMany()
        {
            var selectManyExpression = Expression as MethodCallExpression;

            return selectManyExpression != null
                   && selectManyExpression.Method.MethodIsClosedFormOf(LinqOperatorProvider.SelectMany)
                   && IsShapedQueryExpression(selectManyExpression.Arguments[0] as MethodCallExpression, innerShapedQuery: false)
                   && IsShapedQueryExpression((selectManyExpression.Arguments[1] as LambdaExpression)
                       ?.Body as MethodCallExpression, innerShapedQuery: true);
        }

        private bool IsShapedQueryExpression(MethodCallExpression shapedQueryExpression, bool innerShapedQuery)
        {
            if (shapedQueryExpression == null)
            {
                return false;
            }

            if (innerShapedQuery && (shapedQueryExpression.Method.MethodIsClosedFormOf(LinqOperatorProvider.DefaultIfEmpty)
                                     || shapedQueryExpression.Method.MethodIsClosedFormOf(LinqOperatorProvider.DefaultIfEmptyArg)))
            {
                shapedQueryExpression = shapedQueryExpression.Arguments.Single() as MethodCallExpression;
                if (shapedQueryExpression == null)
                {
                    return false;
                }
            }

            if (shapedQueryExpression.Arguments.Count != 3)
            {
                return false;
            }

            var shaper = shapedQueryExpression.Arguments[2] as ConstantExpression;

            return shaper?.Value is Shaper;
        }

        /// <summary>
        ///     Compile an additional from clause expression.
        /// </summary>
        /// <param name="additionalFromClause"> The additional from clause being compiled. </param>
        /// <param name="queryModel"> The query model. </param>
        /// <returns>
        ///     An Expression.
        /// </returns>
        protected override Expression CompileAdditionalFromClauseExpression(
            AdditionalFromClause additionalFromClause, QueryModel queryModel)
        {
            Check.NotNull(additionalFromClause, nameof(additionalFromClause));
            Check.NotNull(queryModel, nameof(queryModel));

            var expression = base.CompileAdditionalFromClauseExpression(additionalFromClause, queryModel);

            return LiftSubQuery(additionalFromClause, additionalFromClause.FromExpression, expression);
        }

        /// <summary>
        ///     Visit a join clause.
        /// </summary>
        /// <param name="joinClause"> The join clause being visited. </param>
        /// <param name="queryModel"> The query model. </param>
        /// <param name="index"> Index of the node being visited. </param>
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

        /// <summary>
        ///     Compile a join clause inner sequence expression.
        /// </summary>
        /// <param name="joinClause"> The join clause being compiled. </param>
        /// <param name="queryModel"> The query model. </param>
        /// <returns>
        ///     An Expression.
        /// </returns>
        protected override Expression CompileJoinClauseInnerSequenceExpression(
            JoinClause joinClause, QueryModel queryModel)
        {
            Check.NotNull(joinClause, nameof(joinClause));
            Check.NotNull(queryModel, nameof(queryModel));

            var expression = base.CompileJoinClauseInnerSequenceExpression(joinClause, queryModel);

            return LiftSubQuery(joinClause, joinClause.InnerSequence, expression);
        }

        /// <summary>
        ///     Visit a group join clause.
        /// </summary>
        /// <param name="groupJoinClause"> The group join being visited. </param>
        /// <param name="queryModel"> The query model. </param>
        /// <param name="index"> Index of the node being visited. </param>
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
                groupJoin: true);

            // Workaround until #6647 is addressed - GroupJoin requires materialization of entire entity which results in all columns of that entity being projected
            // this in turn causes result operators to be applied on all of those columns, even if the query specifies a subset of columns to perform the operation on
            // this could lead to incorrect results (e.g. for Distinct)
            // This however is safe to do for some operators, e.g. FirstOrDefault, Count(), Take() because their result is the same whether they are applied on single column or entire row
            RequiresClientSingleColumnResultOperator = true;
        }

        /// <summary>
        ///     Optimize a join clause.
        /// </summary>
        /// <param name="joinClause"> The join clause being visited. </param>
        /// <param name="queryModel"> The query model. </param>
        /// <param name="index"> Index of the node being visited. </param>
        /// <param name="baseVisitAction"> The base visit action. </param>
        /// <param name="operatorToFlatten"> The operator to flatten. </param>
        /// <param name="groupJoin"> true if an outer join should be performed. </param>
        protected virtual void OptimizeJoinClause(
            [NotNull] JoinClause joinClause,
            [NotNull] QueryModel queryModel,
            int index,
            [NotNull] Action baseVisitAction,
            [NotNull] MethodInfo operatorToFlatten,
            bool groupJoin = false)
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

            var previousParameter = CurrentParameter;
            var previousMapping = SnapshotQuerySourceMapping(queryModel);

            baseVisitAction();

            if (!RequiresClientSelectMany
                && previousSelectExpression != null
                && CanFlattenJoin())
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
                            = !groupJoin
                                ? previousSelectExpression.AddInnerJoin(tableExpression, projection)
                                : previousSelectExpression.AddLeftOuterJoin(tableExpression, projection);

                        joinExpression.Predicate = predicate;

                        if (groupJoin)
                        {
                            var outerJoinOrderingExtractor = new OuterJoinOrderingExtractor();

                            outerJoinOrderingExtractor.Visit(predicate);

                            foreach (var expression in outerJoinOrderingExtractor.Expressions)
                            {
                                previousSelectExpression
                                    .AddToOrderBy(new Ordering(expression, OrderingDirection.Asc));
                            }

                            var additionalFromClause
                                = queryModel.BodyClauses.ElementAtOrDefault(index + 1)
                                    as AdditionalFromClause;

                            var subQueryModel
                                = (additionalFromClause?.FromExpression as SubQueryExpression)?.QueryModel;

                            if (subQueryModel != null
                                && subQueryModel.ResultOperators.Count == 1
                                && subQueryModel.ResultOperators[0] is DefaultIfEmptyResultOperator)
                            {
                                var groupJoinClause
                                    = (subQueryModel.MainFromClause.FromExpression as QuerySourceReferenceExpression)
                                        ?.ReferencedQuerySource as GroupJoinClause;

                                if (groupJoinClause?.JoinClause == joinClause
                                    && queryModel.CountQuerySourceReferences(groupJoinClause) == 1)
                                {
                                    queryModel.BodyClauses.RemoveAt(index + 1);

                                    var querySourceMapping = new QuerySourceMapping();

                                    querySourceMapping.AddMapping(
                                        additionalFromClause,
                                        new QuerySourceReferenceExpression(joinClause));

                                    queryModel.TransformExpressions(e =>
                                        ReferenceReplacingExpressionVisitor
                                            .ReplaceClauseReferences(
                                                e,
                                                querySourceMapping,
                                                throwOnUnmappedReferences: false));

                                    Expression = ((MethodCallExpression)Expression).Arguments[0];

                                    CurrentParameter = previousParameter;

                                    foreach (var mapping in previousMapping)
                                    {
                                        QueryCompilationContext.QuerySourceMapping
                                            .ReplaceMapping(mapping.Key, mapping.Value);
                                    }

                                    var previousProjectionCount = previousSelectExpression.Projection.Count;

                                    base.VisitJoinClause(joinClause, queryModel, index);

                                    previousSelectExpression.RemoveRangeFromProjection(previousProjectionCount);

                                    QueriesBySource.Remove(joinClause);

                                    operatorToFlatten = LinqOperatorProvider.Join;
                                }
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
                WarnClientEval(joinClause);
            }
        }

        private Dictionary<IQuerySource, Expression> SnapshotQuerySourceMapping(QueryModel queryModel)
        {
            var previousMapping
                = new Dictionary<IQuerySource, Expression>
                {
                    {
                        queryModel.MainFromClause,
                        QueryCompilationContext.QuerySourceMapping
                            .GetExpression(queryModel.MainFromClause)
                    }
                };

            foreach (var querySource in queryModel.BodyClauses.OfType<IQuerySource>())
            {
                if (QueryCompilationContext.QuerySourceMapping.ContainsMapping(querySource))
                {
                    previousMapping[querySource] 
                        = QueryCompilationContext.QuerySourceMapping
                            .GetExpression(querySource);

                    var groupJoinClause = querySource as GroupJoinClause;

                    if (groupJoinClause != null
                        && QueryCompilationContext.QuerySourceMapping
                            .ContainsMapping(groupJoinClause.JoinClause))
                    {
                        previousMapping.Add(
                            groupJoinClause.JoinClause,
                            QueryCompilationContext.QuerySourceMapping
                                .GetExpression(groupJoinClause.JoinClause));
                    }
                }
            }

            return previousMapping;
        }

        private bool CanFlattenJoin()
        {
            var joinExpression = Expression as MethodCallExpression;

            return joinExpression != null
                   && (joinExpression.Method.MethodIsClosedFormOf(LinqOperatorProvider.Join)
                       || joinExpression.Method.MethodIsClosedFormOf(LinqOperatorProvider.GroupJoin))
                   && IsShapedQueryExpression(joinExpression.Arguments[0] as MethodCallExpression, innerShapedQuery: false)
                   && IsShapedQueryExpression(joinExpression.Arguments[1] as MethodCallExpression, innerShapedQuery: true);
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

        /// <summary>
        ///     Compile a group join inner sequence expression.
        /// </summary>
        /// <param name="groupJoinClause"> The group join clause being compiled. </param>
        /// <param name="queryModel"> The query model. </param>
        /// <returns>
        ///     An Expression.
        /// </returns>
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

            var subQueryModel = subQueryExpression.QueryModel;

            var queryModelMapping = new Dictionary<QueryModel, QueryModel>();
            subQueryModel.PopulateQueryModelMapping(queryModelMapping);

            subQueryModelVisitor.VisitSubQueryModel(subQueryModel);

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
                    if (!subSelectExpression.IsIdentityQuery())
                    {
                        subSelectExpression.PushDownSubquery().QuerySource = querySource;
                    }

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

            subQueryModel.RecreateQueryModelFromMapping(queryModelMapping);

            return expression;
        }

        private sealed class QuerySourceUpdater : ExpressionVisitorBase
        {
            private readonly IQuerySource _querySource;
            private readonly RelationalQueryCompilationContext _relationalQueryCompilationContext;
            private readonly ILinqOperatorProvider _linqOperatorProvider;
            private readonly SelectExpression _selectExpression;
            private bool _insideShapedQueryMethod;

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

                    if (_insideShapedQueryMethod
                        && shaper is EntityShaper
                        && !_relationalQueryCompilationContext.QuerySourceRequiresMaterialization(_querySource))
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
                _insideShapedQueryMethod = methodCallExpression.Method.MethodIsClosedFormOf(
                    _relationalQueryCompilationContext.QueryMethodProvider.ShapedQueryMethod);

                var arguments = VisitAndConvert(methodCallExpression.Arguments, "VisitMethodCall");

                if (arguments != methodCallExpression.Arguments)
                {
                    if (_insideShapedQueryMethod)
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

        /// <summary>
        ///     Visit a where clause.
        /// </summary>
        /// <param name="whereClause"> The where clause being visited. </param>
        /// <param name="queryModel"> The query model. </param>
        /// <param name="index"> Index of the node being visited. </param>
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
                WarnClientEval(whereClause.Predicate);

                base.VisitWhereClause(whereClause, queryModel, index);
            }
        }

        /// <summary>
        ///     Visit an order by clause.
        /// </summary>
        /// <param name="orderByClause"> The order by clause. </param>
        /// <param name="queryModel"> The query model. </param>
        /// <param name="index"> Index of the node being visited. </param>
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
                    // we disable this for order by, because you can't have a parameter (that is integer) in the order by
                    var canBindPropertyToOuterParameter = _canBindPropertyToOuterParameter;
                    _canBindPropertyToOuterParameter = false;

                    var sqlOrderingExpression
                        = sqlTranslatingExpressionVisitor
                            .Visit(ordering.Expression);

                    _canBindPropertyToOuterParameter = canBindPropertyToOuterParameter;

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
                WarnClientEval(orderByClause);

                base.VisitOrderByClause(orderByClause, queryModel, index);
            }
        }

        /// <summary>
        ///     Visit a result operator.
        /// </summary>
        /// <param name="resultOperator"> The result operator being visited. </param>
        /// <param name="queryModel"> The query model. </param>
        /// <param name="index"> Index of the node being visited. </param>
        public override void VisitResultOperator(ResultOperatorBase resultOperator, QueryModel queryModel, int index)
        {
            base.VisitResultOperator(resultOperator, queryModel, index);

            if (RequiresClientResultOperator)
            {
                WarnClientEval(resultOperator);
            }
        }

        /// <summary>
        ///     Generated a client-eval warning
        /// </summary>
        /// <param name="expression"> The expression being client-eval'd. </param>
        protected virtual void WarnClientEval([NotNull] object expression)
        {
            Check.NotNull(expression, nameof(expression));

            QueryCompilationContext.Logger.LogWarning(
                RelationalEventId.QueryClientEvaluationWarning,
                () => RelationalStrings.ClientEvalWarning(expression));
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
                    = entityType.GetConcreteTypesInHierarchy().ToList();

                if (concreteEntityTypes.Count != 1
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

        /// <summary>
        ///     Bind a member expression to a value buffer access.
        /// </summary>
        /// <param name="memberExpression"> The member access expression. </param>
        /// <param name="expression"> The target expression. </param>
        /// <returns>
        ///     An Expression.
        /// </returns>
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

        /// <summary>
        ///     Bind a method call expression to a value buffer access.
        /// </summary>
        /// <param name="methodCallExpression"> The method call expression. </param>
        /// <param name="expression"> The target expression. </param>
        /// <returns>
        ///     An Expression.
        /// </returns>
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

        /// <summary>
        ///     Bind a member expression.
        /// </summary>
        /// <typeparam name="TResult"> Type of the result. </typeparam>
        /// <param name="memberExpression"> The member access expression. </param>
        /// <param name="memberBinder"> The member binder. </param>
        /// <param name="bindSubQueries"> true to bind sub queries. </param>
        /// <returns>
        ///     A TResult.
        /// </returns>
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

        public virtual Expression BindMemberToOuterQueryParameter(
            [NotNull] MemberExpression memberExpression)
            => base.BindMemberExpression(
                memberExpression,
                null,
                (property, qs) => BindPropertyToOuterParameter(qs, property, true));

        /// <summary>
        ///     Bind a method call expression.
        /// </summary>
        /// <typeparam name="TResult"> Type of the result. </typeparam>
        /// <param name="methodCallExpression"> The method call expression. </param>
        /// <param name="memberBinder"> The member binder. </param>
        /// <param name="bindSubQueries"> true to bind sub queries. </param>
        /// <returns>
        ///     A TResult.
        /// </returns>
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
            => base.BindMethodCallExpression(
                methodCallExpression,
                querySource,
                (property, qs) => BindMemberOrMethod(memberBinder, qs, property, bindSubQueries));

        /// <summary>
        ///     Bind a local method call expression.
        /// </summary>
        /// <param name="methodCallExpression"> The local method call expression. </param>
        /// <returns>
        ///     An Expression.
        /// </returns>
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

        public virtual Expression BindMethodToOuterQueryParameter(
            [NotNull] MethodCallExpression methodCallExpression)
        {
            Check.NotNull(methodCallExpression, nameof(methodCallExpression));

            return base.BindMethodCallExpression<Expression>(
                methodCallExpression,
                null,
                (property, qs) => BindPropertyToOuterParameter(qs, property, false));
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

        private bool _canBindPropertyToOuterParameter = true;

        private const string OuterQueryParameterNamePrefix = @"_outer_";

        private ParameterExpression BindPropertyToOuterParameter(IQuerySource querySource, IProperty property, bool isMemberExpression)
        {
            if (querySource != null && _canBindPropertyToOuterParameter)
            {
                SelectExpression parentSelectExpression = null;
                ParentQueryModelVisitor?.QueriesBySource.TryGetValue(querySource, out parentSelectExpression);
                if (parentSelectExpression != null)
                {
                    var parameterName = OuterQueryParameterNamePrefix + property.Name;
                    var parameterWithSamePrefixCount
                        = QueryCompilationContext.ParentQueryReferenceParameters.Count(p => p.StartsWith(parameterName, StringComparison.Ordinal));

                    if (parameterWithSamePrefixCount > 0)
                    {
                        parameterName += parameterWithSamePrefixCount;
                    }

                    QueryCompilationContext.ParentQueryReferenceParameters.Add(parameterName);
                    Expression = CreateInjectParametersExpression(Expression, querySource, property, parameterName, isMemberExpression);

                    return Expression.Parameter(
                        property.ClrType,
                        parameterName);
                }
            }

            return null;
        }

        private Expression CreateInjectParametersExpression(
            Expression expression,
            IQuerySource querySource,
            IProperty property,
            string parameterName,
            bool isMemberExpression)
        {
            var querySourceReference = new QuerySourceReferenceExpression(querySource);
            var propertyExpression = isMemberExpression
                ? Expression.Property(querySourceReference, property.PropertyInfo)
                : CreatePropertyExpression(querySourceReference, property);

            if (propertyExpression.Type.GetTypeInfo().IsValueType)
            {
                propertyExpression = Expression.Convert(propertyExpression, typeof(object));
            }

            var parameterNameExpressions = new List<ConstantExpression>();
            var parameterValueExpressions = new List<Expression>();

            var methodCallExpression = expression as MethodCallExpression;
            if (methodCallExpression != null
                && methodCallExpression.Method.MethodIsClosedFormOf(QueryCompilationContext.QueryMethodProvider.InjectParametersMethod))
            {
                var existingParamterNamesExpression = (NewArrayExpression)methodCallExpression.Arguments[2];
                parameterNameExpressions.AddRange(existingParamterNamesExpression.Expressions.Cast<ConstantExpression>());

                var existingParameterValuesExpression = (NewArrayExpression)methodCallExpression.Arguments[3];
                parameterValueExpressions.AddRange(existingParameterValuesExpression.Expressions);

                expression = methodCallExpression.Arguments[1];
            }

            parameterNameExpressions.Add(Expression.Constant(parameterName));
            parameterValueExpressions.Add(propertyExpression);

            var elementType = expression.Type.GetTypeInfo().GenericTypeArguments.Single();

            return Expression.Call(
                QueryCompilationContext.QueryMethodProvider.InjectParametersMethod.MakeGenericMethod(elementType),
                QueryContextParameter,
                expression,
                Expression.NewArrayInit(typeof(string), parameterNameExpressions),
                Expression.NewArrayInit(typeof(object), parameterValueExpressions));
        }
    }
}
