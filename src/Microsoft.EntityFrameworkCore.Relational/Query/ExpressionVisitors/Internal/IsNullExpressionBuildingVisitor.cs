// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Remotion.Linq.Parsing;

namespace Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class IsNullExpressionBuildingVisitor : RelinqExpressionVisitor
    {
        private bool _nullConstantAdded;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Expression ResultExpression { get; private set; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override Expression VisitConstant(ConstantExpression node)
        {
            if (node.Value == null
                && !_nullConstantAdded)
            {
                AddToResult(new IsNullExpression(node));
                _nullConstantAdded = true;
            }

            return node;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override Expression VisitBinary(BinaryExpression node)
        {
            // a ?? b == null <-> a == null && b == null
            if (node.NodeType == ExpressionType.Coalesce)
            {
                var current = ResultExpression;
                ResultExpression = null;
                Visit(node.Left);
                var left = ResultExpression;

                ResultExpression = null;
                Visit(node.Right);
                var right = ResultExpression;

                var coalesce = CombineExpressions(left, right, ExpressionType.AndAlso);

                ResultExpression = current;
                AddToResult(coalesce);
            }

            // a && b == null <-> a == null && b != false || a != false && b == null
            // this transformation would produce a query that is too complex
            // so we just wrap the whole expression into IsNullExpression instead.
            if ((node.NodeType == ExpressionType.AndAlso)
                || (node.NodeType == ExpressionType.OrElse))
            {
                AddToResult(new IsNullExpression(node));
            }

            return node;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override Expression VisitExtension(Expression node)
        {
            var aliasExpression = node as AliasExpression;
            if (aliasExpression != null)
            {
                return Visit(aliasExpression.Expression);
            }

            var notNullableExpression = node as NotNullableExpression;
            if (notNullableExpression != null)
            {
                return node;
            }

            var columnExpression = node as ColumnExpression
                                   ?? node.TryGetColumnExpression();

            if (columnExpression != null
                && columnExpression.IsNullable)
            {
                AddToResult(new IsNullExpression(node));

                return node;
            }

            var isNullExpression = node as IsNullExpression;
            if (isNullExpression != null)
            {
                return node;
            }

            var inExpression = node as InExpression;
            if (inExpression != null)
            {
                return node;
            }

            return node;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override Expression VisitConditional(ConditionalExpression node)
        {
            var current = ResultExpression;

            ResultExpression = null;
            Visit(node.IfTrue);
            var ifTrue = ResultExpression;

            ResultExpression = null;
            Visit(node.IfTrue);
            var ifFalse = ResultExpression;

            ResultExpression = current;

            // condition ? ifTrue : ifFalse == null <-> (condition == true && ifTrue == null) || condition != true && ifFalse == null)
            // this transformation would produce a query that is too complex
            // so we just wrap the whole expression into IsNullExpression instead.
            //
            // small optimization: expression can only be nullable if either (or both) of the possible results (ifTrue, ifFalse) can be nullable
            if ((ifTrue != null)
                || (ifFalse != null))
            {
                AddToResult(new IsNullExpression(node));
            }

            return node;
        }

        private static Expression CombineExpressions(
            Expression left, Expression right, ExpressionType expressionType)
        {
            if ((left == null)
                && (right == null))
            {
                return null;
            }

            if ((left != null)
                && (right != null))
            {
                return expressionType == ExpressionType.AndAlso
                    ? Expression.AndAlso(left, right)
                    : Expression.OrElse(left, right);
            }

            return left ?? right;
        }

        private void AddToResult(Expression expression)
            => ResultExpression = CombineExpressions(ResultExpression, expression, ExpressionType.OrElse);
    }
}
