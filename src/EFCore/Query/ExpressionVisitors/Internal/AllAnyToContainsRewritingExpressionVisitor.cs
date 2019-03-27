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
    public class AllAnyToContainsRewritingExpressionVisitor : ExpressionVisitorBase
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

            if ((fromExpression.NodeType == ExpressionType.Parameter
                 || fromExpression.NodeType == ExpressionType.Constant
                 || fromExpression.NodeType == ExpressionType.ListInit
                 || fromExpression.NodeType == ExpressionType.NewArrayInit)
                && subQueryModel.ResultOperators.Count == 1)
            {
                if (subQueryModel.BodyClauses.Count == 0
                    && subQueryModel.ResultOperators[0] is AllResultOperator all)
                {
                    var containsItem = TryExtractContainsItem(
                        all.Predicate,
                        subQueryModel.SelectClause.Selector,
                        ExpressionType.NotEqual);

                    if (containsItem != null)
                    {
                        subQueryModel.ResultOperators.Clear();
                        subQueryModel.ResultOperators.Add(new ContainsResultOperator(containsItem));

                        return Expression.Not(new SubQueryExpression(subQueryModel));
                    }
                }

                if (subQueryModel.BodyClauses.Count == 1
                    && subQueryModel.ResultOperators[0] is AnyResultOperator
                    && subQueryModel.BodyClauses[0] is WhereClause whereClause)
                {
                    var containsItem = TryExtractContainsItem(
                        whereClause.Predicate,
                        subQueryModel.SelectClause.Selector,
                        ExpressionType.Equal);

                    if (containsItem != null)
                    {
                        subQueryModel.BodyClauses.Clear();
                        subQueryModel.ResultOperators.Clear();
                        subQueryModel.ResultOperators.Add(new ContainsResultOperator(containsItem));

                        return new SubQueryExpression(subQueryModel);
                    }
                }
            }

            return expression;
        }

        private static Expression TryExtractContainsItem(
            Expression predicate,
            Expression selector,
            ExpressionType nodeType)
        {
            Expression left = null;
            Expression right = null;
            if (predicate is BinaryExpression binaryExpression
                && binaryExpression.NodeType == nodeType)
            {
                left = binaryExpression.Left;
                right = binaryExpression.Right;
            }
            else
            {
                if (nodeType == ExpressionType.NotEqual
                    && predicate is UnaryExpression unaryExpression
                    && unaryExpression.NodeType == ExpressionType.Not)
                {
                    predicate = unaryExpression.Operand;
                }

                if (predicate is MethodCallExpression methodCallExpression
                    && methodCallExpression.Method.Name == nameof(object.Equals))
                {
                    if (methodCallExpression.Arguments.Count == 1
                        && methodCallExpression.Object?.Type == methodCallExpression.Arguments[0].Type)
                    {
                        left = methodCallExpression.Object;
                        right = methodCallExpression.Arguments[0];
                    }
                    else if (methodCallExpression.Arguments.Count == 2
                             && methodCallExpression.Arguments[0].Type == methodCallExpression.Arguments[1].Type)
                    {
                        left = methodCallExpression.Arguments[0];
                        right = methodCallExpression.Arguments[1];
                    }
                }
            }

            return left != null
                ? ExpressionEqualityComparer.Instance.Equals(left, selector)
                    ? right
                    : ExpressionEqualityComparer.Instance.Equals(right, selector)
                        ? left
                        : null
                : null;
        }
    }
}
