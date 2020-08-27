// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class NullCheckRemovingExpressionVisitor : ExpressionVisitor
    {
        private readonly NullSafeAccessVerifyingExpressionVisitor _nullSafeAccessVerifyingExpressionVisitor
            = new NullSafeAccessVerifyingExpressionVisitor();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override Expression VisitBinary(BinaryExpression binaryExpression)
        {
            var visitedExpression = base.VisitBinary(binaryExpression);

            return TryOptimizeConditionalEquality(visitedExpression) ?? visitedExpression;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override Expression VisitConditional(ConditionalExpression conditionalExpression)
        {
            Check.NotNull(conditionalExpression, nameof(conditionalExpression));

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

        private Expression TryOptimizeConditionalEquality(Expression expression)
        {
            // Simplify (a ? b : null) == null => !a || b == null
            // Simplify (a ? null : b) == null => a || b == null
            if (expression is BinaryExpression binaryExpression
                && binaryExpression.NodeType == ExpressionType.Equal
                && (binaryExpression.Left is ConditionalExpression
                    || binaryExpression.Right is ConditionalExpression))
            {
                Expression comparedExpression;
                if (binaryExpression.Left is ConditionalExpression conditionalExpression)
                {
                    comparedExpression = binaryExpression.Right;
                }
                else
                {
                    conditionalExpression = binaryExpression.Right as ConditionalExpression;
                    comparedExpression = binaryExpression.Left;
                }

                if (conditionalExpression.IfFalse.IsNullConstantExpression()
                    && comparedExpression.IsNullConstantExpression())
                {
                    return Expression.OrElse(
                        Expression.Not(conditionalExpression.Test),
                        Expression.Equal(conditionalExpression.IfTrue, comparedExpression));
                }

                if (conditionalExpression.IfTrue.IsNullConstantExpression()
                    && comparedExpression.IsNullConstantExpression())
                {
                    return Expression.OrElse(
                        conditionalExpression.Test,
                        Expression.Equal(conditionalExpression.IfFalse, comparedExpression));
                }
            }

            return null;
        }

        private sealed class NullSafeAccessVerifyingExpressionVisitor : ExpressionVisitor
        {
            private readonly ISet<Expression> _nullSafeAccesses = new HashSet<Expression>(ExpressionEqualityComparer.Instance);

            public bool Verify(Expression caller, Expression result)
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
                Check.NotNull(memberExpression, nameof(memberExpression));

                var innerExpression = Visit(memberExpression.Expression);
                if (_nullSafeAccesses.Contains(innerExpression))
                {
                    _nullSafeAccesses.Add(memberExpression);
                }

                return memberExpression;
            }

            protected override Expression VisitUnary(UnaryExpression unaryExpression)
            {
                Check.NotNull(unaryExpression, nameof(unaryExpression));

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
