// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Query.Expressions;
using Remotion.Linq.Parsing;

namespace Microsoft.Data.Entity.Query.ExpressionVisitors
{
    public class CompositePredicateExpressionVisitor : RelinqExpressionVisitor
    {
        private readonly bool _useRelationalNullSemantics;

        public CompositePredicateExpressionVisitor(bool useRelationalNullSemantics)
        {
            _useRelationalNullSemantics = useRelationalNullSemantics;
        }

        public override Expression Visit(
            [NotNull] Expression expression)
        {
            var currentExpression = expression;
            var inExpressionOptimized =
                new EqualityPredicateInExpressionOptimizer().Visit(currentExpression);

            currentExpression = inExpressionOptimized;

            var negationOptimized1 =
                new PredicateNegationExpressionOptimizer()
                    .Visit(currentExpression);

            currentExpression = negationOptimized1;

            var equalityExpanded =
                new EqualityPredicateExpandingVisitor().Visit(currentExpression);

            currentExpression = equalityExpanded;

            var negationOptimized2 =
                new PredicateNegationExpressionOptimizer()
                    .Visit(currentExpression);

            currentExpression = negationOptimized2;

            var parameterDectector = new ParameterExpressionDetectingVisitor();
            parameterDectector.Visit(currentExpression);

            if (!parameterDectector.ContainsParameters
                && !_useRelationalNullSemantics)
            {
                var optimizedNullExpansionVisitor = new NullSemanticsOptimizedExpandingVisitor();
                var nullSemanticsExpandedOptimized = optimizedNullExpansionVisitor.Visit(currentExpression);
                if (optimizedNullExpansionVisitor.OptimizedExpansionPossible)
                {
                    currentExpression = nullSemanticsExpandedOptimized;
                }
                else
                {
                    currentExpression = new NullSemanticsExpandingVisitor()
                        .Visit(currentExpression);
                }
            }

            if (_useRelationalNullSemantics)
            {
                currentExpression = new NotNullableExpression(currentExpression);
            }

            var negationOptimized3 =
                new PredicateNegationExpressionOptimizer()
                    .Visit(currentExpression);

            currentExpression = negationOptimized3;

            return currentExpression;
        }

        private class ParameterExpressionDetectingVisitor : RelinqExpressionVisitor
        {
            public bool ContainsParameters { get; set; }

            protected override Expression VisitParameter(ParameterExpression expression)
            {
                ContainsParameters = true;

                return base.VisitParameter(expression);
            }
        }
    }
}
