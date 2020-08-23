// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Query;
using MethodInfoExtensions = Microsoft.EntityFrameworkCore.Infrastructure.MethodInfoExtensions;

namespace Microsoft.EntityFrameworkCore.TestUtilities
{
    public class ExpectedQueryRewritingVisitor : ExpressionVisitor
    {
        private static readonly MethodInfo _maybeDefaultIfEmpty
            = typeof(QueryTestExtensions).GetMethod(nameof(QueryTestExtensions.MaybeDefaultIfEmpty));

        private static readonly MethodInfo _maybeMethod
            = typeof(QueryTestExtensions).GetMethod(nameof(QueryTestExtensions.Maybe));

        private static readonly MethodInfo _containsMethodInfo
            = typeof(string).GetRuntimeMethod(nameof(string.Contains), new[] { typeof(string) });

        private static readonly MethodInfo _startsWithMethodInfo
            = typeof(string).GetRuntimeMethod(nameof(string.StartsWith), new[] { typeof(string) });

        private static readonly MethodInfo _endsWithMethodInfo
            = typeof(string).GetRuntimeMethod(nameof(string.EndsWith), new[] { typeof(string) });

        private static readonly MethodInfo _maybeScalarNullableMethod;
        private static readonly MethodInfo _maybeScalarNonNullableMethod;

        /// <summary>
        ///     used to map shadow property to a series of non-shadow member expressions,
        ///     e.g. order.CustomerId -> order.Customer + customer.Id
        ///     key: source type + shadow property name
        ///     value: list of MemberInfos that should be used during rewrite
        /// </summary>
        private readonly Dictionary<(Type type, string name), MemberInfo[]> _efPropertyMemberInfoMappings;

        private bool _negated;

        static ExpectedQueryRewritingVisitor()
        {
            var maybeScalarMethods = typeof(QueryTestExtensions).GetMethods()
                .Where(m => m.Name == nameof(QueryTestExtensions.MaybeScalar))
                .Select(m => new { method = m, argument = m.GetParameters()[1].ParameterType.GetGenericArguments()[1] });

            _maybeScalarNullableMethod = maybeScalarMethods.Single(x => x.argument.IsNullableValueType()).method;
            _maybeScalarNonNullableMethod = maybeScalarMethods.Single(x => !x.argument.IsNullableValueType()).method;
        }

        public ExpectedQueryRewritingVisitor(Dictionary<(Type type, string name), MemberInfo[]> efPropertyMemberInfoMappings = null)
        {
            _efPropertyMemberInfoMappings = efPropertyMemberInfoMappings ?? new Dictionary<(Type type, string name), MemberInfo[]>();
        }

        protected override Expression VisitMember(MemberExpression memberExpression)
        {
            if (!memberExpression.Type.IsValueType
                && !memberExpression.Type.IsNullableValueType()
                && memberExpression.Expression != null)
            {
                var expression = Visit(memberExpression.Expression);

                var lambdaParameter = Expression.Parameter(expression.Type, "x");
                var lambda = Expression.Lambda(memberExpression.Update(lambdaParameter), lambdaParameter);
                var method = _maybeMethod.MakeGenericMethod(expression.Type, memberExpression.Type);

                return Expression.Call(method, expression, lambda);
            }

            if (memberExpression.Type == typeof(bool)
                && !_negated
                && memberExpression.Expression != null)
            {
                var expression = Visit(memberExpression.Expression);

                var lambdaParameter = Expression.Parameter(expression.Type, "x");
                var lambda = Expression.Lambda(memberExpression.Update(lambdaParameter), lambdaParameter);
                var method = _maybeScalarNonNullableMethod.MakeGenericMethod(expression.Type, memberExpression.Type);

                return Expression.Equal(
                    Expression.Call(method, expression, lambda),
                    Expression.Constant(true, typeof(bool?)));
            }

            return base.VisitMember(memberExpression);
        }

        protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
        {
            if (methodCallExpression.Method.DeclaringType == typeof(Queryable)
                && methodCallExpression.Method.IsGenericMethod
                && (methodCallExpression.Method.GetGenericMethodDefinition() == QueryableMethods.Join
                    || methodCallExpression.Method.GetGenericMethodDefinition() == QueryableMethods.GroupJoin))
            {
                return RewriteJoinGroupJoin(methodCallExpression);
            }

            if (MethodInfoExtensions.IsEFPropertyMethod(methodCallExpression.Method))
            {
                var rewritten = TryConvertEFPropertyToMemberAccess(methodCallExpression);

                return Visit(rewritten);
            }

            if (!_negated
                && (methodCallExpression.Method == _containsMethodInfo
                    || methodCallExpression.Method == _startsWithMethodInfo
                    || methodCallExpression.Method == _endsWithMethodInfo))
            {
                return RewriteStartsWithEndsWithContains(methodCallExpression);
            }

            if (methodCallExpression.Method.IsGenericMethod
                && methodCallExpression.Method.GetGenericMethodDefinition() == EnumerableMethods.DefaultIfEmptyWithoutArgument)
            {
                var source = Visit(methodCallExpression.Arguments[0]);

                return Expression.Call(
                    _maybeDefaultIfEmpty.MakeGenericMethod(
                        methodCallExpression.Method.GetGenericArguments()[0]),
                    source);
            }

            return base.VisitMethodCall(methodCallExpression);
        }

        private Expression RewriteJoinGroupJoin(MethodCallExpression methodCallExpression)
        {
            //outer.Join<TOuter, TInner, int, Anonymous<...>(inner, ok => ok.Id, ik => i.Fk, (o, i) => new { o, i })
            //gets converted to:
            //outer.Join<TOuter, TInner, int?, Anonymous<...>(inner,
            //  ok => ok.MaybeScalar(x => x.Id),
            //  ik => i.MaybeScalar(x.Fk),
            //  (o, i) => new { o, i })
            var outer = Visit(methodCallExpression.Arguments[0]);
            var inner = Visit(methodCallExpression.Arguments[1]);
            var resultSelector = Visit(methodCallExpression.Arguments[4]);

            var originalLeftKeySelectorLambda = methodCallExpression.Arguments[2].UnwrapLambdaFromQuote();
            var originalRightKeySelectorLambda = methodCallExpression.Arguments[3].UnwrapLambdaFromQuote();
            var leftKeySelectorBody = AddNullProtectionForNonNullableMemberAccess(originalLeftKeySelectorLambda.Body);
            var rightKeySelectorBody = AddNullProtectionForNonNullableMemberAccess(originalRightKeySelectorLambda.Body);

            if (leftKeySelectorBody.Type.IsNullableValueType()
                && rightKeySelectorBody.Type.IsValueType
                && leftKeySelectorBody.Type.UnwrapNullableType() == rightKeySelectorBody.Type)
            {
                rightKeySelectorBody = Expression.Convert(rightKeySelectorBody, leftKeySelectorBody.Type);
            }

            if (rightKeySelectorBody.Type.IsNullableValueType()
                && leftKeySelectorBody.Type.IsValueType
                && rightKeySelectorBody.Type.UnwrapNullableType() == leftKeySelectorBody.Type)
            {
                leftKeySelectorBody = Expression.Convert(leftKeySelectorBody, rightKeySelectorBody.Type);
            }

            var keySelectorTypeChanged = false;
            var joinMethodInfo = methodCallExpression.Method;
            var joinMethodInfoGenericArguments = methodCallExpression.Method.GetGenericArguments();

            if ((leftKeySelectorBody.Type != methodCallExpression.Arguments[2].UnwrapLambdaFromQuote().Body.Type)
                || (rightKeySelectorBody.Type != methodCallExpression.Arguments[3].UnwrapLambdaFromQuote().Body.Type))
            {
                joinMethodInfoGenericArguments[2] = leftKeySelectorBody.Type;
                joinMethodInfo = joinMethodInfo.GetGenericMethodDefinition().MakeGenericMethod(joinMethodInfoGenericArguments);
                keySelectorTypeChanged = true;
            }

            var leftKeySelector = keySelectorTypeChanged
                ? Expression.Lambda(
                    leftKeySelectorBody,
                    methodCallExpression.Arguments[2].UnwrapLambdaFromQuote().Parameters)
                : Expression.Lambda(
                    originalLeftKeySelectorLambda.Type,
                    leftKeySelectorBody,
                    methodCallExpression.Arguments[2].UnwrapLambdaFromQuote().Parameters);

            var rightKeySelector = keySelectorTypeChanged
                ? Expression.Lambda(
                    rightKeySelectorBody,
                    methodCallExpression.Arguments[3].UnwrapLambdaFromQuote().Parameters)
                : Expression.Lambda(
                    originalRightKeySelectorLambda.Type,
                    rightKeySelectorBody,
                    methodCallExpression.Arguments[3].UnwrapLambdaFromQuote().Parameters);

            return Expression.Call(
                joinMethodInfo,
                outer,
                inner,
                leftKeySelector,
                rightKeySelector,
                resultSelector);
        }

        private Expression RewriteStartsWithEndsWithContains(MethodCallExpression methodCallExpression)
        {
            // c.FirstName.StartsWith(c.Nickname)
            // gets converted to:
            // c.Maybe(x => x.FirstName).MaybeScalar(x => c.Maybe(xx => xx.Nickname).MaybeScalar(xx => x.StartsWith(xx)))
            var caller = Visit(methodCallExpression.Object);
            var argument = Visit(methodCallExpression.Arguments[0]);
            var outerMaybeScalarMethod = _maybeScalarNullableMethod.MakeGenericMethod(typeof(string), typeof(bool));
            var innerMaybeScalarMethod = _maybeScalarNonNullableMethod.MakeGenericMethod(typeof(string), typeof(bool));

            var outerMaybeScalarLambdaParameter = Expression.Parameter(typeof(string), "x");
            var innerMaybeScalarLambdaParameter = Expression.Parameter(typeof(string), "xx");
            var innerMaybeScalarLambda = Expression.Lambda(
                methodCallExpression.Update(
                    outerMaybeScalarLambdaParameter,
                    new[] { innerMaybeScalarLambdaParameter }),
                innerMaybeScalarLambdaParameter);

            var innerMaybeScalar = Expression.Call(
                innerMaybeScalarMethod,
                argument,
                innerMaybeScalarLambda);

            var outerMaybeScalarLambda = Expression.Lambda(
                innerMaybeScalar,
                outerMaybeScalarLambdaParameter);

            var outerMaybeScalar = Expression.Call(
                outerMaybeScalarMethod,
                caller,
                outerMaybeScalarLambda);

            return Expression.Equal(outerMaybeScalar, Expression.Constant(true, typeof(bool?)));
        }

        private Expression TryConvertEFPropertyToMemberAccess(Expression expression)
        {
            if (expression is MethodCallExpression methodCallExpression
                && MethodInfoExtensions.IsEFPropertyMethod(methodCallExpression.Method))
            {
                var caller = RemoveConvertToObject(methodCallExpression.Arguments[0]);
                var propertyName = (methodCallExpression.Arguments[1] as ConstantExpression)?.Value as string
                    ?? Expression.Lambda<Func<string>>(methodCallExpression.Arguments[1]).Compile().Invoke();

                if (propertyName != null)
                {
                    var efPropertyMemberInfoMapping = _efPropertyMemberInfoMappings
                        .Where(m => m.Key.type == caller.Type && m.Key.name == propertyName)
                        .Select(m => m.Value).SingleOrDefault();

                    var result = default(Expression);
                    if (efPropertyMemberInfoMapping != null)
                    {
                        result = caller;
                        foreach (var targetMemberInfo in efPropertyMemberInfoMapping)
                        {
                            result = Expression.MakeMemberAccess(result, targetMemberInfo);
                        }
                    }
                    else if (caller.Type.GetMembers().Where(m => m.Name == propertyName).SingleOrDefault() is MemberInfo matchingMember)
                    {
                        result = Expression.Property(caller, propertyName);
                    }

                    if (result != null)
                    {
                        // in case type argument on EF property overrides actual type of the member that is accessed, e.g.
                        // EF.Property<bool>(e, "MyNullableBool")
                        return result.Type != expression.Type
                            ? Expression.Convert(result, expression.Type)
                            : result;
                    }
                }

                throw new InvalidOperationException(
                    $"Couldn't convert EF.Property() method. Caller type: '{caller.Type.Name}'. Property name: '{propertyName}'.");
            }

            return expression;

            static Expression RemoveConvertToObject(Expression expression)
                => expression is UnaryExpression unaryExpression
                    && (expression.NodeType == ExpressionType.Convert
                        || expression.NodeType == ExpressionType.ConvertChecked)
                    && expression.Type == typeof(object)
                        ? RemoveConvertToObject(unaryExpression.Operand)
                        : expression;
        }

        private Expression AddNullProtectionForNonNullableMemberAccess(Expression expression)
        {
            expression = TryConvertEFPropertyToMemberAccess(expression);

            if (expression is MemberExpression memberExpression
                && (memberExpression.Type.IsValueType || memberExpression.Type.IsNullableValueType())
                && memberExpression.Expression != null)
            {
                var instance = Visit(memberExpression.Expression);
                var maybeLambdaParameter = Expression.Parameter(instance.Type, "x");
                var maybeLambda = Expression.Lambda(memberExpression.Update(maybeLambdaParameter), maybeLambdaParameter);

                var methodInfo = (memberExpression.Type.IsNullableValueType()
                    ? _maybeScalarNullableMethod
                    : _maybeScalarNonNullableMethod).MakeGenericMethod(
                    instance.Type,
                    memberExpression.Type.UnwrapNullableType());

                return Expression.Call(methodInfo, instance, maybeLambda);
            }

            return Visit(expression);
        }

        protected override Expression VisitUnary(UnaryExpression unaryExpression)
        {
            if ((unaryExpression.NodeType == ExpressionType.Convert
                    || unaryExpression.NodeType == ExpressionType.ConvertChecked
                    || unaryExpression.NodeType == ExpressionType.TypeAs)
                && unaryExpression.Operand is MemberExpression memberOperand
                && memberOperand.Type.IsValueType
                && !memberOperand.Type.IsNullableValueType()
                && memberOperand.Expression != null
                && unaryExpression.Type.IsNullableValueType()
                && unaryExpression.Type.UnwrapNullableType() == memberOperand.Type)
            {
                var expression = Visit(memberOperand.Expression);

                var lambdaParameter = Expression.Parameter(expression.Type, "x");
                var lambda = Expression.Lambda(memberOperand.Update(lambdaParameter), lambdaParameter);
                var method = _maybeScalarNonNullableMethod.MakeGenericMethod(expression.Type, memberOperand.Type);

                return unaryExpression.Update(
                    Expression.Call(method, expression, lambda));
            }

            if (unaryExpression.NodeType == ExpressionType.Not)
            {
                var negated = _negated;
                _negated = true;
                var operand = Visit(unaryExpression.Operand);
                _negated = negated;

                return unaryExpression.Update(operand);
            }

            return base.VisitUnary(unaryExpression);
        }

        protected override Expression VisitBinary(BinaryExpression binaryExpression)
        {
            if (binaryExpression.NodeType == ExpressionType.Equal
                || binaryExpression.NodeType == ExpressionType.NotEqual
                || binaryExpression.NodeType == ExpressionType.GreaterThan
                || binaryExpression.NodeType == ExpressionType.GreaterThanOrEqual
                || binaryExpression.NodeType == ExpressionType.LessThan
                || binaryExpression.NodeType == ExpressionType.LessThanOrEqual)
            {
                var left = AddNullProtectionForNonNullableMemberAccess(binaryExpression.Left);
                var right = AddNullProtectionForNonNullableMemberAccess(binaryExpression.Right);

                if (left.Type.IsNullableValueType()
                    && right.Type.IsValueType
                    && left.Type.UnwrapNullableType() == right.Type)
                {
                    right = Expression.Convert(right, left.Type);
                }

                if (right.Type.IsNullableValueType()
                    && left.Type.IsValueType
                    && right.Type.UnwrapNullableType() == left.Type)
                {
                    left = Expression.Convert(left, right.Type);
                }

                return binaryExpression.Update(left, binaryExpression.Conversion, right);
            }

            return base.VisitBinary(binaryExpression);
        }
    }
}
