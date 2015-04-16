// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using JetBrains.Annotations;
using Remotion.Linq.Parsing;

namespace Microsoft.Data.Entity.Relational.Query.ExpressionTreeVisitors
{
    public class CompositePredicateExpressionTreeVisitor : ExpressionTreeVisitor
    {
        public override Expression VisitExpression(
            [NotNull]Expression expression)
        {
            var currentExpression = expression;
            var inExpressionOptimized = 
                new EqualityPredicateInExpressionOptimizer().VisitExpression(currentExpression);

            currentExpression = inExpressionOptimized;

            var negationOptimized1 =
                new PredicateNegationExpressionOptimizer()
                .VisitExpression(currentExpression);

            currentExpression = negationOptimized1;

            var equalityExpanded =
                new EqualityPredicateExpandingVisitor().VisitExpression(currentExpression);

            currentExpression = equalityExpanded;

            var negationOptimized2 =
                new PredicateNegationExpressionOptimizer()
                .VisitExpression(currentExpression);

            currentExpression = negationOptimized2;

            var optimizedNullExpansionVisitor = new NullSemanticsOptimizedExpandingVisitor();
            var nullSemanticsExpandedOptimized = optimizedNullExpansionVisitor.VisitExpression(currentExpression);
            if (optimizedNullExpansionVisitor.OptimizedExpansionPossible)
            {
                currentExpression = nullSemanticsExpandedOptimized;
            }
            else
            {
                currentExpression = new NullSemanticsExpandingVisitor()
                    .VisitExpression(currentExpression);
            }

            var negationOptimized3 =
                new PredicateNegationExpressionOptimizer()
                .VisitExpression(currentExpression);

            currentExpression = negationOptimized3;

            var reducedExpression = new ReducingExpressionVisitor()
                .VisitExpression(currentExpression);

            return reducedExpression;
        }

        private class ReducingExpressionVisitor : ExpressionTreeVisitor
        {
            public override Expression VisitExpression(Expression node)
            {
                if (node != null && node.CanReduce)
                {
                    var reduced = node.Reduce();
                    return base.VisitExpression(reduced);
                }

                return base.VisitExpression(node);
            }
        }
    }
}
