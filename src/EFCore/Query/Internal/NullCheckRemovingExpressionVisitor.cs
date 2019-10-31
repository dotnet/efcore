// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq.Expressions;

namespace Microsoft.EntityFrameworkCore.Query.Internal
{
    public class NullCheckRemovingExpressionVisitor : ExpressionVisitor
    {
        private readonly NullSafeAccessVerifyingExpressionVisitor _nullSafeAccessVerifyingExpressionVisitor
            = new NullSafeAccessVerifyingExpressionVisitor();

        protected override Expression VisitConditional(ConditionalExpression conditionalExpression)
        {
            var test = Visit(conditionalExpression.Test);

            if (test is BinaryExpression binaryTest
                && (binaryTest.NodeType == ExpressionType.Equal
                    || binaryTest.NodeType == ExpressionType.NotEqual))
            {
                var isLeftNullConstant = IsNullConstant(binaryTest.Left);
                var isRightNullConstant = IsNullConstant(binaryTest.Right);

                if ((isLeftNullConstant == isRightNullConstant)
                    || (binaryTest.NodeType == ExpressionType.Equal
                        && !IsNullConstant(conditionalExpression.IfTrue))
                    || (binaryTest.NodeType == ExpressionType.NotEqual
                        && !IsNullConstant(conditionalExpression.IfFalse)))
                {
                    return conditionalExpression;
                }

                var caller = isLeftNullConstant ? binaryTest.Right : binaryTest.Left;
                var accessOperation = binaryTest.NodeType == ExpressionType.Equal
                    ? conditionalExpression.IfFalse
                    : conditionalExpression.IfTrue;

                if (_nullSafeAccessVerifyingExpressionVisitor.Verify(caller, accessOperation))
                {
                    return accessOperation;
                }
            }

            return base.VisitConditional(conditionalExpression);
        }

        private class NullSafeAccessVerifyingExpressionVisitor : ExpressionVisitor
        {
            private readonly ISet<Expression> _nullSafeAccesses = new HashSet<Expression>(ExpressionEqualityComparer.Instance);

            public virtual bool Verify(Expression caller, Expression result)
            {
                _nullSafeAccesses.Clear();
                _nullSafeAccesses.Add(caller);
                Visit(result);

                return _nullSafeAccesses.Contains(result);
            }

            public override Expression Visit(Expression expression)
                => expression == null || _nullSafeAccesses.Contains(expression)
                    ? expression
                    : base.Visit(expression);

            protected override Expression VisitMember(MemberExpression memberExpression)
            {
                var innerExpression = Visit(memberExpression.Expression);
                if (_nullSafeAccesses.Contains(innerExpression))
                {
                    _nullSafeAccesses.Add(memberExpression);
                }

                return memberExpression;
            }

            protected override Expression VisitUnary(UnaryExpression unaryExpression)
            {
                var operand = Visit(unaryExpression.Operand);
                if ((unaryExpression.NodeType == ExpressionType.Convert
                        || unaryExpression.NodeType == ExpressionType.ConvertChecked)
                    && _nullSafeAccesses.Contains(operand))
                {
                    _nullSafeAccesses.Add(unaryExpression);
                }

                return unaryExpression;
            }
        }

        private bool IsNullConstant(Expression expression)
            => expression is ConstantExpression constantExpression
                && constantExpression.Value == null;
    }
}
