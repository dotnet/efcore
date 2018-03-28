// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Utilities;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Clauses.ResultOperators;

namespace Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class AllAnyToContainsResultOperatorExpressionRewriter : ExpressionVisitorBase
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override Expression VisitSubQuery(SubQueryExpression expression)
        {
            Check.NotNull(expression, nameof(expression));

            var subQueryModel = expression.QueryModel;

            var fromExpression = subQueryModel.MainFromClause.FromExpression;

            if (fromExpression.NodeType == ExpressionType.Parameter
                || fromExpression.NodeType == ExpressionType.Constant
                || fromExpression.NodeType == ExpressionType.ListInit
                || fromExpression.NodeType == ExpressionType.NewArrayInit)
            {
                if (subQueryModel.ResultOperators.Count == 1
                    && subQueryModel.BodyClauses.Count == 0
                    && subQueryModel.ResultOperators[0] is AllResultOperator all)
                {
                    if (TryMatchInequalityExpression(
                        all.Predicate,
                        out var left,
                        out var right))
                    {
                        var containsItem = CompareReturnOpposite(subQueryModel.SelectClause.Selector, left, right);

                        if (containsItem != null)
                        {
                            subQueryModel.ResultOperators.Clear();
                            subQueryModel.ResultOperators.Add(new ContainsResultOperator(containsItem));

                            return Expression.Not(
                                new SubQueryExpression(subQueryModel));
                        }
                    }
                }

                if (subQueryModel.ResultOperators.Count == 1
                    && subQueryModel.BodyClauses.Count == 1
                    && subQueryModel.ResultOperators[0] is AnyResultOperator
                    && subQueryModel.BodyClauses[0] is WhereClause whereClause)
                {
                    if (TryMatchEqualityExpression(
                        whereClause.Predicate,
                        out var left,
                        out var right))
                    {
                        var containsItem = CompareReturnOpposite(subQueryModel.SelectClause.Selector, left, right);

                        if (containsItem != null)
                        {
                            subQueryModel.BodyClauses.Clear();
                            subQueryModel.ResultOperators.Clear();
                            subQueryModel.ResultOperators.Add(new ContainsResultOperator(containsItem));

                            return new SubQueryExpression(subQueryModel);
                        }
                    }
                }
            }

            return expression;
        }

        private static Expression CompareReturnOpposite(Expression selector, Expression left, Expression right)
            => ExpressionEqualityComparer.Instance.Equals(selector, left)
                ? right
                : ExpressionEqualityComparer.Instance.Equals(selector, right)
                    ? left
                    : null;

        private static bool TryMatchEqualityExpression(Expression expression, out Expression left, out Expression right)
        {
            left = null;
            right = null;

            if (expression is BinaryExpression binaryExpression
                && binaryExpression.NodeType == ExpressionType.Equal)
            {
                left = binaryExpression.Left;
                right = binaryExpression.Right;

                return true;
            }

            if (expression is MethodCallExpression methodCallExpression
                && methodCallExpression.Method.Name == nameof(object.Equals))
            {
                return TryMatchEqualsMethodCallExpression(methodCallExpression, out left, out right);
            }

            return false;
        }

        private static bool TryMatchInequalityExpression(Expression expression, out Expression left, out Expression right)
        {
            left = null;
            right = null;

            if (expression is BinaryExpression binaryExpression
                && binaryExpression.NodeType == ExpressionType.NotEqual)
            {
                left = binaryExpression.Left;
                right = binaryExpression.Right;

                return true;
            }

            if (expression is UnaryExpression unaryExpression
                && unaryExpression.NodeType == ExpressionType.Not
                && unaryExpression.Operand is MethodCallExpression methodCallExpression
                && methodCallExpression.Method.Name == nameof(object.Equals))
            {
                return TryMatchEqualsMethodCallExpression(methodCallExpression, out left, out right);
            }

            return false;
        }

        private static bool TryMatchEqualsMethodCallExpression(MethodCallExpression methodCallExpression, out Expression left, out Expression right)
        {
            left = null;
            right = null;

            if (methodCallExpression.Arguments.Count == 1
                && methodCallExpression.Object?.Type == methodCallExpression.Arguments[0].Type)
            {
                left = methodCallExpression.Object;
                right = methodCallExpression.Arguments[0];

                return true;
            }

            if (methodCallExpression.Arguments.Count == 2
                && methodCallExpression.Arguments[0].Type == methodCallExpression.Arguments[1].Type)
            {
                left = methodCallExpression.Arguments[0];
                right = methodCallExpression.Arguments[1];

                return true;
            }

            return false;
        }
    }
}
