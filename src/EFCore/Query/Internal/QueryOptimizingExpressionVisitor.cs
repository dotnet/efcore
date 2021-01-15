// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Utilities;
using CA = System.Diagnostics.CodeAnalysis;

#nullable enable

namespace Microsoft.EntityFrameworkCore.Query.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class QueryOptimizingExpressionVisitor : ExpressionVisitor
    {
        private static readonly MethodInfo _stringCompareWithComparisonMethod =
            typeof(string).GetRequiredRuntimeMethod(nameof(string.Compare), new[] { typeof(string), typeof(string), typeof(StringComparison) });

        private static readonly MethodInfo _stringCompareWithoutComparisonMethod =
            typeof(string).GetRequiredRuntimeMethod(nameof(string.Compare), new[] { typeof(string), typeof(string) });

        private static readonly MethodInfo _startsWithMethodInfo =
            typeof(string).GetRequiredRuntimeMethod(nameof(string.StartsWith), new[] { typeof(string) });

        private static readonly MethodInfo _endsWithMethodInfo =
            typeof(string).GetRequiredRuntimeMethod(nameof(string.EndsWith), new[] { typeof(string) });

        private static readonly Expression _constantNullString = Expression.Constant(null, typeof(string));

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
        protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
        {
            Check.NotNull(methodCallExpression, nameof(methodCallExpression));

            if (Equals(_startsWithMethodInfo, methodCallExpression.Method)
                || Equals(_endsWithMethodInfo, methodCallExpression.Method))
            {
                if (methodCallExpression.Arguments[0] is ConstantExpression constantArgument
                    && constantArgument.Value is string stringValue
                    && stringValue == string.Empty)
                {
                    // every string starts/ends with empty string.
                    return Expression.Constant(true);
                }

                var newObject = Visit(methodCallExpression.Object)!;
                var newArgument = Visit(methodCallExpression.Arguments[0]);

                var result = Expression.AndAlso(
                    Expression.NotEqual(newObject, _constantNullString),
                    Expression.AndAlso(
                        Expression.NotEqual(newArgument, _constantNullString),
                        methodCallExpression.Update(newObject, new[] { newArgument })));

                return newArgument is ConstantExpression
                    ? result
                    : Expression.OrElse(
                        Expression.Equal(
                            newArgument,
                            Expression.Constant(string.Empty)),
                        result);
            }

            if (methodCallExpression.Method.IsGenericMethod
                && methodCallExpression.Method.GetGenericMethodDefinition() is MethodInfo methodInfo
                && (methodInfo.Equals(EnumerableMethods.AnyWithPredicate) || methodInfo.Equals(EnumerableMethods.All))
                && methodCallExpression.Arguments[0].NodeType is ExpressionType nodeType
                && (nodeType == ExpressionType.Parameter || nodeType == ExpressionType.Constant)
                && methodCallExpression.Arguments[1] is LambdaExpression lambda
                && TryExtractEqualityOperands(lambda.Body, out var left, out var right, out var negated)
                && (left is ParameterExpression || right is ParameterExpression))
            {
                var nonParameterExpression = left is ParameterExpression ? right : left;

                if (methodInfo.Equals(EnumerableMethods.AnyWithPredicate)
                    && !negated)
                {
                    var containsMethod = EnumerableMethods.Contains.MakeGenericMethod(methodCallExpression.Method.GetGenericArguments()[0]);
                    return Expression.Call(null, containsMethod, methodCallExpression.Arguments[0], nonParameterExpression);
                }

                if (methodInfo.Equals(EnumerableMethods.All) && negated)
                {
                    var containsMethod = EnumerableMethods.Contains.MakeGenericMethod(methodCallExpression.Method.GetGenericArguments()[0]);
                    return Expression.Not(Expression.Call(null, containsMethod, methodCallExpression.Arguments[0], nonParameterExpression));
                }
            }

            if (methodCallExpression.Method.IsGenericMethod
                && methodCallExpression.Method.GetGenericMethodDefinition() is MethodInfo containsMethodInfo
                && containsMethodInfo.Equals(QueryableMethods.Contains))
            {
                var typeArgument = methodCallExpression.Method.GetGenericArguments()[0];
                var anyMethod = QueryableMethods.AnyWithPredicate.MakeGenericMethod(typeArgument);

                var anyLambdaParameter = Expression.Parameter(typeArgument, "p");
                var anyLambda = Expression.Lambda(
                    Expression.Equal(
                        anyLambdaParameter,
                        methodCallExpression.Arguments[1]),
                    anyLambdaParameter);

                return Expression.Call(null, anyMethod, new[] { methodCallExpression.Arguments[0], anyLambda });
            }

            var @object = default(Expression);
            if (methodCallExpression.Object != null)
            {
                @object = MatchExpressionType(
                    Visit(methodCallExpression.Object),
                    methodCallExpression.Object.Type);
            }

            var arguments = new Expression[methodCallExpression.Arguments.Count];
            for (var i = 0; i < arguments.Length; i++)
            {
                arguments[i] = MatchExpressionType(
                    Visit(methodCallExpression.Arguments[i]),
                    methodCallExpression.Arguments[i].Type);
            }

            var visited = methodCallExpression.Update(@object!, arguments);

            // In VB.NET, comparison operators between strings (equality, greater-than, less-than) yield
            // calls to a VB-specific CompareString method. Normalize that to string.Compare.
            if (visited.Method.Name == "CompareString"
                && visited.Method.DeclaringType?.Name == "Operators"
                && visited.Method.DeclaringType?.Namespace == "Microsoft.VisualBasic.CompilerServices"
                && visited.Object == null
                && visited.Arguments.Count == 3
                && visited.Arguments[2] is ConstantExpression textCompareConstantExpression
                && _stringCompareWithComparisonMethod != null
                && _stringCompareWithoutComparisonMethod != null)
            {
                return textCompareConstantExpression.Value is bool boolValue
                    && boolValue
                    ? Expression.Call(
                        _stringCompareWithComparisonMethod,
                        visited.Arguments[0],
                        visited.Arguments[1],
                        Expression.Constant(StringComparison.OrdinalIgnoreCase))
                    : Expression.Call(
                        _stringCompareWithoutComparisonMethod,
                        visited.Arguments[0],
                        visited.Arguments[1]);
            }

            return visited;
        }

        private Expression MatchExpressionType(Expression expression, Type typeToMatch)
            => expression.Type != typeToMatch
                ? Expression.Convert(expression, typeToMatch)
                : expression;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override Expression VisitUnary(UnaryExpression unaryExpression)
        {
            Check.NotNull(unaryExpression, nameof(unaryExpression));

            if (unaryExpression.NodeType == ExpressionType.Not
                && unaryExpression.Operand is MethodCallExpression innerMethodCall
                && (Equals(_startsWithMethodInfo, innerMethodCall.Method)
                    || Equals(_endsWithMethodInfo, innerMethodCall.Method)))
            {
                if (innerMethodCall.Arguments[0] is ConstantExpression constantArgument
                    && constantArgument.Value is string stringValue
                    && stringValue == string.Empty)
                {
                    // every string starts/ends with empty string.
                    return Expression.Constant(false);
                }

                var newObject = Visit(innerMethodCall.Object)!;
                var newArgument = Visit(innerMethodCall.Arguments[0]);

                var result = Expression.AndAlso(
                    Expression.NotEqual(newObject, _constantNullString),
                    Expression.AndAlso(
                        Expression.NotEqual(newArgument, _constantNullString),
                        Expression.Not(innerMethodCall.Update(newObject, new[] { newArgument }))));

                return newArgument is ConstantExpression
                    ? result
                    : Expression.AndAlso(
                        Expression.NotEqual(
                            newArgument,
                            Expression.Constant(string.Empty)),
                        result);
            }

            return unaryExpression.Update(
                Visit(unaryExpression.Operand));
        }

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
        protected override Expression VisitLambda<T>(Expression<T> lambdaExpression)
        {
            Check.NotNull(lambdaExpression, nameof(lambdaExpression));

            var body = Visit(lambdaExpression.Body);

            return body.Type != lambdaExpression.Body.Type
                ? Expression.Lambda(Expression.Convert(body, lambdaExpression.Body.Type), lambdaExpression.Parameters)
                : lambdaExpression.Update(body, lambdaExpression.Parameters)!;
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

        private bool TryExtractEqualityOperands(
            Expression expression,
            [CA.NotNullWhen(true)] out Expression? left,
            [CA.NotNullWhen(true)] out Expression? right,
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

                case MethodCallExpression methodCallExpression
                    when methodCallExpression.Method.Name == nameof(object.Equals):
                {
                    negated = false;
                    if (methodCallExpression.Arguments.Count == 1
                        && methodCallExpression.Object?.Type == methodCallExpression.Arguments[0].Type)
                    {
                        (left, right) = (methodCallExpression.Object, methodCallExpression.Arguments[0]);

                        return true;
                    }
                    else if (methodCallExpression.Arguments.Count == 2
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

        private Expression? TryOptimizeMemberAccessOverConditional(Expression expression)
        {
            // Simplify (a != null ? new { Member = b, ... } : null).Member
            // to a != null ? b : null
            // Later null check removal will simplify it further
            if (expression is MemberExpression visitedMemberExpression
                && visitedMemberExpression.Expression is ConditionalExpression conditionalExpression
                && conditionalExpression.Test is BinaryExpression binaryTest
                && (binaryTest.NodeType == ExpressionType.Equal
                    || binaryTest.NodeType == ExpressionType.NotEqual)
                // Exclude HasValue/Value over Nullable<> as they return non-null type and we don't have equivalent for it for null part
                && !(conditionalExpression.Type.IsNullableValueType()
                    && (visitedMemberExpression.Member.Name == nameof(Nullable<int>.HasValue)
                        || visitedMemberExpression.Member.Name == nameof(Nullable<int>.Value))))
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

            return null;
        }

        private bool IsNullConstant(Expression expression)
            => expression is ConstantExpression constantExpression
                && constantExpression.Value == null;
    }
}
