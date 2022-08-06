// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public static class ExpressionExtensions
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static Expression MakeHasDefaultValue(
        this Expression currentValueExpression,
        IReadOnlyPropertyBase? propertyBase)
    {
        if (!currentValueExpression.Type.IsValueType)
        {
            return Expression.ReferenceEqual(
                currentValueExpression,
                Expression.Constant(null, currentValueExpression.Type));
        }

        if (currentValueExpression.Type.IsGenericType
            && currentValueExpression.Type.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            return Expression.Not(
                Expression.Call(
                    currentValueExpression,
                    Check.NotNull(
                        currentValueExpression.Type.GetMethod("get_HasValue"), $"get_HasValue on {currentValueExpression.Type.Name}")));
        }

        var property = propertyBase as IReadOnlyProperty;
        var comparer = property?.GetValueComparer()
            ?? ValueComparer.CreateDefault(
                propertyBase?.ClrType ?? currentValueExpression.Type, favorStructuralComparisons: false);

        return comparer.ExtractEqualsBody(
            comparer.Type != currentValueExpression.Type
                ? Expression.Convert(currentValueExpression, comparer.Type)
                : currentValueExpression,
            Expression.Default(comparer.Type));
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static IReadOnlyList<TMemberInfo>? MatchMemberAccessList<TMemberInfo>(
        this LambdaExpression lambdaExpression,
        Func<Expression, Expression, TMemberInfo?> memberMatcher)
        where TMemberInfo : MemberInfo
    {
        Check.DebugAssert(lambdaExpression.Body != null, "lambdaExpression.Body is null");
        Check.DebugAssert(
            lambdaExpression.Parameters.Count == 1,
            "lambdaExpression.Parameters.Count is " + lambdaExpression.Parameters.Count + ". Should be 1.");

        var parameterExpression = lambdaExpression.Parameters[0];

        if (RemoveConvert(lambdaExpression.Body) is NewExpression newExpression)
        {
            var memberInfos
                = (List<TMemberInfo>)newExpression
                    .Arguments
                    .Select(a => memberMatcher(a, parameterExpression))
                    .Where(p => p != null)
                    .ToList()!;

            return memberInfos.Count != newExpression.Arguments.Count ? null : memberInfos;
        }

        var memberPath = memberMatcher(lambdaExpression.Body, parameterExpression);

        return memberPath != null ? new[] { memberPath } : null;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static TMemberInfo? MatchSimpleMemberAccess<TMemberInfo>(
        this Expression parameterExpression,
        Expression memberAccessExpression)
        where TMemberInfo : MemberInfo
    {
        var memberInfos = MatchMemberAccess<TMemberInfo>(parameterExpression, memberAccessExpression);

        return memberInfos?.Count == 1 ? memberInfos[0] : null;
    }

    private static IReadOnlyList<TMemberInfo>? MatchMemberAccess<TMemberInfo>(
        this Expression parameterExpression,
        Expression memberAccessExpression)
        where TMemberInfo : MemberInfo
    {
        var memberInfos = new List<TMemberInfo>();

        var unwrappedExpression = RemoveTypeAs(RemoveConvert(memberAccessExpression));
        do
        {
            var memberExpression = unwrappedExpression as MemberExpression;

            if (!(memberExpression?.Member is TMemberInfo memberInfo))
            {
                return null;
            }

            memberInfos.Insert(0, memberInfo);

            unwrappedExpression = RemoveTypeAs(RemoveConvert(memberExpression.Expression));
        }
        while (unwrappedExpression != parameterExpression);

        return memberInfos;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static Expression? RemoveTypeAs(this Expression? expression)
    {
        while (expression?.NodeType == ExpressionType.TypeAs)
        {
            expression = ((UnaryExpression)RemoveConvert(expression)).Operand;
        }

        return expression;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static bool IsLogicalOperation(this Expression expression)
        => expression.NodeType == ExpressionType.AndAlso
            || expression.NodeType == ExpressionType.OrElse;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static LambdaExpression? GetLambdaOrNull(this Expression expression)
        => expression is LambdaExpression lambda
            ? lambda
            : expression is UnaryExpression unary && expression.NodeType == ExpressionType.Quote
                ? (LambdaExpression)unary.Operand
                : null;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static bool IsLogicalNot(this UnaryExpression sqlUnaryExpression)
        => sqlUnaryExpression.NodeType == ExpressionType.Not
            && (sqlUnaryExpression.Type == typeof(bool)
                || sqlUnaryExpression.Type == typeof(bool?));

    [return: NotNullIfNotNull("expression")]
    private static Expression? RemoveConvert(Expression? expression)
    {
        if (expression is UnaryExpression unaryExpression
            && (expression.NodeType == ExpressionType.Convert
                || expression.NodeType == ExpressionType.ConvertChecked))
        {
            return RemoveConvert(unaryExpression.Operand);
        }

        return expression;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static Expression BuildPredicate(
        IReadOnlyList<IReadOnlyProperty> keyProperties,
        ValueBuffer keyValues,
        ParameterExpression entityParameter)
    {
        var keyValuesConstant = Expression.Constant(keyValues);

        var predicate = GenerateEqualExpression(entityParameter, keyValuesConstant, keyProperties[0], 0);

        for (var i = 1; i < keyProperties.Count; i++)
        {
            predicate = Expression.AndAlso(predicate, GenerateEqualExpression(entityParameter, keyValuesConstant, keyProperties[i], i));
        }

        return predicate;

        static Expression GenerateEqualExpression(
            Expression entityParameterExpression,
            Expression keyValuesConstantExpression,
            IReadOnlyProperty property,
            int i)
            => property.ClrType.IsValueType
                && property.ClrType.UnwrapNullableType() is Type nonNullableType
                && !(nonNullableType == typeof(bool) || nonNullableType.IsNumeric() || nonNullableType.IsEnum)
                    ? Infrastructure.ExpressionExtensions.BuildEqualsExpression(
                        Expression.Call(
                            EF.PropertyMethod.MakeGenericMethod(typeof(object)),
                            entityParameterExpression,
                            Expression.Constant(property.Name, typeof(string))),
                        Expression.Call(
                            keyValuesConstantExpression,
                            ValueBuffer.GetValueMethod,
                            Expression.Constant(i)))
                    : Expression.Equal(
                        Expression.Call(
                            EF.PropertyMethod.MakeGenericMethod(property.ClrType),
                            entityParameterExpression,
                            Expression.Constant(property.Name, typeof(string))),
                        Expression.Convert(
                            Expression.Call(
                                keyValuesConstantExpression,
                                ValueBuffer.GetValueMethod,
                                Expression.Constant(i)),
                            property.ClrType));
    }
}
