// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Remotion.Linq.Parsing;

namespace Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class PredicateNegationExpressionOptimizer : RelinqExpressionVisitor
    {
        private static readonly Dictionary<ExpressionType, ExpressionType> _nodeTypeMapping
            = new Dictionary<ExpressionType, ExpressionType>
            {
                { ExpressionType.GreaterThan, ExpressionType.LessThanOrEqual },
                { ExpressionType.GreaterThanOrEqual, ExpressionType.LessThan },
                { ExpressionType.LessThanOrEqual, ExpressionType.GreaterThan },
                { ExpressionType.LessThan, ExpressionType.GreaterThanOrEqual }
            };

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override Expression VisitBinary(BinaryExpression binaryExpression)
        {
            var currentExpression = binaryExpression;
            if (currentExpression.NodeType == ExpressionType.Equal
                || currentExpression.NodeType == ExpressionType.NotEqual)
            {
                if (currentExpression.Left is UnaryExpression leftUnary
                    && leftUnary.NodeType == ExpressionType.Not)
                {
                    var leftNullable = BuildIsNullExpression(leftUnary.Operand) != null;
                    var rightNullable = BuildIsNullExpression(currentExpression.Right) != null;

                    if (!leftNullable
                        && !rightNullable)
                    {
                        // e.g. !a == b -> a != b
                        currentExpression = currentExpression.NodeType == ExpressionType.Equal
                            ? Expression.MakeBinary(
                                ExpressionType.NotEqual, leftUnary.Operand, currentExpression.Right)
                            : Expression.MakeBinary(
                                ExpressionType.Equal, leftUnary.Operand, currentExpression.Right);
                    }
                }

                if (currentExpression.Right is UnaryExpression rightUnary
                    && rightUnary.NodeType == ExpressionType.Not)
                {
                    var leftNullable = BuildIsNullExpression(currentExpression.Left) != null;
                    var rightNullable = BuildIsNullExpression(rightUnary) != null;

                    if (!leftNullable
                        && !rightNullable)
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

            return base.VisitBinary(currentExpression);
        }

        private static Expression BuildIsNullExpression(Expression expression)
        {
            var nullableExpressionsExtractor = new IsNullExpressionBuildingVisitor();
            nullableExpressionsExtractor.Visit(expression);

            return nullableExpressionsExtractor.ResultExpression;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override Expression VisitUnary(UnaryExpression unaryExpression)
        {
            if (unaryExpression.NodeType == ExpressionType.Not)
            {
                if (unaryExpression.Operand is ConstantExpression constantExpression
                    && constantExpression.Type == typeof(bool))
                {
                    // !(false) -> true
                    // !(true) -> false
                    return Expression.Constant((bool)constantExpression.Value ? false : true);
                }

                if (unaryExpression.Operand is UnaryExpression innerUnary
                    && innerUnary.NodeType == ExpressionType.Not)
                {
                    // !(!(a)) -> a
                    return Visit(innerUnary.Operand);
                }

                var nullCompensatedExpression = unaryExpression.Operand as NullCompensatedExpression;
                if ((nullCompensatedExpression?.Operand ?? unaryExpression.Operand) is BinaryExpression innerBinary)
                {
                    Expression result = null;
                    if (innerBinary.NodeType == ExpressionType.Equal
                        || innerBinary.NodeType == ExpressionType.NotEqual)
                    {
                        // TODO: this is only valid for non-nullable terms, or if null semantics expansion is performed
                        // if user opts-out of the null semantics, we should not apply this rule
                        // !(a == b) -> a != b
                        // !(a != b) -> a == b
                        result = innerBinary.NodeType == ExpressionType.Equal
                            ? Visit(Expression.NotEqual(innerBinary.Left, innerBinary.Right))
                            : Visit(Expression.Equal(innerBinary.Left, innerBinary.Right));
                    }

                    if (innerBinary.NodeType == ExpressionType.AndAlso)
                    {
                        // !(a && b) -> !a || !b
                        result = Visit(
                            Expression.MakeBinary(
                                ExpressionType.OrElse,
                                Expression.Not(innerBinary.Left),
                                Expression.Not(innerBinary.Right)));
                    }

                    if (innerBinary.NodeType == ExpressionType.OrElse)
                    {
                        // !(a || b) -> !a && !b
                        result = Visit(
                            Expression.MakeBinary(
                                ExpressionType.AndAlso,
                                Expression.Not(innerBinary.Left),
                                Expression.Not(innerBinary.Right)));
                    }

                    if (_nodeTypeMapping.ContainsKey(innerBinary.NodeType))
                    {
                        // e.g. !(a > b) -> a <= b
                        result = Visit(
                            Expression.MakeBinary(
                                _nodeTypeMapping[innerBinary.NodeType],
                                innerBinary.Left,
                                innerBinary.Right));
                    }

                    if (result != null)
                    {
                        return nullCompensatedExpression != null
                            ? new NullCompensatedExpression(result)
                            : result;
                    }
                }
            }

            return base.VisitUnary(unaryExpression);
        }
    }
}
