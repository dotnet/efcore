// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Query.Expressions;
using Microsoft.Data.Entity.Query.ExpressionVisitors;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;
using Remotion.Linq;
using Remotion.Linq.Clauses;

namespace Microsoft.Data.Entity.Query
{
    public class RelationalQueryModelVisitor : EntityQueryModelVisitor
    {
        private readonly RelationalQueryModelVisitor _parentQueryModelVisitor;

        private readonly Dictionary<IQuerySource, SelectExpression> _queriesBySource
            = new Dictionary<IQuerySource, SelectExpression>();

        private readonly Dictionary<IQuerySource, SelectExpression> _subqueriesBySource
            = new Dictionary<IQuerySource, SelectExpression>();

        private bool _requiresClientFilter;
        private bool _requiresClientResultOperator;

        private RelationalProjectionExpressionVisitor _projectionVisitor;

        private Dictionary<IncludeSpecification, List<int>> _navigationIndexMap = new Dictionary<IncludeSpecification, List<int>>();

        public RelationalQueryModelVisitor(
            [NotNull] RelationalQueryCompilationContext queryCompilationContext,
            [CanBeNull] RelationalQueryModelVisitor parentQueryModelVisitor)
            : base(Check.NotNull(queryCompilationContext, nameof(queryCompilationContext)))
        {
            _parentQueryModelVisitor = parentQueryModelVisitor;
        }

        public virtual bool RequiresClientEval { get; set; }
        public virtual bool RequiresClientSelectMany { get; set; }
        public virtual bool RequiresClientFilter => _requiresClientFilter || RequiresClientEval;
        public virtual bool RequiresClientProjection => _projectionVisitor.RequiresClientEval || RequiresClientEval;

        public virtual bool RequiresClientResultOperator
        {
            get { return _requiresClientResultOperator || RequiresClientEval; }
            set { _requiresClientResultOperator = value; }
        }

        public new virtual RelationalQueryCompilationContext QueryCompilationContext
            => (RelationalQueryCompilationContext)base.QueryCompilationContext;

        public virtual ICollection<SelectExpression> Queries => _queriesBySource.Values;

        public virtual void AddQuery([NotNull] IQuerySource querySource, [NotNull] SelectExpression selectExpression)
        {
            Check.NotNull(querySource, nameof(querySource));
            Check.NotNull(selectExpression, nameof(selectExpression));

            _queriesBySource.Add(querySource, selectExpression);
        }

        public virtual void AddSubquery([NotNull] IQuerySource querySource, [NotNull] SelectExpression selectExpression)
        {
            Check.NotNull(querySource, nameof(querySource));
            Check.NotNull(selectExpression, nameof(selectExpression));

            _subqueriesBySource.Add(querySource, selectExpression);
        }

        public virtual SelectExpression TryGetQuery([NotNull] IQuerySource querySource)
        {
            Check.NotNull(querySource, nameof(querySource));

            SelectExpression selectExpression;
            return (_queriesBySource.TryGetValue(querySource, out selectExpression)
                ? selectExpression
                : _queriesBySource.Values.SingleOrDefault(se => se.HandlesQuerySource(querySource)));
        }

        protected override ExpressionVisitor CreateQueryingExpressionVisitor(IQuerySource querySource)
        {
            Check.NotNull(querySource, nameof(querySource));

            return new RelationalEntityQueryableExpressionVisitor(this, querySource);
        }

        protected override ExpressionVisitor CreateProjectionExpressionVisitor(IQuerySource querySource)
        {
            Check.NotNull(querySource, nameof(querySource));

            return _projectionVisitor
                = new RelationalProjectionExpressionVisitor(this, querySource);
        }

        public override void VisitQueryModel(QueryModel queryModel)
        {
            base.VisitQueryModel(queryModel);

            var compositePredicateVisitor
                = new CompositePredicateExpressionVisitor(
                    QueryCompilationContext
                        .GetCustomQueryAnnotations(RelationalQueryableExtensions.UseRelationalNullSemanticsMethodInfo)
                        .Any());

            foreach (var selectExpression in _queriesBySource.Values.Where(se => se.Predicate != null))
            {
                selectExpression.Predicate
                    = compositePredicateVisitor.Visit(selectExpression.Predicate);
            }
        }

        protected override void IncludeNavigations(
            [NotNull] QueryModel queryModel,
            [NotNull] IReadOnlyCollection<IncludeSpecification> includeSpecifications)
        {
            _navigationIndexMap = BuildNavigationIndexMap(includeSpecifications);

            base.IncludeNavigations(queryModel, includeSpecifications);
        }

        private Dictionary<IncludeSpecification, List<int>> BuildNavigationIndexMap(
            IReadOnlyCollection<IncludeSpecification> includeSpecifications)
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

        protected override void IncludeNavigations(
            IncludeSpecification includeSpecification,
            Type resultType,
            LambdaExpression accessorLambda,
            bool querySourceRequiresTracking)
        {
            Check.NotNull(includeSpecification, nameof(includeSpecification));
            Check.NotNull(resultType, nameof(resultType));
            Check.NotNull(accessorLambda, nameof(accessorLambda));

            Expression
                = new IncludeExpressionVisitor(
                    includeSpecification.QuerySource,
                    includeSpecification.NavigationPath,
                    QueryCompilationContext,
                    _navigationIndexMap[includeSpecification],
                    querySourceRequiresTracking)
                    .Visit(Expression);
        }

        protected override Expression CompileMainFromClauseExpression(MainFromClause fromClause)
        {
            var expression = base.CompileMainFromClauseExpression(fromClause);

            SelectExpression subSelectExpression;
            if (_subqueriesBySource.TryGetValue(fromClause, out subSelectExpression))
            {
                subSelectExpression.PushDownSubquery().QuerySource = fromClause;

                AddQuery(fromClause, subSelectExpression);

                _subqueriesBySource.Remove(fromClause);

                var shapedQueryMethodExpression
                    = new ShapedQueryFindingExpressionVisitor(QueryCompilationContext)
                        .Find(expression);

                var shaperLambda = (LambdaExpression)shapedQueryMethodExpression.Arguments[2];
                var shaperMethodCall = (MethodCallExpression)shaperLambda.Body;

                var shaperMethod = shaperMethodCall.Method;
                var shaperMethodArgs = shaperMethodCall.Arguments.ToList();

                if (!QuerySourceRequiresMaterialization(fromClause)
                    && shaperMethod.MethodIsClosedFormOf(CreateEntityMethodInfo))
                {
                    shaperMethod = CreateValueBufferMethodInfo;
                    shaperMethodArgs.RemoveRange(5, 5);
                }
                else
                {
                    subSelectExpression.ExplodeStarProjection();
                }

                var innerQuerySource = (IQuerySource)((ConstantExpression)shaperMethodArgs[0]).Value;

                foreach (var queryAnnotation 
                    in QueryCompilationContext.QueryAnnotations
                        .Where(qa => qa.QuerySource == innerQuerySource))
                {
                    queryAnnotation.QuerySource = fromClause;
                }

                shaperMethodArgs[0] = Expression.Constant(fromClause);

                return Expression.Call(
                    QueryCompilationContext.QueryMethodProvider.ShapedQueryMethod
                        .MakeGenericMethod(shaperMethod.ReturnType),
                    shapedQueryMethodExpression.Arguments[0],
                    shapedQueryMethodExpression.Arguments[1],
                    Expression.Lambda(
                        Expression.Call(shaperMethod, shaperMethodArgs),
                        shaperLambda.Parameters[0]));
            }

            return expression;
        }

        public override void VisitAdditionalFromClause(AdditionalFromClause fromClause, QueryModel queryModel, int index)
        {
            base.VisitAdditionalFromClause(fromClause, queryModel, index);

            RequiresClientSelectMany = true;

            var selectExpression = TryGetQuery(fromClause);

            if (selectExpression != null)
            {
                var previousQuerySource
                    = index == 0
                        ? queryModel.MainFromClause
                        : queryModel.BodyClauses[index - 1] as IQuerySource;

                if (previousQuerySource != null)
                {
                    var previousSelectExpression = TryGetQuery(previousQuerySource);

                    if (previousSelectExpression != null)
                    {
                        var readerOffset = previousSelectExpression.Projection.Count;

                        previousSelectExpression.AddCrossJoin(selectExpression.Tables.Single(), selectExpression.Projection);

                        _queriesBySource.Remove(fromClause);

                        Expression
                            = new QueryFlatteningExpressionVisitor(
                                previousQuerySource,
                                fromClause,
                                QueryCompilationContext,
                                readerOffset,
                                LinqOperatorProvider.SelectMany)
                                .Visit(Expression);

                        RequiresClientSelectMany = false;
                    }
                }
            }
        }

        public override void VisitJoinClause(JoinClause joinClause, QueryModel queryModel, int index)
        {
            var previousQuerySource
                = index == 0
                    ? queryModel.MainFromClause
                    : queryModel.BodyClauses[index - 1] as IQuerySource;

            var previousSelectExpression
                = previousQuerySource != null
                    ? TryGetQuery(previousQuerySource)
                    : null;

            var previousSelectProjectionCount
                = previousSelectExpression?.Projection.Count ?? -1;

            base.VisitJoinClause(joinClause, queryModel, index);

            if (previousSelectExpression != null)
            {
                var selectExpression = TryGetQuery(joinClause);

                if (selectExpression != null)
                {
                    var filteringExpressionVisitor
                        = new SqlTranslatingExpressionVisitor(this);

                    var predicate
                        = filteringExpressionVisitor
                            .Visit(
                                Expression.Equal(
                                    joinClause.OuterKeySelector,
                                    joinClause.InnerKeySelector));

                    if (predicate != null)
                    {
                        _queriesBySource.Remove(joinClause);

                        previousSelectExpression.RemoveRangeFromProjection(previousSelectProjectionCount);

                        var innerJoinExpression
                            = previousSelectExpression
                                .AddInnerJoin(
                                    selectExpression.Tables.Single(),
                                    QuerySourceRequiresMaterialization(joinClause)
                                        ? selectExpression.Projection
                                        : Enumerable.Empty<Expression>());

                        innerJoinExpression.Predicate = predicate;

                        Expression
                            = new QueryFlatteningExpressionVisitor(
                                previousQuerySource,
                                joinClause,
                                QueryCompilationContext,
                                previousSelectProjectionCount,
                                LinqOperatorProvider.Join)
                                .Visit(Expression);
                    }
                    else
                    {
                        previousSelectExpression.RemoveRangeFromProjection(previousSelectProjectionCount);
                    }
                }
            }
        }

        public override void VisitWhereClause(WhereClause whereClause, QueryModel queryModel, int index)
        {
            var selectExpression = TryGetQuery(queryModel.MainFromClause);
            var requiresClientFilter = selectExpression == null;

            if (!requiresClientFilter)
            {
                var translatingVisitor = new SqlTranslatingExpressionVisitor(this, whereClause.Predicate);
                var sqlPredicateExpression = translatingVisitor.Visit(whereClause.Predicate);

                if (sqlPredicateExpression != null)
                {
                    selectExpression.Predicate
                        = selectExpression.Predicate == null
                            ? sqlPredicateExpression
                            : Expression.AndAlso(selectExpression.Predicate, sqlPredicateExpression);
                }
                else
                {
                    requiresClientFilter = true;
                }

                if (translatingVisitor.ClientEvalPredicate != null)
                {
                    requiresClientFilter = true;
                    whereClause = new WhereClause(translatingVisitor.ClientEvalPredicate);
                }
            }

            _requiresClientFilter |= requiresClientFilter;

            if (RequiresClientFilter)
            {
                base.VisitWhereClause(whereClause, queryModel, index);
            }
        }

        public override void VisitOrderByClause(OrderByClause orderByClause, QueryModel queryModel, int index)
        {
            var selectExpression = TryGetQuery(queryModel.MainFromClause);
            var requiresClientOrderBy = selectExpression == null;

            if (!requiresClientOrderBy)
            {
                var sqlTranslatingExpressionVisitor
                    = new SqlTranslatingExpressionVisitor(this);

                var orderings = new List<Ordering>();

                foreach (var ordering in orderByClause.Orderings)
                {
                    var sqlOrderingExpression
                        = sqlTranslatingExpressionVisitor
                            .Visit(ordering.Expression);

                    if (sqlOrderingExpression == null)
                    {
                        break;
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

            if (RequiresClientEval || requiresClientOrderBy)
            {
                base.VisitOrderByClause(orderByClause, queryModel, index);
            }
        }

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
                    });
        }

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
                    });
        }

        public virtual void BindMemberExpression(
            [NotNull] MemberExpression memberExpression,
            [NotNull] Action<IProperty, IQuerySource, SelectExpression> memberBinder)
        {
            Check.NotNull(memberExpression, nameof(memberExpression));
            Check.NotNull(memberBinder, nameof(memberBinder));

            BindMemberExpression(memberExpression, null,
                (property, querySource, selectExpression) =>
                    {
                        memberBinder(property, querySource, selectExpression);

                        return default(object);
                    });
        }

        public virtual TResult BindMemberExpression<TResult>(
            [NotNull] MemberExpression memberExpression,
            [NotNull] Func<IProperty, IQuerySource, SelectExpression, TResult> memberBinder)
        {
            Check.NotNull(memberExpression, nameof(memberExpression));
            Check.NotNull(memberBinder, nameof(memberBinder));

            return BindMemberExpression(memberExpression, null, memberBinder);
        }

        private TResult BindMemberExpression<TResult>(
            [NotNull] MemberExpression memberExpression,
            [CanBeNull] IQuerySource querySource,
            Func<IProperty, IQuerySource, SelectExpression, TResult> memberBinder)
        {
            Check.NotNull(memberExpression, nameof(memberExpression));
            Check.NotNull(memberBinder, nameof(memberBinder));

            return base.BindMemberExpression(memberExpression, querySource,
                (property, qs) => BindMemberOrMethod(memberBinder, qs, property));
        }

        public virtual void BindMethodCallExpression(
            [NotNull] MethodCallExpression methodCallExpression,
            [NotNull] Action<IProperty, IQuerySource, SelectExpression> memberBinder)
        {
            Check.NotNull(methodCallExpression, nameof(methodCallExpression));
            Check.NotNull(memberBinder, nameof(memberBinder));

            BindMethodCallExpression(methodCallExpression, null,
                (property, querySource, selectExpression) =>
                    {
                        memberBinder(property, querySource, selectExpression);

                        return default(object);
                    });
        }

        public virtual TResult BindMethodCallExpression<TResult>(
            [NotNull] MethodCallExpression methodCallExpression,
            [NotNull] Func<IProperty, IQuerySource, SelectExpression, TResult> memberBinder)
        {
            Check.NotNull(methodCallExpression, nameof(methodCallExpression));
            Check.NotNull(memberBinder, nameof(memberBinder));

            return BindMethodCallExpression(methodCallExpression, null, memberBinder);
        }

        private TResult BindMethodCallExpression<TResult>(
            MethodCallExpression methodCallExpression,
            IQuerySource querySource,
            Func<IProperty, IQuerySource, SelectExpression, TResult> memberBinder)
        {
            return base.BindMethodCallExpression(methodCallExpression, querySource,
                (property, qs) => BindMemberOrMethod(memberBinder, qs, property));
        }

        private TResult BindMemberOrMethod<TResult>(
            Func<IProperty, IQuerySource, SelectExpression, TResult> memberBinder,
            IQuerySource querySource,
            IProperty property)
        {
            if (querySource != null)
            {
                var selectExpression = TryGetQuery(querySource);

                if (selectExpression != null)
                {
                    return memberBinder(property, querySource, selectExpression);
                }

                selectExpression
                    = _parentQueryModelVisitor?.TryGetQuery(querySource);

                selectExpression?
                    .AddToProjection(
                        QueryCompilationContext.GetColumnName(property),
                        property,
                        querySource);
            }

            return default(TResult);
        }

        public static readonly MethodInfo CreateValueBufferMethodInfo
            = typeof(RelationalQueryModelVisitor).GetTypeInfo()
                .GetDeclaredMethod(nameof(CreateValueBuffer));

        [UsedImplicitly]
        private static QueryResultScope<ValueBuffer> CreateValueBuffer(
            IQuerySource querySource,
            QueryContext queryContext,
            QueryResultScope parentQueryResultScope,
            ValueBuffer valueBuffer,
            int valueBufferOffset)
        {
            return new QueryResultScope<ValueBuffer>(
                querySource,
                valueBuffer.WithOffset(valueBufferOffset),
                parentQueryResultScope);
        }

        public static readonly MethodInfo CreateEntityMethodInfo
            = typeof(RelationalQueryModelVisitor).GetTypeInfo()
                .GetDeclaredMethod(nameof(CreateEntity));

        [UsedImplicitly]
        private static QueryResultScope<TEntity> CreateEntity<TEntity>(
            IQuerySource querySource,
            QueryContext queryContext,
            QueryResultScope parentQueryResultScope,
            ValueBuffer valueBuffer,
            int valueBufferOffset,
            IEntityType entityType,
            bool queryStateManager,
            EntityKeyFactory entityKeyFactory,
            IReadOnlyList<IProperty> keyProperties,
            Func<ValueBuffer, object> materializer)
            where TEntity : class
        {
            valueBuffer = valueBuffer.WithOffset(valueBufferOffset);

            var entityKey
                = entityKeyFactory.Create(entityType, keyProperties, valueBuffer);

            return new QueryResultScope<TEntity>(
                querySource,
                (TEntity)queryContext.QueryBuffer
                    .GetEntity(
                        entityType,
                        entityKey,
                        new EntityLoadInfo(
                            valueBuffer,
                            materializer),
                        queryStateManager),
                parentQueryResultScope);
        }
    }
}
