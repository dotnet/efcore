// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.Data.Entity.Relational.Query.Expressions;
using JetBrains.Annotations;
using Remotion.Linq.Parsing;

namespace Microsoft.Data.Entity.Relational.Query.ExpressionTreeVisitors
{
    public abstract class NullSemanticsExpressionVisitorBase : ExpressionTreeVisitor
    {
        protected Expression BuildIsNullExpression(List<Expression> nullableExpressions)
        {
            if (nullableExpressions.Count == 0)
            {
                return Expression.Constant(false);
            }

            if (nullableExpressions.Count == 1)
            {
                return new IsNullExpression(nullableExpressions[0]);
            }

            Expression current = new IsNullExpression(nullableExpressions[0]);
            for (int i = 1; i < nullableExpressions.Count; i++)
            {
                current = Expression.OrElse(current, new IsNullExpression(nullableExpressions[i]));
            }

            return current;
        }

        protected Expression BuildIsNotNullExpression(List<Expression> nullableExpressions)
        {
            if (nullableExpressions.Count == 0)
            {
                return Expression.Constant(true);
            }

            if (nullableExpressions.Count == 1)
            {
                return Expression.Not(new IsNullExpression(nullableExpressions[0]));
            }

            Expression current = nullableExpressions[0];
            for (int i = 1; i < nullableExpressions.Count; i++)
            {
                current = Expression.AndAlso(current, Expression.Not(new IsNullExpression(nullableExpressions[i])));
            }

            return current;
        }

        protected List<Expression> ExtractNullableExpressions(Expression expression)
        {
            var nullableExpressionsExtractor = new NullableExpressionsExtractingVisitor();
            nullableExpressionsExtractor.VisitExpression(expression);

            return nullableExpressionsExtractor.NullableExpressions;
        }
    }
}
