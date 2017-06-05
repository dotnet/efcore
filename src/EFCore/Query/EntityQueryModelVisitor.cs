// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Extensions.Internal;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Query.ResultOperators;
using Microsoft.EntityFrameworkCore.Query.ResultOperators.Internal;
using Microsoft.EntityFrameworkCore.Utilities;
using Remotion.Linq;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Clauses.ExpressionVisitors;
using Remotion.Linq.Clauses.ResultOperators;
using Remotion.Linq.Clauses.StreamedData;
using Remotion.Linq.Parsing;

namespace Microsoft.EntityFrameworkCore.Query
{
    /// <summary>
    ///     <para>
    ///         The core visitor that processes a query to be executed.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    public abstract class EntityQueryModelVisitor : QueryModelVisitorBase
    {
        /// <summary>
        ///     Expression to reference the <see cref="QueryContext" /> parameter for a query.
        /// </summary>
        public static readonly ParameterExpression QueryContextParameter
            = Expression.Parameter(typeof(QueryContext), "queryContext");

        private readonly IQueryOptimizer _queryOptimizer;
        private readonly INavigationRewritingExpressionVisitorFactory _navigationRewritingExpressionVisitorFactory;
        private readonly IQuerySourceTracingExpressionVisitorFactory _querySourceTracingExpressionVisitorFactory;
        private readonly IEntityResultFindingExpressionVisitorFactory _entityResultFindingExpressionVisitorFactory;
        private readonly ITaskBlockingExpressionVisitor _taskBlockingExpressionVisitor;
        private readonly IMemberAccessBindingExpressionVisitorFactory _memberAccessBindingExpressionVisitorFactory;
        private readonly IProjectionExpressionVisitorFactory _projectionExpressionVisitorFactory;
        private readonly IEntityQueryableExpressionVisitorFactory _entityQueryableExpressionVisitorFactory;
        private readonly IQueryAnnotationExtractor _queryAnnotationExtractor;
        private readonly IResultOperatorHandler _resultOperatorHandler;
        private readonly IEntityMaterializerSource _entityMaterializerSource;
        private readonly IExpressionPrinter _expressionPrinter;

        private readonly QueryCompilationContext _queryCompilationContext;

        private readonly FilterApplyingExpressionVisitor _filterApplyingExpressionVisitor;

        private Expression _expression;
        private ParameterExpression _currentParameter;

        private int _transparentParameterCounter;

        // TODO: Can these be non-blocking?
        private bool _blockTaskExpressions = true;

        /// <summary>
        ///     Initializes a new instance of the <see cref="EntityQueryModelVisitor" /> class.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this service. </param>
        /// <param name="queryCompilationContext"> The <see cref="QueryCompilationContext" /> to be used when processing the query. </param>
        protected EntityQueryModelVisitor(
            [NotNull] EntityQueryModelVisitorDependencies dependencies,
            [NotNull] QueryCompilationContext queryCompilationContext)
        {
            Check.NotNull(dependencies, nameof(dependencies));
            Check.NotNull(queryCompilationContext, nameof(queryCompilationContext));

            _queryOptimizer = dependencies.QueryOptimizer;
            _navigationRewritingExpressionVisitorFactory = dependencies.NavigationRewritingExpressionVisitorFactory;
            _querySourceTracingExpressionVisitorFactory = dependencies.QuerySourceTracingExpressionVisitorFactory;
            _entityResultFindingExpressionVisitorFactory = dependencies.EntityResultFindingExpressionVisitorFactory;
            _taskBlockingExpressionVisitor = dependencies.TaskBlockingExpressionVisitor;
            _memberAccessBindingExpressionVisitorFactory = dependencies.MemberAccessBindingExpressionVisitorFactory;
            _projectionExpressionVisitorFactory = dependencies.ProjectionExpressionVisitorFactory;
            _entityQueryableExpressionVisitorFactory = dependencies.EntityQueryableExpressionVisitorFactory;
            _queryAnnotationExtractor = dependencies.QueryAnnotationExtractor;
            _resultOperatorHandler = dependencies.ResultOperatorHandler;
            _entityMaterializerSource = dependencies.EntityMaterializerSource;
            _expressionPrinter = dependencies.ExpressionPrinter;

            _queryCompilationContext = queryCompilationContext;

            LinqOperatorProvider = queryCompilationContext.LinqOperatorProvider;

            _filterApplyingExpressionVisitor = new FilterApplyingExpressionVisitor(_queryCompilationContext);
        }

        /// <summary>
        ///     Gets the expression that represents this query.
        /// </summary>
        public virtual Expression Expression
        {
            get { return _expression; }
            [param: NotNull]
            set
            {
                Check.NotNull(value, nameof(value));

                _expression = value;
            }
        }

        /// <summary>
        ///     Gets the expression for the current parameter.
        /// </summary>
        public virtual ParameterExpression CurrentParameter
        {
            get { return _currentParameter; }
            [param: NotNull]
            set
            {
                Check.NotNull(value, nameof(value));

                _currentParameter = value;
            }
        }

        /// <summary>
        ///     Gets the <see cref="Query.QueryCompilationContext" /> being used for this query.
        /// </summary>
        public virtual QueryCompilationContext QueryCompilationContext => _queryCompilationContext;

        /// <summary>
        ///     Gets the <see cref="ILinqOperatorProvider" /> being used for this query.
        /// </summary>
        public virtual ILinqOperatorProvider LinqOperatorProvider { get; private set; }

        /// <summary>
        ///     Creates an action to execute this query.
        /// </summary>
        /// <typeparam name="TResult"> The type of results that the query returns. </typeparam>
        /// <param name="queryModel"> The query. </param>
        /// <returns> An action that returns the results of the query. </returns>
        public virtual Func<QueryContext, IEnumerable<TResult>> CreateQueryExecutor<TResult>([NotNull] QueryModel queryModel)
        {
            Check.NotNull(queryModel, nameof(queryModel));

            using (QueryCompilationContext.Logger.Logger.BeginScope(this))
            {
                QueryCompilationContext.Logger.QueryModelCompiling(queryModel);

                _blockTaskExpressions = false;

                ExtractQueryAnnotations(queryModel);

                OptimizeQueryModel(queryModel, asyncQuery: false);

                QueryCompilationContext.FindQuerySourcesRequiringMaterialization(this, queryModel);
                QueryCompilationContext.DetermineQueryBufferRequirement(queryModel);

                VisitQueryModel(queryModel);

                SingleResultToSequence(queryModel);

                TrackEntitiesInResults<TResult>(queryModel);
                
                InterceptExceptions();

                return CreateExecutorLambda<IEnumerable<TResult>>();
            }
        }

        /// <summary>
        ///     Creates an action to asynchronously execute this query.
        /// </summary>
        /// <typeparam name="TResult"> The type of results that the query returns. </typeparam>
        /// <param name="queryModel"> The query. </param>
        /// <returns> An action that asynchronously returns the results of the query. </returns>
        public virtual Func<QueryContext, IAsyncEnumerable<TResult>> CreateAsyncQueryExecutor<TResult>(
            [NotNull] QueryModel queryModel)
        {
            Check.NotNull(queryModel, nameof(queryModel));

            using (QueryCompilationContext.Logger.Logger.BeginScope(this))
            {
                QueryCompilationContext.Logger.QueryModelCompiling(queryModel);

                _blockTaskExpressions = false;

                ExtractQueryAnnotations(queryModel);

                OptimizeQueryModel(queryModel, asyncQuery: true);

                QueryCompilationContext.FindQuerySourcesRequiringMaterialization(this, queryModel);
                QueryCompilationContext.DetermineQueryBufferRequirement(queryModel);

                VisitQueryModel(queryModel);

                SingleResultToSequence(queryModel, _expression.Type.GetTypeInfo().GenericTypeArguments[0]);

                TrackEntitiesInResults<TResult>(queryModel);

                InterceptExceptions();

                return CreateExecutorLambda<IAsyncEnumerable<TResult>>();
            }
        }

        /// <summary>
        ///     Executes the query and logs any exceptions that occur.
        /// </summary>
        protected virtual void InterceptExceptions()
            => _expression
                = Expression.Call(
                    LinqOperatorProvider.InterceptExceptions
                        .MakeGenericMethod(_expression.Type.GetSequenceType()),
                    _expression,
                    Expression.Constant(QueryCompilationContext.ContextType),
                    Expression.Constant(QueryCompilationContext.Logger),
                    QueryContextParameter);

        /// <summary>
        /// Rewrites collection navigation projections so that they can be handled by the Include pipeline.
        /// </summary>
        /// <param name="queryModel"> The query. </param>
        protected virtual void RewriteProjectedCollectionNavigationsToIncludes([NotNull] QueryModel queryModel)
        {
            Check.NotNull(queryModel, nameof(queryModel));

            var collectionNavigationIncludeRewriter = new CollectionNavigationIncludeExpressionRewriter(this);
            queryModel.SelectClause.Selector = collectionNavigationIncludeRewriter.Visit(queryModel.SelectClause.Selector);
            _queryCompilationContext.AddAnnotations(collectionNavigationIncludeRewriter.CollectionNavigationIncludeResultOperators);
        }

        /// <summary>
        ///     Populates <see cref="Query.QueryCompilationContext.QueryAnnotations" /> based on annotations found in the query.
        /// </summary>
        /// <param name="queryModel"> The query. </param>
        protected virtual void ExtractQueryAnnotations([NotNull] QueryModel queryModel)
        {
            Check.NotNull(queryModel, nameof(queryModel));

            QueryCompilationContext.QueryAnnotations
                = _queryAnnotationExtractor.ExtractQueryAnnotations(queryModel);
        }

        /// <summary>
        ///     Applies optimizations to the query.
        /// </summary>
        /// <param name="queryModel"> The query. </param>
        /// <param name="asyncQuery">True if we are compiling an async query; otherwise false.</param>
        protected virtual void OptimizeQueryModel(
            [NotNull] QueryModel queryModel,
            bool asyncQuery)
        {
            Check.NotNull(queryModel, nameof(queryModel));

            // First pass of optimizations

            _queryOptimizer.Optimize(QueryCompilationContext, queryModel);

            new NondeterministicResultCheckingVisitor(QueryCompilationContext.Logger).VisitQueryModel(queryModel);

            // Rewrite includes/navigations

            RewriteProjectedCollectionNavigationsToIncludes(queryModel);

            var includeCompiler = new IncludeCompiler(QueryCompilationContext, _querySourceTracingExpressionVisitorFactory);

            includeCompiler.CompileIncludes(queryModel, TrackResults(queryModel), asyncQuery);

            queryModel.TransformExpressions(new CollectionNavigationSubqueryInjector(this).Visit);

            var navigationRewritingExpressionVisitor = _navigationRewritingExpressionVisitorFactory.Create(this);

            navigationRewritingExpressionVisitor.Rewrite(queryModel, parentQueryModel: null);

            includeCompiler.CompileIncludes(queryModel, TrackResults(queryModel), asyncQuery);

            navigationRewritingExpressionVisitor.Rewrite(queryModel, parentQueryModel: null);

            includeCompiler.RewriteCollectionQueries(queryModel);

            includeCompiler.LogIgnoredIncludes();

            if (!_queryCompilationContext.IgnoreQueryFilters)
            {
                queryModel.TransformExpressions(_filterApplyingExpressionVisitor.Visit);
            }

            // Second pass of optimizations

            _queryOptimizer.Optimize(QueryCompilationContext, queryModel);

            // Log results

            QueryCompilationContext.Logger.QueryModelOptimized(queryModel);
        }

        private class NondeterministicResultCheckingVisitor : QueryModelVisitorBase
        {
            private readonly IDiagnosticsLogger<DbLoggerCategory.Query> _logger;

            public NondeterministicResultCheckingVisitor([NotNull] IDiagnosticsLogger<DbLoggerCategory.Query> logger)
                => _logger = logger;

            public override void VisitQueryModel(QueryModel queryModel)
            {
                queryModel.TransformExpressions(new TransformingQueryModelExpressionVisitor<NondeterministicResultCheckingVisitor>(this).Visit);

                base.VisitQueryModel(queryModel);
            }

            protected override void VisitResultOperators(ObservableCollection<ResultOperatorBase> resultOperators, QueryModel queryModel)
            {
                if (resultOperators.Any(o => o is SkipResultOperator || o is TakeResultOperator)
                    && !queryModel.BodyClauses.OfType<OrderByClause>().Any())
                {
                    _logger.RowLimitingOperationWithoutOrderByWarning(queryModel);
                }

                if (resultOperators.Any(o => o is FirstResultOperator)
                    && !queryModel.BodyClauses.OfType<OrderByClause>().Any()
                    && !queryModel.BodyClauses.OfType<WhereClause>().Any())
                {
                    _logger.FirstWithoutOrderByAndFilterWarning(queryModel);
                }

                base.VisitResultOperators(resultOperators, queryModel);
            }
        }

        /// <summary>
        ///     Converts the results of the query from a single result to a series of results.
        /// </summary>
        /// <param name="queryModel"> The query. </param>
        /// <param name="type"> The type of results returned by the query. </param>
        protected virtual void SingleResultToSequence([NotNull] QueryModel queryModel, [CanBeNull] Type type = null)
        {
            Check.NotNull(queryModel, nameof(queryModel));

            var streamedDataInfo = queryModel.GetOutputDataInfo();

            if (!(streamedDataInfo is StreamedSequenceInfo)
                && _expression.Type.TryGetSequenceType() != streamedDataInfo.DataType)
            {
                _expression
                    = Expression.Call(
                        LinqOperatorProvider.ToSequence
                            .MakeGenericMethod(type ?? _expression.Type),
                        _expression);
            }
        }

        /// <summary>
        ///     Applies tracking behavior to the query.
        /// </summary>
        /// <typeparam name="TResult"> The type of results returned by the query. </typeparam>
        /// <param name="queryModel"> The query. </param>
        protected virtual void TrackEntitiesInResults<TResult>([NotNull] QueryModel queryModel)
        {
            Check.NotNull(queryModel, nameof(queryModel));

            if (!TrackResults(queryModel))
            {
                return;
            }

            var outputExpression
                = new IncludeRemovingExpressionVisitor()
                    .Visit(queryModel.SelectClause.Selector);

            var resultItemType = _expression.Type.GetSequenceType();
            var isGrouping = resultItemType.IsGrouping();

            if (isGrouping)
            {
                var groupResultOperator
                    = queryModel.ResultOperators.OfType<GroupResultOperator>().LastOrDefault();

                if (groupResultOperator != null)
                {
                    outputExpression = groupResultOperator.ElementSelector;
                }
                else
                {
                    var subqueryExpression
                        = (queryModel.SelectClause.Selector
                            .TryGetReferencedQuerySource() as MainFromClause)?.FromExpression as SubQueryExpression;

                    var nestedGroupResultOperator
                        = subqueryExpression?.QueryModel?.ResultOperators
                            ?.OfType<GroupResultOperator>()
                            .LastOrDefault();

                    if (nestedGroupResultOperator != null)
                    {
                        outputExpression = nestedGroupResultOperator.ElementSelector;
                    }
                }
            }

            var entityTrackingInfos
                = _entityResultFindingExpressionVisitorFactory
                    .Create(QueryCompilationContext)
                    .FindEntitiesInResult(outputExpression);

            if (entityTrackingInfos.Any())
            {
                MethodInfo trackingMethod;

                if (isGrouping)

                {
                    trackingMethod
                        = LinqOperatorProvider.TrackGroupedEntities
                            .MakeGenericMethod(
                                resultItemType.GenericTypeArguments[0],
                                resultItemType.GenericTypeArguments[1]);
                }
                else
                {
                    trackingMethod
                        = LinqOperatorProvider.TrackEntities
                            .MakeGenericMethod(
                                resultItemType,
                                outputExpression.Type);
                }

                _expression
                    = Expression.Call(
                        trackingMethod,
                        _expression,
                        QueryContextParameter,
                        Expression.Constant(entityTrackingInfos),
                        Expression.Constant(
                            _getEntityAccessors
                                .MakeGenericMethod(outputExpression.Type)
                                .Invoke(
                                    null,
                                    new object[]
                                    {
                                        entityTrackingInfos,
                                        outputExpression
                                    })));
            }
        }

        private class IncludeRemovingExpressionVisitor : RelinqExpressionVisitor
        {
            protected override Expression VisitMethodCall(MethodCallExpression node)
                => IncludeCompiler.IsIncludeMethod(node)
                    ? node.Arguments[1]
                    : base.VisitMethodCall(node);

            protected override Expression VisitMember(MemberExpression node)
            {
                var newExpression = Visit(node.Expression);

                return newExpression != node.Expression ? newExpression : node;
            }
        }

        private bool TrackResults(QueryModel queryModel)
        {
            // TODO: Unify with QCC

            var lastTrackingModifier
                = QueryCompilationContext.QueryAnnotations
                    .OfType<TrackingResultOperator>()
                    .LastOrDefault();

            return !(queryModel.GetOutputDataInfo() is StreamedScalarValueInfo)
                   && (QueryCompilationContext.TrackQueryResults || lastTrackingModifier != null)
                   && (lastTrackingModifier == null
                       || lastTrackingModifier.IsTracking);
        }

        private static readonly MethodInfo _getEntityAccessors
            = typeof(EntityQueryModelVisitor)
                .GetTypeInfo()
                .GetDeclaredMethod(nameof(GetEntityAccessors));

        [UsedImplicitly]
        private static ICollection<Func<TResult, object>> GetEntityAccessors<TResult>(
            IEnumerable<EntityTrackingInfo> entityTrackingInfos,
            Expression selector)
            => (from entityTrackingInfo in entityTrackingInfos
                select
                (Func<TResult, object>)
                AccessorFindingExpressionVisitor
                    .FindAccessorLambda(
                        entityTrackingInfo.QuerySourceReferenceExpression,
                        selector,
                        Expression.Parameter(typeof(TResult), "result"))
                    .Compile())
                .ToList();

        /// <summary>
        ///     Creates an action to execute this query.
        /// </summary>
        /// <typeparam name="TResults"> The type of results that the query returns. </typeparam>
        /// <returns> An action that returns the results of the query. </returns>
        /// >
        protected virtual Func<QueryContext, TResults> CreateExecutorLambda<TResults>()
        {
            var expression = _expression;

            var setFilterParameterExpressions 
                = CreateSetFilterParametersExpressions(out var contextVariableExpression);

            if (setFilterParameterExpressions != null)
            {
                expression
                    = Expression.Block(
                        new [] { contextVariableExpression },
                        setFilterParameterExpressions.Concat(new[] { expression }));
            }

            var queryExecutorExpression
                = Expression
                    .Lambda<Func<QueryContext, TResults>>(
                        expression, 
                        QueryContextParameter);

            try
            {
                return queryExecutorExpression.Compile();
            }
            finally
            {
                QueryCompilationContext.Logger.QueryExecutionPlanned(_expressionPrinter, queryExecutorExpression);
            }
        }

        private static readonly MethodInfo _queryContextAddParameterMethodInfo
            = typeof(QueryContext)
                .GetTypeInfo()
                .GetDeclaredMethod(nameof(QueryContext.AddParameter));

        private static readonly PropertyInfo _queryContextContextPropertyInfo
            = typeof(QueryContext)
                .GetTypeInfo()
                .GetDeclaredProperty(nameof(QueryContext.Context));

        private IEnumerable<Expression> CreateSetFilterParametersExpressions(
            out ParameterExpression contextVariableExpression)
        {
            contextVariableExpression = null;

            if (_filterApplyingExpressionVisitor.ContextParameters.Count == 0)
            {
                return null;
            }

            contextVariableExpression
                = Expression.Variable(_queryCompilationContext.ContextType, "context");

            var blockExpressions
                = new List<Expression>
                {
                    Expression.Assign(
                        contextVariableExpression,
                        Expression.Convert(
                            Expression.Property(
                                QueryContextParameter,
                                _queryContextContextPropertyInfo),
                            _queryCompilationContext.ContextType))
                };

            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var keyValuePair in _filterApplyingExpressionVisitor.ContextParameters)
            {
                blockExpressions.Add(
                    Expression.Call(
                        QueryContextParameter,
                        _queryContextAddParameterMethodInfo,
                        Expression.Constant(keyValuePair.Key),
                        Expression.Convert(
                            Expression.Invoke(
                                (LambdaExpression)keyValuePair.Value,
                                contextVariableExpression),
                            typeof(object))));
            }

            return blockExpressions;
        }

        /// <summary>
        ///     Visits the root <see cref="QueryModel" /> node.
        /// </summary>
        /// <param name="queryModel"> The query. </param>
        public override void VisitQueryModel([NotNull] QueryModel queryModel)
        {
            Check.NotNull(queryModel, nameof(queryModel));

            base.VisitQueryModel(queryModel);

            if (_blockTaskExpressions)
            {
                _expression = _taskBlockingExpressionVisitor.Visit(_expression);
            }
        }

        /// <summary>
        ///     Visits the <see cref="MainFromClause" /> node.
        /// </summary>
        /// <param name="fromClause"> The node being visited. </param>
        /// <param name="queryModel"> The query. </param>
        public override void VisitMainFromClause(
            [NotNull] MainFromClause fromClause,
            [NotNull] QueryModel queryModel)
        {
            Check.NotNull(fromClause, nameof(fromClause));
            Check.NotNull(queryModel, nameof(queryModel));

            _expression = CompileMainFromClauseExpression(fromClause, queryModel);

            if (LinqOperatorProvider is AsyncLinqOperatorProvider
                && _expression.Type.TryGetElementType(typeof(IEnumerable<>)) != null)
            {
                LinqOperatorProvider = new LinqOperatorProvider();
            }

            CurrentParameter
                = Expression.Parameter(
                    _expression.Type.GetSequenceType(),
                    fromClause.ItemName);

            QueryCompilationContext.AddOrUpdateMapping(fromClause, CurrentParameter);
        }

        /// <summary>
        ///     Compiles the <see cref="MainFromClause" /> node.
        /// </summary>
        /// <param name="mainFromClause"> The node being compiled. </param>
        /// <param name="queryModel"> The query. </param>
        /// <returns> The compiled result. </returns>
        protected virtual Expression CompileMainFromClauseExpression(
            [NotNull] MainFromClause mainFromClause,
            [NotNull] QueryModel queryModel)
        {
            Check.NotNull(mainFromClause, nameof(mainFromClause));
            Check.NotNull(queryModel, nameof(queryModel));

            return ReplaceClauseReferences(mainFromClause.FromExpression, mainFromClause);
        }

        /// <summary>
        ///     Visits <see cref="AdditionalFromClause" /> nodes.
        /// </summary>
        /// <param name="fromClause"> The node being visited. </param>
        /// <param name="queryModel"> The query. </param>
        /// <param name="index"> Index of the node being visited. </param>
        public override void VisitAdditionalFromClause(
            [NotNull] AdditionalFromClause fromClause,
            [NotNull] QueryModel queryModel,
            int index)
        {
            Check.NotNull(fromClause, nameof(fromClause));
            Check.NotNull(queryModel, nameof(queryModel));

            var fromExpression
                = CompileAdditionalFromClauseExpression(fromClause, queryModel);

            var innerItemParameter
                = Expression.Parameter(
                    fromExpression.Type.GetSequenceType(), fromClause.ItemName);

            var transparentIdentifierType
                = typeof(TransparentIdentifier<,>)
                    .MakeGenericType(CurrentParameter.Type, innerItemParameter.Type);

            _expression
                = Expression.Call(
                    LinqOperatorProvider.SelectMany
                        .MakeGenericMethod(
                            CurrentParameter.Type,
                            innerItemParameter.Type,
                            transparentIdentifierType),
                    _expression,
                    Expression.Lambda(fromExpression, CurrentParameter),
                    Expression.Lambda(
                        CallCreateTransparentIdentifier(
                            transparentIdentifierType, CurrentParameter, innerItemParameter),
                        CurrentParameter,
                        innerItemParameter));

            IntroduceTransparentScope(fromClause, queryModel, index, transparentIdentifierType);
        }

        /// <summary>
        ///     Compiles <see cref="AdditionalFromClause" /> nodes.
        /// </summary>
        /// <param name="additionalFromClause"> The node being compiled. </param>
        /// <param name="queryModel"> The query. </param>
        /// <returns> The compiled result. </returns>
        protected virtual Expression CompileAdditionalFromClauseExpression(
            [NotNull] AdditionalFromClause additionalFromClause,
            [NotNull] QueryModel queryModel)
        {
            Check.NotNull(additionalFromClause, nameof(additionalFromClause));
            Check.NotNull(queryModel, nameof(queryModel));

            return ReplaceClauseReferences(additionalFromClause.FromExpression, additionalFromClause);
        }

        /// <summary>
        ///     Visits <see cref="JoinClause" /> nodes.
        /// </summary>
        /// <param name="joinClause"> The node being visited. </param>
        /// <param name="queryModel"> The query. </param>
        /// <param name="index"> Index of the node being visited. </param>
        public override void VisitJoinClause(
            [NotNull] JoinClause joinClause,
            [NotNull] QueryModel queryModel,
            int index)
        {
            Check.NotNull(joinClause, nameof(joinClause));
            Check.NotNull(queryModel, nameof(queryModel));

            var outerKeySelectorExpression
                = ReplaceClauseReferences(joinClause.OuterKeySelector, joinClause);

            var innerSequenceExpression
                = CompileJoinClauseInnerSequenceExpression(joinClause, queryModel);

            var innerItemParameter
                = Expression.Parameter(
                    innerSequenceExpression.Type.GetSequenceType(), joinClause.ItemName);

            QueryCompilationContext.AddOrUpdateMapping(joinClause, innerItemParameter);

            var innerKeySelectorExpression
                = ReplaceClauseReferences(joinClause.InnerKeySelector, joinClause);

            var transparentIdentifierType
                = typeof(TransparentIdentifier<,>)
                    .MakeGenericType(CurrentParameter.Type, innerItemParameter.Type);

            _expression
                = Expression.Call(
                    LinqOperatorProvider.Join
                        .MakeGenericMethod(
                            CurrentParameter.Type,
                            innerItemParameter.Type,
                            outerKeySelectorExpression.Type,
                            transparentIdentifierType),
                    _expression,
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
        }

        /// <summary>
        ///     Compiles <see cref="JoinClause" /> nodes.
        /// </summary>
        /// <param name="joinClause"> The node being compiled. </param>
        /// <param name="queryModel"> The query. </param>
        /// <returns> The compiled result. </returns>
        protected virtual Expression CompileJoinClauseInnerSequenceExpression(
            [NotNull] JoinClause joinClause,
            [NotNull] QueryModel queryModel)
        {
            Check.NotNull(joinClause, nameof(joinClause));
            Check.NotNull(queryModel, nameof(queryModel));

            return ReplaceClauseReferences(joinClause.InnerSequence, joinClause);
        }

        /// <summary>
        ///     Visits <see cref="GroupJoinClause" /> nodes
        /// </summary>
        /// <param name="groupJoinClause"> The node being visited. </param>
        /// <param name="queryModel"> The query. </param>
        /// <param name="index"> Index of the node being visited. </param>
        public override void VisitGroupJoinClause(
            [NotNull] GroupJoinClause groupJoinClause,
            [NotNull] QueryModel queryModel,
            int index)
        {
            Check.NotNull(groupJoinClause, nameof(groupJoinClause));
            Check.NotNull(queryModel, nameof(queryModel));

            var outerKeySelectorExpression
                = ReplaceClauseReferences(groupJoinClause.JoinClause.OuterKeySelector, groupJoinClause);

            var innerSequenceExpression
                = CompileGroupJoinInnerSequenceExpression(groupJoinClause, queryModel);

            var innerItemParameter
                = Expression.Parameter(
                    innerSequenceExpression.Type.GetSequenceType(),
                    groupJoinClause.JoinClause.ItemName);

            QueryCompilationContext.AddOrUpdateMapping(groupJoinClause.JoinClause, innerItemParameter);

            var innerKeySelectorExpression
                = ReplaceClauseReferences(groupJoinClause.JoinClause.InnerKeySelector, groupJoinClause);

            var innerItemsParameter
                = Expression.Parameter(
                    LinqOperatorProvider.MakeSequenceType(innerItemParameter.Type),
                    groupJoinClause.ItemName);

            var transparentIdentifierType
                = typeof(TransparentIdentifier<,>)
                    .MakeGenericType(CurrentParameter.Type, innerItemsParameter.Type);

            _expression
                = Expression.Call(
                    LinqOperatorProvider.GroupJoin
                        .MakeGenericMethod(
                            CurrentParameter.Type,
                            innerItemParameter.Type,
                            outerKeySelectorExpression.Type,
                            transparentIdentifierType),
                    _expression,
                    innerSequenceExpression,
                    Expression.Lambda(outerKeySelectorExpression, CurrentParameter),
                    Expression.Lambda(innerKeySelectorExpression, innerItemParameter),
                    Expression.Lambda(
                        CallCreateTransparentIdentifier(
                            transparentIdentifierType,
                            CurrentParameter,
                            innerItemsParameter),
                        CurrentParameter,
                        innerItemsParameter));

            IntroduceTransparentScope(groupJoinClause, queryModel, index, transparentIdentifierType);
        }

        /// <summary>
        ///     Compiles <see cref="GroupJoinClause" /> nodes.
        /// </summary>
        /// <param name="groupJoinClause"> The node being compiled. </param>
        /// <param name="queryModel"> The query. </param>
        /// <returns> The compiled result. </returns>
        protected virtual Expression CompileGroupJoinInnerSequenceExpression(
            [NotNull] GroupJoinClause groupJoinClause,
            [NotNull] QueryModel queryModel)
        {
            Check.NotNull(groupJoinClause, nameof(groupJoinClause));
            Check.NotNull(queryModel, nameof(queryModel));

            return ReplaceClauseReferences(groupJoinClause.JoinClause.InnerSequence, groupJoinClause.JoinClause);
        }

        /// <summary>
        ///     Visits <see cref="WhereClause" /> nodes.
        /// </summary>
        /// <param name="whereClause"> The node being visited. </param>
        /// <param name="queryModel"> The query. </param>
        /// <param name="index"> Index of the node being visited. </param>
        public override void VisitWhereClause(
            [NotNull] WhereClause whereClause,
            [NotNull] QueryModel queryModel,
            int index)
        {
            Check.NotNull(whereClause, nameof(whereClause));
            Check.NotNull(queryModel, nameof(queryModel));

            var predicate = ReplaceClauseReferences(whereClause.Predicate);

            _expression
                = Expression.Call(
                    LinqOperatorProvider.Where.MakeGenericMethod(CurrentParameter.Type),
                    _expression,
                    Expression.Lambda(predicate, CurrentParameter));
        }

        /// <summary>
        ///     Visits <see cref="Ordering" /> nodes.
        /// </summary>
        /// <param name="ordering"> The node being visited. </param>
        /// <param name="queryModel"> The query. </param>
        /// <param name="orderByClause"> The <see cref="OrderByClause" /> for the ordering. </param>
        /// <param name="index"> Index of the node being visited. </param>
        public override void VisitOrdering(
            [NotNull] Ordering ordering,
            [NotNull] QueryModel queryModel,
            [NotNull] OrderByClause orderByClause,
            int index)
        {
            Check.NotNull(ordering, nameof(ordering));
            Check.NotNull(queryModel, nameof(queryModel));
            Check.NotNull(orderByClause, nameof(orderByClause));

            var expression = ReplaceClauseReferences(ordering.Expression);

            _expression
                = Expression.Call(
                    (index == 0
                        ? LinqOperatorProvider.OrderBy
                        : LinqOperatorProvider.ThenBy)
                    .MakeGenericMethod(CurrentParameter.Type, expression.Type),
                    _expression,
                    Expression.Lambda(expression, CurrentParameter),
                    Expression.Constant(ordering.OrderingDirection));
        }

        /// <summary>
        ///     Visits <see cref="SelectClause" /> nodes.
        /// </summary>
        /// <param name="selectClause"> The node being visited. </param>
        /// <param name="queryModel"> The query. </param>
        public override void VisitSelectClause(
            [NotNull] SelectClause selectClause,
            [NotNull] QueryModel queryModel)
        {
            Check.NotNull(selectClause, nameof(selectClause));
            Check.NotNull(queryModel, nameof(queryModel));

            var sequenceType = _expression.Type.GetSequenceType();

            if (selectClause.Selector.Type == sequenceType
                && selectClause.Selector is QuerySourceReferenceExpression)
            {
                return;
            }

            var selector
                = ReplaceClauseReferences(
                    _projectionExpressionVisitorFactory
                        .Create(this, queryModel.MainFromClause)
                        .Visit(selectClause.Selector),
                    inProjection: true);

            if ((selector.Type != sequenceType
                 || !(selectClause.Selector is QuerySourceReferenceExpression))
                && !queryModel.ResultOperators
                    .Select(ro => ro.GetType())
                    .Any(
                        t => t == typeof(GroupResultOperator)
                             || t == typeof(AllResultOperator)))
            {
                var asyncSelector = selector;
                var taskLiftingExpressionVisitor = new TaskLiftingExpressionVisitor();

                if (_expression.Type.TryGetElementType(typeof(IAsyncEnumerable<>)) != null)
                {
                    asyncSelector = taskLiftingExpressionVisitor.LiftTasks(selector);
                }

                _expression
                    = asyncSelector == selector
                        ? Expression.Call(
                            LinqOperatorProvider.Select
                                .MakeGenericMethod(CurrentParameter.Type, selector.Type),
                            _expression,
                            Expression.Lambda(selector, CurrentParameter))
                        : Expression.Call(
                            _selectAsync
                                .MakeGenericMethod(CurrentParameter.Type, selector.Type),
                            _expression,
                            Expression.Lambda(
                                asyncSelector,
                                CurrentParameter,
                                taskLiftingExpressionVisitor.CancellationTokenParameter
                                    ?? Expression.Parameter(typeof(CancellationToken), name: "ct")));
            }
        }

        private static readonly MethodInfo _selectAsync
            = typeof(EntityQueryModelVisitor)
                .GetTypeInfo()
                .GetDeclaredMethod(nameof(_SelectAsync));

        // ReSharper disable once InconsistentNaming
        private static IAsyncEnumerable<TResult> _SelectAsync<TSource, TResult>(
            IAsyncEnumerable<TSource> source,
            Func<TSource, CancellationToken, Task<TResult>> selector)
            => new AsyncSelectEnumerable<TSource, TResult>(source, selector);

        private class AsyncSelectEnumerable<TSource, TResult> : IAsyncEnumerable<TResult>
        {
            private readonly IAsyncEnumerable<TSource> _source;
            private readonly Func<TSource, CancellationToken, Task<TResult>> _selector;

            public AsyncSelectEnumerable(
                IAsyncEnumerable<TSource> source,
                Func<TSource, CancellationToken, Task<TResult>> selector)
            {
                _source = source;
                _selector = selector;
            }

            public IAsyncEnumerator<TResult> GetEnumerator() => new AsyncSelectEnumerator(this);

            private class AsyncSelectEnumerator : IAsyncEnumerator<TResult>
            {
                private readonly IAsyncEnumerator<TSource> _enumerator;
                private readonly Func<TSource, CancellationToken, Task<TResult>> _selector;

                public AsyncSelectEnumerator(AsyncSelectEnumerable<TSource, TResult> enumerable)
                {
                    _enumerator = enumerable._source.GetEnumerator();
                    _selector = enumerable._selector;
                }

                public async Task<bool> MoveNext(CancellationToken cancellationToken)
                {
                    if (!await _enumerator.MoveNext(cancellationToken))
                    {
                        return false;
                    }

                    Current = await _selector(_enumerator.Current, cancellationToken);

                    return true;
                }

                public TResult Current { get; private set; }

                public void Dispose() => _enumerator.Dispose();
            }
        }

        /// <summary>
        ///     Visits <see cref="ResultOperatorBase" /> nodes.
        /// </summary>
        /// <param name="resultOperator"> The node being visited. </param>
        /// <param name="queryModel"> The query. </param>
        /// <param name="index"> Index of the node being visited. </param>
        public override void VisitResultOperator(
            [NotNull] ResultOperatorBase resultOperator,
            [NotNull] QueryModel queryModel,
            int index)
        {
            Check.NotNull(resultOperator, nameof(resultOperator));
            Check.NotNull(queryModel, nameof(queryModel));

            _expression
                = _resultOperatorHandler
                    .HandleResultOperator(this, resultOperator, queryModel);
        }

        #region Transparent Identifiers

        private const string CreateTransparentIdentifierMethodName = "CreateTransparentIdentifier";

        private struct TransparentIdentifier<TOuter, TInner>
        {
            [UsedImplicitly]
            public static TransparentIdentifier<TOuter, TInner> CreateTransparentIdentifier(TOuter outer, TInner inner)
                => new TransparentIdentifier<TOuter, TInner>(outer, inner);

            private TransparentIdentifier(TOuter outer, TInner inner)
            {
                Outer = outer;
                Inner = inner;
            }

            [UsedImplicitly]
            public TOuter Outer;

            [UsedImplicitly]
            public TInner Inner;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual Type CreateTransparentIdentifierType([NotNull] Type outerType, [NotNull] Type innerType)
            => typeof(TransparentIdentifier<,>).MakeGenericType(
                Check.NotNull(outerType, nameof(outerType)),
                Check.NotNull(innerType, nameof(innerType)));

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual Expression CallCreateTransparentIdentifier(
            [NotNull] Type transparentIdentifierType,
            [NotNull] Expression outerExpression,
            [NotNull] Expression innerExpression)
        {
            Check.NotNull(transparentIdentifierType, nameof(transparentIdentifierType));
            Check.NotNull(outerExpression, nameof(outerExpression));
            Check.NotNull(innerExpression, nameof(innerExpression));

            var createTransparentIdentifierMethodInfo
                = transparentIdentifierType.GetTypeInfo().GetDeclaredMethod(CreateTransparentIdentifierMethodName);

            return Expression.Call(createTransparentIdentifierMethodInfo, outerExpression, innerExpression);
        }

        private static Expression AccessOuterTransparentField(
            Type transparentIdentifierType,
            Expression targetExpression)
        {
            var fieldInfo = transparentIdentifierType.GetTypeInfo().GetDeclaredField("Outer");

            return Expression.Field(targetExpression, fieldInfo);
        }

        private static Expression AccessInnerTransparentField(
            Type transparentIdentifierType,
            Expression targetExpression)
        {
            var fieldInfo = transparentIdentifierType.GetTypeInfo().GetDeclaredField("Inner");

            return Expression.Field(targetExpression, fieldInfo);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual void IntroduceTransparentScope(
            [NotNull] IQuerySource querySource,
            [NotNull] QueryModel queryModel,
            int index,
            [NotNull] Type transparentIdentifierType)
        {
            Check.NotNull(querySource, nameof(querySource));
            Check.NotNull(queryModel, nameof(queryModel));
            Check.NotNull(transparentIdentifierType, nameof(transparentIdentifierType));

            CurrentParameter
                = Expression.Parameter(
                    transparentIdentifierType,
                    string.Format(CultureInfo.InvariantCulture, "t{0}", _transparentParameterCounter++));

            var outerAccessExpression
                = AccessOuterTransparentField(transparentIdentifierType, CurrentParameter);

            RescopeTransparentAccess(queryModel.MainFromClause, outerAccessExpression);

            for (var i = 0; i < index; i++)
            {
                var bodyClause = queryModel.BodyClauses[i] as IQuerySource;

                if (bodyClause != null)
                {
                    RescopeTransparentAccess(bodyClause, outerAccessExpression);

                    var groupJoinClause = bodyClause as GroupJoinClause;

                    if (groupJoinClause != null
                        && QueryCompilationContext.QuerySourceMapping
                            .ContainsMapping(groupJoinClause.JoinClause))
                    {
                        RescopeTransparentAccess(groupJoinClause.JoinClause, outerAccessExpression);
                    }
                }
            }

            QueryCompilationContext.AddOrUpdateMapping(querySource, AccessInnerTransparentField(transparentIdentifierType, CurrentParameter));
        }

        private void RescopeTransparentAccess(IQuerySource querySource, Expression targetExpression)
        {
            var memberAccessExpression
                = ShiftMemberAccess(
                    targetExpression,
                    _queryCompilationContext.QuerySourceMapping.GetExpression(querySource));

            _queryCompilationContext.QuerySourceMapping.ReplaceMapping(querySource, memberAccessExpression);
        }

        private static Expression ShiftMemberAccess(Expression targetExpression, Expression currentExpression)
        {
            var memberExpression = currentExpression as MemberExpression;

            if (memberExpression == null)
            {
                return targetExpression;
            }

            try
            {
                return Expression.MakeMemberAccess(
                    ShiftMemberAccess(targetExpression, memberExpression.Expression),
                    memberExpression.Member);
            }
            catch (ArgumentException)
            {
                // Member is not defined on the new target expression.
                // This is due to stale QuerySourceMappings, which we can't
                // remove due to there not being an API on QuerySourceMapping.
            }

            return currentExpression;
        }

        #endregion

        /// <summary>
        ///     Translates a re-linq query model expression into a compiled query expression.
        /// </summary>
        /// <param name="expression"> The re-linq query model expression. </param>
        /// <param name="querySource"> The query source. </param>
        /// <param name="inProjection"> True when the expression is a projector. </param>
        /// <returns>
        ///     A compiled query expression fragment.
        /// </returns>
        public virtual Expression ReplaceClauseReferences(
            [NotNull] Expression expression,
            [CanBeNull] IQuerySource querySource = null,
            bool inProjection = false)
        {
            Check.NotNull(expression, nameof(expression));

            expression
                = _entityQueryableExpressionVisitorFactory
                    .Create(this, querySource)
                    .Visit(expression);

            expression
                = _memberAccessBindingExpressionVisitorFactory
                    .Create(QueryCompilationContext.QuerySourceMapping, this, inProjection)
                    .Visit(expression);

            if (!inProjection
                && expression.Type != typeof(string)
                && expression.Type != typeof(byte[])
                && _expression?.Type.TryGetElementType(typeof(IAsyncEnumerable<>)) != null)
            {
                var elementType = expression.Type.TryGetElementType(typeof(IEnumerable<>));

                if (elementType != null)
                {
                    if (LinqOperatorProvider is AsyncLinqOperatorProvider asyncLinqOperatorProvider)
                    {
                        return
                            Expression.Call(
                                asyncLinqOperatorProvider
                                    .ToAsyncEnumerable
                                    .MakeGenericMethod(elementType),
                                expression);
                    }
                }
            }

            return expression;
        }

        #region Binding

        /// <summary>
        ///     Binds a method call to a value buffer access.
        /// </summary>
        /// <param name="methodCallExpression"> The method call expression. </param>
        /// <param name="expression"> The target expression. </param>
        /// <returns>
        ///     A value buffer access expression.
        /// </returns>
        public virtual Expression BindMethodCallToValueBuffer(
            [NotNull] MethodCallExpression methodCallExpression,
            [NotNull] Expression expression)
        {
            Check.NotNull(methodCallExpression, nameof(methodCallExpression));
            Check.NotNull(expression, nameof(expression));

            return BindMethodCallExpression(
                methodCallExpression,
                (property, querySource)
                    => BindReadValueMethod(methodCallExpression.Type, expression, property.GetIndex(), property));
        }

        /// <summary>
        ///     Binds a member access to a value buffer access.
        /// </summary>
        /// <param name="memberExpression"> The member access expression. </param>
        /// <param name="expression"> The target expression. </param>
        /// <returns>
        ///     A value buffer access expression.
        /// </returns>
        public virtual Expression BindMemberToValueBuffer(
            [NotNull] MemberExpression memberExpression,
            [NotNull] Expression expression)
        {
            Check.NotNull(memberExpression, nameof(memberExpression));
            Check.NotNull(expression, nameof(expression));

            return BindMemberExpression(
                memberExpression,
                null,
                (property, querySource)
                    => BindReadValueMethod(memberExpression.Type, expression, property.GetIndex(), property));
        }

        /// <summary>
        ///     Binds a value buffer read.
        /// </summary>
        /// <param name="memberType"> Type of the member. </param>
        /// <param name="expression"> The target expression. </param>
        /// <param name="index"> A value buffer index. </param>
        /// <param name="property">The property being bound.</param>
        /// <returns>
        ///     A value buffer read expression.
        /// </returns>
        public virtual Expression BindReadValueMethod(
            [NotNull] Type memberType,
            [NotNull] Expression expression,
            int index,
            [CanBeNull] IProperty property = null)
        {
            Check.NotNull(memberType, nameof(memberType));
            Check.NotNull(expression, nameof(expression));

            return _entityMaterializerSource
                .CreateReadValueExpression(expression, memberType, index, property);
        }

        /// <summary>
        ///     Binds a navigation path property expression.
        /// </summary>
        /// <typeparam name="TResult"> Type of the result. </typeparam>
        /// <param name="propertyExpression"> The property expression. </param>
        /// <param name="propertyBinder"> The property binder. </param>
        /// <returns>
        ///     A TResult.
        /// </returns>
        public virtual TResult BindNavigationPathPropertyExpression<TResult>(
            [NotNull] Expression propertyExpression,
            [NotNull] Func<IReadOnlyList<IPropertyBase>, IQuerySource, TResult> propertyBinder)
        {
            Check.NotNull(propertyExpression, nameof(propertyExpression));
            Check.NotNull(propertyBinder, nameof(propertyBinder));

            return BindPropertyExpressionCore(propertyExpression, null, propertyBinder);
        }

        /// <summary>
        ///     Binds a member expression.
        /// </summary>
        /// <param name="memberExpression"> The member access expression. </param>
        /// <param name="memberBinder"> The member binder. </param>
        public virtual void BindMemberExpression(
            [NotNull] MemberExpression memberExpression,
            [NotNull] Action<IProperty, IQuerySource> memberBinder)
        {
            Check.NotNull(memberExpression, nameof(memberExpression));
            Check.NotNull(memberBinder, nameof(memberBinder));

            BindMemberExpression(
                memberExpression, null,
                (property, querySource) =>
                    {
                        memberBinder(property, querySource);

                        return default(object);
                    });
        }

        /// <summary>
        ///     Binds a member expression.
        /// </summary>
        /// <typeparam name="TResult"> Type of the result. </typeparam>
        /// <param name="memberExpression"> The member access expression. </param>
        /// <param name="querySource"> The query source. </param>
        /// <param name="memberBinder"> The member binder. </param>
        /// <returns>
        ///     A TResult.
        /// </returns>
        public virtual TResult BindMemberExpression<TResult>(
            [NotNull] MemberExpression memberExpression,
            [CanBeNull] IQuerySource querySource,
            [NotNull] Func<IProperty, IQuerySource, TResult> memberBinder)
        {
            Check.NotNull(memberExpression, nameof(memberExpression));
            Check.NotNull(memberBinder, nameof(memberBinder));

            return BindPropertyExpressionCore(
                memberExpression, querySource,
                (ps, qs) =>
                    {
                        var property = ps.Count == 1 ? ps[0] as IProperty : null;

                        return property != null
                            ? memberBinder(property, qs)
                            : default(TResult);
                    });
        }

        /// <summary>
        ///     Binds a method call expression.
        /// </summary>
        /// <typeparam name="TResult"> Type of the result. </typeparam>
        /// <param name="methodCallExpression"> The method call expression. </param>
        /// <param name="querySource"> The query source. </param>
        /// <param name="methodCallBinder"> The method call binder. </param>
        /// <returns>
        ///     A TResult.
        /// </returns>
        public virtual TResult BindMethodCallExpression<TResult>(
            [NotNull] MethodCallExpression methodCallExpression,
            [CanBeNull] IQuerySource querySource,
            [NotNull] Func<IProperty, IQuerySource, TResult> methodCallBinder)
        {
            Check.NotNull(methodCallExpression, nameof(methodCallExpression));
            Check.NotNull(methodCallBinder, nameof(methodCallBinder));

            return BindPropertyExpressionCore(
                methodCallExpression, querySource,
                (ps, qs) =>
                    {
                        var property = ps.Count == 1 ? ps[0] as IProperty : null;

                        return property != null
                            ? methodCallBinder(property, qs)
                            : default(TResult);
                    });
        }

        private TResult BindPropertyExpressionCore<TResult>(
            Expression propertyExpression,
            IQuerySource querySource,
            Func<IReadOnlyList<IPropertyBase>, IQuerySource, TResult> propertyBinder)
        {
            var properties = MemberAccessBindingExpressionVisitor.GetPropertyPath(
                propertyExpression, QueryCompilationContext, out var querySourceReferenceExpression);

            if (querySourceReferenceExpression != null
                && (querySource == null
                    || querySource == querySourceReferenceExpression.ReferencedQuerySource))
            {
                return propertyBinder(
                    properties,
                    querySourceReferenceExpression.ReferencedQuerySource);
            }

            if (properties.Count > 0)
            {
                return propertyBinder(
                    properties,
                    null);
            }

            return default(TResult);
        }

        /// <summary>
        ///     Binds a method call expression.
        /// </summary>
        /// <typeparam name="TResult"> Type of the result. </typeparam>
        /// <param name="methodCallExpression"> The method call expression. </param>
        /// <param name="methodCallBinder"> The method call binder. </param>
        /// <returns>
        ///     A TResult.
        /// </returns>
        public virtual TResult BindMethodCallExpression<TResult>(
            [NotNull] MethodCallExpression methodCallExpression,
            [NotNull] Func<IProperty, IQuerySource, TResult> methodCallBinder)
        {
            Check.NotNull(methodCallExpression, nameof(methodCallExpression));
            Check.NotNull(methodCallBinder, nameof(methodCallBinder));

            return BindMethodCallExpression(methodCallExpression, null, methodCallBinder);
        }

        /// <summary>
        ///     Binds a method call expression.
        /// </summary>
        /// <param name="methodCallExpression"> The method call expression. </param>
        /// <param name="methodCallBinder"> The method call binder. </param>
        public virtual void BindMethodCallExpression(
            [NotNull] MethodCallExpression methodCallExpression,
            [NotNull] Action<IProperty, IQuerySource> methodCallBinder)
        {
            Check.NotNull(methodCallExpression, nameof(methodCallExpression));
            Check.NotNull(methodCallBinder, nameof(methodCallBinder));

            BindMethodCallExpression(
                methodCallExpression, null,
                (property, querySource) =>
                    {
                        methodCallBinder(property, querySource);

                        return default(object);
                    });
        }

        #endregion
    }
}
