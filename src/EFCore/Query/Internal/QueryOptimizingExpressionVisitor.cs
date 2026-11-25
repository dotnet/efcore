// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Internal;

namespace Microsoft.EntityFrameworkCore.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class QueryOptimizingExpressionVisitor : ExpressionVisitor
{
    private static readonly MethodInfo StringCompareWithComparisonMethod =
        typeof(string).GetRuntimeMethod(nameof(string.Compare), [typeof(string), typeof(string), typeof(StringComparison)])!;

    private static readonly MethodInfo StringCompareWithoutComparisonMethod =
        typeof(string).GetRuntimeMethod(nameof(string.Compare), [typeof(string), typeof(string)])!;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitBinary(BinaryExpression node)
    {
        var left = Visit(node.Left);
        var right = Visit(node.Right);

        if (node.NodeType != ExpressionType.Coalesce
            && left.Type != right.Type
            && left.Type.UnwrapNullableType() == right.Type.UnwrapNullableType())
        {
            if (left.Type.IsNullableValueType())
            {
                right = Expression.Convert(right, left.Type);
            }
            else
            {
                left = Expression.Convert(left, right.Type);
            }
        }

        return node.Update(left, node.Conversion, right);
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
        var ifTrue = Visit(node.IfTrue);
        var ifFalse = Visit(node.IfFalse);

        if (ifTrue.Type != ifFalse.Type
            && ifTrue.Type.UnwrapNullableType() == ifFalse.Type.UnwrapNullableType())
        {
            if (ifTrue.Type.IsNullableValueType())
            {
                ifFalse = Expression.Convert(ifFalse, ifTrue.Type);
            }
            else
            {
                ifTrue = Expression.Convert(ifTrue, ifFalse.Type);
            }

            return Expression.Condition(test, ifTrue, ifFalse);
        }

        return node.Update(test, ifTrue, ifFalse);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override ElementInit VisitElementInit(ElementInit node)
    {
        var arguments = new Expression[node.Arguments.Count];
        for (var i = 0; i < arguments.Length; i++)
        {
            arguments[i] = MatchExpressionType(
                Visit(node.Arguments[i]),
                node.Arguments[i].Type);
        }

        return node.Update(arguments);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitLambda<T>(Expression<T> node)
    {
        var body = Visit(node.Body);

        return body.Type != node.Body.Type
            ? Expression.Lambda(Expression.Convert(body, node.Body.Type), node.Parameters)
            : node.Update(body, node.Parameters);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitMember(MemberExpression node)
    {
        var expression = node.Expression != null
            ? MatchExpressionType(
                Visit(node.Expression),
                node.Expression.Type)
            : null;

        var visitedExpression = node.Update(expression);

        return TryOptimizeMemberAccessOverConditional(visitedExpression) ?? visitedExpression;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override MemberAssignment VisitMemberAssignment(MemberAssignment node)
    {
        var expression = MatchExpressionType(
            Visit(node.Expression),
            node.Expression.Type);

        return node.Update(expression);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitMethodCall(MethodCallExpression node)
    {
        // Normalize x.Any(i => i == foo) to x.Contains(foo)
        // And x.All(i => i != foo) to !x.Contains(foo)
        if (node.Method.IsGenericMethod
            && node.Method.GetGenericMethodDefinition() is var methodInfo
            && (methodInfo == EnumerableMethods.AnyWithPredicate
                || methodInfo == EnumerableMethods.All
                || methodInfo == QueryableMethods.AnyWithPredicate
                || methodInfo == QueryableMethods.All)
            && node.Arguments[1].UnwrapLambdaFromQuote() is var lambda
            && TryExtractEqualityOperands(lambda.Body, out var left, out var right, out var negated))
        {
            var itemExpression = left == lambda.Parameters[0]
                ? right
                : right == lambda.Parameters[0]
                    ? left
                    : null;

            if (itemExpression is not null)
            {
                var containsMethodDefinition = methodInfo.DeclaringType == typeof(Enumerable)
                    ? EnumerableMethods.Contains
                    : QueryableMethods.Contains;

                if ((methodInfo == EnumerableMethods.AnyWithPredicate || methodInfo == QueryableMethods.AnyWithPredicate) && !negated)
                {
                    var containsMethod = containsMethodDefinition.MakeGenericMethod(node.Method.GetGenericArguments()[0]);
                    return Expression.Call(null, containsMethod, node.Arguments[0], itemExpression);
                }

                if ((methodInfo == EnumerableMethods.All || methodInfo == QueryableMethods.All) && negated)
                {
                    var containsMethod = containsMethodDefinition.MakeGenericMethod(node.Method.GetGenericArguments()[0]);
                    return Expression.Not(Expression.Call(null, containsMethod, node.Arguments[0], itemExpression));
                }
            }
        }

        var @object = default(Expression);
        if (node.Object != null)
        {
            @object = MatchExpressionType(
                Visit(node.Object), node.Object.Type);
        }

        var arguments = new Expression[node.Arguments.Count];
        for (var i = 0; i < arguments.Length; i++)
        {
            arguments[i] = MatchExpressionType(
                Visit(node.Arguments[i]), node.Arguments[i].Type);
        }

        var visited = node.Update(@object!, arguments);

        // In VB.NET, comparison operators between strings (equality, greater-than, less-than) yield
        // calls to a VB-specific CompareString method. Normalize that to string.Compare.
        return visited is
        {
            Method:
            {
                Name: "CompareString",
                DeclaringType: { Name: "Operators" or "EmbeddedOperators", Namespace: "Microsoft.VisualBasic.CompilerServices" }
            },
            Object: null,
            Arguments: [_, _, ConstantExpression textCompareConstantExpression]
        }
            ? textCompareConstantExpression.Value is true
                ? Expression.Call(
                    StringCompareWithComparisonMethod,
                    visited.Arguments[0],
                    visited.Arguments[1],
                    Expression.Constant(StringComparison.OrdinalIgnoreCase))
                : Expression.Call(
                    StringCompareWithoutComparisonMethod,
                    visited.Arguments[0],
                    visited.Arguments[1])
            : visited;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitNew(NewExpression node)
    {
        if (node.Arguments.Count == 0)
        {
            return node;
        }

        var arguments = new Expression[node.Arguments.Count];
        for (var i = 0; i < arguments.Length; i++)
        {
            arguments[i] = MatchExpressionType(
                Visit(node.Arguments[i]),
                node.Arguments[i].Type);
        }

        return node.Update(arguments);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitNewArray(NewArrayExpression node)
    {
        var expressions = new Expression[node.Expressions.Count];
        for (var i = 0; i < expressions.Length; i++)
        {
            expressions[i] = MatchExpressionType(
                Visit(node.Expressions[i]),
                node.Expressions[i].Type);
        }

        return node.Update(expressions);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitUnary(UnaryExpression node)
        => node.Update(Visit(node.Operand));

    private static Expression MatchExpressionType(Expression expression, Type typeToMatch)
        => expression.Type != typeToMatch
            ? Expression.Convert(expression, typeToMatch)
            : expression;

    private static bool TryExtractEqualityOperands(
        Expression expression,
        [NotNullWhen(true)] out Expression? left,
        [NotNullWhen(true)] out Expression? right,
        out bool negated)
    {
        (left, right, negated) = (default, default, default);

        switch (expression)
        {
            case BinaryExpression binaryExpression:
                switch (binaryExpression.NodeType)
                {
                    case ExpressionType.Equal:
                        negated = false;
                        break;
                    case ExpressionType.NotEqual:
                        negated = true;
                        break;
                    default:
                        return false;
                }

                (left, right) = (binaryExpression.Left, binaryExpression.Right);
                return true;

            case MethodCallExpression { Method.Name: nameof(object.Equals) } methodCallExpression:
            {
                negated = false;
                if (methodCallExpression.Arguments.Count == 1
                    && methodCallExpression.Object?.Type == methodCallExpression.Arguments[0].Type)
                {
                    (left, right) = (methodCallExpression.Object, methodCallExpression.Arguments[0]);

                    return true;
                }

                if (methodCallExpression.Arguments.Count == 2
                    && methodCallExpression.Arguments[0].Type == methodCallExpression.Arguments[1].Type)
                {
                    (left, right) = (methodCallExpression.Arguments[0], methodCallExpression.Arguments[1]);

                    return true;
                }

                return false;
            }

            case UnaryExpression unaryExpression
                when unaryExpression.IsLogicalNot():
            {
                var result = TryExtractEqualityOperands(unaryExpression.Operand, out left, out right, out negated);
                negated = !negated;
                return result;
            }
        }

        return false;
    }

    private static Expression? TryOptimizeMemberAccessOverConditional(Expression expression)
    {
        // Simplify (a != null ? new { Member = b, ... } : null).Member
        // to a != null ? b : null
        // Later null check removal will simplify it further
        if (expression is MemberExpression { Expression: { } inner } visitedMemberExpression)
        {
            var (conditional, convert) = inner switch
            {
                ConditionalExpression c => (c, null),
                UnaryExpression
                {
                    NodeType: ExpressionType.Convert or ExpressionType.ConvertChecked, Operand: ConditionalExpression cond
                } conv => (cond, conv),
                _ => (null, null)
            };

            if (conditional is
                {
                    Test: BinaryExpression { NodeType: ExpressionType.Equal or ExpressionType.NotEqual } binaryTest
                } conditionalExpression
                && !(conditionalExpression.Type.IsNullableValueType()
                    && visitedMemberExpression.Member.Name is nameof(Nullable<>.HasValue) or nameof(Nullable<>.Value)))
            {
                var isLeftNullConstant = binaryTest.Left is ConstantExpression { Value: null };
                var isRightNullConstant = binaryTest.Right is ConstantExpression { Value: null };

                if (isLeftNullConstant != isRightNullConstant
                    && ((binaryTest.NodeType == ExpressionType.Equal
                            && conditionalExpression.IfTrue is ConstantExpression { Value: null })
                        || (binaryTest.NodeType == ExpressionType.NotEqual
                            && conditionalExpression.IfFalse is ConstantExpression { Value: null })))
                {
                    var nonNullExpression = binaryTest.NodeType == ExpressionType.Equal
                        ? conditionalExpression.IfFalse
                        : conditionalExpression.IfTrue;

                    // if we removed convert around ConditionalExpression
                    // we need to re-apply it before we apply the MemberExpression
                    if (convert is not null)
                    {
                        nonNullExpression = convert.Update(nonNullExpression);
                    }

                    // Use ReplacingExpressionVisitor rather than creating MemberExpression
                    // So that member access chain on NewExpression/MemberInitExpression condenses
                    nonNullExpression = ReplacingExpressionVisitor.Replace(
                        visitedMemberExpression.Expression, nonNullExpression, visitedMemberExpression);

                    nonNullExpression = TryOptimizeMemberAccessOverConditional(nonNullExpression) ?? nonNullExpression;
                    if (!nonNullExpression.Type.IsNullableType())
                    {
                        nonNullExpression = Expression.Convert(nonNullExpression, nonNullExpression.Type.MakeNullable());
                    }

                    var nullExpression = Expression.Constant(null, nonNullExpression.Type);

                    return Expression.Condition(
                        conditionalExpression.Test,
                        binaryTest.NodeType == ExpressionType.Equal ? nullExpression : nonNullExpression,
                        binaryTest.NodeType == ExpressionType.Equal ? nonNullExpression : nullExpression);
                }
            }
        }

        return null;
    }
}
