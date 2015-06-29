// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Query.ExpressionVisitors
{
    public class ShapedQueryFindingExpressionVisitor : ExpressionVisitorBase
    {
        private readonly RelationalQueryCompilationContext _relationalQueryCompilationContext;

        private MethodCallExpression _shapedQueryMethodCall;

        public ShapedQueryFindingExpressionVisitor(
            [NotNull] RelationalQueryCompilationContext relationalQueryCompilationContext)
        {
            Check.NotNull(relationalQueryCompilationContext, nameof(relationalQueryCompilationContext));

            _relationalQueryCompilationContext = relationalQueryCompilationContext;
        }

        public virtual MethodCallExpression Find([NotNull] Expression expression)
        {
            Check.NotNull(expression, nameof(expression));

            Visit(expression);

            return _shapedQueryMethodCall;
        }

        protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
        {
            Check.NotNull(methodCallExpression, nameof(methodCallExpression));

            if (methodCallExpression.Method.MethodIsClosedFormOf(
                _relationalQueryCompilationContext.QueryMethodProvider.ShapedQueryMethod))
            {
                _shapedQueryMethodCall = methodCallExpression;
            }

            return _shapedQueryMethodCall == null
                ? base.VisitMethodCall(methodCallExpression)
                : methodCallExpression;
        }
    }
}
