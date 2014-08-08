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
        protected static readonly ParameterExpression QueryContextParameter
            = Expression.Parameter(typeof(QueryContext));

        protected static readonly ParameterExpression QuerySourceScopeParameter
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

        public QueryCompilationContext QueryCompilationContext
        {
            get { return _queryCompilationContext; }
        }

        public StreamedSequenceInfo StreamedSequenceInfo
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
            return new DefaultExpressionTreeVisitor(this);
        }

        public Func<QueryContext, IEnumerable<TResult>> CreateQueryExecutor<TResult>([NotNull] QueryModel queryModel)
        {
            Check.NotNull(queryModel, "queryModel");

            _blockTaskExpressions = false;

            VisitQueryModel(queryModel);

            if (_streamedSequenceInfo == null)
            {
                _expression
                    = Expression.Call(
                        _queryCompilationContext.LinqOperatorProvider.ToSequence
                            .MakeGenericMethod(typeof(TResult)),
                        _expression);
            }

            var queryExecutor
                = Expression
                    .Lambda<Func<QueryContext, QuerySourceScope, IEnumerable<TResult>>>(
                        _expression, QueryContextParameter, QuerySourceScopeParameter)
                    .Compile();

            return qc => queryExecutor(qc, null);
        }

        public Func<QueryContext, IAsyncEnumerable<TResult>> CreateAsyncQueryExecutor<TResult>([NotNull] QueryModel queryModel)
        {
            Check.NotNull(queryModel, "queryModel");

            _blockTaskExpressions = false;

            VisitQueryModel(queryModel);

            if (_streamedSequenceInfo == null)
            {
                _expression
                    = Expression.Call(
                        _taskToSequenceShim.MakeGenericMethod(typeof(TResult)),
                        _expression);
            }

            var asyncQueryExecutor
                = Expression
                    .Lambda<Func<QueryContext, QuerySourceScope, IAsyncEnumerable<TResult>>>(
                        _expression, QueryContextParameter, QuerySourceScopeParameter)
                    .Compile();

            return qc => asyncQueryExecutor(qc, null);
        }

        private static readonly MethodInfo _taskToSequenceShim
            = typeof(EntityQueryModelVisitor)
                .GetTypeInfo().GetDeclaredMethod("TaskToSequenceShim");

        [UsedImplicitly]
        private static IAsyncEnumerable<T> TaskToSequenceShim<T>(Task<T> task)
        {
            return new TaskResultAsyncEnumerable<T>(task);
        }

        protected bool QuerySourceRequiresMaterialization([NotNull] IQuerySource querySource)
        {
            Check.NotNull(querySource, "querySource");

            return _querySourcesRequiringMaterialization.Contains(querySource);
        }

        public override void VisitQueryModel([NotNull] QueryModel queryModel)
        {
            Check.NotNull(queryModel, "queryModel");

            var requiresEntityMaterializationExpressionTreeVisitor
                = new RequiresEntityMaterializationExpressionTreeVisitor(this);

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

        private class TaskBlockingExpressionTreeVisitor : ExpressionTreeVisitor
        {
            public override Expression VisitExpression(Expression expression)
            {
                if (expression != null)
                {
                    var typeInfo = expression.Type.GetTypeInfo();

                    if (typeInfo.IsGenericType
                        && typeInfo.GetGenericTypeDefinition() == typeof(Task<>))
                    {
                        return Expression.Call(
                            _resultMethodInfo.MakeGenericMethod(typeInfo.GenericTypeArguments[0]),
                            expression);
                    }
                }

                return base.VisitExpression(expression);
            }

            private static readonly MethodInfo _resultMethodInfo
                = typeof(TaskBlockingExpressionTreeVisitor).GetTypeInfo()
                    .GetDeclaredMethod("_Result");

            [UsedImplicitly]
            private static T _Result<T>(Task<T> task)
            {
                return task.Result;
            }
        }

        private class RequiresEntityMaterializationExpressionTreeVisitor : ExpressionTreeVisitor
        {
            private readonly EntityQueryModelVisitor _queryModelVisitor;
            private readonly Dictionary<IQuerySource, int> _querySources = new Dictionary<IQuerySource, int>();

            public RequiresEntityMaterializationExpressionTreeVisitor(EntityQueryModelVisitor queryModelVisitor)
            {
                _queryModelVisitor = queryModelVisitor;
            }

            public ISet<IQuerySource> QuerySourcesRequiringMaterialization
            {
                get { return new HashSet<IQuerySource>(_querySources.Where(kv => kv.Value > 0).Select(kv => kv.Key)); }
            }

            protected override Expression VisitQuerySourceReferenceExpression(
                QuerySourceReferenceExpression querySourceReferenceExpression)
            {
                if (!_querySources.ContainsKey(querySourceReferenceExpression.ReferencedQuerySource))
                {
                    _querySources.Add(querySourceReferenceExpression.ReferencedQuerySource, 0);
                }

                _querySources[querySourceReferenceExpression.ReferencedQuerySource]++;

                return base.VisitQuerySourceReferenceExpression(querySourceReferenceExpression);
            }

            protected override Expression VisitMemberExpression(MemberExpression memberExpression)
            {
                var newExpression = base.VisitMemberExpression(memberExpression);

                _queryModelVisitor
                    .BindMemberExpression(
                        memberExpression,
                        (property, querySource) => _querySources[querySource]--);

                return newExpression;
            }

            protected override Expression VisitMethodCallExpression(MethodCallExpression methodCallExpression)
            {
                var newExpression = base.VisitMethodCallExpression(methodCallExpression);

                _queryModelVisitor
                    .BindMethodCallExpression(
                        methodCallExpression,
                        (property, querySource) => _querySources[querySource]--);

                return newExpression;
            }
        }

        public override void VisitMainFromClause(
            [NotNull] MainFromClause fromClause, [NotNull] QueryModel queryModel)
        {
            Check.NotNull(fromClause, "fromClause");
            Check.NotNull(queryModel, "queryModel");

            _expression
                = ReplaceClauseReferences(
                    CreateQueryingExpressionTreeVisitor(fromClause)
                        .VisitExpression(fromClause.FromExpression));

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

                var itemParameter = Expression.Parameter(elementType);

                _expression
                    = Expression.Call(
                        _queryCompilationContext.LinqOperatorProvider.Select
                            .MakeGenericMethod(elementType, typeof(QuerySourceScope)),
                        _expression,
                        Expression.Lambda(
                            QuerySourceScope
                                .Create(fromClause, itemParameter, QuerySourceScopeParameter),
                            new[] { itemParameter }));
            }

            _expression
                = Expression.Call(
                    _queryCompilationContext.LinqOperatorProvider.SelectMany
                        .MakeGenericMethod(typeof(QuerySourceScope), typeof(QuerySourceScope)),
                    Expression.Call(
                        _queryCompilationContext.LinqOperatorProvider.ToSequence
                            .MakeGenericMethod(typeof(QuerySourceScope)),
                        QuerySourceScopeParameter),
                    Expression.Lambda(_expression,
                        new[] { QuerySourceScopeParameter }));

            _querySourceMapping.AddMapping(
                fromClause,
                QuerySourceScope.GetResult(QuerySourceScopeParameter, fromClause, elementType));
        }

        public override void VisitAdditionalFromClause(
            [NotNull] AdditionalFromClause fromClause, [NotNull] QueryModel queryModel, int index)
        {
            Check.NotNull(fromClause, "fromClause");
            Check.NotNull(queryModel, "queryModel");

            var innerExpression
                = ReplaceClauseReferences(
                    CreateQueryingExpressionTreeVisitor(fromClause)
                        .VisitExpression(fromClause.FromExpression));

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

                var innerItemParameter = Expression.Parameter(innerElementType);

                innerExpression
                    = Expression.Call(
                        _queryCompilationContext.LinqOperatorProvider.Select
                            .MakeGenericMethod(innerElementType, typeof(QuerySourceScope)),
                        innerExpression,
                        Expression.Lambda(
                            QuerySourceScope
                                .Create(fromClause, innerItemParameter, QuerySourceScopeParameter),
                            new[] { innerItemParameter }));
            }

            _expression
                = Expression.Call(
                    _queryCompilationContext.LinqOperatorProvider.SelectMany
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
                = ReplaceClauseReferences(
                    CreateQueryingExpressionTreeVisitor(joinClause)
                        .VisitExpression(joinClause.OuterKeySelector));

            var innerSequenceExpression
                = ReplaceClauseReferences(
                    CreateQueryingExpressionTreeVisitor(joinClause)
                        .VisitExpression(joinClause.InnerSequence));

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
                = ReplaceClauseReferences(
                    CreateQueryingExpressionTreeVisitor(joinClause)
                        .VisitExpression(joinClause.InnerKeySelector));

            _expression
                = Expression.Call(
                    _queryCompilationContext.LinqOperatorProvider.Join.MakeGenericMethod(
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
                = ReplaceClauseReferences(
                    CreateQueryingExpressionTreeVisitor(groupJoinClause)
                        .VisitExpression(groupJoinClause.JoinClause.OuterKeySelector));

            var innerExpression
                = ReplaceClauseReferences(
                    CreateQueryingExpressionTreeVisitor(groupJoinClause.JoinClause)
                        .VisitExpression(groupJoinClause.JoinClause.InnerSequence));

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
                = ReplaceClauseReferences(
                    CreateQueryingExpressionTreeVisitor(groupJoinClause)
                        .VisitExpression(groupJoinClause.JoinClause.InnerKeySelector));

            var innerItemsParameter
                = Expression.Parameter(innerExpression.Type);

            _expression
                = Expression.Call(
                    _queryCompilationContext.LinqOperatorProvider.GroupJoin.MakeGenericMethod(
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
                                        _queryCompilationContext.LinqOperatorProvider.Select
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

            _querySourceMapping.AddMapping(
                groupJoinClause,
                QuerySourceScope
                    .GetResult(
                        QuerySourceScopeParameter,
                        groupJoinClause,
                        typeof(IEnumerable<>).MakeGenericType(innerElementType)));
        }

        public override void VisitWhereClause(
            [NotNull] WhereClause whereClause, [NotNull] QueryModel queryModel, int index)
        {
            Check.NotNull(whereClause, "whereClause");
            Check.NotNull(queryModel, "queryModel");

            var predicate
                = ReplaceClauseReferences(
                    CreateQueryingExpressionTreeVisitor(queryModel.MainFromClause)
                        .VisitExpression(whereClause.Predicate));

            _expression
                = Expression.Call(
                    _queryCompilationContext.LinqOperatorProvider.Where
                        .MakeGenericMethod(typeof(QuerySourceScope)),
                    _expression,
                    Expression.Lambda(predicate, QuerySourceScopeParameter));
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
                    _queryCompilationContext.LinqOperatorProvider.Select
                        .MakeGenericMethod(typeof(QuerySourceScope), selector.Type),
                    _expression,
                    Expression.Lambda(selector, QuerySourceScopeParameter));

            _streamedSequenceInfo
                = (StreamedSequenceInfo)selectClause.GetOutputDataInfo()
                    .AdjustDataType(typeof(IEnumerable<>));
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
                            ? _queryCompilationContext.LinqOperatorProvider.OrderBy
                            : _queryCompilationContext.LinqOperatorProvider.ThenBy)
                            .MakeGenericMethod(elementType, expression.Type),
                        _expression,
                        Expression.Lambda(expression, parameterExpression),
                        Expression.Constant(ordering.OrderingDirection));
            }
        }

        public override void VisitResultOperator(
            [NotNull] ResultOperatorBase resultOperator, [NotNull] QueryModel queryModel, int index)
        {
            Check.NotNull(resultOperator, "resultOperator");
            Check.NotNull(queryModel, "queryModel");

            // TODO: sub-queries in result op. expressions

            var expression
                = _queryCompilationContext.ResultOperatorHandler
                    .HandleResultOperator(this, resultOperator, queryModel);

            if (expression != _expression)
            {
                _expression = expression;

                _streamedSequenceInfo
                    = resultOperator.GetOutputDataInfo(_streamedSequenceInfo) as StreamedSequenceInfo;
            }
        }

        private Expression ReplaceClauseReferences(Expression expression)
        {
            return ReplaceClauseReferences(expression, _querySourceMapping);
        }

        private Expression ReplaceClauseReferences(Expression expression, QuerySourceMapping querySourceMapping)
        {
            return new PropertyAccessBindingExpressionTreeVisitor(querySourceMapping, this)
                .VisitExpression(expression);
        }

        private class PropertyAccessBindingExpressionTreeVisitor : ReferenceReplacingExpressionTreeVisitor
        {
            private readonly EntityQueryModelVisitor _queryModelVisitor;

            public PropertyAccessBindingExpressionTreeVisitor(
                QuerySourceMapping querySourceMapping,
                EntityQueryModelVisitor queryModelVisitor)
                : base(querySourceMapping, throwOnUnmappedReferences: false)
            {
                _queryModelVisitor = queryModelVisitor;
            }

            protected override Expression VisitMemberExpression(MemberExpression memberExpression)
            {
                var newExpression = VisitExpression(memberExpression.Expression);

                if (newExpression != memberExpression.Expression)
                {
                    return newExpression.Type == typeof(IValueReader)
                        ? _queryModelVisitor.BindMemberToValueReader(memberExpression, newExpression)
                        : Expression.MakeMemberAccess(newExpression, memberExpression.Member);
                }

                return memberExpression;
            }

            protected override Expression VisitMethodCallExpression(MethodCallExpression methodCallExpression)
            {
                var newExpression
                    = (MethodCallExpression)base.VisitMethodCallExpression(methodCallExpression);

                if (newExpression != methodCallExpression
                    && newExpression.Arguments.Count > 0
                    && newExpression.Arguments[0].Type == typeof(IValueReader))
                {
                    return
                        _queryModelVisitor
                            .BindMethodCallToValueReader(methodCallExpression, newExpression.Arguments[0])
                        ?? newExpression;
                }

                return _queryModelVisitor
                    .BindMethodCallExpression(
                        methodCallExpression,
                        (property, _) => Expression.Call(
                            _getValueMethodInfo.MakeGenericMethod(newExpression.Method.GetGenericArguments()[0]),
                            QueryContextParameter,
                            newExpression.Arguments[0],
                            Expression.Constant(property)))
                       ?? newExpression;
            }

            private static readonly MethodInfo _getValueMethodInfo
                = typeof(PropertyAccessBindingExpressionTreeVisitor)
                    .GetTypeInfo().GetDeclaredMethod("GetValue");

            [UsedImplicitly]
            private static T GetValue<T>(QueryContext queryContext, object entity, IProperty property)
            {
                return (T)queryContext.StateManager.GetOrCreateEntry(entity)[property];
            }
        }

        protected virtual Expression BindMethodCallToValueReader(
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

        protected virtual Expression BindMemberToValueReader(
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

        protected virtual void BindMemberExpression(
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

        protected virtual TResult BindMemberExpression<TResult>(
            [NotNull] MemberExpression memberExpression,
            [NotNull] Func<IProperty, IQuerySource, TResult> memberBinder)
        {
            Check.NotNull(memberExpression, "memberExpression");
            Check.NotNull(memberBinder, "memberBinder");

            return BindMemberExpression(memberExpression, null, memberBinder);
        }

        protected virtual TResult BindMemberExpression<TResult>(
            [NotNull] MemberExpression memberExpression,
            [CanBeNull] IQuerySource querySource,
            Func<IProperty, IQuerySource, TResult> memberBinder)
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
                    var property = entityType.TryGetProperty(memberExpression.Member.Name);

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

        protected virtual TResult BindMethodCallExpression<TResult>(
            [NotNull] MethodCallExpression methodCallExpression,
            [NotNull] Func<IProperty, IQuerySource, TResult> methodCallBinder)
        {
            Check.NotNull(methodCallExpression, "methodCallExpression");
            Check.NotNull(methodCallBinder, "methodCallBinder");

            return BindMethodCallExpression(methodCallExpression, null, methodCallBinder);
        }

        private static readonly MethodInfo _propertyMethodInfo
            = typeof(QueryExtensions).GetTypeInfo().GetDeclaredMethod("Property");

        protected virtual TResult BindMethodCallExpression<TResult>(
            [NotNull] MethodCallExpression methodCallExpression,
            [CanBeNull] IQuerySource querySource,
            Func<IProperty, IQuerySource, TResult> methodCallBinder)
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

        protected class DefaultExpressionTreeVisitor : ExpressionTreeVisitor
        {
            private readonly EntityQueryModelVisitor _entityQueryModelVisitor;

            public DefaultExpressionTreeVisitor([NotNull] EntityQueryModelVisitor entityQueryModelVisitor)
            {
                Check.NotNull(entityQueryModelVisitor, "entityQueryModelVisitor");

                _entityQueryModelVisitor = entityQueryModelVisitor;
            }

            public EntityQueryModelVisitor QueryModelVisitor
            {
                get { return _entityQueryModelVisitor; }
            }

            protected override Expression VisitSubQueryExpression(SubQueryExpression subQueryExpression)
            {
                var queryModelVisitor = CreateQueryModelVisitor();

                queryModelVisitor.VisitQueryModel(subQueryExpression.QueryModel);

                return queryModelVisitor.Expression;
            }

            protected EntityQueryModelVisitor CreateQueryModelVisitor()
            {
                return QueryModelVisitor.QueryCompilationContext
                    .CreateQueryModelVisitor(_entityQueryModelVisitor);
            }
        }

        protected abstract class QueryingExpressionTreeVisitor : DefaultExpressionTreeVisitor
        {
            protected QueryingExpressionTreeVisitor([NotNull] EntityQueryModelVisitor entityQueryModelVisitor)
                : base(entityQueryModelVisitor)
            {
            }

            protected override Expression VisitConstantExpression(ConstantExpression constantExpression)
            {
                if (constantExpression.Type.GetTypeInfo().IsGenericType
                    && constantExpression.Type.GetGenericTypeDefinition() == typeof(EntityQueryable<>))
                {
                    return VisitEntityQueryable(((IQueryable)constantExpression.Value).ElementType);
                }

                return constantExpression;
            }

            protected abstract Expression VisitEntityQueryable([NotNull] Type elementType);
        }

        protected class ProjectionExpressionTreeVisitor : DefaultExpressionTreeVisitor
        {
            public ProjectionExpressionTreeVisitor([NotNull] EntityQueryModelVisitor entityQueryModelVisitor)
                : base(entityQueryModelVisitor)
            {
            }

            protected override Expression VisitSubQueryExpression(SubQueryExpression subQueryExpression)
            {
                var queryModelVisitor = CreateQueryModelVisitor();

                queryModelVisitor.VisitQueryModel(subQueryExpression.QueryModel);

                var subExpression = queryModelVisitor.Expression;

                if (queryModelVisitor._streamedSequenceInfo == null)
                {
                    return subExpression;
                }

                if (typeof(IQueryable).GetTypeInfo().IsAssignableFrom(subQueryExpression.Type.GetTypeInfo()))
                {
                    subExpression
                        = Expression.Call(
                            _asQueryableShim.MakeGenericMethod(
                                queryModelVisitor._streamedSequenceInfo.ResultItemType),
                            subExpression);
                }

                return Expression.Convert(subExpression, subQueryExpression.Type);
            }

            private static readonly MethodInfo _asQueryableShim
                = typeof(ProjectionExpressionTreeVisitor)
                    .GetTypeInfo().GetDeclaredMethod("AsQueryableShim");

            [UsedImplicitly]
            private static IOrderedQueryable<TSource> AsQueryableShim<TSource>(IEnumerable<TSource> source)
            {
                return new EnumerableQuery<TSource>(source);
            }
        }
    }
}
