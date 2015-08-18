// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Query.Annotations;
using Microsoft.Data.Entity.Query.ExpressionVisitors;
using Microsoft.Data.Entity.Query.Internal;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.Logging;
using Remotion.Linq;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Clauses.ExpressionVisitors;
using Remotion.Linq.Clauses.ResultOperators;
using Remotion.Linq.Clauses.StreamedData;

namespace Microsoft.Data.Entity.Query
{
    public abstract class EntityQueryModelVisitor : QueryModelVisitorBase
    {
        public static readonly ParameterExpression QueryContextParameter
            = Expression.Parameter(typeof(QueryContext));

        public static readonly ParameterExpression QueryResultScopeParameter
            = Expression.Parameter(typeof(QueryResultScope));

        public static readonly MethodInfo PropertyMethodInfo
            = typeof(EF).GetTypeInfo().GetDeclaredMethod(nameof(EF.Property));

        private readonly QueryCompilationContext _queryCompilationContext;

        private Expression _expression;
        private StreamedSequenceInfo _streamedSequenceInfo;

        // TODO: Can these be non-blocking?
        private bool _blockTaskExpressions = true;

        protected EntityQueryModelVisitor(
            [NotNull] QueryCompilationContext queryCompilationContext)
        {
            Check.NotNull(queryCompilationContext, nameof(queryCompilationContext));

            _queryCompilationContext = queryCompilationContext;
        }

        public virtual Expression Expression
        {
            get { return _expression; }
            [param: NotNull]
            protected set
            {
                Check.NotNull(value, nameof(value));

                _expression = value;
            }
        }

        public virtual QueryCompilationContext QueryCompilationContext => _queryCompilationContext;

        public virtual ILinqOperatorProvider LinqOperatorProvider => QueryCompilationContext.LinqOperatorProvider;

        public virtual StreamedSequenceInfo StreamedSequenceInfo => _streamedSequenceInfo;

        protected abstract ExpressionVisitor CreateQueryingExpressionVisitor([NotNull] IQuerySource querySource);

        protected virtual ExpressionVisitor CreateProjectionExpressionVisitor([NotNull] IQuerySource querySource)
            => new ProjectionExpressionVisitor(this);

        protected virtual ExpressionVisitor CreateOrderingExpressionVisitor([NotNull] Ordering ordering)
            => new DefaultQueryExpressionVisitor(this);

        public virtual Func<QueryContext, IEnumerable<TResult>> CreateQueryExecutor<TResult>([NotNull] QueryModel queryModel)
        {
            Check.NotNull(queryModel, nameof(queryModel));

            using (QueryCompilationContext.Logger.BeginScopeImpl(this))
            {
                QueryCompilationContext.Logger.LogInformation(queryModel, Strings.LogCompilingQueryModel);

                _blockTaskExpressions = false;

                ExtractQueryAnnotations(queryModel);

                OptimizeQueryModel(queryModel);

                QueryCompilationContext.FindQuerySourcesRequiringMaterialization(this, queryModel);

                VisitQueryModel(queryModel);

                SingleResultToSequence(queryModel);

                IncludeNavigations(queryModel);

                TrackEntitiesInResults<TResult>(queryModel);

                InterceptExceptions();

                return CreateExecutorLambda<IEnumerable<TResult>>();
            }
        }

        public virtual Func<QueryContext, IAsyncEnumerable<TResult>> CreateAsyncQueryExecutor<TResult>([NotNull] QueryModel queryModel)
        {
            Check.NotNull(queryModel, nameof(queryModel));

            using (QueryCompilationContext.Logger.BeginScopeImpl(this))
            {
                QueryCompilationContext.Logger.LogInformation(queryModel, Strings.LogCompilingQueryModel);

                _blockTaskExpressions = false;

                ExtractQueryAnnotations(queryModel);

                OptimizeQueryModel(queryModel);

                QueryCompilationContext.FindQuerySourcesRequiringMaterialization(this, queryModel);

                VisitQueryModel(queryModel);

                AsyncSingleResultToSequence(queryModel);

                IncludeNavigations(queryModel);

                TrackEntitiesInResults<TResult>(queryModel);

                InterceptExceptions();

                return CreateExecutorLambda<IAsyncEnumerable<TResult>>();
            }
        }

        protected virtual void InterceptExceptions()
        {
            _expression
                = Expression.Call(
                    LinqOperatorProvider.InterceptExceptions
                        .MakeGenericMethod(_expression.Type.GetSequenceType()),
                    Expression.Lambda(_expression),
                    QueryContextParameter);
        }

        protected virtual void ExtractQueryAnnotations([NotNull] QueryModel queryModel)
        {
            Check.NotNull(queryModel, nameof(queryModel));

            QueryCompilationContext.QueryAnnotations
                = new QueryAnnotationExtractor().ExtractQueryAnnotations(queryModel);
        }

        protected virtual void OptimizeQueryModel([NotNull] QueryModel queryModel)
        {
            Check.NotNull(queryModel, nameof(queryModel));

            new QueryOptimizer(QueryCompilationContext.QueryAnnotations).VisitQueryModel(queryModel);

            var navigationRewritingExpressionVisitor 
                = new NavigationRewritingExpressionVisitor(this);

            navigationRewritingExpressionVisitor.Rewrite(queryModel);

            var subQueryMemberPushDownExpressionVisitor = new SubQueryMemberPushDownExpressionVisitor();

            queryModel.TransformExpressions(subQueryMemberPushDownExpressionVisitor.Visit);

            QueryCompilationContext.Logger.LogInformation(queryModel, Strings.LogOptimizedQueryModel);
        }

        protected virtual void SingleResultToSequence([NotNull] QueryModel queryModel)
        {
            Check.NotNull(queryModel, nameof(queryModel));

            if (!(queryModel.GetOutputDataInfo() is StreamedSequenceInfo))
            {
                _expression
                    = Expression.Call(
                        LinqOperatorProvider.ToSequence
                            .MakeGenericMethod(_expression.Type),
                        _expression);
            }
        }

        protected virtual void AsyncSingleResultToSequence([NotNull] QueryModel queryModel)
        {
            Check.NotNull(queryModel, nameof(queryModel));

            if (!(queryModel.GetOutputDataInfo() is StreamedSequenceInfo))
            {
                _expression
                    = Expression.Call(
                        _taskToSequence.MakeGenericMethod(
                            _expression.Type.GetTypeInfo().GenericTypeArguments[0]),
                        _expression);
            }
        }

        private static readonly MethodInfo _taskToSequence
            = typeof(EntityQueryModelVisitor)
                .GetTypeInfo().GetDeclaredMethod(nameof(TaskToSequence));

        [UsedImplicitly]
        private static IAsyncEnumerable<T> TaskToSequence<T>(Task<T> task)
            => new TaskResultAsyncEnumerable<T>(task);

        protected virtual void IncludeNavigations([NotNull] QueryModel queryModel)
        {
            Check.NotNull(queryModel, nameof(queryModel));

            if (queryModel.GetOutputDataInfo() is StreamedScalarValueInfo)
            {
                return;
            }

            var includeSpecifications
                = QueryCompilationContext.QueryAnnotations
                    .OfType<IncludeQueryAnnotation>()
                    .Select(annotation =>
                        {
                            var navigationPath
                                = BindNavigationPathMemberExpression(
                                    (MemberExpression)annotation.NavigationPropertyPath,
                                    (ps, _) =>
                                    {
                                        var properties = ps.ToArray();
                                        var navigations = properties.OfType<INavigation>().ToArray();

                                        if (properties.Length != navigations.Length)
                                        {
                                            throw new InvalidOperationException(
                                                Strings.IncludeNonBindableExpression(annotation.NavigationPropertyPath));
                                        }

                                        return BindChainedNavigations(
                                            navigations,
                                            annotation.ChainedNavigationProperties)
                                            .ToArray();
                                    });

                            if (navigationPath == null)
                            {
                                throw new InvalidOperationException(
                                    Strings.IncludeNonBindableExpression(annotation.NavigationPropertyPath));
                            }

                            return new IncludeSpecification(annotation.QuerySource, navigationPath);
                        })
                    .OrderBy(a => a.NavigationPath.First().PointsToPrincipal())
                    .ToList();

            IncludeNavigations(queryModel, includeSpecifications);
        }

        protected virtual void IncludeNavigations(
            [NotNull] QueryModel queryModel,
            [NotNull] IReadOnlyCollection<IncludeSpecification> includeSpecifications)
        {
            Check.NotNull(queryModel, nameof(queryModel));
            Check.NotNull(includeSpecifications, nameof(includeSpecifications));

            var querySourceTracingExpressionVisitor
                = new QuerySourceTracingExpressionVisitor();

            foreach (var includeSpecification in includeSpecifications)
            {
                var resultQuerySourceReferenceExpression
                    = querySourceTracingExpressionVisitor
                        .FindResultQuerySourceReferenceExpression(
                            queryModel.SelectClause.Selector,
                            includeSpecification.QuerySource);

                if (resultQuerySourceReferenceExpression != null)
                {
                    var accessorLambda
                        = AccessorFindingExpressionVisitor
                            .FindAccessorLambda(
                                resultQuerySourceReferenceExpression,
                                queryModel.SelectClause.Selector,
                                Expression.Parameter(queryModel.SelectClause.Selector.Type));

                    QueryCompilationContext.Logger
                        .LogInformation(
                            includeSpecification.NavigationPath.Join("."),
                            Strings.LogIncludingNavigation);

                    IncludeNavigations(
                        includeSpecification,
                        _expression.Type.GetSequenceType(),
                        accessorLambda,
                        QuerySourceRequiresTracking(includeSpecification.QuerySource));

                    QueryCompilationContext
                        .AddTrackableInclude(
                            resultQuerySourceReferenceExpression.ReferencedQuerySource,
                            includeSpecification.NavigationPath);
                }
            }
        }

        private IEnumerable<INavigation> BindChainedNavigations(
            IEnumerable<INavigation> boundNavigations, IReadOnlyList<PropertyInfo> chainedNavigationProperties)
        {
            var boundChainedNavigations = new List<INavigation>();

            if (chainedNavigationProperties != null)
            {
                foreach (
                    var navigation in
                        from propertyInfo in chainedNavigationProperties
                        let entityType
                            = QueryCompilationContext.Model
                                .FindEntityType(propertyInfo.DeclaringType)
                        select entityType?.FindNavigation(propertyInfo.Name))
                {
                    if (navigation == null)
                    {
                        return null;
                    }

                    boundChainedNavigations.Add(navigation);
                }
            }

            return boundNavigations.Concat(boundChainedNavigations);
        }

        protected virtual void IncludeNavigations(
            [NotNull] IncludeSpecification includeSpecification,
            [NotNull] Type resultType,
            [NotNull] LambdaExpression accessorLambda,
            bool querySourceRequiresTracking)
        {
            // template method

            throw new NotImplementedException(Strings.IncludeNotImplemented);
        }

        protected virtual void TrackEntitiesInResults<TResult>(
            [NotNull] QueryModel queryModel)
        {
            Check.NotNull(queryModel, nameof(queryModel));

            if (queryModel.GetOutputDataInfo() is StreamedScalarValueInfo)
            {
                return;
            }

            var entityTrackingInfos
                = new EntityResultFindingExpressionVisitor(QueryCompilationContext)
                    .FindEntitiesInResult(queryModel.SelectClause.Selector);

            if (entityTrackingInfos.Any())
            {
                QueryCompilationContext.Logger
                    .LogInformation(
                        entityTrackingInfos,
                        etis => Strings.LogTrackingQuerySources(
                            etis.Select(eti => eti.QuerySource.ItemName).Join()));

                var resultItemType
                    = _expression.Type.GetSequenceType();

                var resultItemTypeInfo = resultItemType.GetTypeInfo();

                MethodInfo trackingMethod;

                if (resultItemTypeInfo.IsGenericType
                    && (resultItemTypeInfo.GetGenericTypeDefinition() == typeof(IGrouping<,>)
                        || resultItemTypeInfo.GetGenericTypeDefinition() == typeof(IAsyncGrouping<,>)))
                {
                    trackingMethod
                        = LinqOperatorProvider.TrackGroupedEntities
                            .MakeGenericMethod(
                                resultItemType.GenericTypeArguments[0],
                                resultItemType.GenericTypeArguments[1],
                                queryModel.SelectClause.Selector.Type);
                }
                else
                {
                    trackingMethod
                        = LinqOperatorProvider.TrackEntities
                            .MakeGenericMethod(
                                resultItemType,
                                queryModel.SelectClause.Selector.Type);
                }

                _expression
                    = Expression.Call(
                        trackingMethod,
                        _expression,
                        QueryContextParameter,
                        Expression.Constant(entityTrackingInfos),
                        Expression.Constant(
                            _getEntityAccessors
                                .MakeGenericMethod(queryModel.SelectClause.Selector.Type)
                                .Invoke(
                                    null,
                                    new object[]
                                    {
                                        entityTrackingInfos,
                                        queryModel.SelectClause.Selector
                                    })));
            }
        }

        private static readonly MethodInfo _getEntityAccessors
            = typeof(EntityQueryModelVisitor)
                .GetTypeInfo().GetDeclaredMethod(nameof(GetEntityAccessors));

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
                                Expression.Parameter(typeof(TResult)))
                            .Compile()
                )
                .ToList();

        protected virtual Func<QueryContext, TResults> CreateExecutorLambda<TResults>()
        {
            var queryExecutorExpression =
                Expression
                    .Lambda<Func<QueryContext, QueryResultScope, TResults>>(
                        _expression, QueryContextParameter, QueryResultScopeParameter);

            QueryCompilationContext.Logger.LogInformation(() =>
                {
                    var queryPlan = QueryCompilationContext.CreateExpressionPrinter().Print(queryExecutorExpression);

                    return queryPlan;
                });

            var queryExecutor = queryExecutorExpression.Compile();

            return qc => queryExecutor(qc, null);
        }

        public virtual bool QuerySourceRequiresTracking([NotNull] IQuerySource querySource)
        {
            Check.NotNull(querySource, nameof(querySource));

            if (QueryCompilationContext.QueryAnnotations == null)
            {
                return true;
            }

            return QueryCompilationContext
                .GetCustomQueryAnnotations(EntityFrameworkQueryableExtensions.AsNoTrackingMethodInfo)
                .All(qa => qa.QuerySource != querySource);
        }

        public override void VisitQueryModel([NotNull] QueryModel queryModel)
        {
            Check.NotNull(queryModel, nameof(queryModel));

            base.VisitQueryModel(queryModel);

            if (_blockTaskExpressions)
            {
                _expression
                    = new TaskBlockingExpressionVisitor()
                        .Visit(_expression);
            }
        }

        public override void VisitMainFromClause(
            [NotNull] MainFromClause fromClause, [NotNull] QueryModel queryModel)
        {
            Check.NotNull(fromClause, nameof(fromClause));
            Check.NotNull(queryModel, nameof(queryModel));

            _expression = CompileMainFromClauseExpression(fromClause, queryModel);

            var sequenceType = _expression.Type.GetSequenceType();

            var elementScoped
                = sequenceType.IsConstructedGenericType
                  && sequenceType.GetGenericTypeDefinition() == typeof(QueryResultScope<>);

            Type elementType;

            if (elementScoped)
            {
                elementType = sequenceType.GetTypeInfo().GenericTypeArguments[0];
            }
            else
            {
                elementType = sequenceType;
                _expression = CreateScope(_expression, elementType, fromClause);
            }

            _expression
                = Expression.Call(
                    LinqOperatorProvider.SelectMany
                        .MakeGenericMethod(typeof(QueryResultScope), typeof(QueryResultScope)),
                    Expression.Call(
                        LinqOperatorProvider.ToSequence
                            .MakeGenericMethod(typeof(QueryResultScope)),
                        QueryResultScopeParameter),
                    Expression.Lambda(_expression, QueryResultScopeParameter));

            if (!_queryCompilationContext.QuerySourceMapping.ContainsMapping(fromClause))
            {
                _queryCompilationContext.QuerySourceMapping.AddMapping(
                    fromClause,
                    QueryResultScope.GetResult(QueryResultScopeParameter, fromClause, elementType));
            }
        }

        protected virtual Expression CompileMainFromClauseExpression(
            [NotNull] MainFromClause mainFromClause, [NotNull] QueryModel queryModel)
        {
            Check.NotNull(mainFromClause, nameof(mainFromClause));
            Check.NotNull(queryModel, nameof(queryModel));

            return ReplaceClauseReferences(mainFromClause.FromExpression, mainFromClause);
        }

        public override void VisitAdditionalFromClause(
            [NotNull] AdditionalFromClause fromClause, [NotNull] QueryModel queryModel, int index)
        {
            Check.NotNull(fromClause, nameof(fromClause));
            Check.NotNull(queryModel, nameof(queryModel));

            var innerExpression = CompileAdditionalFromClauseExpression(fromClause, queryModel);

            var innerSequenceType = innerExpression.Type.GetSequenceType();

            var innerElementScoped
                = innerSequenceType.IsConstructedGenericType
                  && innerSequenceType.GetGenericTypeDefinition() == typeof(QueryResultScope<>);

            Type innerElementType;

            if (innerElementScoped)
            {
                innerElementType
                    = innerSequenceType.GetTypeInfo().GenericTypeArguments[0];
            }
            else
            {
                innerElementType = innerSequenceType;
                innerExpression = CreateScope(innerExpression, innerElementType, fromClause);
            }

            _expression
                = Expression.Call(
                    LinqOperatorProvider.SelectMany
                        .MakeGenericMethod(typeof(QueryResultScope), typeof(QueryResultScope)),
                    _expression,
                    Expression.Lambda(innerExpression, QueryResultScopeParameter));

            if (!_queryCompilationContext.QuerySourceMapping.ContainsMapping(fromClause))
            {
                _queryCompilationContext.QuerySourceMapping.AddMapping(
                    fromClause,
                    QueryResultScope.GetResult(QueryResultScopeParameter, fromClause, innerElementType));
            }
        }

        protected virtual Expression CompileAdditionalFromClauseExpression(
            [NotNull] AdditionalFromClause additionalFromClause, [NotNull] QueryModel queryModel)
        {
            Check.NotNull(additionalFromClause, nameof(additionalFromClause));
            Check.NotNull(queryModel, nameof(queryModel));

            return ReplaceClauseReferences(additionalFromClause.FromExpression, additionalFromClause);
        }

        public override void VisitJoinClause(
            [NotNull] JoinClause joinClause, [NotNull] QueryModel queryModel, int index)
        {
            Check.NotNull(joinClause, nameof(joinClause));
            Check.NotNull(queryModel, nameof(queryModel));

            var outerKeySelector
                = ReplaceClauseReferences(joinClause.OuterKeySelector, joinClause);

            var innerSequenceExpression
                = CompileJoinClauseInnerSequenceExpression(joinClause, queryModel);

            var innerSequenceType
                = innerSequenceExpression.Type.GetSequenceType();

            var innerItemParameter
                = Expression.Parameter(innerSequenceType);

            var innerElementScoped
                = innerSequenceType.IsConstructedGenericType
                  && innerSequenceType.GetGenericTypeDefinition() == typeof(QueryResultScope<>);

            Type innerElementType;

            var querySourceMapping = _queryCompilationContext.QuerySourceMapping;
            if (innerElementScoped)
            {
                innerElementType = innerSequenceType.GetTypeInfo().GenericTypeArguments[0];

                querySourceMapping.AddMapping(
                    joinClause,
                    QueryResultScope.GetResult(innerItemParameter, joinClause, innerElementType));
            }
            else
            {
                innerElementType = innerSequenceType;

                querySourceMapping.AddMapping(joinClause, innerItemParameter);
            }

            var innerKeySelector
                = ReplaceClauseReferences(joinClause.InnerKeySelector, joinClause);

            _expression
                = Expression.Call(
                    LinqOperatorProvider.Join.MakeGenericMethod(
                        typeof(QueryResultScope),
                        innerSequenceType,
                        outerKeySelector.Type,
                        typeof(QueryResultScope)),
                    _expression,
                    innerSequenceExpression,
                    Expression.Lambda(outerKeySelector, QueryResultScopeParameter),
                    Expression.Lambda(innerKeySelector, innerItemParameter),
                    Expression.Lambda(
                        QueryResultScope
                            .Create(
                                joinClause,
                                innerElementScoped
                                    ? QueryResultScope.GetResult(innerItemParameter, joinClause, innerElementType)
                                    : innerItemParameter,
                                QueryResultScopeParameter),
                        QueryResultScopeParameter, innerItemParameter));

            querySourceMapping.ReplaceMapping(
                joinClause,
                QueryResultScope.GetResult(QueryResultScopeParameter, joinClause, innerElementType));
        }

        protected virtual Expression CompileJoinClauseInnerSequenceExpression(
            [NotNull] JoinClause joinClause, [NotNull] QueryModel queryModel)
        {
            Check.NotNull(joinClause, nameof(joinClause));
            Check.NotNull(queryModel, nameof(queryModel));
            
            return ReplaceClauseReferences(joinClause.InnerSequence, joinClause);
        }

        public override void VisitGroupJoinClause(
            [NotNull] GroupJoinClause groupJoinClause, [NotNull] QueryModel queryModel, int index)
        {
            Check.NotNull(groupJoinClause, nameof(groupJoinClause));
            Check.NotNull(queryModel, nameof(queryModel));

            var outerKeySelector
                = ReplaceClauseReferences(groupJoinClause.JoinClause.OuterKeySelector, groupJoinClause);

            var innerExpression
                = CompileGroupJoinInnerSequenceExpression(groupJoinClause, queryModel);

            var innerSequenceType
                = innerExpression.Type.GetSequenceType();

            var innerItemParameter
                = Expression.Parameter(innerSequenceType);

            var innerElementScoped
                = innerSequenceType.IsConstructedGenericType
                  && innerSequenceType.GetGenericTypeDefinition() == typeof(QueryResultScope<>);

            Type innerElementType;

            var querySourceMapping = _queryCompilationContext.QuerySourceMapping;
            if (innerElementScoped)
            {
                innerElementType = innerSequenceType.GetTypeInfo().GenericTypeArguments[0];

                querySourceMapping.AddMapping(
                    groupJoinClause.JoinClause,
                    QueryResultScope.GetResult(innerItemParameter, groupJoinClause.JoinClause, innerElementType));
            }
            else
            {
                innerElementType = innerSequenceType;

                querySourceMapping.AddMapping(groupJoinClause.JoinClause, innerItemParameter);
            }

            var innerKeySelector
                = ReplaceClauseReferences(groupJoinClause.JoinClause.InnerKeySelector, groupJoinClause);

            var innerItemsParameter
                = Expression.Parameter(innerExpression.Type);

            _expression
                = Expression.Call(
                    LinqOperatorProvider.GroupJoin.MakeGenericMethod(
                        typeof(QueryResultScope),
                        innerSequenceType,
                        outerKeySelector.Type,
                        typeof(QueryResultScope)),
                    _expression,
                    innerExpression,
                    Expression.Lambda(outerKeySelector, QueryResultScopeParameter),
                    Expression.Lambda(innerKeySelector, innerItemParameter),
                    Expression.Lambda(
                        QueryResultScope
                            .Create(
                                groupJoinClause,
                                innerElementScoped
                                    ? Expression.Call(
                                        LinqOperatorProvider.Select
                                            .MakeGenericMethod(
                                                innerSequenceType,
                                                innerElementType),
                                        innerItemsParameter,
                                        Expression.Lambda(
                                            QueryResultScope.GetResult(
                                                innerItemParameter,
                                                groupJoinClause.JoinClause,
                                                innerElementType),
                                            innerItemParameter))
                                    : (Expression)innerItemsParameter,
                                QueryResultScopeParameter),
                        QueryResultScopeParameter, innerItemsParameter));

            var expressionTypeInfo = _expression.Type.GetTypeInfo();

            querySourceMapping.AddMapping(
                groupJoinClause,
                QueryResultScope
                    .GetResult(
                        QueryResultScopeParameter,
                        groupJoinClause,
                        expressionTypeInfo.GetGenericTypeDefinition()
                            .MakeGenericType(innerElementType)));
        }

        protected virtual Expression CompileGroupJoinInnerSequenceExpression(
            [NotNull] GroupJoinClause groupJoinClause, [NotNull] QueryModel queryModel)
        {
            Check.NotNull(groupJoinClause, nameof(groupJoinClause));
            Check.NotNull(queryModel, nameof(queryModel));

            return ReplaceClauseReferences(groupJoinClause.JoinClause.InnerSequence, groupJoinClause.JoinClause);
        }

        public override void VisitWhereClause(
            [NotNull] WhereClause whereClause, [NotNull] QueryModel queryModel, int index)
        {
            Check.NotNull(whereClause, nameof(whereClause));
            Check.NotNull(queryModel, nameof(queryModel));

            var predicate
                = ReplaceClauseReferences(whereClause.Predicate, queryModel.MainFromClause);

            _expression
                = Expression.Call(
                    LinqOperatorProvider.Where
                        .MakeGenericMethod(typeof(QueryResultScope)),
                    _expression,
                    Expression.Lambda(predicate, QueryResultScopeParameter));
        }

        public override void VisitOrdering(
            [NotNull] Ordering ordering,
            [NotNull] QueryModel queryModel,
            [NotNull] OrderByClause orderByClause,
            int index)
        {
            Check.NotNull(ordering, nameof(ordering));
            Check.NotNull(queryModel, nameof(queryModel));
            Check.NotNull(orderByClause, nameof(orderByClause));

            var elementType = _expression.Type.GetSequenceType();
            var parameterExpression = QueryResultScopeParameter;
            var resultType = queryModel.GetResultType();

            if (resultType.GetTypeInfo().IsGenericType
                && resultType.GetGenericTypeDefinition() == typeof(IOrderedEnumerable<>))
            {
                VisitSelectClause(queryModel.SelectClause, queryModel);

                parameterExpression
                    = Expression.Parameter(_streamedSequenceInfo.ResultItemType);

                _queryCompilationContext.QuerySourceMapping
                    .ReplaceMapping(
                        queryModel.MainFromClause, parameterExpression);

                elementType = _streamedSequenceInfo.ResultItemType;
            }

            var expression
                = CreateOrderingExpressionVisitor(ordering)
                    .Visit(ordering.Expression);

            if (expression != null)
            {
                expression = ReplaceClauseReferences(expression);

                _expression
                    = Expression.Call(
                        (index == 0
                            ? LinqOperatorProvider.OrderBy
                            : LinqOperatorProvider.ThenBy)
                            .MakeGenericMethod(elementType, expression.Type),
                        _expression,
                        Expression.Lambda(expression, parameterExpression),
                        Expression.Constant(ordering.OrderingDirection));
            }
        }

        public override void VisitSelectClause(
            [NotNull] SelectClause selectClause, [NotNull] QueryModel queryModel)
        {
            Check.NotNull(selectClause, nameof(selectClause));
            Check.NotNull(queryModel, nameof(queryModel));

            if (_streamedSequenceInfo != null)
            {
                return;
            }

            var selector
                = ReplaceClauseReferences(
                    CreateProjectionExpressionVisitor(queryModel.MainFromClause)
                        .Visit(selectClause.Selector),
                    inProjection: true);

            _expression
                = Expression.Call(
                    LinqOperatorProvider.Select
                        .MakeGenericMethod(typeof(QueryResultScope), selector.Type),
                    _expression,
                    Expression.Lambda(selector, QueryResultScopeParameter));

            _streamedSequenceInfo
                = new StreamedSequenceInfo(
                    typeof(IEnumerable<>).MakeGenericType(selector.Type),
                    selector);
        }

        public override void VisitResultOperator(
            [NotNull] ResultOperatorBase resultOperator, [NotNull] QueryModel queryModel, int index)
        {
            Check.NotNull(resultOperator, nameof(resultOperator));
            Check.NotNull(queryModel, nameof(queryModel));

            var expression
                = _queryCompilationContext.ResultOperatorHandler
                    .HandleResultOperator(this, resultOperator, queryModel);

            if (expression != _expression)
            {
                _expression = expression;

                if (_streamedSequenceInfo != null
                    && resultOperator is SequenceFromSequenceResultOperatorBase)
                {
                    var sequenceType = _expression.Type.GetSequenceType();

                    _streamedSequenceInfo
                        = new StreamedSequenceInfo(
                            typeof(IEnumerable<>).MakeGenericType(sequenceType),
                            Expression.Default(sequenceType));
                }
                else
                {
                    _streamedSequenceInfo = null;
                }
            }
        }

        public virtual Expression CreateScope(
            [NotNull] Expression expression,
            [NotNull] Type elementType,
            [NotNull] IQuerySource querySource)
        {
            Check.NotNull(expression, nameof(expression));
            Check.NotNull(elementType, nameof(elementType));
            Check.NotNull(querySource, nameof(querySource));

            var innerItemParameter = Expression.Parameter(elementType);

            return
                Expression.Call(
                    LinqOperatorProvider.Select
                        .MakeGenericMethod(elementType, typeof(QueryResultScope)),
                    expression,
                    Expression.Lambda(
                        QueryResultScope
                            .Create(querySource, innerItemParameter, QueryResultScopeParameter),
                        innerItemParameter));
        }

        public virtual Expression ReplaceClauseReferences(
            [NotNull] Expression expression, [NotNull] IQuerySource querySource)
        {
            Check.NotNull(expression, nameof(expression));
            Check.NotNull(querySource, nameof(querySource));

            return QueryCompilationContext.LinqOperatorProvider
                .AdjustSequenceType(
                    ReplaceClauseReferences(
                        CreateQueryingExpressionVisitor(querySource)
                            .Visit(expression)));
        }

        private Expression ReplaceClauseReferences(Expression expression, bool inProjection = false)
            => new MemberAccessBindingExpressionVisitor(_queryCompilationContext.QuerySourceMapping, this, inProjection)
                .Visit(expression);

        public virtual Expression BindMethodCallToValueBuffer(
            [NotNull] MethodCallExpression methodCallExpression,
            [NotNull] Expression expression)
        {
            Check.NotNull(methodCallExpression, nameof(methodCallExpression));
            Check.NotNull(expression, nameof(expression));

            return BindMethodCallExpression(
                methodCallExpression,
                (property, querySource)
                    => BindReadValueMethod(methodCallExpression.Type, expression, property.Index));
        }

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
                    => BindReadValueMethod(memberExpression.Type, expression, property.Index));
        }

        public virtual Expression BindReadValueMethod(
            [NotNull] Type memberType,
            [NotNull] Expression expression,
            int index)
        {
            Check.NotNull(memberType, nameof(memberType));
            Check.NotNull(expression, nameof(expression));

            return QueryCompilationContext.EntityMaterializerSource
                .CreateReadValueExpression(expression, memberType, index);
        }

        public virtual TResult BindNavigationPathMemberExpression<TResult>(
            [NotNull] MemberExpression memberExpression,
            [NotNull] Func<IEnumerable<IPropertyBase>, IQuerySource, TResult> memberBinder)
        {
            Check.NotNull(memberExpression, nameof(memberExpression));
            Check.NotNull(memberBinder, nameof(memberBinder));

            return BindMemberExpressionCore(memberExpression, null, memberBinder);
        }

        public virtual void BindMemberExpression(
            [NotNull] MemberExpression memberExpression,
            [NotNull] Action<IProperty, IQuerySource> memberBinder)
        {
            Check.NotNull(memberExpression, nameof(memberExpression));
            Check.NotNull(memberBinder, nameof(memberBinder));

            BindMemberExpression(memberExpression, null,
                (property, querySource) =>
                    {
                        memberBinder(property, querySource);

                        return default(object);
                    });
        }

        public virtual TResult BindMemberExpression<TResult>(
            [NotNull] MemberExpression memberExpression,
            [CanBeNull] IQuerySource querySource,
            [NotNull] Func<IProperty, IQuerySource, TResult> memberBinder)
        {
            Check.NotNull(memberExpression, nameof(memberExpression));
            Check.NotNull(memberBinder, nameof(memberBinder));

            return BindMemberExpressionCore(memberExpression, querySource,
                (ps, qs) =>
                    {
                        var property = ps.Single() as IProperty;

                        return property != null
                            ? memberBinder(property, qs)
                            : default(TResult);
                    });
        }

        private TResult BindMemberExpressionCore<TResult>(
            MemberExpression memberExpression,
            IQuerySource querySource,
            Func<IEnumerable<IPropertyBase>, IQuerySource, TResult> memberBinder)
        {
            QuerySourceReferenceExpression querySourceReferenceExpression;

            var properties
                = IterateCompositeMemberExpression(memberExpression, out querySourceReferenceExpression);

            if (querySourceReferenceExpression != null
                && (querySource == null
                    || querySource == querySourceReferenceExpression.ReferencedQuerySource))
            {
                return memberBinder(
                    properties,
                    querySourceReferenceExpression.ReferencedQuerySource);
            }

            return default(TResult);
        }

        private IEnumerable<IPropertyBase> IterateCompositeMemberExpression(
            MemberExpression memberExpression, out QuerySourceReferenceExpression querySourceReferenceExpression)
        {
            querySourceReferenceExpression = null;

            var properties = new List<IPropertyBase>();

            while (memberExpression?.Expression != null)
            {
                var entityType
                    = QueryCompilationContext.Model
                        .FindEntityType(memberExpression.Expression.Type);

                if (entityType == null)
                {
                    break;
                }

                var property
                    = (IPropertyBase)entityType.FindProperty(memberExpression.Member.Name)
                      ?? entityType.FindNavigation(memberExpression.Member.Name);

                if (property == null)
                {
                    break;
                }

                properties.Add(property);

                querySourceReferenceExpression
                    = memberExpression.Expression as QuerySourceReferenceExpression;

                memberExpression = memberExpression.Expression as MemberExpression;
            }

            return querySourceReferenceExpression != null
                ? Enumerable.Reverse(properties)
                : Enumerable.Empty<IPropertyBase>();
        }

        public virtual TResult BindMethodCallExpression<TResult>(
            [NotNull] MethodCallExpression methodCallExpression,
            [NotNull] Func<IProperty, IQuerySource, TResult> methodCallBinder)
        {
            Check.NotNull(methodCallExpression, nameof(methodCallExpression));
            Check.NotNull(methodCallBinder, nameof(methodCallBinder));

            return BindMethodCallExpression(methodCallExpression, null, methodCallBinder);
        }

        public virtual void BindMethodCallExpression(
            [NotNull] MethodCallExpression methodCallExpression,
            [NotNull] Action<IProperty, IQuerySource> methodCallBinder)
        {
            Check.NotNull(methodCallExpression, nameof(methodCallExpression));
            Check.NotNull(methodCallBinder, nameof(methodCallBinder));

            BindMethodCallExpression(methodCallExpression, null,
                (property, querySource) =>
                    {
                        methodCallBinder(property, querySource);

                        return default(object);
                    });
        }

        public virtual TResult BindMethodCallExpression<TResult>(
            [NotNull] MethodCallExpression methodCallExpression,
            [CanBeNull] IQuerySource querySource,
            [NotNull] Func<IProperty, IQuerySource, TResult> methodCallBinder)
        {
            Check.NotNull(methodCallExpression, nameof(methodCallExpression));
            Check.NotNull(methodCallBinder, nameof(methodCallBinder));

            if (methodCallExpression.Method.IsGenericMethod)
            {
                var methodInfo = methodCallExpression.Method.GetGenericMethodDefinition();

                if (ReferenceEquals(methodInfo, PropertyMethodInfo))
                {
                    var targetExpression = methodCallExpression.Arguments[0];

                    MemberExpression memberExpression;
                    while ((memberExpression = targetExpression as MemberExpression) != null)
                    {
                        targetExpression = memberExpression.Expression;
                    }

                    var querySourceReferenceExpression
                        = targetExpression as QuerySourceReferenceExpression;

                    if (querySourceReferenceExpression == null
                        || querySource == null
                        || querySource == querySourceReferenceExpression.ReferencedQuerySource)
                    {
                        var entityType
                            = QueryCompilationContext.Model
                                .FindEntityType(methodCallExpression.Arguments[0].Type);

                        if (entityType != null)
                        {
                            var propertyName = (string)((ConstantExpression)methodCallExpression.Arguments[1]).Value;
                            var property = entityType.FindProperty(propertyName);

                            if (property != null)
                            {
                                return methodCallBinder(
                                    property,
                                    querySourceReferenceExpression?.ReferencedQuerySource);
                            }
                        }
                    }
                }
            }

            return default(TResult);
        }
    }
}
