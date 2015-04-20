// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
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
using Microsoft.Data.Entity.Query.ExpressionTreeVisitors;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.Logging;
using Remotion.Linq;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Clauses.ExpressionTreeVisitors;
using Remotion.Linq.Clauses.ResultOperators;
using Remotion.Linq.Clauses.StreamedData;
using Remotion.Linq.Parsing;

namespace Microsoft.Data.Entity.Query
{
    public abstract class EntityQueryModelVisitor : QueryModelVisitorBase
    {
        public static readonly ParameterExpression QueryContextParameter
            = Expression.Parameter(typeof(QueryContext));

        public static readonly ParameterExpression QuerySourceScopeParameter
            = Expression.Parameter(typeof(QuerySourceScope));

        private readonly QuerySourceMapping _querySourceMapping;

        private readonly QueryCompilationContext _queryCompilationContext;

        private Expression _expression;
        private StreamedSequenceInfo _streamedSequenceInfo;

        private ISet<IQuerySource> _querySourcesRequiringMaterialization;

        // TODO: Can these be non-blocking?
        private bool _blockTaskExpressions = true;

        protected EntityQueryModelVisitor(
            [NotNull] QueryCompilationContext queryCompilationContext)
        {
            Check.NotNull(queryCompilationContext, nameof(queryCompilationContext));

            _queryCompilationContext = queryCompilationContext;
            _querySourceMapping = _queryCompilationContext.QuerySourceMapping;
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

        public virtual bool IsWrappingResults { get; private set; }

        protected abstract ExpressionTreeVisitor CreateQueryingExpressionTreeVisitor([NotNull] IQuerySource querySource);

        protected virtual ExpressionTreeVisitor CreateProjectionExpressionTreeVisitor([NotNull] IQuerySource querySource)
        {
            return new ProjectionExpressionTreeVisitor(this, querySource);
        }

        protected virtual ExpressionTreeVisitor CreateOrderingExpressionTreeVisitor(
            [NotNull] Ordering ordering, [NotNull] IQuerySource querySource)
        {
            return new DefaultQueryExpressionTreeVisitor(this, querySource);
        }

        public virtual Func<QueryContext, IEnumerable<TResult>> CreateQueryExecutor<TResult>([NotNull] QueryModel queryModel)
        {
            Check.NotNull(queryModel, nameof(queryModel));

            using (QueryCompilationContext.Logger.BeginScopeImpl(this))
            {
                QueryCompilationContext.Logger.LogInformation(queryModel, Strings.LogCompilingQueryModel);

                _blockTaskExpressions = false;

                QueryCompilationContext.QueryAnnotations = ExtractQueryAnnotations(queryModel);

                OptimizeQueryModel(queryModel);

                VisitQueryModel(queryModel);

                SingleResultToSequence(queryModel);

                IncludeNavigations(queryModel, typeof(TResult));

                TrackEntitiesInResults<TResult>(queryModel);

                UnwrapQueryResults(queryModel);

                InterceptExceptions(typeof(TResult));

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

                QueryCompilationContext.QueryAnnotations = ExtractQueryAnnotations(queryModel);

                OptimizeQueryModel(queryModel);

                VisitQueryModel(queryModel);

                AsyncSingleResultToSequence(queryModel);

                IncludeNavigations(queryModel, typeof(TResult));

                TrackEntitiesInResults<TResult>(queryModel);

                UnwrapQueryResults(queryModel);

                InterceptExceptions(typeof(TResult));

                return CreateExecutorLambda<IAsyncEnumerable<TResult>>();
            }
        }

        protected virtual void InterceptExceptions([NotNull] Type resultType)
        {
            Check.NotNull(resultType, nameof(resultType));

            _expression
                = Expression.Call(
                    LinqOperatorProvider.InterceptExceptions
                        .MakeGenericMethod(_expression.Type.GetSequenceType()),
                    Expression.Lambda(_expression),
                    QueryContextParameter);
        }

        protected virtual ICollection<QueryAnnotation> ExtractQueryAnnotations([NotNull] QueryModel queryModel)
        {
            Check.NotNull(queryModel, nameof(queryModel));

            return new QueryAnnotationExtractor().ExtractQueryAnnotations(queryModel);
        }

        protected virtual void OptimizeQueryModel(
            [NotNull] QueryModel queryModel)
        {
            Check.NotNull(queryModel, nameof(queryModel));

            new QueryOptimizer(QueryCompilationContext.QueryAnnotations).VisitQueryModel(queryModel);

            QueryCompilationContext.Logger
                .LogInformation(queryModel, Strings.LogOptimizedQueryModel);
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
                .GetTypeInfo().GetDeclaredMethod("_TaskToSequence");

        [UsedImplicitly]
        private static IAsyncEnumerable<T> _TaskToSequence<T>(Task<T> task)
        {
            return new TaskResultAsyncEnumerable<T>(task);
        }

        protected virtual void IncludeNavigations(
            [NotNull] QueryModel queryModel,
            [NotNull] Type resultType)
        {
            Check.NotNull(queryModel, nameof(queryModel));
            Check.NotNull(resultType, nameof(resultType));

            var includePaths = QueryCompilationContext.QueryAnnotations.OfType<IncludeQueryAnnotation>()
                .Select(annotation =>
                    {
                        var navigationPath = BindNavigationPathMemberExpression(
                            (MemberExpression)annotation.NavigationPropertyPath,
                            (ns, _) => BindChainedNavigations(ns, annotation.ChainedNavigationProperties).ToArray());

                        if (navigationPath == null)
                        {
                            throw new InvalidOperationException(
                                Strings.IncludeNonBindableExpression(annotation.NavigationPropertyPath));
                        }
                        return new
                            {
                                Annotation = annotation,
                                NavigationPath = navigationPath
                            };
                    });

            var querySourceTracingExpressionTreeVisitor
                = new QuerySourceTracingExpressionTreeVisitor();

            foreach (var include in includePaths.OrderBy(a => a.NavigationPath.First().PointsToPrincipal()))
            {
                var resultQuerySourceReferenceExpression
                    = querySourceTracingExpressionTreeVisitor
                        .FindResultQuerySourceReferenceExpression(
                            queryModel.SelectClause.Selector,
                            include.Annotation.QuerySource);

                if (resultQuerySourceReferenceExpression != null)
                {
                    var accessorLambda
                        = AccessorFindingExpressionTreeVisitor
                            .FindAccessorLambda(
                                resultQuerySourceReferenceExpression,
                                queryModel.SelectClause.Selector,
                                Expression.Parameter(queryModel.SelectClause.Selector.Type));

                    QueryCompilationContext.Logger
                        .LogInformation(
                            include.NavigationPath.Join("."),
                            Strings.LogIncludingNavigation);

                    IncludeNavigations(
                        include.Annotation.QuerySource,
                        resultType,
                        accessorLambda,
                        include.NavigationPath,
                        QuerySourceRequiresTracking(include.Annotation.QuerySource));
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
            [NotNull] IQuerySource querySource,
            [NotNull] Type resultType,
            [NotNull] LambdaExpression accessorLambda,
            [NotNull] IReadOnlyList<INavigation> navigationPath,
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
                = new EntityResultFindingExpressionTreeVisitor(QueryCompilationContext.Model)
                    .FindEntitiesInResult(queryModel.SelectClause.Selector)
                    .Where(eti =>
                        QueryCompilationContext.QueryAnnotations
                            .OfType<AsNoTrackingQueryAnnotation>()
                            .All(qa => qa.QuerySource != eti.QuerySource))
                    .ToList();

            if (entityTrackingInfos.Any())
            {
                QueryCompilationContext.Logger
                    .LogInformation(
                        entityTrackingInfos,
                        qsres => Strings.LogTrackingQuerySources(
                            qsres.Select(eti => eti.QuerySource.ItemName).Join()));

                var resultItemTypeInfo
                    = (_streamedSequenceInfo?.ResultItemType
                       ?? _expression.Type.GetSequenceType())
                        .GetTypeInfo();

                MethodInfo trackingMethod;

                if (resultItemTypeInfo.IsGenericType
                    && (resultItemTypeInfo.GetGenericTypeDefinition() == typeof(IGrouping<,>)
                        || resultItemTypeInfo.GetGenericTypeDefinition() == typeof(IAsyncGrouping<,>)))
                {
                    trackingMethod
                        = LinqOperatorProvider.TrackGroupedEntities
                            .MakeGenericMethod(
                                resultItemTypeInfo.GenericTypeArguments[0],
                                resultItemTypeInfo.GenericTypeArguments[1]
                                    .GetTypeInfo()
                                    .GenericTypeArguments[0],
                                queryModel.SelectClause.Selector.Type);
                }
                else
                {
                    trackingMethod
                        = LinqOperatorProvider.TrackEntities
                            .MakeGenericMethod(
                                resultItemTypeInfo.GenericTypeArguments[0],
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
                .GetTypeInfo().GetDeclaredMethod("GetEntityAccessors");

        [UsedImplicitly]
        private static ICollection<Func<TResult, object>> GetEntityAccessors<TResult>(
            IEnumerable<EntityTrackingInfo> entityTrackingInfos,
            Expression selector)
        {
            return
                (from entityTrackingInfo in entityTrackingInfos
                    select
                        (Func<TResult, object>)
                            AccessorFindingExpressionTreeVisitor
                                .FindAccessorLambda(
                                    entityTrackingInfo.QuerySourceReferenceExpression,
                                    selector,
                                    Expression.Parameter(typeof(TResult)))
                                .Compile()
                    )
                    .ToList();
        }

        private void UnwrapQueryResults([NotNull] QueryModel queryModel)
        {
            Check.NotNull(queryModel, nameof(queryModel));

            var sequenceType = _expression.Type.GetSequenceType();
            var sequenceTypeInfo = sequenceType.GetTypeInfo();

            if (sequenceTypeInfo.IsGenericType)
            {
                var genericTypeDefinition
                    = sequenceTypeInfo.GetGenericTypeDefinition();

                if (genericTypeDefinition == typeof(IGrouping<,>)
                    || genericTypeDefinition == typeof(IAsyncGrouping<,>))
                {
                    _expression
                        = Expression.Call(
                            LinqOperatorProvider.UnwrapGroupedQueryResults
                                .MakeGenericMethod(
                                    sequenceTypeInfo.GenericTypeArguments[0],
                                    sequenceTypeInfo.GenericTypeArguments[1]
                                        .GetTypeInfo()
                                        .GenericTypeArguments[0]),
                            _expression);
                }
                else if (genericTypeDefinition == typeof(QuerySourceScope<>))
                {
                    _expression
                        = Expression.Call(
                            LinqOperatorProvider.UnwrapQueryResults
                                .MakeGenericMethod(
                                    sequenceTypeInfo.GenericTypeArguments[0]),
                            _expression);
                }
            }
        }

        protected virtual Func<QueryContext, TResults> CreateExecutorLambda<TResults>()
        {
            var queryExecutor
                = Expression
                    .Lambda<Func<QueryContext, QuerySourceScope, TResults>>(
                        _expression, QueryContextParameter, QuerySourceScopeParameter)
                    .Compile();

            // TODO: Format expression in log (query plan)
            QueryCompilationContext.Logger
                .LogInformation(_expression, _ => Strings.LogCompiledQueryFunction);

            return qc => queryExecutor(qc, null);
        }

        public virtual bool QuerySourceRequiresMaterialization([NotNull] IQuerySource querySource)
        {
            Check.NotNull(querySource, nameof(querySource));

            return _querySourcesRequiringMaterialization.Contains(querySource);
        }

        public virtual bool QuerySourceRequiresTracking([NotNull] IQuerySource querySource)
        {
            Check.NotNull(querySource, nameof(querySource));

            if (QueryCompilationContext.QueryAnnotations == null)
            {
                return true;
            }

            return QueryCompilationContext.QueryAnnotations
                .OfType<AsNoTrackingQueryAnnotation>()
                .All(qa => qa.QuerySource != querySource);
        }

        public override void VisitQueryModel([NotNull] QueryModel queryModel)
        {
            Check.NotNull(queryModel, nameof(queryModel));

            var requiresEntityMaterializationExpressionTreeVisitor
                = new RequiresMaterializationExpressionTreeVisitor(this);

            queryModel.TransformExpressions(requiresEntityMaterializationExpressionTreeVisitor.VisitExpression);

            _querySourcesRequiringMaterialization
                = requiresEntityMaterializationExpressionTreeVisitor.QuerySourcesRequiringMaterialization;

            foreach (var groupJoinClause in queryModel.BodyClauses.OfType<GroupJoinClause>())
            {
                _querySourcesRequiringMaterialization.Add(groupJoinClause.JoinClause);
            }

            base.VisitQueryModel(queryModel);

            if (_blockTaskExpressions)
            {
                _expression
                    = new TaskBlockingExpressionTreeVisitor()
                        .VisitExpression(_expression);
            }
        }

        public override void VisitMainFromClause(
            [NotNull] MainFromClause fromClause, [NotNull] QueryModel queryModel)
        {
            Check.NotNull(fromClause, nameof(fromClause));
            Check.NotNull(queryModel, nameof(queryModel));

            _expression
                = ReplaceClauseReferences(fromClause.FromExpression, fromClause);

            var sequenceType = _expression.Type.GetSequenceType();

            var elementScoped
                = sequenceType.IsConstructedGenericType
                  && sequenceType.GetGenericTypeDefinition() == typeof(QuerySourceScope<>);

            Type elementType;

            if (elementScoped)
            {
                elementType = sequenceType.GetTypeInfo().GenericTypeArguments[0];

                if (fromClause.FromExpression is QuerySourceReferenceExpression)
                {
                    var itemParameter = Expression.Parameter(sequenceType);

                    _expression
                        = Expression.Call(
                            LinqOperatorProvider.Select
                                .MakeGenericMethod(sequenceType, typeof(QuerySourceScope)),
                            _expression,
                            Expression.Lambda(
                                QuerySourceScope
                                    .Create(
                                        fromClause,
                                        QuerySourceScope
                                            .GetResult(itemParameter, sequenceType.GenericTypeArguments[0]),
                                        itemParameter),
                                itemParameter));
                }
            }
            else
            {
                elementType = sequenceType;

                _expression = CreateScope(_expression, elementType, fromClause);
            }

            _expression
                = Expression.Call(
                    LinqOperatorProvider.SelectMany
                        .MakeGenericMethod(typeof(QuerySourceScope), typeof(QuerySourceScope)),
                    Expression.Call(
                        LinqOperatorProvider.ToSequence
                            .MakeGenericMethod(typeof(QuerySourceScope)),
                        QuerySourceScopeParameter),
                    Expression.Lambda(_expression, QuerySourceScopeParameter));

            if (!_querySourceMapping.ContainsMapping(fromClause))
            {
                _querySourceMapping.AddMapping(
                    fromClause,
                    QuerySourceScope
                        .GetResult(QuerySourceScopeParameter, fromClause, elementType));
            }
        }

        public override void VisitAdditionalFromClause(
            [NotNull] AdditionalFromClause fromClause, [NotNull] QueryModel queryModel, int index)
        {
            Check.NotNull(fromClause, nameof(fromClause));
            Check.NotNull(queryModel, nameof(queryModel));

            var innerExpression
                = ReplaceClauseReferences(fromClause.FromExpression, fromClause);

            var innerSequenceType = innerExpression.Type.GetSequenceType();

            var innerElementScoped
                = innerSequenceType.IsConstructedGenericType
                  && innerSequenceType.GetGenericTypeDefinition() == typeof(QuerySourceScope<>);

            Type innerElementType;

            if (innerElementScoped)
            {
                innerElementType
                    = innerSequenceType.GetTypeInfo().GenericTypeArguments[0];

                var innerItemParameter = Expression.Parameter(innerSequenceType);

                innerExpression
                    = Expression.Call(
                        LinqOperatorProvider.Select
                            .MakeGenericMethod(innerSequenceType, typeof(QuerySourceScope)),
                        innerExpression,
                        Expression.Lambda(
                            QuerySourceScope
                                .Join(
                                    fromClause,
                                    innerItemParameter,
                                    QuerySourceScopeParameter),
                            innerItemParameter));
            }
            else
            {
                innerElementType = innerSequenceType;

                innerExpression = CreateScope(innerExpression, innerElementType, fromClause);
            }

            _expression
                = Expression.Call(
                    LinqOperatorProvider.SelectMany
                        .MakeGenericMethod(typeof(QuerySourceScope), typeof(QuerySourceScope)),
                    _expression,
                    Expression.Lambda(innerExpression, QuerySourceScopeParameter));

            _querySourceMapping.AddMapping(
                fromClause,
                QuerySourceScope.GetResult(QuerySourceScopeParameter, fromClause, innerElementType));
        }

        public override void VisitJoinClause(
            [NotNull] JoinClause joinClause, [NotNull] QueryModel queryModel, int index)
        {
            Check.NotNull(joinClause, nameof(joinClause));
            Check.NotNull(queryModel, nameof(queryModel));

            var outerKeySelector
                = ReplaceClauseReferences(joinClause.OuterKeySelector, joinClause);

            var innerSequenceExpression
                = ReplaceClauseReferences(joinClause.InnerSequence, joinClause);

            var innerSequenceType
                = innerSequenceExpression.Type.GetSequenceType();

            var innerItemParameter
                = Expression.Parameter(innerSequenceType);

            var innerElementScoped
                = innerSequenceType.IsConstructedGenericType
                  && innerSequenceType.GetGenericTypeDefinition() == typeof(QuerySourceScope<>);

            Type innerElementType;

            if (innerElementScoped)
            {
                innerElementType = innerSequenceType.GetTypeInfo().GenericTypeArguments[0];

                _querySourceMapping.AddMapping(
                    joinClause,
                    QuerySourceScope.GetResult(innerItemParameter, joinClause, innerElementType));
            }
            else
            {
                innerElementType = innerSequenceType;

                _querySourceMapping.AddMapping(joinClause, innerItemParameter);
            }

            var innerKeySelector
                = ReplaceClauseReferences(joinClause.InnerKeySelector, joinClause);

            _expression
                = Expression.Call(
                    LinqOperatorProvider.Join.MakeGenericMethod(
                        typeof(QuerySourceScope),
                        innerSequenceType,
                        outerKeySelector.Type,
                        typeof(QuerySourceScope)),
                    _expression,
                    innerSequenceExpression,
                    Expression.Lambda(outerKeySelector, QuerySourceScopeParameter),
                    Expression.Lambda(innerKeySelector, innerItemParameter),
                    Expression.Lambda(
                        innerElementScoped
                            ? QuerySourceScope
                                .Join(
                                    joinClause,
                                    innerItemParameter,
                                    QuerySourceScopeParameter)
                            : QuerySourceScope
                                .Create(
                                    joinClause,
                                    innerItemParameter,
                                    QuerySourceScopeParameter),
                        QuerySourceScopeParameter,
                        innerItemParameter));

            _querySourceMapping.ReplaceMapping(
                joinClause,
                QuerySourceScope.GetResult(QuerySourceScopeParameter, joinClause, innerElementType));
        }

        public override void VisitGroupJoinClause(
            [NotNull] GroupJoinClause groupJoinClause, [NotNull] QueryModel queryModel, int index)
        {
            Check.NotNull(groupJoinClause, nameof(groupJoinClause));
            Check.NotNull(queryModel, nameof(queryModel));

            var outerKeySelector
                = ReplaceClauseReferences(groupJoinClause.JoinClause.OuterKeySelector, groupJoinClause);

            var innerExpression
                = ReplaceClauseReferences(groupJoinClause.JoinClause.InnerSequence, groupJoinClause.JoinClause);

            var innerSequenceType
                = innerExpression.Type.GetSequenceType();

            var innerItemParameter
                = Expression.Parameter(innerSequenceType);

            var innerElementScoped
                = innerSequenceType.IsConstructedGenericType
                  && innerSequenceType.GetGenericTypeDefinition() == typeof(QuerySourceScope<>);

            if (innerElementScoped)
            {
                var innerElementType = innerSequenceType.GetTypeInfo().GenericTypeArguments[0];

                _querySourceMapping.AddMapping(
                    groupJoinClause.JoinClause,
                    QuerySourceScope.GetResult(innerItemParameter, groupJoinClause.JoinClause, innerElementType));
            }
            else
            {
                _querySourceMapping.AddMapping(groupJoinClause.JoinClause, innerItemParameter);
            }

            var innerKeySelector
                = ReplaceClauseReferences(groupJoinClause.JoinClause.InnerKeySelector, groupJoinClause);

            var innerItemsParameter
                = Expression.Parameter(innerExpression.Type);

            var innerElementSelector
                = innerElementScoped
                    ? QuerySourceScope
                        .Join(
                            groupJoinClause.JoinClause,
                            innerItemParameter,
                            QuerySourceScopeParameter)
                    : QuerySourceScope
                        .Create(
                            groupJoinClause.JoinClause,
                            innerItemParameter,
                            QuerySourceScopeParameter);

            var resultSelector
                = QuerySourceScope
                    .Create(
                        groupJoinClause,
                        Expression.Call(
                            LinqOperatorProvider.Select
                                .MakeGenericMethod(innerSequenceType, innerElementSelector.Type),
                            innerItemsParameter,
                            Expression.Lambda(
                                innerElementSelector,
                                innerItemParameter)),
                        QuerySourceScopeParameter);

            _expression
                = Expression.Call(
                    LinqOperatorProvider.GroupJoin
                        .MakeGenericMethod(
                            typeof(QuerySourceScope),
                            innerSequenceType,
                            outerKeySelector.Type,
                            resultSelector.Type),
                    _expression,
                    innerExpression,
                    Expression.Lambda(outerKeySelector, QuerySourceScopeParameter),
                    Expression.Lambda(innerKeySelector, innerItemParameter),
                    Expression.Lambda(resultSelector, QuerySourceScopeParameter, innerItemsParameter));

            _querySourceMapping.AddMapping(
                groupJoinClause,
                QuerySourceScope
                    .GetResult(QuerySourceScopeParameter, groupJoinClause, innerItemsParameter.Type));
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
                        .MakeGenericMethod(typeof(QuerySourceScope)),
                    _expression,
                    Expression.Lambda(predicate, QuerySourceScopeParameter));
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
            var parameterExpression = QuerySourceScopeParameter;
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
                = CreateOrderingExpressionTreeVisitor(ordering, queryModel.MainFromClause)
                    .VisitExpression(ordering.Expression);

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
                    CreateProjectionExpressionTreeVisitor(queryModel.MainFromClause)
                        .VisitExpression(selectClause.Selector),
                    inProjection: true);

            if (_querySourcesRequiringMaterialization.Any()
                && !(queryModel.GetOutputDataInfo() is StreamedScalarValueInfo))
            {
                selector
                    = QuerySourceScope.Create(
                        queryModel.MainFromClause,
                        selector,
                        QuerySourceScopeParameter);

                IsWrappingResults = true;
            }

            _expression
                = Expression.Call(
                    LinqOperatorProvider.Select
                        .MakeGenericMethod(typeof(QuerySourceScope), selector.Type),
                    _expression,
                    Expression.Lambda(selector, QuerySourceScopeParameter));

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
                        .MakeGenericMethod(elementType, typeof(QuerySourceScope)),
                    expression,
                    Expression.Lambda(
                        QuerySourceScope
                            .Create(querySource, innerItemParameter, QuerySourceScopeParameter),
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
                        CreateQueryingExpressionTreeVisitor(querySource)
                            .VisitExpression(expression)));
        }

        public virtual Expression ReplaceClauseReferences(
            [NotNull] Expression expression, bool inProjection = false)
        {
            Check.NotNull(expression, nameof(expression));

            return
                new MemberAccessBindingExpressionTreeVisitor(_querySourceMapping, this, inProjection)
                    .VisitExpression(expression);
        }

        public virtual Expression BindMethodCallToValueReader(
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

        public virtual Expression BindMemberToValueReader(
            [NotNull] MemberExpression memberExpression,
            [NotNull] Expression expression)
        {
            Check.NotNull(memberExpression, nameof(memberExpression));
            Check.NotNull(expression, nameof(expression));

            return BindMemberExpression(
                memberExpression,
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

        public virtual TResult BindNavigationMemberExpression<TResult>(
            [NotNull] MemberExpression memberExpression,
            [NotNull] Func<INavigation, IQuerySource, TResult> memberBinder)
        {
            Check.NotNull(memberExpression, nameof(memberExpression));
            Check.NotNull(memberBinder, nameof(memberBinder));

            return BindMemberExpressionCore(memberExpression, null,
                (ps, qs) =>
                    {
                        var navigation = ps.Single() as INavigation;

                        return navigation != null
                            ? memberBinder(navigation, qs)
                            : default(TResult);
                    });
        }

        public virtual TResult BindNavigationPathMemberExpression<TResult>(
            [NotNull] MemberExpression memberExpression,
            [NotNull] Func<IEnumerable<INavigation>, IQuerySource, TResult> memberBinder)
        {
            Check.NotNull(memberExpression, nameof(memberExpression));
            Check.NotNull(memberBinder, nameof(memberBinder));

            return BindMemberExpressionCore(
                memberExpression,
                null,
                (ps, qs) => memberBinder(ps.Cast<INavigation>(), qs));
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
            [NotNull] Func<IProperty, IQuerySource, TResult> memberBinder)
        {
            Check.NotNull(memberExpression, nameof(memberExpression));
            Check.NotNull(memberBinder, nameof(memberBinder));

            return BindMemberExpression(memberExpression, null, memberBinder);
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

            while (memberExpression != null)
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

            if (methodCallExpression.Method.IsGenericMethod
                && ReferenceEquals(
                    methodCallExpression.Method.GetGenericMethodDefinition(),
                    QueryExtensions.PropertyMethodInfo))
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
                    var itemType = methodCallExpression.Arguments[0].Type;
                    var itemTypeInfo = itemType.GetTypeInfo();

                    if (itemTypeInfo.IsGenericType
                        && itemTypeInfo.GetGenericTypeDefinition() == typeof(QuerySourceScope<>))
                    {
                        itemType = itemTypeInfo.GenericTypeArguments[0];
                    }

                    var entityType
                        = QueryCompilationContext.Model
                            .FindEntityType(itemType);

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

            return default(TResult);
        }
    }
}
