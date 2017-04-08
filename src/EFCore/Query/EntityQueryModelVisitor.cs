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
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Query.ResultOperators.Internal;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.Logging;
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

        private static readonly string _efTypeName = typeof(EF).FullName;

        /// <summary>
        ///     Determines if a <see cref="MethodInfo" /> is referencing the <see cref="EF.Property{TProperty}(object, string)" /> method.
        /// </summary>
        /// <param name="methodInfo"> The method info to check. </param>
        /// <returns>
        ///     True if <paramref name="methodInfo" /> is referencing <see cref="EF.Property{TProperty}(object, string)" />; otherwise fale;
        /// </returns>
        public static bool IsPropertyMethod([CanBeNull] MethodInfo methodInfo) =>
            Equals(methodInfo, EF.PropertyMethod)
            // fallback to string comparison because MethodInfo.Equals is not
            // always true in .NET Native even if methods are the same
            || methodInfo != null
            && methodInfo.IsGenericMethod
            && methodInfo.Name == nameof(EF.Property)
            && methodInfo.DeclaringType?.FullName == _efTypeName;

        /// <summary>
        ///     Creates an expression to access the given property on an given entity.
        /// </summary>
        /// <param name="target"> The entity. </param>
        /// <param name="property"> The property to be accessed. </param>
        /// <returns> The newly created expression. </returns>
        public static Expression CreatePropertyExpression(
            [NotNull] Expression target,
            [NotNull] IPropertyBase property)
            => Expression.Call(
                EF.PropertyMethod.MakeGenericMethod(property.ClrType.MakeNullable()),
                target,
                Expression.Constant(property.Name));

        private readonly IQueryOptimizer _queryOptimizer;
        private readonly INavigationRewritingExpressionVisitorFactory _navigationRewritingExpressionVisitorFactory;
        private readonly ISubQueryMemberPushDownExpressionVisitor _subQueryMemberPushDownExpressionVisitor;
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
            _subQueryMemberPushDownExpressionVisitor = dependencies.SubQueryMemberPushDownExpressionVisitor;
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

            using (QueryCompilationContext.Logger.BeginScope(this))
            {
                QueryCompilationContext.Logger
                    .LogDebug(
                        CoreEventId.CompilingQueryModel,
                        () => CoreStrings.LogCompilingQueryModel(Environment.NewLine, queryModel.Print()));

                _blockTaskExpressions = false;

                ExtractQueryAnnotations(queryModel);

                var includeResultOperators
                    = QueryCompilationContext.QueryAnnotations
                        .OfType<IncludeResultOperator>()
                        .ToList();

                OptimizeQueryModel(queryModel, includeResultOperators, asyncQuery: false);

                QueryCompilationContext.FindQuerySourcesRequiringMaterialization(this, queryModel);
                QueryCompilationContext.DetermineQueryBufferRequirement(queryModel);

                VisitQueryModel(queryModel);

                SingleResultToSequence(queryModel);

                IncludeNavigations(queryModel, includeResultOperators);

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

            using (QueryCompilationContext.Logger.BeginScope(this))
            {
                QueryCompilationContext.Logger
                    .LogDebug(
                        CoreEventId.CompilingQueryModel,
                        () => CoreStrings.LogCompilingQueryModel(Environment.NewLine, queryModel.Print()));

                _blockTaskExpressions = false;

                ExtractQueryAnnotations(queryModel);

                var includeResultOperators
                    = QueryCompilationContext.QueryAnnotations
                        .OfType<IncludeResultOperator>()
                        .ToList();

                OptimizeQueryModel(queryModel, includeResultOperators, asyncQuery: true);

                QueryCompilationContext.FindQuerySourcesRequiringMaterialization(this, queryModel);
                QueryCompilationContext.DetermineQueryBufferRequirement(queryModel);

                VisitQueryModel(queryModel);

                SingleResultToSequence(queryModel, _expression.Type.GetTypeInfo().GenericTypeArguments[0]);

                IncludeNavigations(queryModel, includeResultOperators);

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
        /// <param name="includeResultOperators">TODO: This parameter is to be removed.</param>
        /// <param name="asyncQuery">True if we are compiling an async query; otherwise false.</param>
        protected virtual void OptimizeQueryModel(
            [NotNull] QueryModel queryModel,
            [NotNull] ICollection<IncludeResultOperator> includeResultOperators,
            bool asyncQuery)
        {
            Check.NotNull(queryModel, nameof(queryModel));
            Check.NotNull(includeResultOperators, nameof(includeResultOperators));

            // First pass of optimizations

            var additionalFromClauseOptimizer
                = new AdditionalFromClauseOptimizingQueryModelVisitor();

            additionalFromClauseOptimizer.VisitQueryModel(queryModel);

            _queryOptimizer.Optimize(QueryCompilationContext.QueryAnnotations, queryModel);

            var entityEqualityRewritingExpressionVisitor
                = new EntityEqualityRewritingExpressionVisitor(QueryCompilationContext.Model);

            entityEqualityRewritingExpressionVisitor.Rewrite(queryModel);

            queryModel.TransformExpressions(_subQueryMemberPushDownExpressionVisitor.Visit);

            // Rewrite navigations

            new NondeterministicResultCheckingVisitor(QueryCompilationContext.Logger)
                .VisitQueryModel(queryModel);

            var includeCompiler = new IncludeCompiler(QueryCompilationContext, _querySourceTracingExpressionVisitorFactory);

            includeCompiler.CompileIncludes(queryModel, includeResultOperators, TrackResults(queryModel), asyncQuery);

            _navigationRewritingExpressionVisitorFactory
                .Create(this)
                .Rewrite(queryModel, parentQueryModel: null);

            includeCompiler.RewriteCollectionQueries(queryModel);

            // Second pass of optimizations

            additionalFromClauseOptimizer.VisitQueryModel(queryModel);

            entityEqualityRewritingExpressionVisitor.Rewrite(queryModel);

            queryModel.TransformExpressions(_subQueryMemberPushDownExpressionVisitor.Visit);

            // Log results

            QueryCompilationContext.Logger
                .LogDebug(
                    CoreEventId.OptimizedQueryModel,
                    () => CoreStrings.LogOptimizedQueryModel(Environment.NewLine, queryModel.Print()));
        }

        private class NondeterministicResultCheckingVisitor : QueryModelVisitorBase
        {
            private const int QueryModelStringLengthLimit = 100;
            private readonly ILogger _logger;

            public NondeterministicResultCheckingVisitor([NotNull] ILogger logger) => _logger = logger;

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
                    _logger.LogWarning(
                        CoreEventId.CompilingQueryModel,
                        () => CoreStrings.RowLimitingOperationWithoutOrderBy(
                            queryModel.Print(removeFormatting: true, characterLimit: QueryModelStringLengthLimit)));
                }

                if (resultOperators.Any(o => o is FirstResultOperator)
                    && !queryModel.BodyClauses.OfType<OrderByClause>().Any()
                    && !queryModel.BodyClauses.OfType<WhereClause>().Any())
                {
                    _logger.LogWarning(
                        CoreEventId.CompilingQueryModel,
                        () => CoreStrings.FirstWithoutOrderByAndFilter(
                            queryModel.Print(removeFormatting: true, characterLimit: QueryModelStringLengthLimit)));
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
        ///     Includes related data requested in the LINQ query.
        /// </summary>
        /// <param name="queryModel"> The query. </param>
        /// <param name="includeResultOperators"></param>
        protected virtual void IncludeNavigations(
            [NotNull] QueryModel queryModel,
            [NotNull] ICollection<IncludeResultOperator> includeResultOperators)
        {
            Check.NotNull(queryModel, nameof(queryModel));

            if (queryModel.GetOutputDataInfo() is StreamedScalarValueInfo)
            {
                return;
            }

            var includeSpecifications
                = includeResultOperators
                    .Select(
                        includeResultOperator =>
                            {
                                var entityType = QueryCompilationContext.Model.FindEntityType(
                                    includeResultOperator.PathFromQuerySource.Type);

                                var parts = includeResultOperator.NavigationPropertyPaths.ToArray();
                                var navigationPath = new INavigation[parts.Length];
                                for (var i = 0; i < parts.Length; i++)
                                {
                                    navigationPath[i] = entityType.FindNavigation(parts[i]);

                                    if (navigationPath[i] == null)
                                    {
                                        throw new InvalidOperationException(
                                            CoreStrings.IncludeBadNavigation(parts[i], entityType.DisplayName()));
                                    }

                                    entityType = navigationPath[i].GetTargetType();
                                }

                                return new
                                {
                                    specification = new IncludeSpecification(includeResultOperator.QuerySource, navigationPath),
                                    order = string.Concat(navigationPath.Select(n => n.IsCollection() ? "1" : "0"))
                                };
                            })
                    .OrderByDescending(e => e.order)
                    .ThenBy(e => e.specification.NavigationPath.First().IsDependentToPrincipal())
                    .Select(e => e.specification)
                    .ToList();

            IncludeNavigations(queryModel, includeSpecifications);
        }

        /// <summary>
        ///     Includes related data requested in the LINQ query.
        /// </summary>
        /// <param name="queryModel"> The query. </param>
        /// <param name="includeSpecifications"> Related data to be included. </param>
        protected virtual void IncludeNavigations(
            [NotNull] QueryModel queryModel,
            [NotNull] IReadOnlyCollection<IncludeSpecification> includeSpecifications)
        {
            Check.NotNull(queryModel, nameof(queryModel));
            Check.NotNull(includeSpecifications, nameof(includeSpecifications));

            foreach (var includeSpecification in includeSpecifications)
            {
                var resultQuerySourceReferenceExpression
                    = _querySourceTracingExpressionVisitorFactory
                        .Create()
                        .FindResultQuerySourceReferenceExpression(
                            queryModel.SelectClause.Selector,
                            includeSpecification.QuerySource);

                if (resultQuerySourceReferenceExpression != null)
                {
                    var accessorExpression = QueryCompilationContext.QuerySourceMapping.GetExpression(
                        resultQuerySourceReferenceExpression.ReferencedQuerySource);

                    var sequenceType = resultQuerySourceReferenceExpression.Type.TryGetSequenceType();

                    if (sequenceType != null
                        && QueryCompilationContext.Model.FindEntityType(sequenceType) != null)
                    {
                        includeSpecification.IsEnumerableTarget = true;
                    }

                    QueryCompilationContext.Logger
                        .LogDebug(
                            CoreEventId.IncludingNavigation,
                            () => CoreStrings.LogIncludingNavigation(includeSpecification));

                    IncludeNavigations(
                        includeSpecification,
                        _expression.Type.GetSequenceType(),
                        accessorExpression,
                        QueryCompilationContext.IsTrackingQuery);

                    QueryCompilationContext
                        .AddTrackableInclude(
                            resultQuerySourceReferenceExpression.ReferencedQuerySource,
                            includeSpecification.NavigationPath);
                }
                else
                {
                    QueryCompilationContext.Logger
                        .LogWarning(
                            CoreEventId.IncludeIgnoredWarning,
                            () => CoreStrings.LogIgnoredInclude(includeSpecification));
                }
            }
        }

        /// <summary>
        ///     Includes a specific navigation property requested in the LINQ query.
        /// </summary>
        /// <param name="includeSpecification"> The navigation property to be included. </param>
        /// <param name="resultType"> The type of results returned by the query. </param>
        /// <param name="accessorExpression"> Expression for the navigation property to be included. </param>
        /// <param name="querySourceRequiresTracking"> A value indicating whether results of this query are to be tracked. </param>
        protected virtual void IncludeNavigations(
            [NotNull] IncludeSpecification includeSpecification,
            [NotNull] Type resultType,
            [NotNull] Expression accessorExpression,
            bool querySourceRequiresTracking)
        {
            // template method
            throw new NotImplementedException(CoreStrings.IncludeNotImplemented);
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
                    var subqueryExpression = ((queryModel.SelectClause.Selector as QuerySourceReferenceExpression)
                        ?.ReferencedQuerySource as MainFromClause)?.FromExpression as SubQueryExpression;

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
            var queryExecutorExpression
                = Expression
                    .Lambda<Func<QueryContext, TResults>>(
                        _expression, QueryContextParameter);

            try
            {
                return queryExecutorExpression.Compile();
            }
            finally
            {
                QueryCompilationContext.Logger.LogDebug(
                    CoreEventId.QueryPlan,
                    () =>
                        {
                            var queryPlan = _expressionPrinter.Print(queryExecutorExpression);

                            return queryPlan;
                        });
            }
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

            AddOrUpdateMapping(fromClause, CurrentParameter);
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

            AddOrUpdateMapping(joinClause, innerItemParameter);

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

            AddOrUpdateMapping(groupJoinClause.JoinClause, innerItemParameter);

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
                var taskLiftingExpressionVisitor = new TaskLiftingExpressionVisitor();
                var asyncSelector = taskLiftingExpressionVisitor.LiftTasks(selector);

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
                                taskLiftingExpressionVisitor.CancellationTokenParameter));
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

            AddOrUpdateMapping(querySource, AccessInnerTransparentField(transparentIdentifierType, CurrentParameter));
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
                    var asyncLinqOperatorProvider = LinqOperatorProvider as AsyncLinqOperatorProvider;
                    if (asyncLinqOperatorProvider != null)
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

        /// <summary>
        ///     Adds or updates the expression mapped to a query source.
        /// </summary>
        /// <param name="querySource"> The query source. </param>
        /// <param name="expression"> The expression mapped to the query source. </param>
        public virtual void AddOrUpdateMapping(
            [NotNull] IQuerySource querySource,
            [NotNull] Expression expression)
        {
            Check.NotNull(querySource, nameof(querySource));
            Check.NotNull(expression, nameof(expression));

            QueryCompilationContext.AddOrUpdateMapping(querySource, expression);
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
            [NotNull] Func<IEnumerable<IPropertyBase>, IQuerySource, TResult> propertyBinder)
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
            QuerySourceReferenceExpression querySourceReferenceExpression;

            var properties
                = IterateCompositePropertyExpression(propertyExpression, out querySourceReferenceExpression);

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

        private IReadOnlyList<IPropertyBase> IterateCompositePropertyExpression(
            Expression expression,
            out QuerySourceReferenceExpression querySourceReferenceExpression)
        {
            var properties = new List<IPropertyBase>();
            var memberExpression = expression as MemberExpression;
            var methodCallExpression = expression as MethodCallExpression;
            querySourceReferenceExpression = null;

            while (memberExpression?.Expression != null
                   || IsPropertyMethod(methodCallExpression?.Method)
                   && methodCallExpression?.Arguments[0] != null)
            {
                var propertyName = memberExpression?.Member.Name
                                   ?? (string)(methodCallExpression.Arguments[1] as ConstantExpression)?.Value;

                expression = memberExpression?.Expression ?? methodCallExpression.Arguments[0];

                // in case of inheritance there might be convert to derived type here, so we want to check it first
                var entityType = QueryCompilationContext.Model.FindEntityType(expression.Type);

                expression = expression.RemoveConvert();

                if (entityType == null)
                {
                    entityType = QueryCompilationContext.Model.FindEntityType(expression.Type);

                    if (entityType == null)
                    {
                        break;
                    }
                }

                var property
                    = (IPropertyBase)entityType.FindProperty(propertyName)
                      ?? entityType.FindNavigation(propertyName);

                if (property == null)
                {
                    if (IsPropertyMethod(methodCallExpression?.Method))
                    {
                        throw new InvalidOperationException(
                            CoreStrings.PropertyNotFound(propertyName, entityType.DisplayName()));
                    }

                    break;
                }

                properties.Add(property);

                querySourceReferenceExpression = expression as QuerySourceReferenceExpression;
                memberExpression = expression as MemberExpression;
                methodCallExpression = expression as MethodCallExpression;
            }

            return Enumerable.Reverse(properties).ToList();
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
