// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
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
        protected override Expression VisitConstant(ConstantExpression constantExpression)
        {
            if (constantExpression.Value == null
                && !_nullConstantAdded)
            {
                AddToResult(new IsNullExpression(constantExpression));
                _nullConstantAdded = true;
            }

            return constantExpression;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override Expression VisitBinary(BinaryExpression binaryExpression)
        {
            // a ?? b == null <-> a == null && b == null
            if (binaryExpression.NodeType == ExpressionType.Coalesce)
            {
                var current = ResultExpression;
                ResultExpression = null;
                Visit(binaryExpression.Left);
                var left = ResultExpression;

                ResultExpression = null;
                Visit(binaryExpression.Right);
                var right = ResultExpression;

                var coalesce = CombineExpressions(left, right, ExpressionType.AndAlso);

                ResultExpression = current;
                AddToResult(coalesce);
            }

            // a && b == null <-> a == null && b != false || a != false && b == null
            // this transformation would produce a query that is too complex
            // so we just wrap the whole expression into IsNullExpression instead.
            if (binaryExpression.NodeType == ExpressionType.AndAlso
                || binaryExpression.NodeType == ExpressionType.OrElse)
            {
                AddToResult(new IsNullExpression(binaryExpression));
            }

            return binaryExpression;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override Expression VisitExtension(Expression extensionExpression)
        {
            if (extensionExpression is AliasExpression aliasExpression)
            {
                return Visit(aliasExpression.Expression);
            }

            if (extensionExpression is ColumnExpression columnExpression
                && columnExpression.Property.IsNullable)
            {
                AddToResult(new IsNullExpression(extensionExpression));

                return extensionExpression;
            }

            if (extensionExpression is NullableExpression nullableExpression)
            {
                AddToResult(new IsNullExpression(nullableExpression.Operand));

                return extensionExpression;
            }

            return extensionExpression;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override Expression VisitConditional(ConditionalExpression conditionalExpression)
        {
            var current = ResultExpression;

            ResultExpression = null;
            Visit(conditionalExpression.IfTrue);
            var ifTrue = ResultExpression;

            ResultExpression = null;
            Visit(conditionalExpression.IfTrue);
            var ifFalse = ResultExpression;

            ResultExpression = current;

            // condition ? ifTrue : ifFalse == null <-> (condition == true && ifTrue == null) || condition != true && ifFalse == null)
            // this transformation would produce a query that is too complex
            // so we just wrap the whole expression into IsNullExpression instead.
            //
            // small optimization: expression can only be nullable if either (or both) of the possible results (ifTrue, ifFalse) can be nullable
            if (ifTrue != null
                || ifFalse != null)
            {
                AddToResult(new IsNullExpression(conditionalExpression));
            }

            return conditionalExpression;
        }

        private static Expression CombineExpressions(
            Expression left, Expression right, ExpressionType expressionType)
        {
            if (left == null
                && right == null)
            {
                return null;
            }

            if (left != null
                && right != null)
            {
                return Expression.MakeBinary(expressionType, left, right);
            }

            return left ?? right;
        }

        private void AddToResult(Expression expression)
            => ResultExpression = CombineExpressions(ResultExpression, expression, ExpressionType.OrElse);
    }
}
