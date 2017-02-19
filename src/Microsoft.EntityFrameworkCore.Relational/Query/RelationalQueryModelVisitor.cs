// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using Microsoft.EntityFrameworkCore.Query.Expressions.Internal;
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
using Microsoft.EntityFrameworkCore.Query.ResultOperators.Internal;

namespace Microsoft.EntityFrameworkCore.Query
{
    /// <summary>
    ///     The default relational <see cref="QueryModel" /> visitor.
    /// </summary>
    public class RelationalQueryModelVisitor : EntityQueryModelVisitor
    {
        private readonly Dictionary<IQuerySource, SelectExpression> _queriesBySource
            = new Dictionary<IQuerySource, SelectExpression>();

        private readonly IRelationalAnnotationProvider _relationalAnnotationProvider;
        private readonly IIncludeExpressionVisitorFactory _includeExpressionVisitorFactory;
        private readonly ISqlTranslatingExpressionVisitorFactory _sqlTranslatingExpressionVisitorFactory;
        private readonly ICompositePredicateExpressionVisitorFactory _compositePredicateExpressionVisitorFactory;
        private readonly IConditionalRemovingExpressionVisitorFactory _conditionalRemovingExpressionVisitorFactory;

        private bool _requiresClientSelectMany;
        private bool _requiresClientJoin;
        private bool _requiresClientFilter;
        private bool _requiresClientProjection;
        private bool _requiresClientOrderBy;
        private bool _requiresClientResultOperator;
        private bool _requiresOuterParameterInjection;

        private Dictionary<IncludeSpecification, List<int>> _navigationIndexMap = new Dictionary<IncludeSpecification, List<int>>();

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public RelationalQueryModelVisitor(
            [NotNull] IQueryOptimizer queryOptimizer,
            [NotNull] INavigationRewritingExpressionVisitorFactory navigationRewritingQueryModelVisitorFactory,
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
            [NotNull] IDbContextOptions contextOptions,
            [NotNull] RelationalQueryCompilationContext queryCompilationContext,
            [CanBeNull] RelationalQueryModelVisitor parentQueryModelVisitor)
            : base(
                Check.NotNull(queryOptimizer, nameof(queryOptimizer)),
                Check.NotNull(navigationRewritingQueryModelVisitorFactory, nameof(navigationRewritingQueryModelVisitorFactory)),
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
            Check.NotNull(contextOptions, nameof(contextOptions));

            _relationalAnnotationProvider = relationalAnnotationProvider;
            _includeExpressionVisitorFactory = includeExpressionVisitorFactory;
            _sqlTranslatingExpressionVisitorFactory = sqlTranslatingExpressionVisitorFactory;
            _compositePredicateExpressionVisitorFactory = compositePredicateExpressionVisitorFactory;
            _conditionalRemovingExpressionVisitorFactory = conditionalRemovingExpressionVisitorFactory;

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
        ///     Gets or sets a value indicating whether the query is able to bind any outer parameters.
        /// </summary>
        public virtual bool CanBindOuterParameters { get; protected set; } = true;

        /// <summary>
        ///     Gets or sets a value indicating whether the query is able to bind any outer properties.
        /// </summary>
        public virtual bool CanBindOuterProperties { get; protected set; } = true;

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
        public virtual IEnumerable<SelectExpression> Queries => _queriesBySource.Values.Distinct();

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
        public virtual void MapQuery([NotNull] IQuerySource querySource, [NotNull] SelectExpression selectExpression)
        {
            Check.NotNull(querySource, nameof(querySource));
            Check.NotNull(selectExpression, nameof(selectExpression));

            _queriesBySource[querySource] = selectExpression;
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

            SelectExpression selectExpression;

            if (_queriesBySource.TryGetValue(querySource, out selectExpression))
            {
                return selectExpression;
            }

            return selectExpression;
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

            if (ParentQueryModelVisitor != null)
            {
                RequiresOuterParameterInjection |= ResultOperatorNecessitatesOuterParameterInjection(queryModel);
            }

            var typeIsExpressionTranslatingVisitor
                = new TypeIsExpressionTranslatingVisitor(QueryCompilationContext.Model, _relationalAnnotationProvider);

            queryModel.TransformExpressions(typeIsExpressionTranslatingVisitor.Visit);

            base.VisitQueryModel(queryModel);
        }

        private static bool ResultOperatorNecessitatesOuterParameterInjection(QueryModel queryModel)
        {
            foreach (var resultOperator in queryModel.ResultOperators)
            {
                var singleResultOperator = resultOperator as SingleResultOperator;
                if (singleResultOperator != null)
                {
                    return true;
                }

                var firstResultOperator = resultOperator as FirstResultOperator;
                if (firstResultOperator != null)
                {
                    return !firstResultOperator.ReturnDefaultWhenEmpty;
                }

                var lastResultOperator = resultOperator as LastResultOperator;
                if (lastResultOperator != null)
                {
                    return !lastResultOperator.ReturnDefaultWhenEmpty;
                }
            }

            return false;
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

            if (IsEnumerableRelationalTypeProperty(mainFromClause.FromExpression))
            {
                RequiresClientEval = true;
            }
            else if (ParentQueryModelVisitor != null && !ParentQueryModelVisitor.CanBindOuterProperties)
            {
                var referencedQuerySource
                    = (mainFromClause.FromExpression as QuerySourceReferenceExpression)
                        ?.ReferencedQuerySource;

                if (referencedQuerySource != null)
                {
                    var selectExpression = QueryCompilationContext.FindSelectExpression(referencedQuerySource);

                    if (selectExpression != null)
                    {
                        MapQuery(mainFromClause, selectExpression);
                    }
                }
            }

            var compiled = base.CompileMainFromClauseExpression(mainFromClause, queryModel);

            return LiftSubQuery(mainFromClause, mainFromClause.FromExpression, compiled);
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
            var outerSelectExpression = TryGetQuery(previousQuerySource);

            var canBindOuterProperties = CanBindOuterProperties;
            CanBindOuterProperties = QueryCompilationContext.IsLateralJoinSupported;

            base.VisitAdditionalFromClause(fromClause, queryModel, index);

            CanBindOuterProperties = canBindOuterProperties;

            var innerSelectExpression = TryGetQuery(fromClause);

            if (outerSelectExpression != null 
                && !RequiresClientJoin
                && innerSelectExpression?.Tables.Count == 1)
            {
                var selectManyExpression = Expression as MethodCallExpression;

                var outerShapedQuery
                    = selectManyExpression.Arguments.FirstOrDefault() as MethodCallExpression;

                var innerShapedQuery 
                    = (selectManyExpression.Arguments.Skip(1).FirstOrDefault() as LambdaExpression)
                        ?.Body as MethodCallExpression;

                var canFlattenSelectMany
                    = selectManyExpression != null
                        && selectManyExpression.Method.MethodIsClosedFormOf(LinqOperatorProvider.SelectMany)
                        && IsShapedQueryExpression(outerShapedQuery)
                        && IsShapedQueryExpression(innerShapedQuery);

                if (canFlattenSelectMany)
                {
                    var correlated = innerSelectExpression.IsCorrelated();

                    if (!correlated || QueryCompilationContext.IsLateralJoinSupported)
                    {
                        outerSelectExpression.ExplodeStarProjection();
                        innerSelectExpression.ExplodeStarProjection();

                        var outerProjectionCount = outerSelectExpression.Projection.Count;
                        var innerProjectionCount = innerSelectExpression.Projection.Count;

                        if (correlated)
                        {
                            outerSelectExpression.AddCrossJoinLateral(
                                innerSelectExpression.Tables.First(),
                                innerSelectExpression.Projection);
                        }
                        else
                        {
                            outerSelectExpression.AddCrossJoin(
                                innerSelectExpression.Tables.First(),
                                innerSelectExpression.Projection);
                        }

                        MapQuery(fromClause, outerSelectExpression);

                        var outerShaper = ExtractShaper(outerShapedQuery, 0);
                        var innerShaper = ExtractShaper(innerShapedQuery, outerProjectionCount);

                        var materializerLambda = (LambdaExpression)selectManyExpression.Arguments.Last();
                        var materializer = materializerLambda.Compile();

                        var compositeShaper
                            = CompositeShaper.Create(fromClause, outerShaper, innerShaper, materializer);

                        compositeShaper.SaveAccessorExpression(QueryCompilationContext.QuerySourceMapping);

                        innerShaper.UpdateQuerySource(fromClause);

                        Expression 
                            = Expression.Call(
                                outerShapedQuery.Method
                                    .GetGenericMethodDefinition()
                                    .MakeGenericMethod(materializerLambda.ReturnType),
                                outerShapedQuery.Arguments[0],
                                outerShapedQuery.Arguments[1],
                                Expression.Constant(compositeShaper));

                        return;
                    }
                }
            }

            RequiresClientSelectMany = true;
            WarnClientEval(fromClause);
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

            if (IsEnumerableRelationalTypeProperty(additionalFromClause.FromExpression))
            {
                RequiresClientSelectMany = true;
            }

            var expression = base.CompileAdditionalFromClauseExpression(additionalFromClause, queryModel);

            return LiftSubQuery(additionalFromClause, additionalFromClause.FromExpression, expression);
        }

        /// <summary>
        ///     Visit a join clause.
        /// </summary>
        /// <param name="joinClause"> The join clause being visited. </param>
        /// <param name="queryModel"> The query model. </param>
        /// <param name="index"> Index of the node being visited. </param>
        public override void VisitJoinClause(
            JoinClause joinClause, QueryModel queryModel, int index)
        {
            Check.NotNull(joinClause, nameof(joinClause));
            Check.NotNull(queryModel, nameof(queryModel));

            if (!RequiresClientSelectMany && !RequiresClientJoin)
            {
                var previousQuerySource = FindPreviousQuerySource(queryModel, index);

                // Set mutateProjections to true so that when the key selector
                // expressions are compiled, all necessary columns will be present
                // in the SelectExpressions.
                var sqlTranslatingExpressionVisitor
                    = _sqlTranslatingExpressionVisitorFactory.Create(
                        queryModelVisitor: this,
                        topLevelPredicate: null,
                        inProjection: false,
                        mutateProjections: true);

                // Compile the outer key selector

                var outerSelectExpression = TryGetQuery(previousQuerySource);
                var outerProjectionCount = outerSelectExpression?.Projection.Count ?? -1;
                var outerKeySqlExpression = sqlTranslatingExpressionVisitor.Visit(joinClause.OuterKeySelector);
                var outerKeySelectorExpression = CompileJoinClauseOuterKeySelectorExpression(joinClause, joinClause.OuterKeySelector, queryModel);

                // Compile the inner sequence

                var innerSequenceExpression = CompileJoinClauseInnerSequenceExpression(joinClause, joinClause.InnerSequence, queryModel);

                // Prepare to compile the inner key selector

                var innerRequiresMaterialization = QueryCompilationContext.QuerySourceRequiresMaterialization(joinClause);

                var innerItemParameter
                    = Expression.Parameter(
                        innerRequiresMaterialization
                            ? joinClause.ItemType
                            : typeof(ValueBuffer),
                        joinClause.ItemName);

                AddOrUpdateMapping(joinClause, innerItemParameter);

                var transparentIdentifierType
                    = CreateTransparentIdentifierType(
                        CurrentParameter.Type,
                        innerItemParameter.Type);

                // Compile the inner key selector

                var innerSelectExpression = TryGetQuery(joinClause);
                var innerProjectionCount = innerSelectExpression?.Projection.Count ?? -1;
                var innerKeySqlExpression = sqlTranslatingExpressionVisitor.Visit(joinClause.InnerKeySelector);
                var innerKeySelectorExpression 
                    = CompileJoinClauseInnerKeySelectorExpression(
                        joinClause, 
                        joinClause.InnerKeySelector, 
                        innerItemParameter,
                        queryModel);

                if (IsShapedQueryExpression(innerSequenceExpression)
                    && outerKeySqlExpression != null
                    && innerKeySqlExpression != null)
                {
                    var predicate
                        = sqlTranslatingExpressionVisitor.Visit(
                            Expression.Equal(
                                joinClause.OuterKeySelector,
                                joinClause.InnerKeySelector));

                    // Now that we are flattening the join, we can reset the projections
                    // to what they were before. If any of the added columns are actually needed
                    // they will be re-added later.
                    outerSelectExpression.RemoveRangeFromProjection(outerProjectionCount);

                    var tableToJoin = innerSelectExpression.Tables.First();

                    var projection
                        = QueryCompilationContext
                            .QuerySourceRequiresMaterialization(tableToJoin.QuerySource)
                                ? innerSelectExpression.Projection
                                : Enumerable.Empty<Expression>();

                    var joinExpression
                        = outerSelectExpression.AddInnerJoin(tableToJoin, projection, innerSelectExpression.Predicate);

                    joinExpression.QuerySource = joinClause;
                    joinExpression.Predicate = predicate;

                    MapQuery(joinClause, outerSelectExpression);

                    var outerShapedQuery = (MethodCallExpression)Expression;
                    var innerShapedQuery = (MethodCallExpression)innerSequenceExpression;

                    var outerShaper = ExtractShaper(outerShapedQuery, 0);
                    var innerShaper = ExtractShaper(innerShapedQuery, outerProjectionCount);

                    if (!innerRequiresMaterialization)
                    {
                        innerShaper = new ValueBufferShaper(joinClause);
                    }
                    else
                    {
                        innerShaper.UpdateQuerySource(joinClause);
                    }

                    var compositeShaper
                        = CompositeShaper.Create(
                            joinClause,
                            outerShaper,
                            innerShaper,
                            Expression.Lambda(
                                CallCreateTransparentIdentifier(
                                    transparentIdentifierType,
                                    CurrentParameter,
                                    innerItemParameter),
                                CurrentParameter,
                                innerItemParameter).Compile());

                    Expression
                        = Expression.Call(
                            outerShapedQuery.Method
                                .GetGenericMethodDefinition()
                                .MakeGenericMethod(transparentIdentifierType),
                            outerShapedQuery.Arguments[0],
                            outerShapedQuery.Arguments[1],
                            Expression.Constant(compositeShaper));

                    Expression = ReplaceClauseReferences(Expression, joinClause);

                    IntroduceTransparentScope(joinClause, queryModel, index, transparentIdentifierType);

                    compositeShaper.SaveAccessorExpression(QueryCompilationContext.QuerySourceMapping);

                    return;
                }

                if (!innerRequiresMaterialization)
                {
                    innerSequenceExpression = MakeInnerSequenceShapedQueryExpression(innerSequenceExpression, joinClause);
                }

                Expression
                    = Expression.Call(
                        LinqOperatorProvider.Join
                            .MakeGenericMethod(
                                CurrentParameter.Type,
                                innerItemParameter.Type,
                                outerKeySelectorExpression.Type,
                                transparentIdentifierType),
                        Expression,
                        innerSequenceExpression,
                        Expression.Lambda(outerKeySelectorExpression, CurrentParameter),
                        Expression.Lambda(innerKeySelectorExpression, innerItemParameter),
                        Expression.Lambda(
                            CallCreateTransparentIdentifier(
                                transparentIdentifierType,
                                CurrentParameter,
                                innerItemParameter),
                            CurrentParameter,
                            innerItemParameter));

                IntroduceTransparentScope(joinClause, queryModel, index, transparentIdentifierType);

                RequiresClientJoin = true;
                WarnClientEval(joinClause);

                return;
            }

            RequiresClientJoin = true;
            WarnClientEval(joinClause);
            base.VisitJoinClause(joinClause, queryModel, index);
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

            if (queryModel.CountQuerySourceReferences(groupJoinClause) == 0)
            {
                // If a GroupJoinClause is not referenced anywhere else in the query,
                // there is no reason to be adding it to a select expression
                // or trying to bind expressions to it -- this is unlike a JoinClause, 
                // which has some effect on the result set whether it is referenced
                // or not.

                // This could probably be handled beforehand by the 'optimizer'.
                // return;
            }

            var previousQuerySource = FindPreviousQuerySource(queryModel, index);

            if (TryFlattenGroupJoinSelectMany(groupJoinClause, queryModel, index))
            {
                return;
            }

            if (!RequiresClientSelectMany && !RequiresClientJoin)
            {
                // Set mutateProjections to true so that when the key selector
                // expressions are compiled, all necessary columns will be present
                // in the SelectExpressions.
                var sqlTranslatingExpressionVisitor
                    = _sqlTranslatingExpressionVisitorFactory.Create(
                        queryModelVisitor: this,
                        topLevelPredicate: null,
                        inProjection: false,
                        mutateProjections: true);

                // Compile the outer key selector

                var outerSelectExpression = TryGetQuery(previousQuerySource);
                var outerProjectionCount = outerSelectExpression?.Projection.Count ?? -1;
                var outerKeySqlExpression = sqlTranslatingExpressionVisitor.Visit(groupJoinClause.JoinClause.OuterKeySelector);
                
                // Compile the inner sequence

                var innerSequenceExpression = CompileJoinClauseInnerSequenceExpression(groupJoinClause, groupJoinClause.JoinClause.InnerSequence, queryModel);

                // Prepare to compile the inner key selector

                var innerRequiresMaterialization = QueryCompilationContext.QuerySourceRequiresMaterialization(groupJoinClause.JoinClause);
                
                var innerItemParameter
                    = Expression.Parameter(
                        innerRequiresMaterialization 
                            ? groupJoinClause.JoinClause.ItemType 
                            : typeof(ValueBuffer),
                        groupJoinClause.JoinClause.ItemName);

                AddOrUpdateMapping(groupJoinClause.JoinClause, innerItemParameter);

                var innerItemsParameter
                    = Expression.Parameter(
                        LinqOperatorProvider.MakeSequenceType(innerItemParameter.Type),
                        groupJoinClause.ItemName);

                AddOrUpdateMapping(groupJoinClause, innerItemsParameter);

                var transparentIdentifierType
                    = CreateTransparentIdentifierType(
                        CurrentParameter.Type,
                        innerItemsParameter.Type);

                var groupJoinSelectExpression = TryGetQuery(groupJoinClause);

                if (groupJoinSelectExpression != null)
                {
                    MapQuery(groupJoinClause.JoinClause, groupJoinSelectExpression);
                }

                // Compile the inner key selector

                var innerSelectExpression = groupJoinSelectExpression;
                var innerProjectionCount = innerSelectExpression?.Projection.Count ?? -1;
                var innerKeySqlExpression = sqlTranslatingExpressionVisitor.Visit(groupJoinClause.JoinClause.InnerKeySelector);

                if (IsShapedQueryExpression(innerSequenceExpression)
                    && outerKeySqlExpression != null
                    && innerKeySqlExpression != null
                    && !(innerSelectExpression.OrderBy.Count > 0 && innerSelectExpression.Limit == null))
                {
                    var predicate
                        = sqlTranslatingExpressionVisitor.Visit(
                            Expression.Equal(
                                groupJoinClause.JoinClause.OuterKeySelector,
                                groupJoinClause.JoinClause.InnerKeySelector));

                    // Now that we are flattening the join, we can reset the projections
                    // to what they were before. If any of the added columns are actually needed
                    // they will be re-added later.

                    outerSelectExpression.RemoveRangeFromProjection(outerProjectionCount);

                    var tableToJoin = innerSelectExpression.Tables.First();

                    outerSelectExpression.AddTable(new LeftOuterJoinExpression(tableToJoin)
                    {
                        QuerySource = groupJoinClause,
                        Predicate = predicate
                    });

                    var outerComparer = typeof(EqualityComparer<>)
                        .MakeGenericType(Expression.Type.GetSequenceType())
                        .GetRuntimeProperty("Default")
                        .GetMethod
                        .Invoke(null, new object[0]);

                    if (!QueryCompilationContext.QuerySourceRequiresMaterialization(previousQuerySource))
                    {
                        var indices = new List<int>();

                        var previousQuerySources = new List<IQuerySource> { queryModel.MainFromClause };

                        previousQuerySources.AddRange(
                            queryModel.BodyClauses.TakeWhile((c, i) => i < index)
                                .OfType<IQuerySource>());

                        foreach (var querySource in previousQuerySources)
                        {
                            var outerPrimaryKey
                                = QueryCompilationContext.Model
                                    .FindEntityType(querySource.ItemType)
                                    .FindPrimaryKey();

                            if (outerPrimaryKey != null)
                            {
                                foreach (var property in outerPrimaryKey.Properties)
                                {
                                    indices.Add(outerSelectExpression.AddToProjection(
                                        _relationalAnnotationProvider.For(property).ColumnName,
                                        property,
                                        querySource));
                                }
                            }
                        }

                        var outerAccessor = QueryCompilationContext.QuerySourceMapping.GetExpression(previousQuerySource);
                        var outerParameter = outerAccessor.GetRootExpression<ParameterExpression>();

                        outerComparer = GroupJoinOuterEqualityComparer.Create(
                            Expression.Type.GetSequenceType(),
                            Expression.Lambda(outerAccessor, outerParameter).Compile(),
                            indices.ToArray());

                        outerProjectionCount = outerSelectExpression.Projection.Count;
                    }

                    var predicateAnalyzer = new JoinPredicateAnalyzingVisitor();

                    predicateAnalyzer.Visit(predicate);

                    if (QueryCompilationContext.QuerySourceRequiresMaterialization(groupJoinClause.JoinClause))
                    {
                        foreach (var innerExpression in innerSelectExpression.Projection)
                        {
                            outerSelectExpression.AddToProjection(innerExpression);
                        }
                    }
                    else
                    {
                        // Is this block needed?
                        foreach (var innerExpression in predicateAnalyzer.InnerExpressions)
                        {
                            outerSelectExpression.AddToProjection(innerExpression);
                        }
                    }

                    var previousOrderingCount = outerSelectExpression.OrderBy.Count;

                    if (!predicateAnalyzer.FoundDependentToPrincipal)
                    {
                        foreach (var expression in predicateAnalyzer.OuterExpressions)
                        {
                            outerSelectExpression.AddToOrderBy(new Ordering(expression, OrderingDirection.Asc));
                        }
                    }

                    if (innerSelectExpression.OrderBy.Any())
                    {
                        outerSelectExpression.AddToOrderBy(innerSelectExpression.OrderBy);
                    }
                    else if (QueryCompilationContext.QuerySourceRequiresMaterialization(previousQuerySource)
                        && QueryCompilationContext.QuerySourceRequiresMaterialization(groupJoinClause))
                    {
                        // It's hard to tell when this optimization can be run if both the
                        // outer and inner query sources don't require materialization,
                        // because we aren't certain yet whether the projection 
                        // is going to include columns other than the key columns
                        // for the previous query source. If the projection ends up
                        // only containing key columns, the RDBMS engine may optimize
                        // by skipping the outer table altogether and using the value from the
                        // inner table, serving out results as clustered on the inner table.

                        // For now it is safer to leave the ORDER BY in place in those cases
                        // and let the RDBMS do its own optimizations.

                        outerSelectExpression.RemoveRangeFromOrderBy(previousOrderingCount);
                    }

                    outerSelectExpression.QuerySource = groupJoinClause;
                    MapQuery(groupJoinClause, outerSelectExpression);
                    MapQuery(groupJoinClause.JoinClause, outerSelectExpression);

                    var outerKeyExpression 
                        = CompileJoinClauseOuterKeySelectorExpression(
                            groupJoinClause, 
                            groupJoinClause.JoinClause.OuterKeySelector, 
                            queryModel);

                    var innerKeyExpression
                        = CompileJoinClauseInnerKeySelectorExpression(
                            groupJoinClause,
                            groupJoinClause.JoinClause.InnerKeySelector,
                            innerItemParameter,
                            queryModel);

                    // Take over parts of what the base implementation would have done.

                    var outerShapedQuery = (MethodCallExpression)Expression;
                    var innerShapedQuery = (MethodCallExpression)innerSequenceExpression;

                    var outerShaper = ExtractShaper(outerShapedQuery, 0);
                    var innerShaper = ExtractShaper(innerShapedQuery, outerProjectionCount);

                    if (!innerRequiresMaterialization)
                    {
                        var innerExpression = predicateAnalyzer.InnerExpressions.First();
                        var projectionIndex = outerSelectExpression.AddToProjection(innerExpression);

                        innerShaper = new GroupJoinValueBufferShaper(groupJoinClause.JoinClause, projectionIndex);
                    }
                    else
                    {
                        innerShaper.UpdateQuerySource(groupJoinClause.JoinClause);
                    }

                    outerShaper.SaveAccessorExpression(QueryCompilationContext.QuerySourceMapping);
                    innerShaper.SaveAccessorExpression(QueryCompilationContext.QuerySourceMapping);

                    var queryMethodProvider = QueryCompilationContext.QueryMethodProvider;

                    var groupJoinMethod
                        = queryMethodProvider.GroupJoinMethod.MakeGenericMethod(
                            outerShaper.Type,
                            innerShaper.Type,
                            innerKeyExpression.Type,
                            transparentIdentifierType);

                    var newShapedQueryMethod
                        = Expression.Call(
                            queryMethodProvider.QueryMethod,
                            outerShapedQuery.Arguments[0],
                            outerShapedQuery.Arguments[1],
                            Expression.Default(typeof(int?)));

                    var defaultGroupJoinInclude
                        = Expression.Default(
                            queryMethodProvider.GroupJoinIncludeType);

                    Expression
                        = Expression.Call(
                            groupJoinMethod,
                            Expression.Convert(
                                QueryContextParameter,
                                typeof(RelationalQueryContext)),
                            newShapedQueryMethod,
                            Expression.Constant(outerShaper),
                            Expression.Constant(innerShaper),
                            Expression.Constant(outerComparer),
                            Expression.Lambda(
                                innerKeyExpression,
                                innerItemParameter),
                            Expression.Lambda(
                                CallCreateTransparentIdentifier(
                                    transparentIdentifierType,
                                    CurrentParameter,
                                    innerItemsParameter),
                                CurrentParameter,
                                innerItemsParameter),
                            defaultGroupJoinInclude,
                            defaultGroupJoinInclude);

                    IntroduceTransparentScope(groupJoinClause, queryModel, index, transparentIdentifierType);

                    return;
                }
                else
                {
                    var outerKeyExpression 
                        = CompileJoinClauseOuterKeySelectorExpression(
                            groupJoinClause, 
                            groupJoinClause.JoinClause.OuterKeySelector, 
                            queryModel);

                    var innerKeyExpression
                        = CompileJoinClauseInnerKeySelectorExpression(
                            groupJoinClause,
                            groupJoinClause.JoinClause.InnerKeySelector,
                            innerItemParameter,
                            queryModel);

                    if (!innerRequiresMaterialization)
                    {
                        innerSequenceExpression = MakeInnerSequenceShapedQueryExpression(innerSequenceExpression, groupJoinClause.JoinClause);
                    }

                    Expression
                        = Expression.Call(
                            LinqOperatorProvider.GroupJoin
                                .MakeGenericMethod(
                                    CurrentParameter.Type,
                                    innerItemParameter.Type,
                                    innerKeyExpression.Type,
                                    transparentIdentifierType),
                            Expression,
                            innerSequenceExpression,
                            Expression.Lambda(outerKeyExpression, CurrentParameter),
                            Expression.Lambda(innerKeyExpression, innerItemParameter),
                            Expression.Lambda(
                                CallCreateTransparentIdentifier(
                                    transparentIdentifierType,
                                    CurrentParameter,
                                    innerItemsParameter),
                                CurrentParameter,
                                innerItemsParameter));

                    IntroduceTransparentScope(groupJoinClause, queryModel, index, transparentIdentifierType);

                    RequiresClientJoin = true;
                    WarnClientEval(groupJoinClause);

                    return;
                }
            }

            RequiresClientJoin = true;
            WarnClientEval(groupJoinClause);
            base.VisitGroupJoinClause(groupJoinClause, queryModel, index);
        }

        /// <summary>
        ///     Compiles the inner sequence expression for <see cref="JoinClause" /> 
        ///     and <see cref="GroupJoinClause" /> nodes.
        /// </summary>
        /// <param name="querySource"> The node being compiled. </param>
        /// <param name="innerSequence"> The inner sequence being compiled. </param>
        /// <param name="queryModel"> The query. </param>
        /// <returns> The compiled result. </returns>
        protected override Expression CompileJoinClauseInnerSequenceExpression(
            IQuerySource querySource,
            Expression innerSequence,
            QueryModel queryModel)
        {
            Check.NotNull(querySource, nameof(querySource));
            Check.NotNull(queryModel, nameof(queryModel));

            var expression = base.CompileJoinClauseInnerSequenceExpression(querySource, innerSequence, queryModel);

            return LiftSubQuery(querySource, innerSequence, expression);
        }

        private bool TryFlattenGroupJoinSelectMany(
            GroupJoinClause groupJoinClause,
            QueryModel queryModel,
            int index)
        {
            var previousQuerySource = FindPreviousQuerySource(queryModel, index);
            var outerSelectExpression = TryGetQuery(previousQuerySource);
            var outerProjectionCount = outerSelectExpression?.Projection.Count ?? 0;

            if (outerSelectExpression == null)
            {
                // We can't do anything without this so...
                return false;
            }

            if (queryModel.CountQuerySourceReferences(groupJoinClause) != 1)
            {
                // Either the group join clause is not referenced anywhere
                // or it's referenced in multiple places -- either way, we
                // cannot flatten it.
                return false;
            }

            if (index + 1 == queryModel.BodyClauses.Count)
            {
                // The group join clause is referenced somewhere, but 
                // it's the last body clause so it's probably being used
                // in the selector or something. 
                return false;
            }

            var additionalFromClause = queryModel.BodyClauses[index + 1] as AdditionalFromClause;
            var additionalFromSubQuery = additionalFromClause?.FromExpression as SubQueryExpression;
            var additionalFromSubQueryModel = additionalFromSubQuery?.QueryModel;
            var querySourceReference = additionalFromSubQueryModel?.MainFromClause.FromExpression as QuerySourceReferenceExpression;
            var lastResultOperator = additionalFromSubQueryModel?.ResultOperators.LastOrDefault();

            if (querySourceReference?.ReferencedQuerySource != groupJoinClause)
            {
                // We have to be particular about what 'shape' of subquery
                // we can accept for flattening.
                return false;
            }

            var correlated
                = additionalFromSubQueryModel.ResultOperators.OfType<TakeResultOperator>().Any()
                    || additionalFromSubQueryModel.ResultOperators.OfType<SkipResultOperator>().Any()
                    || additionalFromSubQueryModel.BodyClauses.OfType<OrderByClause>().Any();

            if (correlated && !QueryCompilationContext.IsLateralJoinSupported)
            {
                return false;
            }

            var joinClause = groupJoinClause.JoinClause;
            var joinSubQuery = joinClause.InnerSequence as SubQueryExpression;
            var joinSubQueryModel = joinSubQuery?.QueryModel;

            // Shift the additional from clause's subquery to be the join clause's 
            // inner sequence expression, then replace the join clause's inner 
            // sequence expression with the subquery. This ensures any subquery
            // lifting and pushing down will take place if needed.
            additionalFromSubQueryModel.MainFromClause.FromExpression = joinClause.InnerSequence;
            joinClause.InnerSequence = additionalFromSubQuery;

            // Remove these items since we are optimizing them away.
            if (lastResultOperator is DefaultIfEmptyResultOperator)
            {
                additionalFromSubQueryModel.ResultOperators.Remove(lastResultOperator);
            }
            queryModel.BodyClauses.Insert(index, joinClause);
            queryModel.BodyClauses.Remove(groupJoinClause);
            queryModel.BodyClauses.Remove(additionalFromClause);

            // Replace all references to the AdditionalFromClause with references
            // to the flattened JoinClause.
            //queryModel.BodyClauses.Remove(joinClause);
            var mapping = new QuerySourceMapping();
            mapping.AddMapping(additionalFromClause, new QuerySourceReferenceExpression(joinClause));
            queryModel.TransformExpressions(expression =>
                ReferenceReplacingExpressionVisitor.ReplaceClauseReferences(
                    expression,
                    mapping,
                    throwOnUnmappedReferences: false));

            // Set mutateProjections to true so that when the key selector
            // expressions are compiled, all necessary columns will be present
            // in the SelectExpressions.
            var sqlTranslatingExpressionVisitor
                = _sqlTranslatingExpressionVisitorFactory.Create(
                    queryModelVisitor: this,
                    topLevelPredicate: null,
                    inProjection: false,
                    mutateProjections: true);

            var outerKeySqlExpression = sqlTranslatingExpressionVisitor.Visit(groupJoinClause.JoinClause.OuterKeySelector);
            var outerKeyExpression 
                = CompileJoinClauseOuterKeySelectorExpression(
                    groupJoinClause, 
                    groupJoinClause.JoinClause.OuterKeySelector, 
                    queryModel);

            var innerSequenceExpression
                = CompileJoinClauseInnerSequenceExpression(
                    groupJoinClause.JoinClause,
                    groupJoinClause.JoinClause.InnerSequence,
                    queryModel);

            var innerItemParameter
                = Expression.Parameter(
                    innerSequenceExpression.Type.GetSequenceType(),
                    joinClause.ItemName);

            AddOrUpdateMapping(joinClause, innerItemParameter);

            var transparentIdentifierType
                = CreateTransparentIdentifierType(
                    CurrentParameter.Type,
                    innerItemParameter.Type);

            var innerSelectExpression = TryGetQuery(groupJoinClause.JoinClause);
            var innerProjectionCount = innerSelectExpression?.Projection.Count ?? -1;
            var innerKeySqlExpression = sqlTranslatingExpressionVisitor.Visit(groupJoinClause.JoinClause.InnerKeySelector);
            var innerKeyExpression 
                = CompileJoinClauseInnerKeySelectorExpression(
                    groupJoinClause, 
                    groupJoinClause.JoinClause.InnerKeySelector, 
                    innerItemParameter,
                    queryModel);

            if (!IsShapedQueryExpression(innerSequenceExpression)
                || outerKeySqlExpression == null
                || innerKeySqlExpression == null)
            {
                return false;
            }

            var predicate
                = sqlTranslatingExpressionVisitor.Visit(
                    Expression.Equal(
                        groupJoinClause.JoinClause.OuterKeySelector,
                        groupJoinClause.JoinClause.InnerKeySelector));

            // Now that we are flattening the join, we can reset the projections
            // to what they were before. If any of the added columns are actually needed
            // they will be re-added later.
            outerSelectExpression.RemoveRangeFromProjection(outerProjectionCount);
            innerSelectExpression.RemoveRangeFromProjection(innerProjectionCount);

            var innerTable = innerSelectExpression.Tables.First();
            innerTable.QuerySource = joinClause;
            innerTable.Alias = joinClause.ItemName;

            if (outerSelectExpression.HasTableUsingAlias(innerTable.Alias))
            {
                innerTable.Alias = QueryCompilationContext.CreateUniqueTableAlias(innerTable.Alias);
            }
            else
            {
                QueryCompilationContext.RegisterUsedTableAlias(innerTable.Alias);
            }

            var innerTableSubQuery = innerTable as SelectExpression;

            if (innerTableSubQuery != null)
            {
                var innerTableSubQueryTable
                    = innerTableSubQuery.GetTableForQuerySource(
                        joinSubQueryModel?.MainFromClause
                            ?? additionalFromSubQueryModel.MainFromClause);

                innerTableSubQueryTable.Alias = innerTable.Alias;

                if (innerTableSubQuery.ProjectStarAlias == additionalFromSubQueryModel?.MainFromClause.ItemName
                    || innerTableSubQuery.ProjectStarAlias == joinSubQueryModel?.MainFromClause.ItemName)
                {
                    innerTableSubQuery.ProjectStarAlias = innerTable.Alias;
                }
            }

            if (correlated)
            {
                Debug.Assert(innerTableSubQuery != null);

                MapQuery(joinClause, innerTableSubQuery);

                var leftJoinLateral = new LeftJoinLateralExpression(innerTable)
                {
                    QuerySource = joinClause,
                };

                if (lastResultOperator is DefaultIfEmptyResultOperator)
                {
                    outerSelectExpression.AddTable(new LeftJoinLateralExpression(innerTable)
                    {
                        QuerySource = joinClause,
                    });
                }
                else
                {
                    outerSelectExpression.AddTable(new CrossJoinLateralExpression(innerTable)
                    {
                        QuerySource = joinClause,
                    });
                }

                var oldPredicate = innerTableSubQuery.Predicate;

                if (oldPredicate == null)
                {
                    innerTableSubQuery.Predicate = predicate;
                }
                else
                {
                    innerTableSubQuery.Predicate = Expression.AndAlso(predicate, oldPredicate);
                }
            }
            else
            {
                if (lastResultOperator is DefaultIfEmptyResultOperator)
                {
                    outerSelectExpression.AddTable(new LeftOuterJoinExpression(innerTable)
                    {
                        QuerySource = joinClause,
                        Predicate = predicate,
                    });
                }
                else
                {
                    outerSelectExpression.AddTable(new InnerJoinExpression(innerTable)
                    {
                        QuerySource = joinClause,
                        Predicate = predicate,
                    });
                }
            }

            if (QueryCompilationContext.QuerySourceRequiresMaterialization(joinClause))
            {
                outerSelectExpression.AddToProjection(innerSelectExpression.Projection);
            }

            var predicateAnalyzer = new JoinPredicateAnalyzingVisitor();

            predicateAnalyzer.Visit(predicate);

            if (predicateAnalyzer.FoundPrincipalToDependent)
            {
                if (ParentQueryModelVisitor != null)
                {
                    foreach (var expression in predicateAnalyzer.OuterExpressions)
                    {
                        outerSelectExpression.AddToOrderBy(new Ordering(expression, OrderingDirection.Asc));
                    }
                }
            }

            MapQuery(joinClause, outerSelectExpression);

            var outerShapedQuery = (MethodCallExpression)Expression;
            var innerShapedQuery = (MethodCallExpression)innerSequenceExpression;

            var outerShaper = ExtractShaper(outerShapedQuery, 0);
            var innerShaper = ExtractShaper(innerShapedQuery, outerProjectionCount);

            var compositeShaper
                = CompositeShaper.Create(
                    joinClause,
                    outerShaper,
                    innerShaper,
                    Expression.Lambda(
                        CallCreateTransparentIdentifier(
                            transparentIdentifierType,
                            CurrentParameter,
                            innerItemParameter),
                        CurrentParameter,
                        innerItemParameter).Compile());

            innerShaper.UpdateQuerySource(joinClause);

            Expression
                = Expression.Call(
                    outerShapedQuery.Method
                        .GetGenericMethodDefinition()
                        .MakeGenericMethod(transparentIdentifierType),
                    outerShapedQuery.Arguments[0],
                    outerShapedQuery.Arguments[1],
                    Expression.Constant(compositeShaper));

            Expression = ReplaceClauseReferences(Expression, joinClause);

            IntroduceTransparentScope(joinClause, queryModel, index, transparentIdentifierType);

            compositeShaper.SaveAccessorExpression(QueryCompilationContext.QuerySourceMapping);

            foreach (var annotation in QueryCompilationContext.QueryAnnotations)
            {
                if (annotation.QuerySource == additionalFromClause)
                {
                    annotation.QuerySource = joinClause;
                }
            }

            return true;
        }

        private class JoinPredicateAnalyzingVisitor : ExpressionVisitor
        {
            private readonly List<Expression> _outerExpressions = new List<Expression>();
            private readonly List<Expression> _innerExpressions = new List<Expression>();

            private IForeignKey _matchingCandidate;
            private List<IProperty> _matchingCandidateProperties;

            public IForeignKey ExtractedForeignKey { get; private set; }

            public bool FoundDependentToPrincipal { get; private set; }

            public bool FoundPrincipalToDependent { get; private set; }

            public IEnumerable<Expression> OuterExpressions => _outerExpressions;

            public IEnumerable<Expression> InnerExpressions => _innerExpressions;

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
                if (ExtractedForeignKey != null)
                {
                    return node;
                }

                if (node.NodeType == ExpressionType.Equal)
                {
                    var leftProperty = node.Left.RemoveConvert().TryGetColumnExpression()?.Property;
                    var rightProperty = node.Right.RemoveConvert().TryGetColumnExpression()?.Property;

                    if (leftProperty != null && rightProperty != null)
                    {
                        if (leftProperty.IsForeignKey() && rightProperty.IsKey())
                        {
                            if (MatchKeyProperties(rightProperty, leftProperty))
                            {
                                ExtractedForeignKey = _matchingCandidate;
                                FoundDependentToPrincipal = true;
                            }
                        }
                        else if (leftProperty.IsKey() && rightProperty.IsForeignKey())
                        {
                            if (MatchKeyProperties(leftProperty, rightProperty))
                            {
                                ExtractedForeignKey = _matchingCandidate;
                                FoundPrincipalToDependent = true;
                            }
                        }
                    }

                    _outerExpressions.Add(node.Left.RemoveConvert());
                    _innerExpressions.Add(node.Right.RemoveConvert());

                    return node;
                }

                if (node.NodeType == ExpressionType.AndAlso)
                {
                    return base.VisitBinary(node);
                }

                return node;
            }

            private bool MatchKeyProperties(IProperty principalProperty, IProperty dependentProperty)
            {
                var keyDeclaringEntityType = principalProperty.GetContainingKeys().First().DeclaringEntityType;
                var matchingForeignKeys = dependentProperty.GetContainingForeignKeys().Where(k => k.PrincipalKey.DeclaringEntityType == keyDeclaringEntityType);

                if (matchingForeignKeys.Count() == 1)
                {
                    var matchingKey = matchingForeignKeys.Single();

                    if (principalProperty.GetContainingKeys().Contains(matchingKey.PrincipalKey))
                    {
                        var matchingForeignKey = matchingKey;

                        if (_matchingCandidate == null)
                        {
                            _matchingCandidate = matchingForeignKey;
                            _matchingCandidateProperties = new List<IProperty> { dependentProperty };
                        }
                        else if (_matchingCandidate == matchingForeignKey)
                        {
                            _matchingCandidateProperties.Add(dependentProperty);
                        }

                        if (_matchingCandidate.Properties.All(p => _matchingCandidateProperties.Contains(p)))
                        {
                            return true;
                        }
                    }
                }

                return false;
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

        private Expression LiftSubQuery(
            IQuerySource querySource,
            Expression itemsExpression,
            Expression currentExpression)
        {
            var subQueryModel = (itemsExpression as SubQueryExpression)?.QueryModel;

            if (subQueryModel == null)
            {
                return currentExpression;
            }

            var subQueryModelVisitor = QueryCompilationContext.GetQueryModelVisitor(subQueryModel);

            if (subQueryModelVisitor == null)
            {
                subQueryModelVisitor = QueryCompilationContext.CreateQueryModelVisitor(subQueryModel, this);
                subQueryModelVisitor.VisitQueryModel(subQueryModel);
            }

            var selectExpression = subQueryModelVisitor.TryGetQuery(subQueryModel.MainFromClause);

            if (selectExpression == null)
            {
                var referencedQuerySource 
                    = (subQueryModel.MainFromClause.FromExpression as QuerySourceReferenceExpression)
                        ?.ReferencedQuerySource;

                if (referencedQuerySource != null)
                {
                    selectExpression = TryGetQuery(referencedQuerySource);

                    if (selectExpression != null)
                    {
                        MapQuery(querySource, selectExpression);
                    }
                }

                return currentExpression;
            }

            MapQuery(querySource, selectExpression);
            
            // Special case needed for lifting main from clause subqueries.
            if (querySource is MainFromClause)
            {
                MapQuery(subQueryModel.MainFromClause, selectExpression);
            }

            if (selectExpression.OrderBy.Any() && selectExpression.Limit == null)
            {
                return currentExpression;
            }

            if (selectExpression.IsCorrelated() && !QueryCompilationContext.IsLateralJoinSupported)
            {
                return currentExpression;
            }

            if (subQueryModelVisitor.IsInlinable)
            {
                if (selectExpression.IsSimpleQuery())
                {
                    selectExpression.Tables.First().QuerySource = querySource;
                }
                else
                {
                    selectExpression.PushDownSubquery().QuerySource = querySource;
                    selectExpression.ExplodeStarProjection();
                }

                var updater = new QuerySourceUpdater(querySource, QueryCompilationContext, LinqOperatorProvider);

                var newExpression = updater.Visit(subQueryModelVisitor.Expression);

                var methodCallExpression = newExpression as MethodCallExpression;

                if (methodCallExpression != null
                    && methodCallExpression.Method.MethodIsClosedFormOf(LinqOperatorProvider.Select))
                {
                    var shapedQuery = (MethodCallExpression)methodCallExpression.Arguments[0];

                    var newShaper = ProjectionShaper.Create(
                        querySource,
                        (Shaper)((ConstantExpression)shapedQuery.Arguments[2]).Value,
                        ((LambdaExpression)methodCallExpression.Arguments[1]).Compile());

                    var newShapedQueryMethod
                        = shapedQuery.Method
                            .GetGenericMethodDefinition()
                            .MakeGenericMethod(newExpression.Type.GetSequenceType());

                    var newShapedQueryArguments = new[]
                    {
                        shapedQuery.Arguments[0],
                        shapedQuery.Arguments[1],
                        Expression.Constant(newShaper)
                    };

                    return Expression.Call(newShapedQueryMethod, newShapedQueryArguments);
                }

                return newExpression;
            }

            return currentExpression;
        }

        private sealed class QuerySourceUpdater : ExpressionVisitorBase
        {
            private readonly IQuerySource _querySource;
            private readonly RelationalQueryCompilationContext _relationalQueryCompilationContext;
            private readonly ILinqOperatorProvider _linqOperatorProvider;

            public QuerySourceUpdater(
                IQuerySource querySource,
                RelationalQueryCompilationContext relationalQueryCompilationContext,
                ILinqOperatorProvider linqOperatorProvider)
            {
                _querySource = querySource;
                _relationalQueryCompilationContext = relationalQueryCompilationContext;
                _linqOperatorProvider = linqOperatorProvider;
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
            var requiresClientFilter = selectExpression == null;

            var sqlTranslatingExpressionVisitor
                = _sqlTranslatingExpressionVisitorFactory.Create(
                    queryModelVisitor: this,
                    topLevelPredicate: whereClause.Predicate,
                    inProjection: false,
                    mutateProjections: true);

            // Visit the predicate expression unconditionally so that any columns that may be
            // required for client evaluation will be present in the select expression.
            var sqlPredicateExpression = sqlTranslatingExpressionVisitor.Visit(whereClause.Predicate);

            if (!requiresClientFilter)
            {
                if (sqlPredicateExpression != null)
                {
                    sqlPredicateExpression 
                        = sqlPredicateExpression.MaybeAnonymousSubquery();

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
                        mutateProjections: true);

                var orderings = new List<Ordering>();
                var previousProjectionCount = selectExpression.Projection.Count;

                foreach (var ordering in orderByClause.Orderings)
                {
                    // we disable this for order by, because you can't have a parameter (that is integer) in the order by
                    var canBindOuterParameters = CanBindOuterParameters;
                    CanBindOuterParameters = false;

                    var sqlOrderingExpression
                        = sqlTranslatingExpressionVisitor
                            .Visit(ordering.Expression)
                            .MaybeAnonymousSubquery();

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
                    var querySource
                        = typeBinaryExpression.Expression.GetRootExpression<QuerySourceReferenceExpression>()
                            ?.ReferencedQuerySource;

                    var discriminatorProperty
                        = _relationalAnnotationProvider.For(concreteEntityTypes[0]).DiscriminatorProperty;

                    var discriminatorPropertyExpression = CreatePropertyExpression(typeBinaryExpression.Expression, discriminatorProperty);

                    var discriminatorPredicate
                        = new DiscriminatorPredicateExpression(
                            concreteEntityTypes
                                .Select(concreteEntityType =>
                                    Expression.Equal(
                                        discriminatorPropertyExpression,
                                        Expression.Constant(
                                            _relationalAnnotationProvider.For(concreteEntityType).DiscriminatorValue,
                                            discriminatorPropertyExpression.Type)))
                                .Aggregate((current, next) => Expression.OrElse(next, current)),
                            querySource);

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
                    if (_queryModelVisitor.IsInlinable
                        && (_queryModelVisitor.ParentQueryModelVisitor?.CanBindOuterProperties ?? true))
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
        }

        private bool IsEnumerableRelationalTypeProperty(Expression expression)
        {
            // Need to add tests for this with JoinClause and GroupJoinClause.
            // Actually, just tests for this in general outside of the MonsterFixup tests.

            var memberExpression = expression as MemberExpression;
            var referencedQuerySource = (memberExpression?.Expression as QuerySourceReferenceExpression)?.ReferencedQuerySource;

            if (referencedQuerySource != null
                && QueryCompilationContext.Model.FindEntityType(referencedQuerySource.ItemType) != null
                && (memberExpression.Type == typeof(string) || memberExpression.Type == typeof(byte[])))
            {
                var selectExpression = QueryCompilationContext.FindSelectExpression(referencedQuerySource);

                if (selectExpression != null)
                {
                    var sqlTranslatingExpressionVisitor
                        = _sqlTranslatingExpressionVisitorFactory.Create(
                            queryModelVisitor: ParentQueryModelVisitor ?? this,
                            topLevelPredicate: null,
                            inProjection: false,
                            mutateProjections: true);

                    var sqlExpression
                        = sqlTranslatingExpressionVisitor
                            .Visit(expression);

                    if (sqlExpression != null)
                    {
                        selectExpression.AddToProjection(sqlExpression);

                        return true;
                    }
                }
            }

            return false;
        }

        #region Query Flattening

        private bool IsShapedQueryExpression(Expression expression)
        {
            var methodCallExpression = expression as MethodCallExpression;

            if (methodCallExpression == null)
            {
                return false;
            }

            var linqMethods = QueryCompilationContext.LinqOperatorProvider;

            if (methodCallExpression.Method.MethodIsClosedFormOf(linqMethods.Select) ||
                methodCallExpression.Method.MethodIsClosedFormOf(linqMethods.DefaultIfEmpty) ||
                methodCallExpression.Method.MethodIsClosedFormOf(linqMethods.DefaultIfEmptyArg))
            {
                methodCallExpression = methodCallExpression.Arguments[0] as MethodCallExpression;

                if (methodCallExpression == null)
                {
                    return false;
                }
            }

            var queryMethods = QueryCompilationContext.QueryMethodProvider;

            if (methodCallExpression.Method.MethodIsClosedFormOf(queryMethods.ShapedQueryMethod) ||
                methodCallExpression.Method.MethodIsClosedFormOf(queryMethods.DefaultIfEmptyShapedQueryMethod))
            {
                return true;
            }

            return false;
        }

        private Expression MakeInnerSequenceShapedQueryExpression(Expression innerSequenceExpression, IQuerySource querySource)
        {
            var innerShapedQuery = UnwrapShapedQueryExpression((MethodCallExpression)innerSequenceExpression);

            return Expression.Call(
                QueryCompilationContext.QueryMethodProvider.ShapedQueryMethod
                    .MakeGenericMethod(typeof(ValueBuffer)),
                innerShapedQuery.Arguments[0],
                innerShapedQuery.Arguments[1],
                Expression.Constant(new ValueBufferShaper(querySource)));
        }

        private MethodCallExpression UnwrapShapedQueryExpression(MethodCallExpression expression)
        {
            if (expression.Method.MethodIsClosedFormOf(LinqOperatorProvider.Select)
                || expression.Method.MethodIsClosedFormOf(LinqOperatorProvider.DefaultIfEmpty)
                || expression.Method.MethodIsClosedFormOf(LinqOperatorProvider.DefaultIfEmptyArg))
            {
                return (MethodCallExpression)expression.Arguments[0];
            }

            return expression;
        }

        private Shaper ExtractShaper(MethodCallExpression shapedQueryExpression, int offset)
        {
            var shaper = (Shaper)((ConstantExpression)UnwrapShapedQueryExpression(shapedQueryExpression).Arguments[2]).Value;

            var entityshaper = shaper as EntityShaper;

            if (entityshaper != null)
            {
                shaper = entityshaper.WithOffset(offset);
            }

            return shaper;
        }

        #endregion

        #region Binding

        public override Expression BindValueBufferReadExpression(
            [NotNull] ValueBufferReadExpression valueBufferReadExpression,
            int index)
        {
            Check.NotNull(valueBufferReadExpression, nameof(valueBufferReadExpression));

            if (valueBufferReadExpression.QuerySource == null)
            {
                return valueBufferReadExpression;
            }

            var selectExpression 
                = GetTargetSelectExpression(valueBufferReadExpression.QuerySource);

            if (selectExpression == null)
            {
                return valueBufferReadExpression;
            }

            var projectionIndex
                = selectExpression.GetProjectionIndex(
                    valueBufferReadExpression.Property,
                    valueBufferReadExpression.QuerySource);

            if (projectionIndex == -1)
            {
                return valueBufferReadExpression;
            }

            return base.BindValueBufferReadExpression(valueBufferReadExpression, projectionIndex);
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

        public virtual Expression BindMemberToOuterQueryParameter(
            [NotNull] MemberExpression memberExpression)
        {
            return base.BindMemberExpression<Expression>(memberExpression, (property, querySource) =>
            {
                return BindPropertyToOuterParameter(property, querySource, true);
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

        public virtual Expression BindMethodCallToOuterQueryParameter(
            [NotNull] MethodCallExpression expression)
        {
            Check.NotNull(expression, nameof(expression));

            return base.BindMethodCallExpression<Expression>(expression, (property, querySource) =>
            {
                return BindPropertyToOuterParameter(property, querySource, false);
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

        private SelectExpression GetTargetSelectExpression(IQuerySource querySource)
        {
            var selectExpression = TryGetQuery(querySource);

            if (selectExpression != null)
            {
                return selectExpression;
            }

            selectExpression = QueryCompilationContext.FindSelectExpression(querySource);

            if (selectExpression != null)
            {
                return selectExpression;
            }

            var referencedQuerySource
                = ((querySource as MainFromClause)
                    ?.FromExpression as QuerySourceReferenceExpression)
                        ?.ReferencedQuerySource;

            if (referencedQuerySource != null)
            {
                return QueryCompilationContext.FindSelectExpression(referencedQuerySource);
            }

            return null;
        }

        private const string OuterQueryParameterNamePrefix = @"_outer_";

        private ParameterExpression BindPropertyToOuterParameter(
            IProperty property,
            IQuerySource querySource,
            bool isMemberExpression)
        {
            if (CanBindOuterParameters && querySource != null && ParentQueryModelVisitor != null)
            {
                RelationalQueryModelVisitor ancestor = ParentQueryModelVisitor;
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
