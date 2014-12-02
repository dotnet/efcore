// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Query.ExpressionTreeVisitors;
using Microsoft.Data.Entity.Utilities;
using Remotion.Linq.Clauses.Expressions;

namespace Microsoft.Data.Entity.Relational.Query.ExpressionTreeVisitors
{
    public class RelationalProjectionExpressionTreeVisitor : ProjectionExpressionTreeVisitor
    {
        private bool _requiresClientEval;

        public RelationalProjectionExpressionTreeVisitor([NotNull] RelationalQueryModelVisitor queryModelVisitor)
            : base(Check.NotNull(queryModelVisitor, "queryModelVisitor"))
        {
        }

        private new RelationalQueryModelVisitor QueryModelVisitor => (RelationalQueryModelVisitor)base.QueryModelVisitor;

        public virtual bool RequiresClientEval => _requiresClientEval;

        protected override Expression VisitMemberExpression([NotNull] MemberExpression memberExpression)
        {
            Check.NotNull(memberExpression, "memberExpression");

            if (!((RelationalQueryModelVisitor)base.QueryModelVisitor)
                .BindMemberExpression(
                    memberExpression,
                    (property, querySource, selectExpression) =>
                        {
                            selectExpression.AddToProjection(
                                QueryModelVisitor.QueryCompilationContext
                                    .GetColumnName(property),
                                property,
                                querySource);

                            return true;
                        }))
            {
                _requiresClientEval = true;
            }

            return base.VisitMemberExpression(memberExpression);
        }

        protected override Expression VisitMethodCallExpression([NotNull] MethodCallExpression methodCallExpression)
        {
            Check.NotNull(methodCallExpression, "methodCallExpression");

            if (!((RelationalQueryModelVisitor)base.QueryModelVisitor)
                .BindMethodCallExpression(
                    methodCallExpression,
                    (property, querySource, selectExpression)
                        =>
                        {
                            selectExpression.AddToProjection(
                                QueryModelVisitor.QueryCompilationContext
                                    .GetColumnName(property),
                                property,
                                querySource);

                            return true;
                        }))
            {
                _requiresClientEval = true;
            }

            return base.VisitMethodCallExpression(methodCallExpression);
        }

        public override Expression VisitExpression([NotNull] Expression expression)
        {
            if (!(expression is MemberExpression
                  || expression is MethodCallExpression
                  || expression is QuerySourceReferenceExpression))
            {
                _requiresClientEval = true;
            }

            return base.VisitExpression(expression);
        }
    }
}
