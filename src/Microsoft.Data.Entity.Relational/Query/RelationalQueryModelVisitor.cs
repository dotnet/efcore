// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Query;
using Microsoft.Data.Entity.Relational.Query.Expressions;
using Microsoft.Data.Entity.Relational.Query.Sql;
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
        private readonly Dictionary<IQuerySource, SqlSelect> _queriesBySource
            = new Dictionary<IQuerySource, SqlSelect>();

        public RelationalQueryModelVisitor(
            [NotNull] RelationalQueryCompilationContext queryCompilationContext)
            : base(Check.NotNull(queryCompilationContext, "queryCompilationContext"))
        {
        }

        public virtual SqlSelect TryGetSqlSelect([NotNull] IQuerySource querySource)
        {
            Check.NotNull(querySource, "querySource");

            SqlSelect sqlSelect;
            return _queriesBySource.TryGetValue(querySource, out sqlSelect)
                ? sqlSelect
                : null;
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

        public override void VisitWhereClause(WhereClause whereClause, QueryModel queryModel, int index)
        {
            base.VisitWhereClause(whereClause, queryModel, index);

            foreach (var sourceQuery in _queriesBySource)
            {
                var filteringVisitor
                    = new FilteringExpressionTreeVisitor(this, sourceQuery.Key);

                filteringVisitor.VisitExpression(whereClause.Predicate);

                sourceQuery.Value.SetPredicate(filteringVisitor.Predicate);
            }
        }

        private class FilteringExpressionTreeVisitor : ThrowingExpressionTreeVisitor
        {
            private readonly RelationalQueryModelVisitor _queryModelVisitor;
            private readonly IQuerySource _querySource;

            private Expression _predicate;

            public FilteringExpressionTreeVisitor(
                RelationalQueryModelVisitor queryModelVisitor, IQuerySource querySource)
            {
                _queryModelVisitor = queryModelVisitor;
                _querySource = querySource;
            }

            public Expression Predicate
            {
                get { return _predicate; }
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
                var leftExpression = ProcessOperand(expression.Left);
                var rightExpression = ProcessOperand(expression.Right);

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
                            = left as PropertyAccessExpression
                              ?? right as PropertyAccessExpression;

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

            private Expression ProcessOperand(Expression expression)
            {
                var memberExpression = expression as MemberExpression;

                if (memberExpression == null)
                {
                    return expression as ConstantExpression;
                }

                return _queryModelVisitor
                    .BindMemberExpression(
                        memberExpression,
                        _querySource,
                        (property, _) => new PropertyAccessExpression(property));
            }

            protected override Expression VisitMethodCallExpression(MethodCallExpression expression)
            {
                return expression;
            }

            protected override Expression VisitConstantExpression(ConstantExpression expression)
            {
                _predicate = expression;

                return expression;
            }

            protected override Expression VisitSubQueryExpression(SubQueryExpression expression)
            {
                return expression;
            }

            protected override Exception CreateUnhandledItemException<T>(T unhandledItem, string visitMethod)
            {
                return new NotImplementedException("Filter expression not handled: " + unhandledItem.GetType().Name);
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
                            (property, sqlSelect) =>
                                Expression.Call(
                                    newExpression,
                                    _readValueMethodInfo.MakeGenericMethod(expression.Type),
                                    new Expression[]
                                        {
                                            Expression.Constant(sqlSelect.GetProjectionIndex(property))
                                        }))
                        : Expression.MakeMemberAccess(newExpression, expression.Member);
                }

                return expression;
            }
        }

        private void BindMemberExpression(
            MemberExpression memberExpression,
            Action<IProperty, SqlSelect> memberBinder)
        {
            BindMemberExpression(memberExpression, null,
                (property, sqlSelect) =>
                    {
                        memberBinder(property, sqlSelect);

                        return default(object);
                    });
        }

        private TResult BindMemberExpression<TResult>(
            MemberExpression memberExpression,
            Func<IProperty, SqlSelect, TResult> memberBinder)
        {
            return BindMemberExpression(memberExpression, null, memberBinder);
        }

        private TResult BindMemberExpression<TResult>(
            MemberExpression memberExpression,
            IQuerySource querySource,
            Func<IProperty, SqlSelect, TResult> memberBinder)
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
                        SqlSelect sqlSelect;
                        if (_queriesBySource
                            .TryGetValue(querySourceReferenceExpression.ReferencedQuerySource, out sqlSelect))
                        {
                            return memberBinder(property, sqlSelect);
                        }
                    }
                }
            }

            return default(TResult);
        }

        private class RelationalQueryingExpressionTreeVisitor : QueryingExpressionTreeVisitor
        {
            private readonly RelationalQueryModelVisitor _queryModelVisitor;
            private readonly IQuerySource _querySource;

            public RelationalQueryingExpressionTreeVisitor(
                RelationalQueryModelVisitor queryModelVisitor, IQuerySource querySource)
                : base(queryModelVisitor.QueryCompilationContext)
            {
                _queryModelVisitor = queryModelVisitor;
                _querySource = querySource;
            }

            protected override Expression VisitMemberExpression(MemberExpression expression)
            {
                _queryModelVisitor
                    .BindMemberExpression(
                        expression,
                        (property, sqlSelect)
                            => sqlSelect.AddToProjection(property));

                return base.VisitMemberExpression(expression);
            }

            protected override Expression VisitEntityQueryable(Type elementType)
            {
                var relationalQueryCompilationContext = ((RelationalQueryCompilationContext)QueryCompilationContext);
                var queryMethodInfo = relationalQueryCompilationContext.EnumerableMethodProvider.QueryValues;
                var entityType = QueryCompilationContext.Model.GetEntityType(elementType);

                var sqlSelect 
                    = new SqlSelect(relationalQueryCompilationContext.SqlGeneratingExpressionTreeVisitor)
                    .SetTableSource(entityType.StorageName);

                _queryModelVisitor._queriesBySource.Add(_querySource, sqlSelect);

                if (_queryModelVisitor.QuerySourceRequiresMaterialization(_querySource))
                {
                    foreach (var property in entityType.Properties)
                    {
                        sqlSelect.AddToProjection(property);
                    }

                    queryMethodInfo
                        = relationalQueryCompilationContext.EnumerableMethodProvider.QueryEntities
                            .MakeGenericMethod(elementType);
                }

                return Expression.Call(
                    queryMethodInfo, QueryContextParameter, Expression.Constant(sqlSelect));
            }
        }

        private class RelationalProjectionSubQueryExpressionTreeVisitor : ProjectionExpressionTreeVisitor
        {
            private readonly RelationalQueryModelVisitor _queryModelVisitor;

            public RelationalProjectionSubQueryExpressionTreeVisitor(RelationalQueryModelVisitor queryModelVisitor)
                : base(queryModelVisitor.QueryCompilationContext)
            {
                _queryModelVisitor = queryModelVisitor;
            }

            protected override Expression VisitMemberExpression(MemberExpression expression)
            {
                _queryModelVisitor
                    .BindMemberExpression(
                        expression,
                        (property, sqlSelect)
                            => sqlSelect.AddToProjection(property));

                return base.VisitMemberExpression(expression);
            }
        }

        private class RelationalOrderingSubQueryExpressionTreeVisitor : RelationalQueryingExpressionTreeVisitor
        {
            private readonly RelationalQueryModelVisitor _queryModelVisitor;
            private readonly Ordering _ordering;

            public RelationalOrderingSubQueryExpressionTreeVisitor(
                RelationalQueryModelVisitor queryModelVisitor, Ordering ordering)
                : base(queryModelVisitor, null)
            {
                _queryModelVisitor = queryModelVisitor;
                _ordering = ordering;
            }

            protected override Expression VisitMemberExpression(MemberExpression expression)
            {
                _queryModelVisitor
                    .BindMemberExpression(
                        expression,
                        (property, sqlSelect)
                            => sqlSelect.AddToOrderBy(property, _ordering.OrderingDirection));

                // TODO: Remove expressions when fully server eval'd
                return base.VisitMemberExpression(expression);
            }
        }
    }
}
