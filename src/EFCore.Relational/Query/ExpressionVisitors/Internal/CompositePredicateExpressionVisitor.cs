// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Remotion.Linq.Parsing;

namespace Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class CompositePredicateExpressionVisitor : RelinqExpressionVisitor
    {
        private readonly bool _useRelationalNulls;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public CompositePredicateExpressionVisitor(bool useRelationalNulls)
        {
            _useRelationalNulls = useRelationalNulls;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override Expression VisitExtension(Expression expression)
        {
            var selectExpression = expression as SelectExpression;
            if (selectExpression?.Predicate != null)
            {
                var predicate = new EqualityPredicateInExpressionOptimizer().Visit(selectExpression.Predicate);

                var predicateNegationExpressionOptimizer = new PredicateNegationExpressionOptimizer();

                predicate = predicateNegationExpressionOptimizer.Visit(predicate);

                predicate = new PredicateReductionExpressionOptimizer().Visit(predicate);

                predicate = new EqualityPredicateExpandingVisitor().Visit(predicate);

                predicate = predicateNegationExpressionOptimizer.Visit(predicate);

                if (_useRelationalNulls)
                {
                    predicate = new NullCompensatedExpression(predicate);
                }

                selectExpression.Predicate = predicate;
            }
            else if (expression is PredicateJoinExpressionBase joinExpression)
            {
                joinExpression.Predicate = new DiscriminatorPredicateOptimizingVisitor().Visit(joinExpression.Predicate);

                joinExpression.Predicate = new EqualityPredicateInExpressionOptimizer().Visit(joinExpression.Predicate);
            }

            return base.VisitExtension(expression);
        }

        private class DiscriminatorPredicateOptimizingVisitor : RelinqExpressionVisitor
        {
            protected override Expression VisitBinary(BinaryExpression binaryExpression)
            {
                return binaryExpression.NodeType == ExpressionType.Equal
                       && binaryExpression.Left.RemoveConvert() is ConditionalExpression conditionalExpression
                       && conditionalExpression.Test is DiscriminatorPredicateExpression discriminatorPredicateExpression
                       && conditionalExpression.IfFalse.IsNullConstantExpression()
                    ? Expression.AndAlso(
                        discriminatorPredicateExpression.Reduce(),
                        Expression.Equal(
                            conditionalExpression.IfTrue.Type == binaryExpression.Right.Type
                                ? conditionalExpression.IfTrue
                                : Expression.Convert(conditionalExpression.IfTrue, binaryExpression.Right.Type),
                            binaryExpression.Right))
                    : base.VisitBinary(binaryExpression);
            }
        }
    }
}
