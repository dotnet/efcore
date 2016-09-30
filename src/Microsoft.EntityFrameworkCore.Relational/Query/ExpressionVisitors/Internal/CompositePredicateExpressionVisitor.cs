// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Remotion.Linq.Parsing;

namespace Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class CompositePredicateExpressionVisitor : RelinqExpressionVisitor
    {
        private readonly bool _useRelationalNulls;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public CompositePredicateExpressionVisitor(bool useRelationalNulls)
        {
            _useRelationalNulls = useRelationalNulls;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
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
