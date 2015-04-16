// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Remotion.Linq.Parsing;

namespace Microsoft.Data.Entity.Relational.Query.ExpressionTreeVisitors
{
    public class PredicateNegationExpressionOptimizer : ExpressionTreeVisitor
    {
        private static Dictionary<ExpressionType, ExpressionType> _nodeTypeMapping
            = new Dictionary<ExpressionType, ExpressionType>
            {
                { ExpressionType.GreaterThan, ExpressionType.LessThanOrEqual },
                { ExpressionType.GreaterThanOrEqual, ExpressionType.LessThan },
                { ExpressionType.LessThanOrEqual, ExpressionType.GreaterThan },
                { ExpressionType.LessThan, ExpressionType.GreaterThanOrEqual },
            };

        protected override Expression VisitBinaryExpression(
            [NotNull]BinaryExpression expression)
        {
            var currentExpression = expression;
            if (currentExpression.NodeType == ExpressionType.Equal
                || currentExpression.NodeType == ExpressionType.NotEqual)
            {
                var leftUnary = currentExpression.Left as UnaryExpression;
                if (leftUnary != null && leftUnary.NodeType == ExpressionType.Not)
                {
                    var leftNullable = ExtractNullableExpressions(leftUnary.Operand).Count > 0;
                    var rightNullable = ExtractNullableExpressions(currentExpression.Right).Count > 0;

                    if (!leftNullable && !rightNullable)
                    {
                        // e.g. !a == b -> a != b
                        currentExpression = currentExpression.NodeType == ExpressionType.Equal
                            ? Expression.MakeBinary(
                                ExpressionType.NotEqual, leftUnary.Operand, currentExpression.Right)
                            : Expression.MakeBinary(
                                ExpressionType.Equal, leftUnary.Operand, currentExpression.Right);
                    }
                }

                var rightUnary = currentExpression.Right as UnaryExpression;
                if (rightUnary != null && rightUnary.NodeType == ExpressionType.Not)
                {
                    var leftNullable = ExtractNullableExpressions(currentExpression.Left).Count > 0;
                    var rightNullable = ExtractNullableExpressions(rightUnary).Count > 0;

                    if (!leftNullable && !rightNullable)
                    {
                        // e.g. a != !b -> a == b
                        currentExpression = currentExpression.NodeType == ExpressionType.Equal
                        ? Expression.MakeBinary(
                            ExpressionType.NotEqual, currentExpression.Left, rightUnary.Operand)
                        : Expression.MakeBinary(
                            ExpressionType.Equal, currentExpression.Left, rightUnary.Operand);
                    }
                }
            }

            return base.VisitBinaryExpression(currentExpression);
        }

        private Expression UnwrapConvert(Expression expression, out Type conversionType)
        {
            conversionType = null;
            var unary = expression as UnaryExpression;
            if (unary != null && unary.NodeType == ExpressionType.Convert)
            {
                conversionType = unary.Type;
                return unary.Operand;
            }

            return expression;
        }

        private List<Expression> ExtractNullableExpressions(Expression expression)
        {
            var nullableExpressionsExtractor = new NullableExpressionsExtractingVisitor();
            nullableExpressionsExtractor.VisitExpression(expression);

            return nullableExpressionsExtractor.NullableExpressions;
        }

        protected override Expression VisitUnaryExpression(
            [NotNull]UnaryExpression expression)
        {
            if (expression.NodeType == ExpressionType.Not)
            {
                var innerUnary = expression.Operand as UnaryExpression;
                if (innerUnary != null && innerUnary.NodeType == ExpressionType.Not)
                {
                    // !(!(a)) => a
                    return VisitExpression(innerUnary.Operand);
                }

                var innerBinary = expression.Operand as BinaryExpression;
                if (innerBinary != null)
                {
                    if (innerBinary.NodeType == ExpressionType.Equal 
                        || innerBinary.NodeType == ExpressionType.NotEqual)
                    {
                        // TODO: this is only valid for non-nullable terms, or if null semantics expansion is performed
                        // if user opts-out of the null semantics, we should not apply this rule
                        // !(a == b) -> a != b
                        // !(a != b) -> a == b
                        return innerBinary.NodeType == ExpressionType.Equal
                            ? VisitExpression(Expression.NotEqual(innerBinary.Left, innerBinary.Right))
                            : VisitExpression(Expression.Equal(innerBinary.Left, innerBinary.Right));
                    }

                    if (innerBinary.NodeType == ExpressionType.AndAlso)
                    {
                        // !(a && b) -> !a || !b
                        return VisitExpression(
                            Expression.MakeBinary(
                                ExpressionType.OrElse,
                                Expression.Not(innerBinary.Left), 
                                Expression.Not(innerBinary.Right)));
                    }

                    if (innerBinary.NodeType == ExpressionType.OrElse)
                    {
                        // !(a || b) -> !a && !b
                        return VisitExpression(
                            Expression.MakeBinary(
                                ExpressionType.AndAlso,
                                Expression.Not(innerBinary.Left),
                                Expression.Not(innerBinary.Right)));
                    }

                    if (_nodeTypeMapping.ContainsKey(innerBinary.NodeType))
                    {
                        // e.g. !(a > b) -> a <= b
                        return VisitExpression(
                            Expression.MakeBinary(
                                _nodeTypeMapping[innerBinary.NodeType],
                                innerBinary.Left,
                                innerBinary.Right));
                    }
                }
            }

            return base.VisitUnaryExpression(expression);
        }
    }
}
