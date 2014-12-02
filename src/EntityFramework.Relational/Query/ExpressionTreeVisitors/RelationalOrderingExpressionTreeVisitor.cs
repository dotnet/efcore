// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Query.ExpressionTreeVisitors;
using Microsoft.Data.Entity.Utilities;
using Remotion.Linq.Clauses;

namespace Microsoft.Data.Entity.Relational.Query.ExpressionTreeVisitors
{
    public class RelationalOrderingExpressionTreeVisitor : DefaultQueryExpressionTreeVisitor
    {
        private readonly Ordering _ordering;

        public RelationalOrderingExpressionTreeVisitor(
            [NotNull] RelationalQueryModelVisitor queryModelVisitor, [NotNull] Ordering ordering)
            : base(Check.NotNull(queryModelVisitor, "queryModelVisitor"))
        {
            Check.NotNull(ordering, "ordering");

            _ordering = ordering;
        }

        private new RelationalQueryModelVisitor QueryModelVisitor
        {
            get { return (RelationalQueryModelVisitor)base.QueryModelVisitor; }
        }

        protected override Expression VisitMemberExpression([NotNull] MemberExpression memberExpression)
        {
            Check.NotNull(memberExpression, "memberExpression");

            ((RelationalQueryModelVisitor)base.QueryModelVisitor)
                .BindMemberExpression(
                    memberExpression,
                    (property, querySource, selectExpression)
                        => selectExpression
                            .AddToProjection(
                                selectExpression
                                    .AddToOrderBy(
                                        QueryModelVisitor.QueryCompilationContext.GetColumnName(property),
                                        property,
                                        querySource,
                                        _ordering.OrderingDirection)));

            return base.VisitMemberExpression(memberExpression);
        }

        protected override Expression VisitMethodCallExpression([NotNull] MethodCallExpression methodCallExpression)
        {
            Check.NotNull(methodCallExpression, "methodCallExpression");

            ((RelationalQueryModelVisitor)base.QueryModelVisitor)
                .BindMethodCallExpression(
                    methodCallExpression,
                    (property, querySource, selectExpression)
                        => selectExpression
                            .AddToProjection(
                                selectExpression
                                    .AddToOrderBy(
                                        QueryModelVisitor.QueryCompilationContext.GetColumnName(property),
                                        property,
                                        querySource,
                                        _ordering.OrderingDirection)));

            return base.VisitMethodCallExpression(methodCallExpression);
        }
    }
}
