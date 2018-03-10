// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Utilities;
using Remotion.Linq.Parsing;

namespace Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class EqualityPredicateInExpressionOptimizer : RelinqExpressionVisitor
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override Expression VisitBinary(BinaryExpression binaryExpression)
        {
            Check.NotNull(binaryExpression, nameof(binaryExpression));

            switch (binaryExpression.NodeType)
            {
                case ExpressionType.OrElse:
                {
                    return Optimize(
                        binaryExpression,
                        equalityType: ExpressionType.Equal,
                        inExpressionFactory: (c, vs) => new InExpression(c, vs));
                }

                case ExpressionType.AndAlso:
                {
                    return Optimize(
                        binaryExpression,
                        equalityType: ExpressionType.NotEqual,
                        inExpressionFactory: (c, vs) => Expression.Not(new InExpression(c, vs)));
                }
            }

            return base.VisitBinary(binaryExpression);
        }

        private Expression Optimize(
            BinaryExpression binaryExpression,
            ExpressionType equalityType,
            Func<Expression, List<Expression>, Expression> inExpressionFactory)
        {
            var leftExpression = Visit(binaryExpression.Left);
            var rightExpression = Visit(binaryExpression.Right);

            IReadOnlyList<Expression> leftInValues = null;
            IReadOnlyList<Expression> rightInValues = null;

            var leftColumnExpression
                = MatchEqualityExpression(
                    leftExpression,
                    equalityType,
                    out var leftNonColumnExpression);

            var rightColumnExpression
                = MatchEqualityExpression(
                    rightExpression,
                    equalityType,
                    out var rightNonColumnExpression);

            if (leftColumnExpression == null)
            {
                leftColumnExpression = equalityType == ExpressionType.Equal
                    ? MatchInExpression(leftExpression, ref leftInValues)
                    : MatchNotInExpression(leftExpression, ref leftInValues);
            }

            if (rightColumnExpression == null)
            {
                rightColumnExpression = equalityType == ExpressionType.Equal
                    ? MatchInExpression(rightExpression, ref rightInValues)
                    : MatchNotInExpression(rightExpression, ref rightInValues);
            }

            if (leftColumnExpression != null
                && rightColumnExpression != null
                && leftColumnExpression.Equals(rightColumnExpression))
            {
                var inArguments = new List<Expression>();
                if (leftNonColumnExpression != null)
                {
                    inArguments.Add(leftNonColumnExpression);
                }

                if (leftInValues != null)
                {
                    inArguments.AddRange(leftInValues);
                }

                if (rightNonColumnExpression != null)
                {
                    inArguments.Add(rightNonColumnExpression);
                }

                if (rightInValues != null)
                {
                    inArguments.AddRange(rightInValues);
                }

                return inExpressionFactory(
                    leftColumnExpression,
                    inArguments);
            }

            return binaryExpression.Update(leftExpression, binaryExpression.Conversion, rightExpression);
        }

        private static Expression MatchEqualityExpression(
            Expression expression,
            ExpressionType equalityType,
            out Expression nonColumnExpression)
        {
            nonColumnExpression = null;

            var binaryExpression = expression as BinaryExpression;

            if (binaryExpression?.NodeType == equalityType)
            {
                var left = binaryExpression.Left;
                var right = binaryExpression.Right;

                var isLeftConstantOrParameter = left is ConstantExpression || left is ParameterExpression;

                if (isLeftConstantOrParameter
                    || right is ConstantExpression
                    || right is ParameterExpression)
                {
                    nonColumnExpression = isLeftConstantOrParameter ? left : right;

                    return isLeftConstantOrParameter ? right : left;
                }
            }

            return null;
        }

        private static Expression MatchInExpression(
            Expression expression,
            ref IReadOnlyList<Expression> values)
        {
            if (expression is InExpression inExpression
                // We can merge InExpression only when it is from values.
                && inExpression.Values != null)
            {
                values = inExpression.Values;

                return inExpression.Operand;
            }

            return null;
        }

        private static Expression MatchNotInExpression(
            Expression expression,
            ref IReadOnlyList<Expression> values)
            => expression is UnaryExpression unaryExpression && unaryExpression.NodeType == ExpressionType.Not
                ? MatchInExpression(unaryExpression.Operand, ref values)
                : null;
    }
}
