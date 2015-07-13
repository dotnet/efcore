// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using Microsoft.Data.Entity.Query.Expressions;
using Remotion.Linq.Parsing;

namespace Microsoft.Data.Entity.Query.ExpressionVisitors
{
    public class IsNullExpressionBuildingVisitor : RelinqExpressionVisitor
    {
        public virtual Expression ResultExpression { get; private set; }

        protected override Expression VisitConstant(ConstantExpression expression)
        {
            if (expression.Value == null)
            {
                AddToResult(expression);
            }

            return expression;
        }

        protected override Expression VisitBinary(BinaryExpression expression)
        {
            // a ?? b == null <-> a == null && b == null
            if (expression.NodeType == ExpressionType.Coalesce)
            {
                var current = ResultExpression;
                ResultExpression = null;
                Visit(expression.Left);
                var left = ResultExpression;

                ResultExpression = null;
                Visit(expression.Right);
                var right = ResultExpression;

                var coalesce = CombineExpressions(left, right, ExpressionType.AndAlso);

                ResultExpression = current;
                AddToResult(coalesce);
            }

            // a && b == null <-> a == null && b != false || a != false && b == null
            // this transformation would produce a query that is too complex
            // so we just wrap the whole expression into IsNullExpression instead.
            if (expression.NodeType == ExpressionType.AndAlso
                || expression.NodeType == ExpressionType.OrElse)
            {
                AddToResult(new IsNullExpression(expression));
            }

            return expression;
        }

        protected override Expression VisitExtension(Expression expression)
        {
            var aliasExpression = expression as AliasExpression;
            if (aliasExpression != null)
            {
                return Visit(aliasExpression.Expression);
            }

            var notNullableExpression = expression as NotNullableExpression;
            if (notNullableExpression != null)
            {
                return expression;
            }

            var columnExpression = expression as ColumnExpression
                                   ?? expression.TryGetColumnExpression();

            if (columnExpression != null
                && columnExpression.Property.IsNullable)
            {
                AddToResult(new IsNullExpression(expression));

                return expression;
            }

            var isNullExpression = expression as IsNullExpression;
            if (isNullExpression != null)
            {
                return expression;
            }

            var inExpression = expression as InExpression;
            if (inExpression != null)
            {
                return expression;
            }

            return expression;
        }

        protected override Expression VisitConditional(ConditionalExpression expression)
        {
            var current = ResultExpression;

            ResultExpression = null;
            Visit(expression.IfTrue);
            var ifTrue = ResultExpression;

            ResultExpression = null;
            Visit(expression.IfTrue);
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
                AddToResult(new IsNullExpression(expression));
            }

            return expression;
        }

        private Expression CombineExpressions(Expression left, Expression right, ExpressionType expressionType)
        {
            if (left == null
                && right == null)
            {
                return null;
            }

            if (left != null
                && right != null)
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
