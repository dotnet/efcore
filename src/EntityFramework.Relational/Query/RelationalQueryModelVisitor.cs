// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Query;
using Microsoft.Data.Entity.Query.Annotations;
using Microsoft.Data.Entity.Relational.Query.Expressions;
using Microsoft.Data.Entity.Relational.Query.ExpressionTreeVisitors;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;
using Remotion.Linq;
using Remotion.Linq.Clauses;
using Remotion.Linq.Parsing;

namespace Microsoft.Data.Entity.Relational.Query
{
    public class RelationalQueryModelVisitor : EntityQueryModelVisitor
    {
        private readonly RelationalQueryModelVisitor _parentQueryModelVisitor;

        private readonly Dictionary<IQuerySource, SelectExpression> _queriesBySource
            = new Dictionary<IQuerySource, SelectExpression>();

        private bool _requiresClientFilter;
        private bool _requiresClientResultOperator;

        private RelationalProjectionExpressionTreeVisitor _projectionTreeVisitor;

        public RelationalQueryModelVisitor(
            [NotNull] RelationalQueryCompilationContext queryCompilationContext,
            [CanBeNull] RelationalQueryModelVisitor parentQueryModelVisitor)
            : base(Check.NotNull(queryCompilationContext, nameof(queryCompilationContext)))
        {
            _parentQueryModelVisitor = parentQueryModelVisitor;
        }

        public virtual bool RequiresClientEval { get; set; }
        public virtual bool RequiresClientFilter => _requiresClientFilter || RequiresClientEval;
        public virtual bool RequiresClientProjection => _projectionTreeVisitor.RequiresClientEval || RequiresClientEval;
        public virtual bool RequiresClientResultOperator
        {
            get { return _requiresClientResultOperator || RequiresClientEval; }
            set { _requiresClientResultOperator = value; }
        }

        public new virtual RelationalQueryCompilationContext QueryCompilationContext
            => (RelationalQueryCompilationContext)base.QueryCompilationContext;

        public virtual void AddQuery([NotNull] IQuerySource querySource, [NotNull] SelectExpression selectExpression)
        {
            Check.NotNull(querySource, nameof(querySource));
            Check.NotNull(selectExpression, nameof(selectExpression));

            _queriesBySource.Add(querySource, selectExpression);
        }

        public virtual SelectExpression TryGetQuery([NotNull] IQuerySource querySource)
        {
            Check.NotNull(querySource, nameof(querySource));

            SelectExpression selectExpression;
            return (_queriesBySource.TryGetValue(querySource, out selectExpression)
                ? selectExpression
                : _queriesBySource.Values.SingleOrDefault(se => se.HandlesQuerySource(querySource)));
        }

        public virtual IEnumerable<Type> GetProjectionTypes([NotNull] IQuerySource querySource)
            => (TryGetQuery(Check.NotNull(querySource, nameof(querySource)))
                ?? _queriesBySource.First().Value)
                .GetProjectionTypes(0);

        protected override ExpressionTreeVisitor CreateQueryingExpressionTreeVisitor(IQuerySource querySource)
        {
            Check.NotNull(querySource, nameof(querySource));

            return new RelationalEntityQueryableExpressionTreeVisitor(this, querySource);
        }

        protected override ExpressionTreeVisitor CreateProjectionExpressionTreeVisitor(IQuerySource querySource)
        {
            Check.NotNull(querySource, nameof(querySource));

            return _projectionTreeVisitor
                = new RelationalProjectionExpressionTreeVisitor(this, querySource);
        }

        public override void VisitQueryModel(QueryModel queryModel)
        {
            base.VisitQueryModel(queryModel);
            var compositePredicateVisitor = new CompositePredicateExpressionTreeVisitor(
                QueryCompilationContext.QueryAnnotations.OfType<UseRelationalNullSemanticsQueryAnnotation>().Any());

            foreach (var selectExpression in _queriesBySource.Values.Where(se => se.Predicate != null))
            {
                selectExpression.Predicate
                    = compositePredicateVisitor.VisitExpression(selectExpression.Predicate);
            }
        }

        protected override void IncludeNavigations(
            IQuerySource querySource,
            Type resultType,
            LambdaExpression accessorLambda,
            IReadOnlyList<INavigation> navigationPath,
            bool querySourceRequiresTracking)
        {
            Check.NotNull(querySource, nameof(querySource));
            Check.NotNull(resultType, nameof(resultType));
            Check.NotNull(accessorLambda, nameof(accessorLambda));
            Check.NotNull(navigationPath, nameof(navigationPath));

            Expression
                = new IncludeExpressionTreeVisitor(
                    querySource,
                    navigationPath,
                    QueryCompilationContext,
                    querySourceRequiresTracking)
                    .VisitExpression(Expression);
        }

        public override void VisitAdditionalFromClause(AdditionalFromClause fromClause, QueryModel queryModel, int index)
        {
            base.VisitAdditionalFromClause(fromClause, queryModel, index);

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

                        previousSelectExpression
                            .AddCrossJoin(selectExpression.Tables.Single(), selectExpression.Projection);

                        _queriesBySource.Remove(fromClause);

                        Expression
                            = new QueryFlatteningExpressionTreeVisitor(
                                previousQuerySource,
                                fromClause,
                                QueryCompilationContext,
                                readerOffset,
                                LinqOperatorProvider.SelectMany)
                                .VisitExpression(Expression);
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
                    var filteringExpressionTreeVisitor
                        = new SqlTranslatingExpressionTreeVisitor(this);

                    var predicate
                        = filteringExpressionTreeVisitor
                            .VisitExpression(
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
                            = new QueryFlatteningExpressionTreeVisitor(
                                previousQuerySource,
                                joinClause,
                                QueryCompilationContext,
                                previousSelectProjectionCount,
                                LinqOperatorProvider.Join)
                                .VisitExpression(Expression);
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
                var translatingVisitor = new SqlTranslatingExpressionTreeVisitor(this, whereClause.Predicate);
                var sqlPredicateExpression = translatingVisitor.VisitExpression(whereClause.Predicate);

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
                var sqlTranslatingExpressionTreeVisitor
                    = new SqlTranslatingExpressionTreeVisitor(this);

                var orderings = new List<Ordering>();

                foreach (var ordering in orderByClause.Orderings)
                {
                    var sqlOrderingExpression
                        = sqlTranslatingExpressionTreeVisitor
                            .VisitExpression(ordering.Expression);

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

            if (RequiresClientEval | requiresClientOrderBy)
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
        private static QuerySourceScope<ValueBuffer> CreateValueBuffer(
            IQuerySource querySource,
            QueryContext queryContext,
            QuerySourceScope parentQuerySourceScope,
            IRelationalValueBufferFactory valueBufferFactory,
            DbDataReader dataReader)
        {
            return new QuerySourceScope<ValueBuffer>(
                querySource,
                valueBufferFactory.CreateValueBuffer(dataReader),
                parentQuerySourceScope);
        }

        public static readonly MethodInfo CreateEntityMethodInfo
            = typeof(RelationalQueryModelVisitor).GetTypeInfo()
                .GetDeclaredMethod(nameof(CreateEntity));

        [UsedImplicitly]
        private static QuerySourceScope<TEntity> CreateEntity<TEntity>(
            IQuerySource querySource,
            QueryContext queryContext,
            QuerySourceScope parentQuerySourceScope,
            IRelationalValueBufferFactory valueBufferFactory,
            DbDataReader dataReader,
            IEntityType entityType,
            bool queryStateManager,
            EntityKeyFactory entityKeyFactory,
            IReadOnlyList<IProperty> keyProperties,
            Func<ValueBuffer, object> materializer)
            where TEntity : class
        {
            var valueBuffer = valueBufferFactory.CreateValueBuffer(dataReader);

            var entityKey
                = entityKeyFactory.Create(entityType, keyProperties, valueBuffer);

            return new QuerySourceScope<TEntity>(
                querySource,
                (TEntity)queryContext.QueryBuffer
                    .GetEntity(
                        entityType,
                        entityKey,
                        new EntityLoadInfo(
                            valueBuffer,
                            materializer),
                        queryStateManager),
                parentQuerySourceScope);
        }
    }
}
