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
using Microsoft.Data.Entity.Query.ExpressionTreeVisitors;
using Microsoft.Data.Entity.Query.ResultOperators;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.Logging;
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
        public static readonly ParameterExpression QueryContextParameter
            = Expression.Parameter(typeof(QueryContext));

        public static readonly ParameterExpression QuerySourceScopeParameter
            = Expression.Parameter(typeof(QuerySourceScope));

        private readonly QuerySourceMapping _querySourceMapping = new QuerySourceMapping();
        private readonly QueryCompilationContext _queryCompilationContext;

        private Expression _expression;
        private StreamedSequenceInfo _streamedSequenceInfo;

        private ISet<IQuerySource> _querySourcesRequiringMaterialization;
        private ICollection<QueryAnnotation> _queryAnnotations;

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

        protected abstract ExpressionTreeVisitor CreateQueryingExpressionTreeVisitor([NotNull] IQuerySource querySource);

        protected virtual ExpressionTreeVisitor CreateProjectionExpressionTreeVisitor()
        {
            return new ProjectionExpressionTreeVisitor(this);
        }

        protected virtual ExpressionTreeVisitor CreateOrderingExpressionTreeVisitor([NotNull] Ordering ordering)
        {
            return new DefaultQueryExpressionTreeVisitor(this);
        }

        public virtual Func<QueryContext, IEnumerable<TResult>> CreateQueryExecutor<TResult>([NotNull] QueryModel queryModel)
        {
            Check.NotNull(queryModel, nameof(queryModel));

            using (QueryCompilationContext.Logger.BeginScope(this))
            {
                QueryCompilationContext.Logger.WriteInformation(queryModel, Strings.LogCompilingQueryModel);

                _blockTaskExpressions = false;

                _queryAnnotations = ExtractQueryAnnotations(queryModel);

                OptimizeQueryModel(queryModel);

                VisitQueryModel(queryModel);

                SingleResultToSequence(queryModel, typeof(TResult));

                IncludeNavigations(queryModel, typeof(TResult));

                TrackEntitiesInResults<TResult>(queryModel);

                InterceptExceptions(typeof(TResult));

                return CreateExecutorLambda<IEnumerable<TResult>>();
            }
        }

        public virtual Func<QueryContext, IAsyncEnumerable<TResult>> CreateAsyncQueryExecutor<TResult>([NotNull] QueryModel queryModel)
        {
            Check.NotNull(queryModel, nameof(queryModel));

            using (QueryCompilationContext.Logger.BeginScope(this))
            {
                QueryCompilationContext.Logger.WriteInformation(queryModel, Strings.LogCompilingQueryModel);

                _blockTaskExpressions = false;

                _queryAnnotations = ExtractQueryAnnotations(queryModel);

                OptimizeQueryModel(queryModel);

                VisitQueryModel(queryModel);

                AsyncSingleResultToSequence(queryModel, typeof(TResult));

                IncludeNavigations(queryModel, typeof(TResult));

                TrackEntitiesInResults<TResult>(queryModel);

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
                        .MakeGenericMethod(resultType),
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

            new QueryOptimizer(_queryAnnotations).VisitQueryModel(queryModel);

            QueryCompilationContext.Logger
                .WriteInformation(queryModel, Strings.LogOptimizedQueryModel);
        }

        protected virtual void SingleResultToSequence(
            [NotNull] QueryModel queryModel,
            [NotNull] Type resultType)
        {
            Check.NotNull(queryModel, nameof(queryModel));
            Check.NotNull(resultType, nameof(resultType));

            if (!(queryModel.GetOutputDataInfo() is StreamedSequenceInfo))
            {
                _expression
                    = Expression.Call(
                        LinqOperatorProvider.ToSequence
                            .MakeGenericMethod(resultType),
                        _expression);
            }
        }

        protected virtual void AsyncSingleResultToSequence(
            [NotNull] QueryModel queryModel,
            [NotNull] Type resultType)
        {
            Check.NotNull(queryModel, nameof(queryModel));
            Check.NotNull(resultType, nameof(resultType));

            if (!(queryModel.GetOutputDataInfo() is StreamedSequenceInfo))
            {
                _expression
                    = Expression.Call(
                        _taskToSequence.MakeGenericMethod(resultType),
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

            var querySourceTracingExpressionTreeVisitor
                = new QuerySourceTracingExpressionTreeVisitor();

            foreach (var include
                in from queryAnnotation in _queryAnnotations
                    let includeResultOperator = queryAnnotation.ResultOperator as IncludeResultOperator
                    where includeResultOperator != null
                    let navigationPath
                        = BindNavigationPathMemberExpression(
                            (MemberExpression)includeResultOperator.NavigationPropertyPath,
                            (ns, _) => BindChainedNavigations(ns, includeResultOperator.ChainedNavigationProperties).ToArray())
                    orderby navigationPath != null
                            && navigationPath.First().PointsToPrincipal
                    select new
                        {
                            navigationPath,
                            queryAnnotation.QuerySource,
                            includeResultOperator.NavigationPropertyPath
                        })
            {
                if (include.navigationPath != null)
                {
                    var resultQuerySourceReferenceExpression
                        = querySourceTracingExpressionTreeVisitor
                            .FindResultQuerySourceReferenceExpression(
                                queryModel.SelectClause.Selector,
                                include.QuerySource);

                    if (resultQuerySourceReferenceExpression != null)
                    {
                        var accessorLambda
                            = AccessorFindingExpressionTreeVisitor
                                .FindAccessorLambda(
                                    resultQuerySourceReferenceExpression,
                                    queryModel.SelectClause.Selector,
                                    Expression.Parameter(queryModel.SelectClause.Selector.Type));

                        QueryCompilationContext.Logger
                            .WriteInformation(
                                include.navigationPath.Join("."),
                                Strings.LogIncludingNavigation);

                        IncludeNavigations(
                            include.QuerySource,
                            resultType,
                            accessorLambda,
                            include.navigationPath,
                            QuerySourceRequiresTracking(include.QuerySource));
                    }
                }
                else
                {
                    throw new InvalidOperationException(
                        Strings.IncludeNonBindableExpression(include.NavigationPropertyPath));
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
                                .TryGetEntityType(propertyInfo.DeclaringType)
                        select entityType?.TryGetNavigation(propertyInfo.Name))
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

            if (!typeof(TResult).GetTypeInfo()
                .IsAssignableFrom(queryModel.SelectClause.Selector.Type.GetTypeInfo()))
            {
                return;
            }

            var querySourceReferenceExpressionsToTrack
                = new EntityResultFindingExpressionTreeVisitor(QueryCompilationContext.Model)
                    .FindEntitiesInResult(queryModel.SelectClause.Selector)
                    .Where(qsre => !_queryAnnotations
                        .Any(qa => qa.ResultOperator is AsNoTrackingResultOperator
                                   && qa.QuerySource == qsre.ReferencedQuerySource))
                    .ToList();

            if (querySourceReferenceExpressionsToTrack.Any())
            {
                QueryCompilationContext.Logger
                    .WriteInformation(
                        querySourceReferenceExpressionsToTrack,
                        qsres => Strings.LogTrackingQuerySources(
                            qsres.Select(qsre => qsre.ReferencedQuerySource.ItemName).Join()));

                _expression
                    = Expression.Call(
                        LinqOperatorProvider.TrackEntities
                            .MakeGenericMethod(
                                typeof(TResult),
                                queryModel.SelectClause.Selector.Type),
                        _expression,
                        QueryContextParameter,
                        Expression.Constant(
                            _getEntityAccessors
                                .MakeGenericMethod(queryModel.SelectClause.Selector.Type)
                                .Invoke(null, new object[]
                                    {
                                        querySourceReferenceExpressionsToTrack,
                                        queryModel.SelectClause.Selector
                                    })));
            }
        }

        private static readonly MethodInfo _getEntityAccessors
            = typeof(EntityQueryModelVisitor)
                .GetTypeInfo().GetDeclaredMethod("GetEntityAccessors");

        [UsedImplicitly]
        private static ICollection<Func<TResult, object>> GetEntityAccessors<TResult>(
            IEnumerable<QuerySourceReferenceExpression> querySourceReferenceExpressions,
            Expression selector)
        {
            return
                (from qsre in querySourceReferenceExpressions
                    select
                        (Func<TResult, object>)
                            AccessorFindingExpressionTreeVisitor
                                .FindAccessorLambda(
                                    qsre,
                                    selector,
                                    Expression.Parameter(typeof(TResult)))
                                .Compile()
                    )
                    .ToList();
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
                .WriteInformation(_expression, _ => Strings.LogCompiledQueryFunction);

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

            if (_queryAnnotations == null)
            {
                return true;
            }

            return _queryAnnotations.Where(en => en.ResultOperator is AsNoTrackingResultOperator)
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

            _querySourceMapping.AddMapping(
                fromClause,
                QuerySourceScope.GetResult(QuerySourceScopeParameter, fromClause, elementType));

            if (fromClause.ItemType.GetTypeInfo().IsGenericType
                && fromClause.ItemType.GetTypeInfo().GetGenericTypeDefinition() == typeof(IGrouping<,>))
            {
                queryModel.TransformExpressions(ReplaceClauseReferences);
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
                        QuerySourceScope
                            .Create(
                                joinClause,
                                innerElementScoped
                                    ? QuerySourceScope.GetResult(innerItemParameter, joinClause, innerElementType)
                                    : innerItemParameter,
                                QuerySourceScopeParameter),
                        QuerySourceScopeParameter, innerItemParameter));

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

            Type innerElementType;

            if (innerElementScoped)
            {
                innerElementType = innerSequenceType.GetTypeInfo().GenericTypeArguments[0];

                _querySourceMapping.AddMapping(
                    groupJoinClause.JoinClause,
                    QuerySourceScope.GetResult(innerItemParameter, groupJoinClause.JoinClause, innerElementType));
            }
            else
            {
                innerElementType = innerSequenceType;

                _querySourceMapping.AddMapping(groupJoinClause.JoinClause, innerItemParameter);
            }

            var innerKeySelector
                = ReplaceClauseReferences(groupJoinClause.JoinClause.InnerKeySelector, groupJoinClause);

            var innerItemsParameter
                = Expression.Parameter(innerExpression.Type);

            _expression
                = Expression.Call(
                    LinqOperatorProvider.GroupJoin.MakeGenericMethod(
                        typeof(QuerySourceScope),
                        innerSequenceType,
                        outerKeySelector.Type,
                        typeof(QuerySourceScope)),
                    _expression,
                    innerExpression,
                    Expression.Lambda(outerKeySelector, QuerySourceScopeParameter),
                    Expression.Lambda(innerKeySelector, innerItemParameter),
                    Expression.Lambda(
                        QuerySourceScope
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
                                            QuerySourceScope.GetResult(
                                                innerItemParameter,
                                                groupJoinClause.JoinClause,
                                                innerElementType),
                                            innerItemParameter))
                                    : (Expression)innerItemsParameter,
                                QuerySourceScopeParameter),
                        QuerySourceScopeParameter, innerItemsParameter));

            var expressionTypeInfo = _expression.Type.GetTypeInfo();

            _querySourceMapping.AddMapping(
                groupJoinClause,
                QuerySourceScope
                    .GetResult(
                        QuerySourceScopeParameter,
                        groupJoinClause,
                        expressionTypeInfo.GetGenericTypeDefinition()
                            .MakeGenericType(innerElementType)));

            if (expressionTypeInfo.GetGenericTypeDefinition() == typeof(IAsyncEnumerable<>))
            {
                queryModel.TransformExpressions(ReplaceClauseReferences);
            }
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
            [NotNull] Ordering ordering, [NotNull] QueryModel queryModel, [NotNull] OrderByClause orderByClause, int index)
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
                = CreateOrderingExpressionTreeVisitor(ordering)
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
                    CreateProjectionExpressionTreeVisitor()
                        .VisitExpression(selectClause.Selector));

            _expression
                = Expression.Call(
                    LinqOperatorProvider.Select
                        .MakeGenericMethod(typeof(QuerySourceScope), selector.Type),
                    _expression,
                    Expression.Lambda(selector, QuerySourceScopeParameter));

            _streamedSequenceInfo
                = (StreamedSequenceInfo)selectClause.GetOutputDataInfo()
                    .AdjustDataType(typeof(IEnumerable<>));
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

                _streamedSequenceInfo
                    = resultOperator.GetOutputDataInfo(_streamedSequenceInfo)
                        as StreamedSequenceInfo;
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

        private Expression ReplaceClauseReferences(Expression expression)
        {
            return new PropertyAccessBindingExpressionTreeVisitor(_querySourceMapping, this)
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

        protected Expression BindReadValueMethod(Type memberType, Expression expression, int index)
        {
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
                        .TryGetEntityType(memberExpression.Expression.Type);

                if (entityType == null)
                {
                    break;
                }

                var property
                    = (IPropertyBase)entityType.TryGetProperty(memberExpression.Member.Name)
                      ?? entityType.TryGetNavigation(memberExpression.Member.Name);

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
                var querySourceReferenceExpression
                    = methodCallExpression.Arguments[0] as QuerySourceReferenceExpression;

                if (querySourceReferenceExpression != null
                    && (querySource == null
                        || querySource == querySourceReferenceExpression.ReferencedQuerySource))
                {
                    var entityType
                        = QueryCompilationContext.Model
                            .TryGetEntityType(
                                querySourceReferenceExpression.ReferencedQuerySource.ItemType);

                    if (entityType != null)
                    {
                        var propertyName = (string)((ConstantExpression)methodCallExpression.Arguments[1]).Value;
                        var property = entityType.TryGetProperty(propertyName);

                        if (property != null)
                        {
                            return methodCallBinder(
                                property,
                                querySourceReferenceExpression.ReferencedQuerySource);
                        }
                    }
                }
            }

            return default(TResult);
        }
    }
}
