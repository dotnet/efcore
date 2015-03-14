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

            if (inExpressionOptimized != null)
            {
                currentExpression = inExpressionOptimized;
            }

            var equalityExpanded = 
                new EqualityPredicateExpandingVisitor().VisitExpression(currentExpression);

            if (equalityExpanded != null)
            {
                currentExpression = equalityExpanded;
            }

            var negationOptimized =
                new PredicateNegationExpressionOptimizer().VisitExpression(currentExpression);

            if (negationOptimized != null)
            {
                currentExpression = negationOptimized;
            }

            return currentExpression;
        }
    }
}
