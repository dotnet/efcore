// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using Microsoft.EntityFrameworkCore.Query.ResultOperators.Internal;
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

        private readonly IRelationalAnnotationProvider _relationalAnnotationProvider;
        private readonly IIncludeExpressionVisitorFactory _includeExpressionVisitorFactory;
        private readonly ISqlTranslatingExpressionVisitorFactory _sqlTranslatingExpressionVisitorFactory;
        private readonly ICompositePredicateExpressionVisitorFactory _compositePredicateExpressionVisitorFactory;
        private readonly IConditionalRemovingExpressionVisitorFactory _conditionalRemovingExpressionVisitorFactory;
        private readonly IQueryFlattenerFactory _queryFlattenerFactory;

        private bool _requiresClientSelectMany;
        private bool _requiresClientJoin;
        private bool _requiresClientFilter;
        private bool _requiresClientProjection;
        private bool _requiresClientOrderBy;
        private bool _requiresClientResultOperator;
        private bool _requiresClientSingleColumnResultOperator;
        private bool _requiresOuterParameterInjection;

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
            [NotNull] IRelationalResultOperatorHandler resultOperatorHandler,
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
        ///     Gets or sets a value indicating whether the query requires outer parameter binding.
        /// </summary>
        /// <value>
        ///     true if the query requires outer parameter binding, false if not.
        /// </value>
        public virtual bool RequiresOuterParameterInjection
        {
            get { return ParentQueryModelVisitor != null && (_requiresOuterParameterInjection || RequiresClientEval); }
            set { _requiresOuterParameterInjection = value; }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether the query is able to bind any outer properties.
        /// </summary>
        public virtual bool CanBindOuterProperties { get; protected set; } = true;

        /// <summary>
        ///     Gets or sets a value indicating whether the query is able to bind any outer parameters.
        /// </summary>
        public virtual bool CanBindOuterParameters { get; protected set; } = true;

        /// <summary>
        ///     Gets a value indicating whether the query could be inlined into a parent query.
        /// </summary>
        public virtual bool IsInlinable
        {
            get
            {
                return !RequiresClientEval
                    && !RequiresClientSelectMany
                    && !RequiresClientJoin
                    && !RequiresClientFilter
                    && !RequiresClientOrderBy
                    && !RequiresClientProjection
                    && !RequiresClientResultOperator
                    && !RequiresOuterParameterInjection;
            }
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

            base.VisitQueryModel(queryModel);
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

            Expression expression = null;

            if (!HandleEnumerableRelationalTypeProperty(mainFromClause.FromExpression)
                && mainFromClause.FromExpression is SubQueryExpression subQueryExpression)
            {
                expression = LiftSubQuery(mainFromClause, subQueryExpression);
            }

            expression = expression ?? base.CompileMainFromClauseExpression(mainFromClause, queryModel);

            return expression;
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
            
            CanBindOuterProperties = QueryCompilationContext.IsLateralJoinSupported;

            base.VisitAdditionalFromClause(fromClause, queryModel, index);

            CanBindOuterProperties = true;

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

                    if (correlated && !QueryCompilationContext.IsLateralJoinSupported)
                    {
                        return;
                    }

                    if (selectExpression.Limit != null)
                    {
                        selectExpression.PushDownSubquery();
                        selectExpression.ExplodeStarProjection();
                    }

                    var table = selectExpression.Tables.First();

                    var joinExpression
                        = correlated
                            ? previousSelectExpression.AddCrossJoinLateral(
                                selectExpression.Tables.First(),
                                selectExpression.Projection)
                            : previousSelectExpression.AddCrossJoin(
                                selectExpression.Tables.First(),
                                selectExpression.Projection);

                    joinExpression.QuerySource = fromClause;

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

            Expression expression = null;

            if (!HandleEnumerableRelationalTypeProperty(additionalFromClause.FromExpression)
                && additionalFromClause.FromExpression is SubQueryExpression subQueryExpression)
            {
                expression = LiftSubQuery(additionalFromClause, subQueryExpression);
            }

            expression = expression ?? base.CompileAdditionalFromClauseExpression(additionalFromClause, queryModel);

            return expression;
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

            Expression expression = null;

            if (!HandleEnumerableRelationalTypeProperty(joinClause.InnerSequence)
                && joinClause.InnerSequence is SubQueryExpression subQueryExpression)
            {
                expression = LiftSubQuery(joinClause, subQueryExpression);
            }

            expression = expression ?? base.CompileJoinClauseInnerSequenceExpression(joinClause, queryModel);

            return expression;
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

            RequiresClientJoin = true;

            if (!RequiresClientSelectMany
                && previousSelectExpression != null
                && CanFlattenJoin())
            {
                var selectExpression = TryGetQuery(joinClause);

                if (selectExpression != null)
                {
                    var sqlTranslatingExpressionVisitor
                        = _sqlTranslatingExpressionVisitorFactory.Create(
                            queryModelVisitor: this,
                            topLevelPredicate: null,
                            inProjection: false,
                            addToProjections: true);

                    var predicate
                        = sqlTranslatingExpressionVisitor
                            .Visit(
                                Expression.Equal(
                                    joinClause.OuterKeySelector,
                                    joinClause.InnerKeySelector));

                    if (predicate != null)
                    {
                        previousSelectExpression.RemoveRangeFromProjection(previousSelectProjectionCount);
                        var tableExpression = selectExpression.Tables.Single();

                        if (groupJoin
                            && selectExpression.Predicate != null)
                        {
                            selectExpression.PushDownSubquery();
                            selectExpression.ExplodeStarProjection();
                            tableExpression = selectExpression.Tables.Single();
                            tableExpression.QuerySource = joinClause;

                            predicate = sqlTranslatingExpressionVisitor.Visit(
                                Expression.Equal(joinClause.OuterKeySelector, joinClause.InnerKeySelector));
                        }

                        QueriesBySource.Remove(joinClause);

                        var projection
                            = QueryCompilationContext
                                .QuerySourceRequiresMaterialization(joinClause)
                                ? selectExpression.Projection
                                : Enumerable.Empty<Expression>();

                        var joinExpression = !groupJoin
                            ? previousSelectExpression.AddInnerJoin(tableExpression, projection, selectExpression.Predicate)
                            : previousSelectExpression.AddLeftOuterJoin(tableExpression, projection);
                        joinExpression.Predicate = predicate;
                        joinExpression.QuerySource = joinClause;

                        if (groupJoin)
                        {
                            var outerJoinOrderingExtractor = new OuterJoinOrderingExtractor();
                            outerJoinOrderingExtractor.Visit(predicate);

                            var previousOrderingCount = previousSelectExpression.OrderBy.Count;
                            if (!outerJoinOrderingExtractor.DependentToPrincipalFound)
                            {
                                foreach (var expression in outerJoinOrderingExtractor.Expressions)
                                {
                                    previousSelectExpression.AddToOrderBy(
                                        new Ordering(expression, OrderingDirection.Asc));
                                }
                            }

                            var additionalFromClause
                                = queryModel.BodyClauses.ElementAtOrDefault(index + 1)
                                    as AdditionalFromClause;

                            var subQueryModel
                                = (additionalFromClause?.FromExpression as SubQueryExpression)?.QueryModel;

                            if (subQueryModel != null
                                && subQueryModel.BodyClauses.Count == 0
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

                                    foreach (var annotation in QueryCompilationContext.QueryAnnotations
                                        .OfType<IncludeResultOperator>()
                                        .Where(a => a.QuerySource == additionalFromClause))
                                    {
                                        annotation.QuerySource = joinClause;
                                        annotation.PathFromQuerySource = ReferenceReplacingExpressionVisitor.ReplaceClauseReferences(
                                            annotation.PathFromQuerySource,
                                            querySourceMapping,
                                            throwOnUnmappedReferences: false);
                                    }

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

                                    if (previousOrderingCount != previousSelectExpression.OrderBy.Count
                                        && !selectExpression.OrderBy.Any())
                                    {
                                        previousSelectExpression.RemoveRangeFromOrderBy(previousOrderingCount);
                                    }
                                }
                            }
                        }

                        foreach (var ordering in selectExpression.OrderBy)
                        {
                            previousSelectExpression.AddToOrderBy(
                                new Ordering(ordering.Expression, ordering.OrderingDirection));
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

            public bool DependentToPrincipalFound { get; private set; }

            public IEnumerable<Expression> Expressions => _expressions;

            private IForeignKey _matchingCandidate;
            private List<IProperty> _matchingCandidateProperties;

            public override Expression Visit(Expression expression)
            {
                var binaryExpression = expression as BinaryExpression;

                if (binaryExpression != null)
                {
                    return VisitBinary(binaryExpression);
                }

                return expression;
            }

            protected override Expression VisitBinary(BinaryExpression node)
            {
                if (DependentToPrincipalFound)
                {
                    return node;
                }

                if (node.NodeType == ExpressionType.Equal)
                {
                    var leftProperty = node.Left.RemoveConvert().TryGetColumnExpression()?.Property;
                    var rightProperty = node.Right.RemoveConvert().TryGetColumnExpression()?.Property;
                    if (leftProperty != null && rightProperty != null && leftProperty.IsForeignKey() && rightProperty.IsKey())
                    {
                        var keyDeclaringEntityType = rightProperty.GetContainingKeys().First().DeclaringEntityType;
                        var matchingForeignKeys = leftProperty.GetContainingForeignKeys().Where(k => k.PrincipalKey.DeclaringEntityType == keyDeclaringEntityType);
                        if (matchingForeignKeys.Count() == 1)
                        {
                            var matchingKey = matchingForeignKeys.Single();
                            if (rightProperty.GetContainingKeys().Contains(matchingKey.PrincipalKey))
                            {
                                var matchingForeignKey = matchingKey;
                                if (_matchingCandidate == null)
                                {
                                    _matchingCandidate = matchingForeignKey;
                                    _matchingCandidateProperties = new List<IProperty> { leftProperty };
                                }
                                else if (_matchingCandidate == matchingForeignKey)
                                {
                                    _matchingCandidateProperties.Add(leftProperty);
                                }

                                if (_matchingCandidate.Properties.All(p => _matchingCandidateProperties.Contains(p)))
                                {
                                    DependentToPrincipalFound = true;
                                    return node;
                                }
                            }
                        }
                    }

                    _expressions.Add(node.Left.RemoveConvert());

                    return node;
                }

                if (node.NodeType == ExpressionType.AndAlso)
                {
                    return base.VisitBinary(node);
                }

                return node;
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

            Expression expression = null;

            if (!HandleEnumerableRelationalTypeProperty(groupJoinClause.JoinClause.InnerSequence)
                && groupJoinClause.JoinClause.InnerSequence is SubQueryExpression subQueryExpression)
            {
                expression = LiftSubQuery(groupJoinClause.JoinClause, subQueryExpression);
            }

            expression = expression ?? base.CompileGroupJoinInnerSequenceExpression(groupJoinClause, queryModel);

            return expression;
        }

        private Expression LiftSubQuery(
            IQuerySource querySource, SubQueryExpression subQueryExpression)
        {
            var subQueryModel = subQueryExpression.QueryModel;            
            var subQueryModelVisitor = QueryCompilationContext.GetQueryModelVisitor(subQueryModel);

            if (subQueryModelVisitor == null)
            {
                subQueryModelVisitor = QueryCompilationContext.CreateQueryModelVisitor(subQueryModel, this);

                subQueryModelVisitor.RequiresOuterParameterInjection
                    = RequiresClientEval || RequiresClientJoin || RequiresClientSelectMany;

                subQueryModelVisitor.VisitQueryModel(subQueryModel);
            }

            var selectExpression = subQueryModelVisitor.TryGetQuery(subQueryModel.MainFromClause);

            if (selectExpression == null)
            {
                return null;
            }

            AddQuery(querySource, selectExpression);

            if (selectExpression.OrderBy.Any() && selectExpression.Limit == null)
            {
                return null;
            }

            if (querySource is AdditionalFromClause
                && selectExpression.IsCorrelated() 
                && !QueryCompilationContext.IsLateralJoinSupported)
            {
                return null;
            }

            if (subQueryModelVisitor.IsInlinable)
            {
                if (selectExpression.IsIdentityQuery())
                {
                    selectExpression.Tables.First().QuerySource = querySource;
                }
                else
                {
                    selectExpression.PushDownSubquery().QuerySource = querySource;
                }
                
                var newExpression
                    = new QuerySourceUpdater(
                            querySource,
                            QueryCompilationContext,
                            LinqOperatorProvider,
                            selectExpression)
                        .Visit(subQueryModelVisitor.Expression);

                return newExpression;
            }

            return null;
        }

        private bool HandleEnumerableRelationalTypeProperty(Expression expression)
        {
            var handled = false;

            expression = expression.RemoveConvert();

            void BindExpression(IProperty property, IQuerySource querySource)
            {
                if (property.ClrType != typeof(string) && property.ClrType != typeof(byte[]))
                {
                    return;
                }

                var selectExpression = QueryCompilationContext.FindSelectExpression(querySource);

                if (selectExpression == null)
                {
                    return;
                }

                var sqlTranslatingExpressionVisitor
                    = _sqlTranslatingExpressionVisitorFactory.Create(
                        queryModelVisitor: ParentQueryModelVisitor ?? this,
                        topLevelPredicate: null,
                        inProjection: false,
                        addToProjections: false);

                var sqlExpression
                    = sqlTranslatingExpressionVisitor
                        .Visit(expression);

                if (sqlExpression != null)
                {
                    selectExpression.AddToProjection(sqlExpression);
                    handled = true;
                }
            }

            if (expression is MemberExpression memberExpression)
            {
                BindMemberExpression(memberExpression, BindExpression);
            }
            else if (expression is MethodCallExpression methodCallExpression)
            {
                BindMethodCallExpression(methodCallExpression, BindExpression);
            }

            return handled;
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
            var previousProjectionCount = selectExpression?.Projection.Count ?? 0;
            var requiresClientFilter = selectExpression == null || RequiresClientEval;

            var sqlTranslatingExpressionVisitor
                = _sqlTranslatingExpressionVisitorFactory.Create(
                    queryModelVisitor: this,
                    topLevelPredicate: whereClause.Predicate,
                    inProjection: false,
                    addToProjections: true);

            // Visit the predicate expression unconditionally so that any columns that may be
            // required for client evaluation will be present in the select expression.
            var sqlPredicateExpression = sqlTranslatingExpressionVisitor.Visit(whereClause.Predicate);

            if (!requiresClientFilter)
            {
                if (sqlPredicateExpression != null)
                {
                    sqlPredicateExpression
                        = _conditionalRemovingExpressionVisitorFactory
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
            else
            {
                selectExpression?.RemoveRangeFromProjection(previousProjectionCount);
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
                    = _sqlTranslatingExpressionVisitorFactory.Create(
                        queryModelVisitor: this,
                        topLevelPredicate: null,
                        inProjection: true,
                        addToProjections: true);

                var orderings = new List<Ordering>();
                var previousProjectionCount = selectExpression.Projection.Count;

                foreach (var ordering in orderByClause.Orderings)
                {
                    // we disable this for order by, because you can't have a parameter (that is integer) in the order by
                    var canBindOuterParameters = CanBindOuterParameters;
                    CanBindOuterParameters = false;

                    var sqlOrderingExpression
                        = sqlTranslatingExpressionVisitor
                            .Visit(ordering.Expression);

                    CanBindOuterParameters = canBindOuterParameters;

                    // Instead of breaking, continue to visit each ordering so we can
                    // ensure that all necessary projection columns are added to the select
                    // expression.
                    if (sqlOrderingExpression == null || requiresClientOrderBy)
                    {
                        requiresClientOrderBy = true;
                        continue;
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

                if (!requiresClientOrderBy)
                {
                    selectExpression.RemoveRangeFromProjection(previousProjectionCount);
                    selectExpression.PrependToOrderBy(orderings);
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
        ///     Visits <see cref="SelectClause" /> nodes.
        /// </summary>
        /// <param name="selectClause"> The node being visited. </param>
        /// <param name="queryModel"> The query. </param>
        public override void VisitSelectClause(
            [NotNull] SelectClause selectClause, [NotNull] QueryModel queryModel)
        {
            Check.NotNull(selectClause, nameof(selectClause));
            Check.NotNull(queryModel, nameof(queryModel));

            base.VisitSelectClause(selectClause, queryModel);

            // Workaround until #6647 is addressed
            if (ParentQueryModelVisitor != null
                && Expression is MethodCallExpression methodCallExpression
                && methodCallExpression.Method.MethodIsClosedFormOf(LinqOperatorProvider.Select)
                && queryModel.BodyClauses.OfType<IQuerySource>().Concat(new[] { queryModel.MainFromClause })
                    .Any(qs => QueryCompilationContext.QuerySourceRequiresMaterialization(qs)))
            {
                RequiresClientProjection = true;
            }
        }

        /// <summary>
        ///     Visit the result operators for a query model.
        /// </summary>
        /// <param name="resultOperators"> The result operators. </param>
        /// <param name="queryModel"> The query model. </param>
        protected override void VisitResultOperators(ObservableCollection<ResultOperatorBase> resultOperators, QueryModel queryModel)
        {
            base.VisitResultOperators(resultOperators, queryModel);

            // Able to visit these things before the base.VisitQueryModel exits and changes anything
            // (Task blocking, etc.)

            var compositePredicateVisitor = _compositePredicateExpressionVisitorFactory.Create();
            var unresolvedOuterPropertyVisitor = new OuterPropertyExpressionResolvingVisitor(this);

            foreach (var selectExpression in Queries)
            {
                compositePredicateVisitor.Visit(selectExpression);
                unresolvedOuterPropertyVisitor.Visit(selectExpression);
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
        ///     Applies optimizations to the query.
        /// </summary>
        /// <param name="queryModel"> The query. </param>
        /// <param name="includeResultOperators">TODO: This parameter is to be removed.</param>
        protected override void OptimizeQueryModel(
            QueryModel queryModel,
            ICollection<IncludeResultOperator> includeResultOperators)
        {
            Check.NotNull(queryModel, nameof(queryModel));
            Check.NotNull(includeResultOperators, nameof(includeResultOperators));

            var typeIsExpressionTranslatingVisitor
                = new TypeIsExpressionTranslatingVisitor(QueryCompilationContext.Model, _relationalAnnotationProvider);

            queryModel.TransformExpressions(typeIsExpressionTranslatingVisitor.Visit);

            base.OptimizeQueryModel(queryModel, includeResultOperators);
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
                                    Expression.Constant(_relationalAnnotationProvider.For(concreteEntityType).DiscriminatorValue, discriminatorPropertyExpression.Type)))
                            .Aggregate((current, next) => Expression.OrElse(next, current));

                    return discriminatorPredicate;
                }

                return Expression.Constant(true, typeof(bool));
            }
        }

        private class OuterPropertyExpressionResolvingVisitor : ExpressionVisitorBase
        {
            private readonly RelationalQueryModelVisitor _queryModelVisitor;

            public OuterPropertyExpressionResolvingVisitor(RelationalQueryModelVisitor queryModelVisitor)
            {
                _queryModelVisitor = queryModelVisitor;
            }

            protected override Expression VisitExtension(Expression node)
            {
                var outerPropertyExpression = node as OuterPropertyExpression;

                if (outerPropertyExpression != null && !outerPropertyExpression.Resolved)
                {
                    if (_queryModelVisitor.IsInlinable && CanResolve(outerPropertyExpression))
                    {
                        outerPropertyExpression.Resolve();
                    }
                    else
                    {
                        var outerParameter = _queryModelVisitor.BindPropertyToOuterParameter(
                            outerPropertyExpression.Property,
                            outerPropertyExpression.QuerySource,
                            outerPropertyExpression.SourceExpression is MemberExpression);

                        if (outerParameter != null)
                        {
                            outerPropertyExpression.Resolve(outerParameter);
                        }

                        _queryModelVisitor.RequiresOuterParameterInjection = true;
                    }

                    return node;
                }

                return base.VisitExtension(node);
            }

            private bool CanResolve(OuterPropertyExpression outerPropertyExpression)
            {
                var ancestor = _queryModelVisitor.ParentQueryModelVisitor;

                if (ancestor == null || !ancestor.CanBindOuterProperties)
                {
                    return false;
                }

                do
                {
                    if (ancestor.TryGetQuery(outerPropertyExpression.QuerySource) != null)
                    {
                        return true;
                    }

                    if (ancestor.RequiresOuterParameterInjection)
                    {
                        return false;
                    }

                    ancestor = ancestor.ParentQueryModelVisitor;
                }
                while (ancestor != null);

                return false;
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

                        return projectionIndex > -1
                            ? BindReadValueMethod(memberExpression.Type, expression, projectionIndex)
                            : null;
                    })
                ?? ParentQueryModelVisitor?.BindMemberToValueBuffer(memberExpression, expression);
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

                        return projectionIndex > -1
                            ? BindReadValueMethod(methodCallExpression.Type, expression, projectionIndex)
                            : null;
                    })
                ?? ParentQueryModelVisitor?.BindMethodCallToValueBuffer(methodCallExpression, expression);
        }

        /// <summary>
        ///     Bind a member expression.
        /// </summary>
        /// <typeparam name="TResult"> Type of the result. </typeparam>
        /// <param name="expression"> The member access expression. </param>
        /// <param name="binder"> The member binder. </param>
        /// <returns>
        ///     A TResult.
        /// </returns>
        public virtual TResult BindMemberExpression<TResult>(
            [NotNull] MemberExpression expression,
            [NotNull] Func<IProperty, IQuerySource, SelectExpression, TResult> binder)
        {
            Check.NotNull(expression, nameof(expression));
            Check.NotNull(binder, nameof(binder));

            return base.BindMemberExpression(expression, (property, querySource) =>
            {
                return BindPropertyToSelectExpression(property, querySource, binder);
            });
        }

        /// <summary>
        ///     Bind a method call expression.
        /// </summary>
        /// <typeparam name="TResult"> Type of the result. </typeparam>
        /// <param name="expression"> The method call expression. </param>
        /// <param name="binder"> The member binder. </param>
        public virtual TResult BindMethodCallExpression<TResult>(
            [NotNull] MethodCallExpression expression,
            [NotNull] Func<IProperty, IQuerySource, SelectExpression, TResult> binder)
        {
            Check.NotNull(expression, nameof(expression));
            Check.NotNull(binder, nameof(binder));

            return base.BindMethodCallExpression(expression, (property, querySource) =>
            {
                return BindPropertyToSelectExpression(property, querySource, binder);
            });
        }

        /// <summary>
        ///     Bind a local method call expression.
        /// </summary>
        /// <param name="expression"> The local method call expression. </param>
        /// <returns>
        ///     An Expression.
        /// </returns>
        public virtual Expression BindLocalMethodCallExpression(
            [NotNull] MethodCallExpression expression)
        {
            Check.NotNull(expression, nameof(expression));

            return base.BindMethodCallExpression(expression, (property, querySource) =>
            {
                var parameterExpression = expression.Arguments[0] as ParameterExpression;

                if (parameterExpression != null)
                {
                    return new PropertyParameterExpression(parameterExpression.Name, property);
                }

                var constantExpression = expression.Arguments[0] as ConstantExpression;

                if (constantExpression != null)
                {
                    return Expression.Constant(
                        property.GetGetter().GetClrValue(constantExpression.Value),
                        expression.Method.GetGenericArguments()[0]);
                }

                return default(Expression);
            });
        }

        private TResult BindPropertyToSelectExpression<TResult>(
            IProperty property,
            IQuerySource querySource,
            Func<IProperty, IQuerySource, SelectExpression, TResult> member)
        {
            if (querySource != null)
            {
                var selectExpression = TryGetQuery(querySource);

                if (selectExpression != null)
                {
                    return member(property, querySource, selectExpression);
                }
            }

            return default(TResult);
        }

        private const string OuterQueryParameterNamePrefix = @"_outer_";

        private ParameterExpression BindPropertyToOuterParameter(
            IProperty property,
            IQuerySource querySource,
            bool isMemberExpression)
        {
            if (CanBindOuterParameters && querySource != null && ParentQueryModelVisitor != null)
            {
                var ancestor = ParentQueryModelVisitor;
                SelectExpression outerQuery = null;

                do
                {
                    outerQuery = ancestor.TryGetQuery(querySource);
                    ancestor = ancestor.ParentQueryModelVisitor;
                }
                while (outerQuery == null && ancestor != null);

                if (outerQuery != null)
                {
                    // Ensure the property we need is included in the parent query.
                    var column = _relationalAnnotationProvider.For(property).ColumnName;

                    var index = outerQuery.AddToProjection(column, property, querySource);

                    var aliasExpression = outerQuery.Projection[index] as AliasExpression;

                    var parameterName = OuterQueryParameterNamePrefix + property.Name;

                    var parameterWithSamePrefixCount
                        = QueryCompilationContext.ParentQueryReferenceParameters
                            .Count(p => p.StartsWith(parameterName, StringComparison.Ordinal));

                    if (parameterWithSamePrefixCount > 0)
                    {
                        parameterName += parameterWithSamePrefixCount;
                    }

                    QueryCompilationContext.ParentQueryReferenceParameters.Add(parameterName);

                    var querySourceReference = new QuerySourceReferenceExpression(querySource);

                    var parameterValue
                        = isMemberExpression
                            ? Expression.Property(
                                querySourceReference,
                                aliasExpression?.SourceMember as PropertyInfo ?? property.PropertyInfo)
                            : QueryCompilationContext.Model.FindEntityType(querySource.ItemType) != null
                                ? CreatePropertyExpression(querySourceReference, property)
                                : ReplaceClauseReferences(querySourceReference);

                    if (parameterValue.Type.GetTypeInfo().IsValueType)
                    {
                        parameterValue = Expression.Convert(parameterValue, typeof(object));
                    }

                    Expression = CreateInjectParametersExpression(
                        Expression,
                        parameterName,
                        ReplaceClauseReferences(parameterValue));

                    return Expression.Parameter(
                        property.ClrType,
                        parameterName);
                }
            }

            return null;
        }

        private Expression CreateInjectParametersExpression(
            Expression expression,
            string parameterName,
            Expression parameterValue)
        {
            var queryMethods = QueryCompilationContext.QueryMethodProvider;
            var parameterNameExpressions = new List<ConstantExpression>();
            var parameterValueExpressions = new List<Expression>();
            var methodCallExpression = expression as MethodCallExpression;

            if (methodCallExpression != null
                && (methodCallExpression.Method.MethodIsClosedFormOf(queryMethods.InjectParametersItemMethod)
                    || methodCallExpression.Method.MethodIsClosedFormOf(queryMethods.InjectParametersSequenceMethod)))
            {
                var existingParamterNamesExpression = (NewArrayExpression)methodCallExpression.Arguments[2];
                parameterNameExpressions.AddRange(existingParamterNamesExpression.Expressions.Cast<ConstantExpression>());

                var existingParameterValuesExpression = (NewArrayExpression)methodCallExpression.Arguments[3];
                parameterValueExpressions.AddRange(existingParameterValuesExpression.Expressions);

                expression = methodCallExpression.Arguments[1];
            }

            parameterNameExpressions.Add(Expression.Constant(parameterName));
            parameterValueExpressions.Add(parameterValue);

            // This can be happening either before or after result operators,
            // so we need to account not only for the typical IEnumerable case
            // but also the Contains(), FirstOrDefault(), etc. cases.
            var elementType = expression.Type.TryGetSequenceType();

            if (elementType != null && expression.Type != typeof(string) && expression.Type != typeof(byte[]))
            {
                return Expression.Call(
                    QueryCompilationContext.QueryMethodProvider.InjectParametersSequenceMethod
                        .MakeGenericMethod(elementType),
                    QueryContextParameter,
                    expression,
                    Expression.NewArrayInit(typeof(string), parameterNameExpressions),
                    Expression.NewArrayInit(typeof(object), parameterValueExpressions));
            }
            else
            {
                return Expression.Call(
                    QueryCompilationContext.QueryMethodProvider.InjectParametersItemMethod
                        .MakeGenericMethod(expression.Type.UnwrapTaskResultType()),
                    QueryContextParameter,
                    Expression.Lambda(expression),
                    Expression.NewArrayInit(typeof(string), parameterNameExpressions),
                    Expression.NewArrayInit(typeof(object), parameterValueExpressions));
            }
        }

        #endregion
    }
}
