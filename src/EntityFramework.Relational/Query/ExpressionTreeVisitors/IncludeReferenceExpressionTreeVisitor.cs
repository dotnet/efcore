// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;
using Remotion.Linq.Clauses;
using Remotion.Linq.Parsing;

namespace Microsoft.Data.Entity.Relational.Query.ExpressionTreeVisitors
{
    public class IncludeReferenceExpressionTreeVisitor : ExpressionTreeVisitor
    {
        private readonly IQuerySource _querySource;
        private readonly INavigation _navigation;
        private readonly int _readerOffset;

        public IncludeReferenceExpressionTreeVisitor(
            [NotNull] IQuerySource querySource, [NotNull] INavigation navigation, int readerOffset)
        {
            Check.NotNull(querySource, "querySource");
            Check.NotNull(navigation, "navigation");

            _querySource = querySource;
            _navigation = navigation;
            _readerOffset = readerOffset;
        }

        protected override Expression VisitMethodCallExpression([NotNull] MethodCallExpression expression)
        {
            Check.NotNull(expression, "expression");

            if (expression.Method.MethodIsClosedFormOf(RelationalQueryModelVisitor.CreateEntityMethodInfo))
            {
                var querySource = ((ConstantExpression)expression.Arguments[0]).Value;

                if (querySource == _querySource)
                {
                    return Expression.Call(
                        RelationalQueryModelVisitor.IncludeReferenceMethodInfo
                            .MakeGenericMethod(expression.Method.GetGenericArguments()[0]),
                        expression.Arguments[1],
                        expression,
                        Expression.Constant(_navigation),
                        expression.Arguments[3],
                        Expression.Constant(_readerOffset)
                        );
                }
            }

            return base.VisitMethodCallExpression(expression);
        }
    }
}
