// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Query;
using Microsoft.Data.Entity.Relational.Utilities;
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

        private readonly IEnumerableMethodProvider _enumerableMethodProvider;

        public RelationalQueryModelVisitor(
            [NotNull] QueryCompilationContext queryCompilationContext,
            [NotNull] IEnumerableMethodProvider enumerableMethodProvider)
            : base(Check.NotNull(queryCompilationContext, "queryCompilationContext"))
        {
            Check.NotNull(enumerableMethodProvider, "enumerableMethodProvider");

            _enumerableMethodProvider = enumerableMethodProvider;
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
                    if (newExpression.Type == typeof(IValueReader))
                    {
                        var querySourceReferenceExpression
                            = (QuerySourceReferenceExpression)expression.Expression;

                        var entityType
                            = _queryModelVisitor.QueryCompilationContext.Model
                                .GetEntityType(querySourceReferenceExpression.ReferencedQuerySource.ItemType);

                        var property = entityType.GetProperty(expression.Member.Name);

                        SqlSelect entityQuery;
                        if (_queryModelVisitor._queriesBySource
                            .TryGetValue(querySourceReferenceExpression.ReferencedQuerySource, out entityQuery))
                        {
                            return Expression.Call(
                                newExpression,
                                _readValueMethodInfo.MakeGenericMethod(expression.Type),
                                new Expression[] { Expression.Constant(entityQuery.GetProjectionIndex(property)) });
                        }
                    }

                    return Expression.MakeMemberAccess(newExpression, expression.Member);
                }

                return expression;
            }
        }

        private void ProcessMemberExpression(MemberExpression expression, Action<SqlSelect, IProperty> queryAction)
        {
            var querySourceReferenceExpression
                = expression.Expression as QuerySourceReferenceExpression;

            if (querySourceReferenceExpression != null)
            {
                var querySource = querySourceReferenceExpression.ReferencedQuerySource;

                var entityType
                    = QueryCompilationContext.Model
                        .TryGetEntityType(querySource.ItemType);

                if (entityType != null)
                {
                    var property = entityType.TryGetProperty(expression.Member.Name);

                    if (property != null)
                    {
                        SqlSelect entityQuery;
                        if (_queriesBySource.TryGetValue(querySource, out entityQuery))
                        {
                            queryAction(entityQuery, property);
                        }
                    }
                }
            }
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
                    .ProcessMemberExpression(
                        expression,
                        (sqlSelect, property)
                            => sqlSelect.AddToProjection(property));

                return base.VisitMemberExpression(expression);
            }

            protected override Expression VisitEntityQueryable(Type elementType)
            {
                var queryMethodInfo = _queryModelVisitor._enumerableMethodProvider.QueryValues;
                var entityType = QueryCompilationContext.Model.GetEntityType(elementType);

                var entityQuery = new SqlSelect().SetTableSource(entityType.StorageName);

                _queryModelVisitor._queriesBySource.Add(_querySource, entityQuery);

                if (_queryModelVisitor.QuerySourceRequiresMaterialization(_querySource))
                {
                    foreach (var property in entityType.Properties)
                    {
                        entityQuery.AddToProjection(property);
                    }

                    queryMethodInfo
                        = _queryModelVisitor._enumerableMethodProvider.QueryEntities
                            .MakeGenericMethod(elementType);
                }

                return Expression.Call(
                    queryMethodInfo, QueryContextParameter, Expression.Constant(entityQuery));
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
                    .ProcessMemberExpression(
                        expression,
                        (sqlSelect, property)
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
                    .ProcessMemberExpression(
                        expression,
                        (sqlSelect, property)
                            => sqlSelect.AddToOrderBy(property, _ordering.OrderingDirection));

                // TODO: Remove expressions when fully server eval'd
                return base.VisitMemberExpression(expression);
            }
        }
    }
}
