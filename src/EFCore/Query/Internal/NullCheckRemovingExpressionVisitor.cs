// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class NullCheckRemovingExpressionVisitor : ExpressionVisitor
{
    private readonly NullSafeAccessVerifyingExpressionVisitor _nullSafeAccessVerifyingExpressionVisitor
        = new();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitBinary(BinaryExpression node)
    {
        var visitedExpression = base.VisitBinary(node);

        return TryOptimizeConditionalEquality(visitedExpression)
            ?? ProcessNullCheck(visitedExpression)
            ?? visitedExpression;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitConditional(ConditionalExpression node)
    {
        var test = Visit(node.Test);

        if (test is BinaryExpression { NodeType: ExpressionType.Equal or ExpressionType.NotEqual } binaryTest)
        {
            var isLeftNullConstant = binaryTest.Left is ConstantExpression { Value: null };
            var isRightNullConstant = binaryTest.Right is ConstantExpression { Value: null };

            if ((isLeftNullConstant == isRightNullConstant)
                || (binaryTest.NodeType == ExpressionType.Equal
                    && node.IfTrue is not ConstantExpression { Value: null })
                || (binaryTest.NodeType == ExpressionType.NotEqual
                    && node.IfFalse is not ConstantExpression { Value: null }))
            {
                return node;
            }

            var caller = isLeftNullConstant ? binaryTest.Right : binaryTest.Left;
            var accessOperation = binaryTest.NodeType == ExpressionType.Equal
                ? node.IfFalse
                : node.IfTrue;

            if (accessOperation is UnaryExpression { NodeType: ExpressionType.Convert or ExpressionType.ConvertChecked } outerUnary
                && accessOperation.Type.IsNullableType()
                && accessOperation.Type.UnwrapNullableType() == outerUnary.Operand.Type
                && outerUnary.Operand is UnaryExpression { NodeType: ExpressionType.Convert or ExpressionType.ConvertChecked } innerUnary)
            {
                // If expression is of type Convert(Convert(a, type), type?)
                // then we convert it to Convert(a, type?) since a can be nullable after removing check
                accessOperation = outerUnary.Update(innerUnary.Operand);
            }

            if (_nullSafeAccessVerifyingExpressionVisitor.Verify(caller, accessOperation))
            {
                return accessOperation;
            }
        }

        return base.VisitConditional(node);
    }

    private static BinaryExpression? TryOptimizeConditionalEquality(Expression expression)
    {
        // Simplify (a ? b : null) == null => !a || b == null
        // Simplify (a ? null : b) == null => a || b == null
        // Expression.Equal is fine here since we match the binary expression of same kind.
        if (expression is BinaryExpression { NodeType: ExpressionType.Equal } binaryExpression
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
                conditionalExpression = (ConditionalExpression)binaryExpression.Right;
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

    private static ConstantExpression? ProcessNullCheck(Expression expression)
    {
        // Optimize IQueryable/DbSet null checks for expressions that are guaranteed to be non-null:
        // * queryableMethodCall != null => true
        // * queryableMethodCall == null => false
        // This applies to method calls and member accesses that produce IQueryable results, which can never be null.
        // We do NOT optimize null checks for parameters/variables, as they could legitimately be null.
        if (expression is BinaryExpression
            {
                NodeType: ExpressionType.Equal or ExpressionType.NotEqual
            } binaryExpression)
        {
            var isLeftNull = binaryExpression.Left is ConstantExpression { Value: null };
            var isRightNull = binaryExpression.Right is ConstantExpression { Value: null };

            if (isLeftNull || isRightNull)
            {
                // null == null => true
                // null != null => false
                if (isLeftNull && isRightNull)
                {
                    return Expression.Constant(binaryExpression.NodeType is ExpressionType.Equal);
                }

                var nonNullExpression = isLeftNull ? binaryExpression.Right : binaryExpression.Left;

                // Only optimize if the expression is a query operation that cannot be null
                // (method call returning IQueryable, DbSet property access, or QueryRootExpression)
                if (nonNullExpression.Type.IsAssignableTo(typeof(IQueryable))
                    && (nonNullExpression is MethodCallExpression or QueryRootExpression))
                {
                    return Expression.Constant(binaryExpression.NodeType is ExpressionType.NotEqual);
                }
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

        [return: NotNullIfNotNull(nameof(node))]
        public override Expression? Visit(Expression? node)
            => node == null || _nullSafeAccesses.Contains(node)
                ? node
                : base.Visit(node);

        protected override Expression VisitMember(MemberExpression node)
        {
            var innerExpression = Visit(node.Expression);
            if (innerExpression != null
                && _nullSafeAccesses.Contains(innerExpression))
            {
                _nullSafeAccesses.Add(node);
            }

            return node;
        }

        protected override Expression VisitUnary(UnaryExpression node)
        {
            var operand = Visit(node.Operand);
            if (node.NodeType is ExpressionType.Convert or ExpressionType.ConvertChecked
                && _nullSafeAccesses.Contains(operand))
            {
                _nullSafeAccesses.Add(node);
            }

            return node;
        }
    }
}
