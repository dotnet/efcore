// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Internal;
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
        protected override Expression VisitBinary(BinaryExpression node)
        {
            Check.NotNull(node, nameof(node));

            switch (node.NodeType)
            {
                case ExpressionType.OrElse:
                {
                    return Optimize(
                        node,
                        equalityType: ExpressionType.Equal,
                        inExpressionFactory: (c, vs) => new InExpression(c, vs));
                }

                case ExpressionType.AndAlso:
                {
                    return Optimize(
                        node,
                        equalityType: ExpressionType.NotEqual,
                        inExpressionFactory: (c, vs) => Expression.Not(new InExpression(c, vs)));
                }
            }

            return base.VisitBinary(node);
        }

        private Expression Optimize(
            BinaryExpression binaryExpression,
            ExpressionType equalityType,
            Func<AliasExpression, List<Expression>, Expression> inExpressionFactory)
        {
            var leftExpression = Visit(binaryExpression.Left);
            var rightExpression = Visit(binaryExpression.Right);

            Expression leftNonColumnExpression, rightNonColumnExpression;
            IReadOnlyList<Expression> leftInValues = null;
            IReadOnlyList<Expression> rightInValues = null;

            var leftAliasExpression
                = MatchEqualityExpression(
                    leftExpression,
                    equalityType,
                    out leftNonColumnExpression);

            var rightAliasExpression
                = MatchEqualityExpression(
                    rightExpression,
                    equalityType,
                    out rightNonColumnExpression);

            if (leftAliasExpression == null)
            {
                leftAliasExpression = (equalityType == ExpressionType.Equal
                    ? MatchInExpression(leftExpression, ref leftInValues)
                    : MatchNotInExpression(leftExpression, ref leftInValues)) as AliasExpression;
            }

            if (rightAliasExpression == null)
            {
                rightAliasExpression = (equalityType == ExpressionType.Equal
                    ? MatchInExpression(rightExpression, ref rightInValues)
                    : MatchNotInExpression(rightExpression, ref rightInValues)) as AliasExpression;
            }

            if (leftAliasExpression.HasColumnExpression()
                && rightAliasExpression.HasColumnExpression()
                && leftAliasExpression.TryGetColumnExpression().Equals(rightAliasExpression.TryGetColumnExpression()))
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
                    leftAliasExpression,
                    inArguments);
            }

            return binaryExpression.Update(leftExpression, binaryExpression.Conversion, rightExpression);
        }

        private static AliasExpression MatchEqualityExpression(
            Expression expression,
            ExpressionType equalityType,
            out Expression nonColumnExpression)
        {
            nonColumnExpression = null;

            var binaryExpression = expression as BinaryExpression;

            if (binaryExpression?.NodeType == equalityType)
            {
                nonColumnExpression
                    = binaryExpression.Right as ConstantExpression
                      ?? binaryExpression.Right as ParameterExpression
                      ?? (Expression)(binaryExpression.Left as ConstantExpression)
                      ?? binaryExpression.Left as ParameterExpression;

                if (nonColumnExpression != null)
                {
                    return binaryExpression.Right as AliasExpression
                           ?? binaryExpression.Left as AliasExpression;
                }
            }

            return null;
        }

        private static Expression MatchInExpression(
            Expression expression,
            ref IReadOnlyList<Expression> values)
        {
            var inExpression = expression as InExpression;

            if (inExpression != null)
            {
                values = inExpression.Values;

                return inExpression.Operand;
            }

            return null;
        }

        private static Expression MatchNotInExpression(
            Expression expression,
            ref IReadOnlyList<Expression> values)
        {
            var unaryExpression = expression as UnaryExpression;

            return (unaryExpression != null)
                   && (unaryExpression.NodeType == ExpressionType.Not)
                ? MatchInExpression(unaryExpression.Operand, ref values)
                : null;
        }
    }
}
