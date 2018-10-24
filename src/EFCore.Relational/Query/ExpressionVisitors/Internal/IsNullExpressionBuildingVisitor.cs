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
            // a && b == null <-> a == null && b != false || a != false && b == null
            // this transformation would produce a query that is too complex
            // so we just wrap the whole expression into IsNullExpression instead.
            if (binaryExpression.NodeType == ExpressionType.AndAlso
                || binaryExpression.NodeType == ExpressionType.OrElse)
            {
                AddToResult(new IsNullExpression(binaryExpression));
            }
            else
            {
                // a ?? b == null <-> a == null && b == null
                // for other binary operators f(a, b) == null <=> a == null || b == null
                var joinOperator = binaryExpression.NodeType == ExpressionType.Coalesce
                    ? ExpressionType.AndAlso
                    : ExpressionType.OrElse;

                var current = ResultExpression;
                ResultExpression = null;
                Visit(binaryExpression.Left);
                var left = ResultExpression;

                ResultExpression = null;
                Visit(binaryExpression.Right);
                var right = ResultExpression;

                var result = CombineExpressions(left, right, joinOperator);

                ResultExpression = current;
                AddToResult(result);
            }

            return binaryExpression;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override Expression VisitExtension(Expression extensionExpression)
        {
            switch (extensionExpression)
            {
                case NullableExpression nullableExpression:
                    AddToResult(new IsNullExpression(nullableExpression.Operand));
                    return extensionExpression;

                case ExplicitCastExpression explicitCastExpression:
                    return Visit(explicitCastExpression.Operand);

                case SqlFunctionExpression sqlFunctionExpression:
                    var current = ResultExpression;
                    Expression result = null;
                    foreach (var argument in sqlFunctionExpression.Arguments)
                    {
                        ResultExpression = null;
                        Visit(argument);
                        if (result == null)
                        {
                            result = ResultExpression;
                        }
                        else
                        {
                            result = CombineExpressions(result, ResultExpression, ExpressionType.OrElse);
                        }
                    }

                    ResultExpression = current;
                    AddToResult(result);

                    return extensionExpression;
            }

            if (ContainsNullableColumnExpression(extensionExpression))
            {
                AddToResult(new IsNullExpression(extensionExpression));

                return extensionExpression;
            }

            return extensionExpression;
        }

        private bool ContainsNullableColumnExpression(Expression extensionExpression)
        {
            if (extensionExpression is ColumnExpression columnExpression)
            {
                return columnExpression.Property.IsNullable;
            }

            if (extensionExpression is ColumnReferenceExpression columnReferenceExpression)
            {
                return ContainsNullableColumnExpression(columnReferenceExpression.Expression);
            }

            return extensionExpression is AliasExpression aliasExpression ? ContainsNullableColumnExpression(aliasExpression.Expression) : false;
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

            return left != null
                && right != null
                ? Expression.MakeBinary(expressionType, left, right)
                : left ?? right;
        }

        private void AddToResult(Expression expression)
            => ResultExpression = CombineExpressions(ResultExpression, expression, ExpressionType.OrElse);
    }
}
