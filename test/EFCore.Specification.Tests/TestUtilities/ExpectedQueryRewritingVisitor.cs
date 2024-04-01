// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestUtilities;

public class ExpectedQueryRewritingVisitor(Dictionary<(Type, string), Func<object, object>>? shadowPropertyMappings = null) : ExpressionVisitor
{
    private static readonly MethodInfo _maybeDefaultIfEmpty
        = typeof(TestExtensions).GetMethod(nameof(TestExtensions.MaybeDefaultIfEmpty))!;

    private static readonly MethodInfo _maybeMethod
        = typeof(TestExtensions).GetMethod(nameof(TestExtensions.Maybe))!;

    private static readonly MethodInfo _getShadowPropertyValueMethodInfo
        = typeof(ExpectedQueryRewritingVisitor).GetMethod(nameof(GetShadowPropertyValue))!;

    private static readonly MethodInfo _maybeScalarNullableMethod;
    private static readonly MethodInfo _maybeScalarNonNullableMethod;

    private readonly Dictionary<(Type, string), Func<object, object>> _shadowPropertyMappings = shadowPropertyMappings ?? new Dictionary<(Type, string), Func<object, object>>();

    private bool _negated;

    static ExpectedQueryRewritingVisitor()
    {
        var maybeScalarMethods = typeof(TestExtensions).GetMethods()
            .Where(m => m.Name == nameof(TestExtensions.MaybeScalar))
            .Select(m => new { method = m, argument = m.GetParameters()[1].ParameterType.GetGenericArguments()[1] });

        _maybeScalarNullableMethod = maybeScalarMethods.Single(x => x.argument.IsNullableValueType()).method;
        _maybeScalarNonNullableMethod = maybeScalarMethods.Single(x => !x.argument.IsNullableValueType()).method;
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

        if (methodCallExpression.Method.IsEFPropertyMethod())
        {
            var rewritten = TryConvertEFPropertyToMemberAccess(methodCallExpression);

            return Visit(rewritten);
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

    public static TResult GetShadowPropertyValue<TEntity, TResult>(TEntity entity, Func<object, object> shadowPropertyAccessor)
        => (TResult)shadowPropertyAccessor(entity!);

    private Expression TryConvertEFPropertyToMemberAccess(Expression expression)
    {
        if (expression is MethodCallExpression methodCallExpression
            && methodCallExpression.Method.IsEFPropertyMethod())
        {
            var caller = RemoveConvertToObject(methodCallExpression.Arguments[0]);
            var propertyName = (methodCallExpression.Arguments[1] as ConstantExpression)?.Value as string
                ?? Expression.Lambda<Func<string?>>(methodCallExpression.Arguments[1]).Compile().Invoke();

            if (propertyName != null)
            {
                var shadowPropertyMapping = _shadowPropertyMappings
                    .Where(m => caller.Type.GetTypesInHierarchy().Contains(m.Key.Item1) && m.Key.Item2 == propertyName)
                    .Select(m => m.Value).SingleOrDefault();

                var result = default(Expression);
                if (shadowPropertyMapping != null)
                {
                    var methodInfo = _getShadowPropertyValueMethodInfo.MakeGenericMethod(caller.Type, methodCallExpression.Type);
                    result = Expression.Call(methodInfo, caller, Expression.Constant(shadowPropertyMapping));
                }
                else if (caller.Type.GetMembers().SingleOrDefault(m => m.Name == propertyName) is not null)
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
            => expression is UnaryExpression { NodeType: ExpressionType.Convert or ExpressionType.ConvertChecked } unaryExpression
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

            var maybeMethodCall = Expression.Call(methodInfo, instance, maybeLambda);

            return memberExpression.Member.DeclaringType!.IsNullableType()
                && memberExpression.Member.Name == "HasValue"
                    ? Expression.Coalesce(maybeMethodCall, Expression.Constant(false))
                    : maybeMethodCall;
        }

        return Visit(expression);
    }

    protected override Expression VisitUnary(UnaryExpression unaryExpression)
    {
        if (unaryExpression is
            {
                NodeType: ExpressionType.Convert or ExpressionType.ConvertChecked or ExpressionType.TypeAs,
                Operand: MemberExpression
                {
                    Type.IsValueType: true,
                    Expression: not null
                } memberOperand
            }
            && !memberOperand.Type.IsNullableValueType()
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
        if (binaryExpression.NodeType is ExpressionType.Equal
            or ExpressionType.NotEqual
            or ExpressionType.GreaterThan
            or ExpressionType.GreaterThanOrEqual
            or ExpressionType.LessThan
            or ExpressionType.LessThanOrEqual)
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
