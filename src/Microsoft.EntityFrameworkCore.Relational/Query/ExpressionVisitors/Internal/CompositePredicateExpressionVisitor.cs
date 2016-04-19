// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Remotion.Linq.Parsing;

namespace Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal
{
    public class CompositePredicateExpressionVisitor : RelinqExpressionVisitor
    {
        private readonly bool _useRelationalNulls;

        public CompositePredicateExpressionVisitor(bool useRelationalNulls)
        {
            _useRelationalNulls = useRelationalNulls;
        }

        public override Expression Visit(Expression expression)
        {
            if (expression != null)
            {
                expression = new EqualityPredicateInExpressionOptimizer().Visit(expression);

                var predicateNegationExpressionOptimizer = new PredicateNegationExpressionOptimizer();

                expression = predicateNegationExpressionOptimizer.Visit(expression);

                expression = new PredicateReductionExpressionOptimizer().Visit(expression);

                expression = new EqualityPredicateExpandingVisitor().Visit(expression);

                expression = predicateNegationExpressionOptimizer.Visit(expression);

                if (_useRelationalNulls)
                {
                    expression = new NotNullableExpression(expression);
                }
            }

            return expression;
        }
    }
}
