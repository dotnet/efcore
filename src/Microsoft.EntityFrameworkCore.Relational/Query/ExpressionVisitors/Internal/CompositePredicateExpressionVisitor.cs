// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using JetBrains.Annotations;
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

        public override Expression Visit([NotNull] Expression node)
        {
            node = new EqualityPredicateInExpressionOptimizer().Visit(node);

            var predicateNegationExpressionOptimizer = new PredicateNegationExpressionOptimizer();

            node = predicateNegationExpressionOptimizer.Visit(node);

            node = new EqualityPredicateExpandingVisitor().Visit(node);

            node = predicateNegationExpressionOptimizer.Visit(node);

            if (_useRelationalNulls)
            {
                node = new NotNullableExpression(node);
            }

            return node;
        }
    }
}
