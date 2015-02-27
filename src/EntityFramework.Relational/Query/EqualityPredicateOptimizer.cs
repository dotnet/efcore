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
                            expressionType: ExpressionType.OrElse,
                            equalityType: ExpressionType.Equal,
                            inFactory: (c, vs) => new InExpression(c, vs));

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
                            expressionType: ExpressionType.AndAlso,
                            equalityType: ExpressionType.NotEqual,
                            inFactory: (c, vs) => new NotInExpression(c, vs));

                    if (optimized != null)
                    {
                        return optimized;
                    }

                    break;
                }
            }

            return base.VisitBinaryExpression(binaryExpression);
        }

        private Expression TryOptimize(
            BinaryExpression binaryExpression,
            ExpressionType expressionType,
            ExpressionType equalityType,
            Func<ColumnExpression, Expression[], InExpressionBase> inFactory)
        {
            Expression value;

            var column = MatchEqualityExpression(binaryExpression.Left, equalityType, out value);

            var newLeft
                = column != null
                    ? inFactory(column, new[] { value })
                    : VisitExpression(binaryExpression.Left) as InExpressionBase;

            column = MatchEqualityExpression(binaryExpression.Right, equalityType, out value);

            var newRight
                = column != null
                    ? inFactory(column, new[] { value })
                    : VisitExpression(binaryExpression.Right) as InExpressionBase;

            if (newLeft != null
                && newRight != null)
            {
                if (newLeft.Column.Equals(newRight.Column))
                {
                    return inFactory(
                        newLeft.Column,
                        newLeft.Values.Concat(newRight.Values).ToArray());
                }

                return Expression.MakeBinary(
                    expressionType,
                    newLeft.Values.Count > 1 ? newLeft : binaryExpression.Left,
                    newRight.Values.Count > 1 ? newRight : binaryExpression.Right);
            }

            if (newLeft != null)
            {
                return Expression.MakeBinary(
                    expressionType,
                    newLeft.Values.Count > 1 ? newLeft : binaryExpression.Left,
                    binaryExpression.Right);
            }

            if (newRight != null)
            {
                return Expression.MakeBinary(
                    expressionType,
                    binaryExpression.Left,
                    newRight.Values.Count > 1 ? newRight : binaryExpression.Right);
            }

            return null;
        }

        private static ColumnExpression MatchEqualityExpression(
            Expression expression,
            ExpressionType equalityType,
            out Expression value)
        {
            value = null;

            var binaryExpression = expression as BinaryExpression;

            if (binaryExpression?.NodeType == equalityType)
            {
                value
                    = binaryExpression.Right as ConstantExpression
                      ?? binaryExpression.Left as ConstantExpression;

                if (value != null)
                {
                    return binaryExpression.Right as ColumnExpression
                           ?? binaryExpression.Left as ColumnExpression;
                }
            }

            return null;
        }
    }
}
