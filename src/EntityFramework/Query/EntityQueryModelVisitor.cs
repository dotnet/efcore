// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Query.ExpressionTreeVisitors;
using Microsoft.Data.Entity.Query.ResultOperators;
using Microsoft.Data.Entity.Utilities;
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

        // TODO: Can these be non-blocking?
        private bool _blockTaskExpressions = true;

        protected EntityQueryModelVisitor(
            [NotNull] QueryCompilationContext queryCompilationContext)
        {
            Check.NotNull(queryCompilationContext, "queryCompilationContext");

            _queryCompilationContext = queryCompilationContext;
        }

        public virtual Expression Expression
        {
            get { return _expression; }
            [param: NotNull]
            protected set
            {
                Check.NotNull(value, "value");

                _expression = value;
            }
        }

        public virtual QueryCompilationContext QueryCompilationContext
        {
            get { return _queryCompilationContext; }
        }

        public virtual ILinqOperatorProvider LinqOperatorProvider
        {
            get { return QueryCompilationContext.LinqOperatorProvider; }
        }

        public virtual StreamedSequenceInfo StreamedSequenceInfo
        {
            get { return _streamedSequenceInfo; }
        }

        protected abstract ExpressionTreeVisitor CreateQueryingExpressionTreeVisitor([CanBeNull] IQuerySource querySource);

        protected virtual ExpressionTreeVisitor CreateProjectionExpressionTreeVisitor()
        {
            return new ProjectionExpressionTreeVisitor(this);
        }

        protected virtual ExpressionTreeVisitor CreateOrderingExpressionTreeVisitor(Ordering ordering)
        {
            return new DefaultQueryExpressionTreeVisitor(this);
        }

        public virtual Func<QueryContext, IEnumerable<TResult>> CreateQueryExecutor<TResult>([NotNull] QueryModel queryModel)
        {
            Check.NotNull(queryModel, "queryModel");

            _blockTaskExpressions = false;

            var queryAnnotations = ExtractQueryAnnotations(queryModel);

            OptimizeQueryModel(queryModel, queryAnnotations);

            VisitQueryModel(queryModel);

            SingleResultToSequence(queryModel, typeof(TResult));

            IncludeNavigations(queryModel, typeof(TResult), queryAnnotations);

            TrackEntitiesInResults<TResult>(queryModel, queryAnnotations);

            return CreateExecutorLambda<IEnumerable<TResult>>();
        }

        public virtual Func<QueryContext, IAsyncEnumerable<TResult>> CreateAsyncQueryExecutor<TResult>([NotNull] QueryModel queryModel)
        {
            Check.NotNull(queryModel, "queryModel");

            _blockTaskExpressions = false;

            var queryAnnotations = ExtractQueryAnnotations(queryModel);

            OptimizeQueryModel(queryModel, queryAnnotations);

            VisitQueryModel(queryModel);

            AsyncSingleResultToSequence(queryModel, typeof(TResult));

            IncludeNavigations(queryModel, typeof(TResult), queryAnnotations);

            TrackEntitiesInResults<TResult>(queryModel, queryAnnotations);

            return CreateExecutorLambda<IAsyncEnumerable<TResult>>();
        }

        protected virtual ICollection<QueryAnnotation> ExtractQueryAnnotations([NotNull] QueryModel queryModel)
        {
            Check.NotNull(queryModel, "queryModel");

            return new QueryAnnotationExtractor().ExtractQueryAnnotations(queryModel);
        }

        protected virtual void OptimizeQueryModel(
            [NotNull] QueryModel queryModel,
            [NotNull] ICollection<QueryAnnotation> queryAnnotations)
        {
            Check.NotNull(queryModel, "queryModel");
            Check.NotNull(queryAnnotations, "queryAnnotations");

            new QueryOptimizer(queryAnnotations).VisitQueryModel(queryModel);
        }

        protected virtual void SingleResultToSequence(
            [NotNull] QueryModel queryModel,
            [NotNull] Type resultType)
        {
            Check.NotNull(queryModel, "queryModel");
            Check.NotNull(resultType, "resultType");

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
            Check.NotNull(queryModel, "queryModel");
            Check.NotNull(resultType, "resultType");

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
            [NotNull] Type resultType,
            [NotNull] ICollection<QueryAnnotation> queryAnnotations)
        {
            Check.NotNull(queryModel, "queryModel");
            Check.NotNull(resultType, "resultType");
            Check.NotNull(queryAnnotations, "queryAnnotations");

            var querySourceTracingExpressionTreeVisitor
                = new QuerySourceTracingExpressionTreeVisitor();

            foreach (var queryAnnotation in queryAnnotations)
            {
                var includeResultOperator
                    = queryAnnotation.ResultOperator as IncludeResultOperator;

                if (includeResultOperator != null)
                {
                    var resultQuerySourceReferenceExpression
                        = querySourceTracingExpressionTreeVisitor
                            .FindResultQuerySourceReferenceExpression(
                                queryModel.SelectClause.Selector,
                                queryAnnotation.QuerySource);

                    if (resultQuerySourceReferenceExpression != null)
                    {
                        var accessorLambda
                            = AccessorFindingExpressionTreeVisitor
                                .FindAccessorLambda(
                                    resultQuerySourceReferenceExpression,
                                    queryModel.SelectClause.Selector,
                                    Expression.Parameter(queryModel.SelectClause.Selector.Type));

                        IncludeNavigation(
                            queryAnnotation.QuerySource,
                            resultType,
                            accessorLambda,
                            includeResultOperator.NavigationPropertyPath);
                    }
                }
            }
        }

        protected virtual void IncludeNavigation(
            IQuerySource querySource,
            Type resultType,
            LambdaExpression accessorLambda,
            Expression navigationPropertyPath)
        {
            // template method

            throw new NotImplementedException(Strings.IncludeNotImplemented);
        }

        protected virtual void TrackEntitiesInResults<TResult>(
            [NotNull] QueryModel queryModel,
            [NotNull] ICollection<QueryAnnotation> queryAnnotations)
        {
            Check.NotNull(queryModel, "queryModel");
            Check.NotNull(queryAnnotations, "queryAnnotations");

            if (!typeof(TResult).GetTypeInfo()
                .IsAssignableFrom(queryModel.SelectClause.Selector.Type.GetTypeInfo()))
            {
                return;
            }

            var querySourceReferenceExpressionsToTrack
                = new EntityResultFindingExpressionTreeVisitor(QueryCompilationContext.Model)
                    .FindEntitiesInResult(queryModel.SelectClause.Selector)
                    .Where(qsre => !queryAnnotations
                        .Any(qa => qa.ResultOperator is AsNoTrackingResultOperator
                                   && qa.QuerySource == qsre.ReferencedQuerySource))
                    .ToList();

            if (querySourceReferenceExpressionsToTrack.Any())
            {
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

            return qc => queryExecutor(qc, null);
        }

        public virtual bool QuerySourceRequiresMaterialization([NotNull] IQuerySource querySource)
        {
            Check.NotNull(querySource, "querySource");

            return _querySourcesRequiringMaterialization.Contains(querySource);
        }

        public override void VisitQueryModel([NotNull] QueryModel queryModel)
        {
            Check.NotNull(queryModel, "queryModel");

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
            Check.NotNull(fromClause, "fromClause");
            Check.NotNull(queryModel, "queryModel");

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
                    Expression.Lambda(_expression,
                        new[] { QuerySourceScopeParameter }));

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
            Check.NotNull(fromClause, "fromClause");
            Check.NotNull(queryModel, "queryModel");

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
                    Expression.Lambda(
                        innerExpression,
                        new[] { QuerySourceScopeParameter }));

            _querySourceMapping.AddMapping(
                fromClause,
                QuerySourceScope.GetResult(QuerySourceScopeParameter, fromClause, innerElementType));
        }

        public override void VisitJoinClause(
            [NotNull] JoinClause joinClause, [NotNull] QueryModel queryModel, int index)
        {
            Check.NotNull(joinClause, "joinClause");
            Check.NotNull(queryModel, "queryModel");

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
                        new[] { QuerySourceScopeParameter, innerItemParameter }));

            _querySourceMapping.ReplaceMapping(
                joinClause,
                QuerySourceScope.GetResult(QuerySourceScopeParameter, joinClause, innerElementType));
        }

        public override void VisitGroupJoinClause(
            [NotNull] GroupJoinClause groupJoinClause, [NotNull] QueryModel queryModel, int index)
        {
            Check.NotNull(groupJoinClause, "groupJoinClause");
            Check.NotNull(queryModel, "queryModel");

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
                                            new[] { innerItemParameter }))
                                    : (Expression)innerItemsParameter,
                                QuerySourceScopeParameter),
                        new[] { QuerySourceScopeParameter, innerItemsParameter }));

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
            Check.NotNull(whereClause, "whereClause");
            Check.NotNull(queryModel, "queryModel");

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
            Check.NotNull(ordering, "ordering");
            Check.NotNull(queryModel, "queryModel");
            Check.NotNull(orderByClause, "orderByClause");

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
            Check.NotNull(selectClause, "selectClause");
            Check.NotNull(queryModel, "queryModel");

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
            Check.NotNull(resultOperator, "resultOperator");
            Check.NotNull(queryModel, "queryModel");

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
            Check.NotNull(expression, "expression");
            Check.NotNull(elementType, "elementType");
            Check.NotNull(querySource, "querySource");

            var innerItemParameter = Expression.Parameter(elementType);

            return
                Expression.Call(
                    LinqOperatorProvider.Select
                        .MakeGenericMethod(elementType, typeof(QuerySourceScope)),
                    expression,
                    Expression.Lambda(
                        QuerySourceScope
                            .Create(querySource, innerItemParameter, QuerySourceScopeParameter),
                        new[] { innerItemParameter }));
        }

        public virtual Expression ReplaceClauseReferences(
            [NotNull] Expression expression, [NotNull] IQuerySource querySource)
        {
            Check.NotNull(expression, "expression");
            Check.NotNull(querySource, "querySource");

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
            Check.NotNull(methodCallExpression, "methodCallExpression");
            Check.NotNull(expression, "expression");

            return BindMethodCallExpression(
                methodCallExpression,
                (property, querySource)
                    => BindReadValueMethod(methodCallExpression.Type, expression, property.Index));
        }

        public virtual Expression BindMemberToValueReader(
            [NotNull] MemberExpression memberExpression,
            [NotNull] Expression expression)
        {
            Check.NotNull(memberExpression, "memberExpression");
            Check.NotNull(expression, "expression");

            return BindMemberExpression(
                memberExpression,
                (property, querySource)
                    => BindReadValueMethod(memberExpression.Type, expression, property.Index));
        }

        private static readonly MethodInfo _readValueMethodInfo
            = typeof(IValueReader).GetTypeInfo().GetDeclaredMethod("ReadValue");

        protected static MethodCallExpression BindReadValueMethod(Type memberType, Expression expression, int index)
        {
            return Expression.Call(
                expression,
                _readValueMethodInfo.MakeGenericMethod(memberType),
                Expression.Constant(index));
        }

        public virtual TResult BindNavigationMemberExpression<TResult>(
            [NotNull] MemberExpression memberExpression,
            [NotNull] Func<INavigation, IQuerySource, TResult> memberBinder)
        {
            Check.NotNull(memberExpression, "memberExpression");
            Check.NotNull(memberBinder, "memberBinder");

            return BindMemberExpressionCore(memberExpression, null,
                (p, qs) =>
                    {
                        var navigation = p as INavigation;

                        return navigation != null
                            ? memberBinder(navigation, qs)
                            : default(TResult);
                    });
        }

        public virtual void BindMemberExpression(
            [NotNull] MemberExpression memberExpression,
            [NotNull] Action<IProperty, IQuerySource> memberBinder)
        {
            Check.NotNull(memberExpression, "memberExpression");
            Check.NotNull(memberBinder, "memberBinder");

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
            Check.NotNull(memberExpression, "memberExpression");
            Check.NotNull(memberBinder, "memberBinder");

            return BindMemberExpression(memberExpression, null, memberBinder);
        }

        public virtual TResult BindMemberExpression<TResult>(
            [NotNull] MemberExpression memberExpression,
            [CanBeNull] IQuerySource querySource,
            [NotNull] Func<IProperty, IQuerySource, TResult> memberBinder)
        {
            Check.NotNull(memberExpression, "memberExpression");
            Check.NotNull(memberBinder, "memberBinder");

            return BindMemberExpressionCore(memberExpression, querySource,
                (p, qs) =>
                    {
                        var property = p as IProperty;

                        return property != null
                            ? memberBinder(property, qs)
                            : default(TResult);
                    });
        }

        private TResult BindMemberExpressionCore<TResult>(
            [NotNull] MemberExpression memberExpression,
            [CanBeNull] IQuerySource querySource,
            [NotNull] Func<IPropertyBase, IQuerySource, TResult> memberBinder)
        {
            Check.NotNull(memberExpression, "memberExpression");
            Check.NotNull(memberBinder, "memberBinder");

            var querySourceReferenceExpression
                = memberExpression.Expression as QuerySourceReferenceExpression;

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
                    var property
                        = (IPropertyBase)entityType.TryGetProperty(memberExpression.Member.Name)
                          ?? entityType.TryGetNavigation(memberExpression.Member.Name);

                    if (property != null)
                    {
                        return memberBinder(
                            property,
                            querySourceReferenceExpression.ReferencedQuerySource);
                    }
                }
            }

            return default(TResult);
        }

        public virtual TResult BindMethodCallExpression<TResult>(
            [NotNull] MethodCallExpression methodCallExpression,
            [NotNull] Func<IProperty, IQuerySource, TResult> methodCallBinder)
        {
            Check.NotNull(methodCallExpression, "methodCallExpression");
            Check.NotNull(methodCallBinder, "methodCallBinder");

            return BindMethodCallExpression(methodCallExpression, null, methodCallBinder);
        }

        private static readonly MethodInfo _propertyMethodInfo
            = typeof(QueryExtensions).GetTypeInfo().GetDeclaredMethod("Property");

        public virtual TResult BindMethodCallExpression<TResult>(
            [NotNull] MethodCallExpression methodCallExpression,
            [CanBeNull] IQuerySource querySource,
            [NotNull] Func<IProperty, IQuerySource, TResult> methodCallBinder)
        {
            Check.NotNull(methodCallExpression, "methodCallExpression");
            Check.NotNull(methodCallBinder, "methodCallBinder");

            if (methodCallExpression.Method.IsGenericMethod
                && ReferenceEquals(
                    methodCallExpression.Method.GetGenericMethodDefinition(),
                    _propertyMethodInfo))
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
