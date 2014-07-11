// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Query;
using Microsoft.Data.Entity.Relational.Query.Expressions;
using Microsoft.Data.Entity.Relational.Query.Methods;
using Microsoft.Data.Entity.Relational.Utilities;
using Remotion.Linq;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Clauses.ExpressionTreeVisitors;
using Remotion.Linq.Parsing;

namespace Microsoft.Data.Entity.Relational.Query
{
    public class RelationalQueryModelVisitor : EntityQueryModelVisitor
    {
        private readonly RelationalQueryModelVisitor _parentQueryModelVisitor;

        private readonly Dictionary<IQuerySource, SelectExpression> _queriesBySource
            = new Dictionary<IQuerySource, SelectExpression>();

        private readonly IMethodCallTranslator _methodCallTranslator = new CompositeMethodCallTranslator();

        private Expression _preOrderingExpression;

        public RelationalQueryModelVisitor(
            [NotNull] RelationalQueryCompilationContext queryCompilationContext,
            [CanBeNull] RelationalQueryModelVisitor parentQueryModelVisitor)
            : base(Check.NotNull(queryCompilationContext, "queryCompilationContext"))
        {
            _parentQueryModelVisitor = parentQueryModelVisitor;
        }

        public virtual SelectExpression TryGetSelectExpression([NotNull] IQuerySource querySource)
        {
            Check.NotNull(querySource, "querySource");

            SelectExpression selectExpression;
            return (_queriesBySource.TryGetValue(querySource, out selectExpression)
                ? selectExpression
                : _queriesBySource.Values.SingleOrDefault(se => se.HandlesQuerySource(querySource)));
        }

        protected override ExpressionTreeVisitor CreateQueryingExpressionTreeVisitor(IQuerySource querySource)
        {
            return new RelationalQueryingExpressionTreeVisitor(this, querySource);
        }

        protected override ExpressionTreeVisitor CreateProjectionExpressionTreeVisitor()
        {
            return new RelationalProjectionSubQueryExpressionTreeVisitor(this);
        }

        protected override ExpressionTreeVisitor CreateOrderingExpressionTreeVisitor(Ordering ordering)
        {
            return new RelationalOrderingSubQueryExpressionTreeVisitor(this, ordering);
        }

        public override void VisitAdditionalFromClause(AdditionalFromClause fromClause, QueryModel queryModel, int index)
        {
            base.VisitAdditionalFromClause(fromClause, queryModel, index);

            var selectExpression = TryGetSelectExpression(fromClause);

            if (selectExpression != null)
            {
                var previousQuerySource
                    = index == 0
                        ? queryModel.MainFromClause
                        : queryModel.BodyClauses[index - 1] as IQuerySource;

                if (previousQuerySource != null)
                {
                    var previousSelectExpression = TryGetSelectExpression(previousQuerySource);

                    if (previousSelectExpression != null)
                    {
                        var readerOffset = previousSelectExpression.Projection.Count;

                        selectExpression.Merge(previousSelectExpression);

                        _queriesBySource.Remove(previousQuerySource);

                        Expression
                            = new QueryFlatteningExpressionTreeVisitor(
                                previousQuerySource,
                                fromClause,
                                (RelationalQueryCompilationContext)QueryCompilationContext,
                                readerOffset)
                                .VisitExpression(Expression);
                    }
                }
            }
        }

        private class QueryFlatteningExpressionTreeVisitor : ExpressionTreeVisitor
        {
            private readonly IQuerySource _outerQuerySource;
            private readonly IQuerySource _innerQuerySource;
            private readonly RelationalQueryCompilationContext _relationalQueryCompilationContext;
            private readonly int _readerOffset;

            private MethodCallExpression _outerSelectManyExpression;
            private Expression _outerShaperExpression;

            public QueryFlatteningExpressionTreeVisitor(
                IQuerySource outerQuerySource,
                IQuerySource innerQuerySource,
                RelationalQueryCompilationContext relationalQueryCompilationContext,
                int readerOffset)
            {
                _outerQuerySource = outerQuerySource;
                _innerQuerySource = innerQuerySource;
                _relationalQueryCompilationContext = relationalQueryCompilationContext;
                _readerOffset = readerOffset;
            }

            protected override Expression VisitMethodCallExpression(MethodCallExpression expression)
            {
                var newExpression
                    = (MethodCallExpression)base.VisitMethodCallExpression(expression);

                if ((ReferenceEquals(newExpression.Method, RelationalQueryingExpressionTreeVisitor.CreateValueReaderMethodInfo)
                     || MethodIsClosedFormOf(newExpression.Method, RelationalQueryingExpressionTreeVisitor.CreateEntityMethodInfo)))
                {
                    var constantExpression = (ConstantExpression)newExpression.Arguments[0];

                    if (constantExpression.Value == _outerQuerySource)
                    {
                        _outerShaperExpression = newExpression;
                    }
                    else if (constantExpression.Value == _innerQuerySource)
                    {
                        var newArguments = new List<Expression>(newExpression.Arguments);
                        newArguments[2] = _outerShaperExpression;

                        if (newArguments.Count == 5)
                        {
                            newArguments[4]
                                = Expression.Constant(
                                    _readerOffset
                                    + (int)((ConstantExpression)newArguments[4]).Value);
                        }

                        newExpression
                            = Expression.Call(newExpression.Method, newArguments);
                    }
                }
                else if (_outerShaperExpression != null
                         && _outerSelectManyExpression == null
                         && MethodIsClosedFormOf(
                             newExpression.Method,
                             _relationalQueryCompilationContext.LinqOperatorProvider.SelectMany))
                {
                    _outerSelectManyExpression = newExpression;
                }
                else if (_outerSelectManyExpression != null
                         && MethodIsClosedFormOf(
                             newExpression.Method,
                             _relationalQueryCompilationContext.LinqOperatorProvider.SelectMany))
                {
                    newExpression
                        = Expression.Call(
                            _relationalQueryCompilationContext.LinqOperatorProvider.SelectMany
                                .MakeGenericMethod(
                                    typeof(QuerySourceScope),
                                    typeof(QuerySourceScope)),
                            _outerSelectManyExpression.Arguments[0],
                            newExpression.Arguments[1]);
                }

                return newExpression;
            }

            private static bool MethodIsClosedFormOf(MethodInfo method, MethodInfo genericMethod)
            {
                return method.IsGenericMethod
                       && ReferenceEquals(
                           method.GetGenericMethodDefinition(),
                           genericMethod);
            }
        }

        public override void VisitWhereClause(WhereClause whereClause, QueryModel queryModel, int index)
        {
            //var previousExpression = Expression;
            var requiresClientEval = !_queriesBySource.Any();

            base.VisitWhereClause(whereClause, queryModel, index);

            foreach (var sourceQuery in _queriesBySource)
            {
                var filteringVisitor = new FilteringExpressionTreeVisitor(this);

                filteringVisitor.VisitExpression(whereClause.Predicate);

                sourceQuery.Value.Predicate = filteringVisitor.Predicate;

                requiresClientEval |= filteringVisitor.RequiresClientEval;
            }

            if (!requiresClientEval)
            {
                //Expression = previousExpression;
            }
        }

        private class FilteringExpressionTreeVisitor : ThrowingExpressionTreeVisitor
        {
            private readonly RelationalQueryModelVisitor _queryModelVisitor;

            private Expression _predicate;

            private bool _requiresClientEval;

            public FilteringExpressionTreeVisitor(RelationalQueryModelVisitor queryModelVisitor)
            {
                _queryModelVisitor = queryModelVisitor;
            }

            public Expression Predicate
            {
                get { return _predicate; }
            }

            public bool RequiresClientEval
            {
                get { return _requiresClientEval; }
            }

            protected override Expression VisitBinaryExpression(BinaryExpression expression)
            {
                _predicate = null;

                switch (expression.NodeType)
                {
                    case ExpressionType.Equal:
                    case ExpressionType.NotEqual:
                    case ExpressionType.GreaterThan:
                    case ExpressionType.GreaterThanOrEqual:
                    case ExpressionType.LessThan:
                    case ExpressionType.LessThanOrEqual:
                    {
                        _predicate = ProcessComparisonExpression(expression);

                        break;
                    }

                    case ExpressionType.AndAlso:
                    {
                        VisitExpression(expression.Left);

                        var left = _predicate;

                        VisitExpression(expression.Right);

                        var right = _predicate;

                        _predicate
                            = left != null
                              && right != null
                                ? Expression.AndAlso(left, right)
                                : (left ?? right);

                        break;
                    }

                    case ExpressionType.OrElse:
                    {
                        VisitExpression(expression.Left);

                        var left = _predicate;

                        VisitExpression(expression.Right);

                        var right = _predicate;

                        _predicate
                            = left != null
                              && right != null
                                ? Expression.OrElse(left, right)
                                : null;

                        break;
                    }

                    default:
                        throw new ArgumentOutOfRangeException();
                }

                return expression;
            }

            private Expression ProcessComparisonExpression(BinaryExpression expression)
            {
                var leftExpression = BindOperand(expression.Left);
                var rightExpression = BindOperand(expression.Right);

                if (leftExpression == null
                    || rightExpression == null)
                {
                    return null;
                }

                var nullExpression
                    = TransformNullComparison(leftExpression, rightExpression, expression.NodeType);

                if (nullExpression != null)
                {
                    return nullExpression;
                }

                return Expression
                    .MakeBinary(expression.NodeType, leftExpression, rightExpression);
            }

            private Expression TransformNullComparison(
                Expression left, Expression right, ExpressionType expressionType)
            {
                if (expressionType == ExpressionType.Equal
                    || expressionType == ExpressionType.NotEqual)
                {
                    var constant
                        = right as ConstantExpression
                          ?? left as ConstantExpression;

                    if (constant != null
                        && constant.Value == null)
                    {
                        var propertyAccess
                            = left as ColumnExpression
                              ?? right as ColumnExpression;

                        if (propertyAccess != null)
                        {
                            return expressionType == ExpressionType.Equal
                                ? (Expression)new IsNullExpression(propertyAccess)
                                : new IsNotNullExpression(propertyAccess);
                        }
                    }
                }

                return null;
            }

            protected override Expression VisitMethodCallExpression(MethodCallExpression expression)
            {
                var operand = BindOperand(expression.Object);

                if (operand != null)
                {
                    var arguments
                        = expression.Arguments
                            .Select(BindOperand)
                            .ToArray();

                    if (arguments.Length == expression.Arguments.Count)
                    {
                        var boundExpression
                            = Expression.Call(
                                operand,
                                expression.Method,
                                arguments);

                        _predicate
                            = _queryModelVisitor._methodCallTranslator
                                .Translate(boundExpression);

                        return expression;
                    }
                }

                _requiresClientEval = true;

                return expression;
            }

            private Expression BindOperand(Expression expression)
            {
                var memberExpression = expression as MemberExpression;

                if (memberExpression == null)
                {
                    var constantExpression = expression as ConstantExpression;

                    if (constantExpression == null)
                    {
                        _requiresClientEval = true;
                    }

                    return constantExpression;
                }

                var columnExpression 
                    = _queryModelVisitor
                    .BindMemberExpression(
                        memberExpression,
                        (property, querySource, selectExpression)
                            => new ColumnExpression(
                                property,
                                selectExpression.FindTableForQuerySource(querySource).Alias));

                if (columnExpression == null)
                {
                    _requiresClientEval = true;
                }

                return columnExpression;
            }

            protected override Expression VisitConstantExpression(ConstantExpression expression)
            {
                _predicate = expression;

                return expression;
            }

            protected override Expression VisitSubQueryExpression(SubQueryExpression expression)
            {
                _requiresClientEval = true;

                return expression;
            }

            protected override Exception CreateUnhandledItemException<T>(T unhandledItem, string visitMethod)
            {
                return new NotImplementedException("Filter expression not handled: " + unhandledItem.GetType().Name);
            }
        }

        public override void VisitOrderByClause(OrderByClause orderByClause, QueryModel queryModel, int index)
        {
            if (_preOrderingExpression == null)
            {
                _preOrderingExpression = Expression;
            }

            var orderingCounts
                = _queriesBySource
                    .Where(kv => kv.Value.OrderBy.Count > 0)
                    .Select(kv => new { kv.Key, kv.Value.OrderBy.Count })
                    .ToList();

            base.VisitOrderByClause(orderByClause, queryModel, index);

            foreach (var querySourceOrdering in orderingCounts)
            {
                var selectExpression = _queriesBySource[querySourceOrdering.Key];

                if (querySourceOrdering.Count != selectExpression.OrderBy.Count)
                {
                    var orderBy = selectExpression.OrderBy.ToList();

                    selectExpression.ClearOrderBy();
                    selectExpression.AddToOrderBy(orderBy.Skip(querySourceOrdering.Count));
                    selectExpression.AddToOrderBy(orderBy.Take(querySourceOrdering.Count));
                }
            }

            if (index == queryModel.BodyClauses.Count - 1)
            {
                var queriesWithOrdering
                    = _queriesBySource
                        .Where(kv => kv.Value.OrderBy.Any())
                        .Select(kv => kv.Value)
                        .ToList();

                if (queriesWithOrdering.Count == 1
                    && queriesWithOrdering[0].OrderBy.Count
                    == queryModel.BodyClauses
                        .OfType<OrderByClause>()
                        .SelectMany(ob => ob.Orderings)
                        .Count())
                {
                    queriesWithOrdering[0].RemoveFromProjection(queriesWithOrdering[0].OrderBy);

                    Expression = _preOrderingExpression;
                }
            }
        }

        protected override Expression ReplaceClauseReferences(
            Expression expression, QuerySourceMapping querySourceMapping)
        {
            Check.NotNull(expression, "expression");
            Check.NotNull(querySourceMapping, "querySourceMapping");

            return new MemberAccessToValueReaderReferenceReplacingExpressionTreeVisitor(querySourceMapping, this)
                .VisitExpression(expression);
        }

        private class MemberAccessToValueReaderReferenceReplacingExpressionTreeVisitor : ReferenceReplacingExpressionTreeVisitor
        {
            private readonly RelationalQueryModelVisitor _queryModelVisitor;

            public MemberAccessToValueReaderReferenceReplacingExpressionTreeVisitor(
                QuerySourceMapping querySourceMapping,
                RelationalQueryModelVisitor queryModelVisitor)
                : base(querySourceMapping, throwOnUnmappedReferences: false)
            {
                _queryModelVisitor = queryModelVisitor;
            }

            private static readonly MethodInfo _readValueMethodInfo
                = typeof(IValueReader).GetTypeInfo().GetDeclaredMethod("ReadValue");

            protected override Expression VisitMemberExpression(MemberExpression expression)
            {
                var newExpression = VisitExpression(expression.Expression);

                if (newExpression != expression.Expression)
                {
                    return newExpression.Type == typeof(IValueReader)
                        ? (Expression)_queryModelVisitor.BindMemberExpression(expression,
                            (property, querySource, selectExpression) =>
                                {
                                    var projectionIndex = selectExpression.GetProjectionIndex(property, querySource);

                                    Contract.Assert(projectionIndex > -1);

                                    return Expression.Call(
                                        newExpression,
                                        _readValueMethodInfo.MakeGenericMethod(expression.Type),
                                        new Expression[]
                                            {
                                                Expression.Constant(projectionIndex)
                                            });
                                })
                        : Expression.MakeMemberAccess(newExpression, expression.Member);
                }

                return expression;
            }
        }

        private void BindMemberExpression(
            MemberExpression memberExpression,
            Action<IProperty, IQuerySource, SelectExpression> memberBinder)
        {
            BindMemberExpression(memberExpression, null,
                (property, querySource, selectExpression) =>
                    {
                        memberBinder(property, querySource, selectExpression);

                        return default(object);
                    });
        }

        private TResult BindMemberExpression<TResult>(
            MemberExpression memberExpression,
            Func<IProperty, IQuerySource, SelectExpression, TResult> memberBinder)
        {
            return BindMemberExpression(memberExpression, null, memberBinder);
        }

        private TResult BindMemberExpression<TResult>(
            MemberExpression memberExpression,
            IQuerySource querySource,
            Func<IProperty, IQuerySource, SelectExpression, TResult> memberBinder)
        {
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
                        var selectExpression
                            = TryGetSelectExpression(querySourceReferenceExpression.ReferencedQuerySource);

                        if (selectExpression != null)
                        {
                            return memberBinder(
                                property,
                                querySourceReferenceExpression.ReferencedQuerySource,
                                selectExpression);
                        }

                        selectExpression
                            = _parentQueryModelVisitor != null
                                ? _parentQueryModelVisitor
                                    .TryGetSelectExpression(querySourceReferenceExpression.ReferencedQuerySource)
                                : null;

                        if (selectExpression != null)
                        {
                            selectExpression
                                .AddToProjection(property, querySourceReferenceExpression.ReferencedQuerySource);
                        }
                    }
                }
            }

            return default(TResult);
        }

        private class RelationalQueryingExpressionTreeVisitor : QueryingExpressionTreeVisitor
        {
            private static readonly ParameterExpression _readerParameter
                = Expression.Parameter(typeof(DbDataReader));

            private readonly RelationalQueryModelVisitor _queryModelVisitor;
            private readonly IQuerySource _querySource;

            public RelationalQueryingExpressionTreeVisitor(
                RelationalQueryModelVisitor queryModelVisitor, IQuerySource querySource)
                : base(queryModelVisitor)
            {
                _queryModelVisitor = queryModelVisitor;
                _querySource = querySource;
            }

            protected override Expression VisitMemberExpression(MemberExpression expression)
            {
                _queryModelVisitor
                    .BindMemberExpression(
                        expression,
                        (property, querySource, selectExpression)
                            => selectExpression.AddToProjection(property, querySource));

                return base.VisitMemberExpression(expression);
            }

            protected override Expression VisitEntityQueryable(Type elementType)
            {
                var relationalQueryCompilationContext = ((RelationalQueryCompilationContext)QueryCompilationContext);
                var queryMethodInfo = CreateValueReaderMethodInfo;
                var entityType = QueryCompilationContext.Model.GetEntityType(elementType);

                var selectExpression = new SelectExpression();

                selectExpression
                    .AddTable(
                        new TableExpression(
                            entityType.TableName(),
                            entityType.Schema(),
                            _querySource.ItemName.Replace("<generated>_", "t"),
                            _querySource));

                _queryModelVisitor._queriesBySource.Add(_querySource, selectExpression);

                var queryMethodArguments
                    = new List<Expression>
                        {
                            Expression.Constant(_querySource),
                            QueryContextParameter,
                            QuerySourceScopeParameter,
                            _readerParameter
                        };

                if (_queryModelVisitor.QuerySourceRequiresMaterialization(_querySource))
                {
                    foreach (var property in entityType.Properties)
                    {
                        selectExpression.AddToProjection(property, _querySource);
                    }

                    queryMethodInfo = CreateEntityMethodInfo.MakeGenericMethod(elementType);
                    queryMethodArguments.Add(Expression.Constant(0));
                }

                return Expression.Call(
                    relationalQueryCompilationContext.QueryMethodProvider.QueryMethod
                        .MakeGenericMethod(queryMethodInfo.ReturnType),
                    QueryContextParameter,
                    Expression.Constant(new CommandBuilder(selectExpression, relationalQueryCompilationContext)),
                    Expression.Lambda(
                        Expression.Call(queryMethodInfo, queryMethodArguments),
                        _readerParameter));
            }

            public static readonly MethodInfo CreateValueReaderMethodInfo
                = typeof(RelationalQueryingExpressionTreeVisitor).GetTypeInfo()
                    .GetDeclaredMethod("CreateValueReader");

            [UsedImplicitly]
            private static QuerySourceScope<IValueReader> CreateValueReader(
                IQuerySource querySource,
                QueryContext queryContext,
                QuerySourceScope parentQuerySourceScope,
                DbDataReader dataReader)
            {
                var relationalQueryContext = (RelationalQueryContext)queryContext;

                return new QuerySourceScope<IValueReader>(
                    querySource,
                    relationalQueryContext.ValueReaderFactory.Create(dataReader),
                    parentQuerySourceScope);
            }

            public static readonly MethodInfo CreateEntityMethodInfo
                = typeof(RelationalQueryingExpressionTreeVisitor).GetTypeInfo()
                    .GetDeclaredMethod("CreateEntity");

            [UsedImplicitly]
            private static QuerySourceScope<TEntity> CreateEntity<TEntity>(
                IQuerySource querySource,
                QueryContext queryContext,
                QuerySourceScope parentQuerySourceScope,
                DbDataReader dataReader,
                int readerOffset)
            {
                var relationalQueryContext = (RelationalQueryContext)queryContext;

                var valueReader
                    = relationalQueryContext.ValueReaderFactory.Create(dataReader);

                if (readerOffset > 0)
                {
                    valueReader
                        = new OffsetValueReaderDecorator(valueReader, readerOffset);
                }

                return new QuerySourceScope<TEntity>(
                    querySource,
                    // ReSharper disable once AssignNullToNotNullAttribute
                    (TEntity)queryContext.StateManager
                        .GetOrMaterializeEntry(
                            queryContext.Model.GetEntityType(typeof(TEntity)),
                            valueReader).Entity,
                    parentQuerySourceScope);
            }

            private class OffsetValueReaderDecorator : IValueReader
            {
                private readonly IValueReader _valueReader;
                private readonly int _offset;

                public OffsetValueReaderDecorator(IValueReader valueReader, int offset)
                {
                    _valueReader = valueReader;
                    _offset = offset;
                }

                public bool IsNull(int index)
                {
                    return _valueReader.IsNull(_offset + index);
                }

                public T ReadValue<T>(int index)
                {
                    return _valueReader.ReadValue<T>(_offset + index);
                }

                public int Count
                {
                    get { return _valueReader.Count; }
                }
            }
        }

        private class RelationalProjectionSubQueryExpressionTreeVisitor : ProjectionExpressionTreeVisitor
        {
            private readonly RelationalQueryModelVisitor _queryModelVisitor;

            public RelationalProjectionSubQueryExpressionTreeVisitor(RelationalQueryModelVisitor queryModelVisitor)
                : base(queryModelVisitor)
            {
                _queryModelVisitor = queryModelVisitor;
            }

            protected override Expression VisitMemberExpression(MemberExpression expression)
            {
                _queryModelVisitor
                    .BindMemberExpression(
                        expression,
                        (property, querySource, selectExpression)
                            => selectExpression.AddToProjection(property, querySource));

                return base.VisitMemberExpression(expression);
            }
        }

        private class RelationalOrderingSubQueryExpressionTreeVisitor : DefaultExpressionTreeVisitor
        {
            private readonly RelationalQueryModelVisitor _queryModelVisitor;
            private readonly Ordering _ordering;

            public RelationalOrderingSubQueryExpressionTreeVisitor(
                RelationalQueryModelVisitor queryModelVisitor, Ordering ordering)
                : base(queryModelVisitor)
            {
                _queryModelVisitor = queryModelVisitor;
                _ordering = ordering;
            }

            protected override Expression VisitMemberExpression(MemberExpression expression)
            {
                _queryModelVisitor
                    .BindMemberExpression(
                        expression,
                        (property, querySource, selectExpression)
                            => selectExpression
                                .AddToProjection(
                                    selectExpression
                                        .AddToOrderBy(property, querySource, _ordering.OrderingDirection)));

                return base.VisitMemberExpression(expression);
            }
        }
    }
}
