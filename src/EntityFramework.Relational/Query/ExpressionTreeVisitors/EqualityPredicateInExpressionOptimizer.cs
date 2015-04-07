// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Relational.Query.Expressions;
using Microsoft.Data.Entity.Utilities;
using Remotion.Linq.Parsing;
using System.Collections.Generic;
using Remotion.Linq.Clauses.Expressions;

namespace Microsoft.Data.Entity.Relational.Query.ExpressionTreeVisitors
{
    public class EqualityPredicateInExpressionOptimizer : ExpressionTreeVisitor
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
                            inExpressionFactory: (c, vs) => Expression.Not(new InExpression(c, vs)));

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
            ExpressionType equalityType,
            Func<AliasExpression, List<Expression>, Expression> inExpressionFactory)
        {
            var leftExpression = VisitExpression(binaryExpression.Left);
            var rightExpression = VisitExpression(binaryExpression.Right);

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
                leftAliasExpression = equalityType == ExpressionType.Equal
                    ? MatchInExpression(leftExpression, ref leftInValues)
                    : MatchNotInExpression(leftExpression, ref leftInValues);
            }

            if (rightAliasExpression == null)
            {
                rightAliasExpression = equalityType == ExpressionType.Equal
                    ? MatchInExpression(rightExpression, ref rightInValues)
                    : MatchNotInExpression(rightExpression, ref rightInValues);
            }

            if (leftAliasExpression?.ColumnExpression() != null
                && rightAliasExpression?.ColumnExpression() != null
                && leftAliasExpression.ColumnExpression().Equals(rightAliasExpression.ColumnExpression()))
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

            if (leftExpression != binaryExpression.Left || rightExpression != binaryExpression.Right)
            {
                return Expression.MakeBinary(binaryExpression.NodeType, leftExpression, rightExpression);
            }

            return null;
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

        private static AliasExpression MatchInExpression(
            Expression expression,
            ref IReadOnlyList<Expression> values)
        {
            var inExpression = expression as InExpression;
            if (inExpression != null)
            {
                values = inExpression.Values;

                return inExpression.Alias;
            }

            return null;
        }

        private static AliasExpression MatchNotInExpression(
            Expression expression,
            ref IReadOnlyList<Expression> values)
        {
            var unaryExpression = expression as UnaryExpression;
            if (unaryExpression != null && unaryExpression.NodeType == ExpressionType.Not)
            {
                return MatchInExpression(unaryExpression.Operand, ref values);
            }

            return null;
        }
    }
}
