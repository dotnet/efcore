// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Relational.Query.Expressions;
using Microsoft.Data.Entity.Utilities;
using Remotion.Linq.Parsing;

namespace Microsoft.Data.Entity.Relational.Query
{
    public class EqualityPredicateOptimizer : ExpressionTreeVisitor
    {
        protected override Expression VisitBinaryExpression(
            [NotNull] BinaryExpression binaryExpression)
        {
            Check.NotNull(binaryExpression, nameof(binaryExpression));

            switch (binaryExpression.NodeType)
            {
                case ExpressionType.OrElse:
                {
                    var optimized
                        = TryOptimize(
                            binaryExpression,
                            equalityType: ExpressionType.Equal,
                            inExpressionFactory: (c, vs) => new InExpression(c, vs));

                    if (optimized != null)
                    {
                        return optimized;
                    }

                    break;
                }

                case ExpressionType.AndAlso:
                {
                    var optimized
                        = TryOptimize(
                            binaryExpression,
                            equalityType: ExpressionType.NotEqual,
                            inExpressionFactory: (c, vs) => new NotInExpression(c, vs));

                    if (optimized != null)
                    {
                        return optimized;
                    }

                    break;
                }
            }

            return base.VisitBinaryExpression(binaryExpression);
        }

        private Expression TryOptimize<TInExpression>(
            BinaryExpression binaryExpression,
            ExpressionType equalityType,
            Func<ColumnExpression, Expression[], TInExpression> inExpressionFactory)
            where TInExpression : InExpressionBase
        {
            ConstantExpression leftConstantExpression, rightConstantExpression;

            var leftColumnExpression
                = MatchEqualityExpression(
                    binaryExpression.Left,
                    equalityType,
                    out leftConstantExpression);

            var rightColumnExpression
                = MatchEqualityExpression(
                    binaryExpression.Right,
                    equalityType,
                    out rightConstantExpression);

            if (leftColumnExpression != null
                && rightColumnExpression != null
                && leftColumnExpression.Equals(rightColumnExpression))
            {
                return inExpressionFactory(
                    leftColumnExpression,
                    new Expression[] { leftConstantExpression, rightConstantExpression });
            }

            if (leftColumnExpression != null)
            {
                var rightInExpression
                    = VisitExpression(binaryExpression.Right) as TInExpression;

                if (rightInExpression != null
                    && rightInExpression.Values != null
                    && leftColumnExpression.Equals(rightInExpression.Column))
                {
                    return inExpressionFactory(
                        leftColumnExpression,
                        new[] { leftConstantExpression }
                            .Concat(rightInExpression.Values)
                            .ToArray());
                }
            }

            if (rightColumnExpression != null)
            {
                var leftInExpression
                    = VisitExpression(binaryExpression.Left) as TInExpression;

                if (leftInExpression != null 
                    && leftInExpression.Values != null
                    && rightColumnExpression.Equals(leftInExpression.Column))
                {
                    return inExpressionFactory(
                        rightColumnExpression,
                        leftInExpression.Values
                            .Concat(new[] { rightConstantExpression })
                            .ToArray());
                }
            }

            return null;
        }

        private static ColumnExpression MatchEqualityExpression(
            Expression expression,
            ExpressionType equalityType,
            out ConstantExpression constantExpression)
        {
            constantExpression = null;

            var binaryExpression = expression as BinaryExpression;

            if (binaryExpression?.NodeType == equalityType)
            {
                constantExpression
                    = binaryExpression.Right as ConstantExpression
                      ?? binaryExpression.Left as ConstantExpression;

                if (constantExpression != null)
                {
                    return binaryExpression.Right as ColumnExpression
                           ?? binaryExpression.Left as ColumnExpression;
                }
            }

            return null;
        }
    }
}
