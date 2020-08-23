// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

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
            typeof(string).GetRuntimeMethod(nameof(string.Compare), new[] { typeof(string), typeof(string), typeof(StringComparison) });

        private static readonly MethodInfo _stringCompareWithoutComparisonMethod =
            typeof(string).GetRuntimeMethod(nameof(string.Compare), new[] { typeof(string), typeof(string) });

        private static readonly MethodInfo _startsWithMethodInfo =
            typeof(string).GetRuntimeMethod(nameof(string.StartsWith), new[] { typeof(string) });

        private static readonly MethodInfo _endsWithMethodInfo =
            typeof(string).GetRuntimeMethod(nameof(string.EndsWith), new[] { typeof(string) });

        private static readonly Expression _constantNullString = Expression.Constant(null, typeof(string));

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override Expression VisitMember(MemberExpression memberExpression)
        {
            var visitedExpression = base.VisitMember(memberExpression);

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

            if (_startsWithMethodInfo.Equals(methodCallExpression.Method)
                || _endsWithMethodInfo.Equals(methodCallExpression.Method))
            {
                if (methodCallExpression.Arguments[0] is ConstantExpression constantArgument
                    && (string)constantArgument.Value == string.Empty)
                {
                    // every string starts/ends with empty string.
                    return Expression.Constant(true);
                }

                var newObject = Visit(methodCallExpression.Object);
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

            var visited = (MethodCallExpression)base.VisitMethodCall(methodCallExpression);

            // In VB.NET, comparison operators between strings (equality, greater-than, less-than) yield
            // calls to a VB-specific CompareString method. Normalize that to string.Compare.
            if (visited.Method.Name == "CompareString"
                && visited.Method.DeclaringType?.Name == "Operators"
                && visited.Method.DeclaringType?.Namespace == "Microsoft.VisualBasic.CompilerServices"
                && visited.Object == null
                && visited.Arguments.Count == 3
                && visited.Arguments[2] is ConstantExpression textCompareConstantExpression)
            {
                return (bool)textCompareConstantExpression.Value
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
                && (_startsWithMethodInfo.Equals(innerMethodCall.Method)
                    || _endsWithMethodInfo.Equals(innerMethodCall.Method)))
            {
                if (innerMethodCall.Arguments[0] is ConstantExpression constantArgument
                    && (string)constantArgument.Value == string.Empty)
                {
                    // every string starts/ends with empty string.
                    return Expression.Constant(false);
                }

                var newObject = Visit(innerMethodCall.Object);
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

            return base.VisitUnary(unaryExpression);
        }

        private bool TryExtractEqualityOperands(Expression expression, out Expression left, out Expression right, out bool negated)
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
                        && methodCallExpression.Object.Type == methodCallExpression.Arguments[0].Type)
                    {
                        (left, right) = (methodCallExpression.Object, methodCallExpression.Arguments[0]);
                    }
                    else if (methodCallExpression.Arguments.Count == 2
                        && methodCallExpression.Arguments[0].Type == methodCallExpression.Arguments[1].Type)
                    {
                        (left, right) = (methodCallExpression.Arguments[0], methodCallExpression.Arguments[1]);
                    }

                    return true;
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

        private Expression TryOptimizeMemberAccessOverConditional(Expression expression)
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
