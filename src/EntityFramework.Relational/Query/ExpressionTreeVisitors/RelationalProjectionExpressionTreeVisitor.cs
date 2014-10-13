// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Query.ExpressionTreeVisitors;
using Microsoft.Data.Entity.Relational.Utilities;

namespace Microsoft.Data.Entity.Relational.Query.ExpressionTreeVisitors
{
    public class RelationalProjectionExpressionTreeVisitor : ProjectionExpressionTreeVisitor
    {
        public RelationalProjectionExpressionTreeVisitor([NotNull] RelationalQueryModelVisitor queryModelVisitor)
            : base(Check.NotNull(queryModelVisitor, "queryModelVisitor"))
        {
        }

        protected override Expression VisitMemberExpression([NotNull] MemberExpression memberExpression)
        {
            Check.NotNull(memberExpression, "memberExpression");

            ((RelationalQueryModelVisitor)base.QueryModelVisitor)
                .BindMemberExpression(
                    memberExpression,
                    (property, querySource, selectExpression)
                        => selectExpression.AddToProjection(property, querySource));

            return base.VisitMemberExpression(memberExpression);
        }

        protected override Expression VisitMethodCallExpression([NotNull] MethodCallExpression methodCallExpression)
        {
            Check.NotNull(methodCallExpression, "methodCallExpression");

            ((RelationalQueryModelVisitor)base.QueryModelVisitor)
                .BindMethodCallExpression(
                    methodCallExpression,
                    (property, querySource, selectExpression)
                        => selectExpression.AddToProjection(property, querySource));

            return base.VisitMethodCallExpression(methodCallExpression);
        }
    }
}
