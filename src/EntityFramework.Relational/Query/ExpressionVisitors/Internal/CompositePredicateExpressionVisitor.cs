// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Query.Expressions;
using Remotion.Linq.Parsing;

namespace Microsoft.Data.Entity.Query.ExpressionVisitors.Internal
{
    public class CompositePredicateExpressionVisitor : RelinqExpressionVisitor
    {
        private readonly bool _useRelationalNulls;

        public CompositePredicateExpressionVisitor(bool useRelationalNulls)
        {
            _useRelationalNulls = useRelationalNulls;
        }

        public override Expression Visit([NotNull] Expression expression)
        {
            expression = new EqualityPredicateInExpressionOptimizer().Visit(expression);

            var predicateNegationExpressionOptimizer = new PredicateNegationExpressionOptimizer();

            expression = predicateNegationExpressionOptimizer.Visit(expression);

            expression = new EqualityPredicateExpandingVisitor().Visit(expression);

            expression = predicateNegationExpressionOptimizer.Visit(expression);

            if (_useRelationalNulls)
            {
                expression = new NotNullableExpression(expression);
            }

            return expression;
        }
    }
}
