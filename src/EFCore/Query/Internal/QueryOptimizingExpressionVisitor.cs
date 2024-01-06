// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
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
    private static readonly List<MethodInfo> SingleResultMethodInfos =
    [
        QueryableMethods.FirstWithPredicate,
        QueryableMethods.FirstWithoutPredicate,
        QueryableMethods.FirstOrDefaultWithPredicate,
        QueryableMethods.FirstOrDefaultWithoutPredicate,
        QueryableMethods.SingleWithPredicate,
        QueryableMethods.SingleWithoutPredicate,
        QueryableMethods.SingleOrDefaultWithPredicate,
        QueryableMethods.SingleOrDefaultWithoutPredicate,
        QueryableMethods.LastWithPredicate,
        QueryableMethods.LastWithoutPredicate,
        QueryableMethods.LastOrDefaultWithPredicate,
        QueryableMethods.LastOrDefaultWithoutPredicate
    ];

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
    protected override Expression VisitBinary(BinaryExpression binaryExpression)
    {
        var left = Visit(binaryExpression.Left);
        var right = Visit(binaryExpression.Right);

        if (binaryExpression.NodeType != ExpressionType.Coalesce
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

        return binaryExpression.Update(left, binaryExpression.Conversion, right);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitConditional(ConditionalExpression conditionalExpression)
    {
        var test = Visit(conditionalExpression.Test);
        var ifTrue = Visit(conditionalExpression.IfTrue);
        var ifFalse = Visit(conditionalExpression.IfFalse);

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

        return conditionalExpression.Update(test, ifTrue, ifFalse);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override ElementInit VisitElementInit(ElementInit elementInit)
    {
        var arguments = new Expression[elementInit.Arguments.Count];
        for (var i = 0; i < arguments.Length; i++)
        {
            arguments[i] = MatchExpressionType(
                Visit(elementInit.Arguments[i]),
                elementInit.Arguments[i].Type);
        }

        return elementInit.Update(arguments);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitLambda<T>(Expression<T> lambdaExpression)
    {
        var body = Visit(lambdaExpression.Body);

        return body.Type != lambdaExpression.Body.Type
            ? Expression.Lambda(Expression.Convert(body, lambdaExpression.Body.Type), lambdaExpression.Parameters)
            : lambdaExpression.Update(body, lambdaExpression.Parameters);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitMember(MemberExpression memberExpression)
    {
        var expression = memberExpression.Expression != null
            ? MatchExpressionType(
                Visit(memberExpression.Expression),
                memberExpression.Expression.Type)
            : null;

        var visitedExpression = memberExpression.Update(expression);

        return TryOptimizeMemberAccessOverConditional(visitedExpression) ?? visitedExpression;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override MemberAssignment VisitMemberAssignment(MemberAssignment memberAssignment)
    {
        var expression = MatchExpressionType(
            Visit(memberAssignment.Expression),
            memberAssignment.Expression.Type);

        return memberAssignment.Update(expression);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
    {
        // Normalize x.Any(i => i == foo) to x.Contains(foo)
        // And x.All(i => i != foo) to !x.Contains(foo)
        if (methodCallExpression.Method.IsGenericMethod
            && methodCallExpression.Method.GetGenericMethodDefinition() is MethodInfo methodInfo
            && (methodInfo == EnumerableMethods.AnyWithPredicate
                || methodInfo == EnumerableMethods.All
                || methodInfo == QueryableMethods.AnyWithPredicate
                || methodInfo == QueryableMethods.All)
            && methodCallExpression.Arguments[1].UnwrapLambdaFromQuote() is var lambda
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
                    var containsMethod = containsMethodDefinition.MakeGenericMethod(methodCallExpression.Method.GetGenericArguments()[0]);
                    return Expression.Call(null, containsMethod, methodCallExpression.Arguments[0], itemExpression);
                }

                if ((methodInfo == EnumerableMethods.All || methodInfo == QueryableMethods.All) && negated)
                {
                    var containsMethod = containsMethodDefinition.MakeGenericMethod(methodCallExpression.Method.GetGenericArguments()[0]);
                    return Expression.Not(Expression.Call(null, containsMethod, methodCallExpression.Arguments[0], itemExpression));
                }
            }
        }

        var @object = default(Expression);
        if (methodCallExpression.Object != null)
        {
            @object = MatchExpressionType(
                Visit(methodCallExpression.Object), methodCallExpression.Object.Type);
        }

        var arguments = new Expression[methodCallExpression.Arguments.Count];
        for (var i = 0; i < arguments.Length; i++)
        {
            arguments[i] = MatchExpressionType(
                Visit(methodCallExpression.Arguments[i]), methodCallExpression.Arguments[i].Type);
        }

        var visited = methodCallExpression.Update(@object!, arguments);

        // In VB.NET, comparison operators between strings (equality, greater-than, less-than) yield
        // calls to a VB-specific CompareString method. Normalize that to string.Compare.
        if (visited is
            {
                Method:
                {
                    Name: "CompareString",
                    DeclaringType: { Name: "Operators" or "EmbeddedOperators", Namespace: "Microsoft.VisualBasic.CompilerServices" }
                },
                Object: null,
                Arguments: [_, _, ConstantExpression textCompareConstantExpression]
            })
        {
            return textCompareConstantExpression.Value is true
                ? Expression.Call(
                    StringCompareWithComparisonMethod,
                    visited.Arguments[0],
                    visited.Arguments[1],
                    Expression.Constant(StringComparison.OrdinalIgnoreCase))
                : Expression.Call(
                    StringCompareWithoutComparisonMethod,
                    visited.Arguments[0],
                    visited.Arguments[1]);
        }

        return visited;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitNew(NewExpression newExpression)
    {
        if (newExpression.Arguments.Count == 0)
        {
            return newExpression;
        }

        var arguments = new Expression[newExpression.Arguments.Count];
        for (var i = 0; i < arguments.Length; i++)
        {
            arguments[i] = MatchExpressionType(
                Visit(newExpression.Arguments[i]),
                newExpression.Arguments[i].Type);
        }

        return newExpression.Update(arguments);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitNewArray(NewArrayExpression newArrayExpression)
    {
        var expressions = new Expression[newArrayExpression.Expressions.Count];
        for (var i = 0; i < expressions.Length; i++)
        {
            expressions[i] = MatchExpressionType(
                Visit(newArrayExpression.Expressions[i]),
                newArrayExpression.Expressions[i].Type);
        }

        return newArrayExpression.Update(expressions);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitUnary(UnaryExpression unaryExpression)
        => unaryExpression.Update(Visit(unaryExpression.Operand));

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
        if (expression is MemberExpression { Expression: Expression inner } visitedMemberExpression)
        {
            var (conditional, convert) = inner switch
            {
                ConditionalExpression c => (c, null),
                UnaryExpression { NodeType: ExpressionType.Convert or ExpressionType.ConvertChecked, Operand: ConditionalExpression cond } conv => (cond, conv),
                _ => (null, null)
            };

            if (conditional is { Test: BinaryExpression { NodeType: ExpressionType.Equal or ExpressionType.NotEqual } binaryTest } conditionalExpression
                && !(conditionalExpression.Type.IsNullableValueType()
                    && visitedMemberExpression.Member.Name is nameof(Nullable<int>.HasValue) or nameof(Nullable<int>.Value)))
            {
                var isLeftNullConstant = IsNullConstant(binaryTest.Left);
                var isRightNullConstant = IsNullConstant(binaryTest.Right);

                if (isLeftNullConstant != isRightNullConstant
                    && ((binaryTest.NodeType == ExpressionType.Equal
                            && IsNullConstant(conditionalExpression.IfTrue))
                        || (binaryTest.NodeType == ExpressionType.NotEqual
                            && IsNullConstant(conditionalExpression.IfFalse))))
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

    private static bool IsNullConstant(Expression expression)
        => expression is ConstantExpression { Value: null };
}
