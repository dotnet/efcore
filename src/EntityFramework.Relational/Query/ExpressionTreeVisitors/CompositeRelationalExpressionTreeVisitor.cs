// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using JetBrains.Annotations;
using Remotion.Linq.Parsing;
using Microsoft.Data.Entity.Relational.Query.Expressions;

namespace Microsoft.Data.Entity.Relational.Query.ExpressionTreeVisitors
{
    public class CompositePredicateExpressionTreeVisitor : ExpressionTreeVisitor
    {
        private bool _useRelationalNullSemantics;

        public CompositePredicateExpressionTreeVisitor(bool useRelationalNullSemantics)
        {
            _useRelationalNullSemantics = useRelationalNullSemantics;
        }

        public override Expression VisitExpression(
            [NotNull] Expression expression)
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

            var parameterDectector = new ParameterExpressionDetectingVisitor();
            parameterDectector.VisitExpression(currentExpression);

            if (!parameterDectector.ContainsParameters && !_useRelationalNullSemantics)
            {
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
            }

            if (_useRelationalNullSemantics)
            {
                currentExpression = new NotNullableExpression(currentExpression);
            }

            var negationOptimized3 =
                new PredicateNegationExpressionOptimizer()
                    .VisitExpression(currentExpression);

            currentExpression = negationOptimized3;

            return currentExpression;
        }

        private class ReducingExpressionVisitor : ExpressionTreeVisitor
        {
            public override Expression VisitExpression(Expression node)
                => node != null
                   && node.CanReduce
                    ? base.VisitExpression(node.Reduce())
                    : base.VisitExpression(node);
        }

        private class ParameterExpressionDetectingVisitor : ExpressionTreeVisitor
        {
            public bool ContainsParameters { get; set; }

            protected override Expression VisitParameterExpression(ParameterExpression expression)
            {
                ContainsParameters = true;

                return base.VisitParameterExpression(expression);
            }
        }
    }
}
