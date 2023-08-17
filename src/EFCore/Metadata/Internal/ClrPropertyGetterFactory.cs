// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class ClrPropertyGetterFactory : ClrAccessorFactory<IClrPropertyGetter>
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override IClrPropertyGetter Create(IPropertyBase property)
        => property as IClrPropertyGetter ?? Create(property.GetMemberInfo(forMaterialization: false, forSet: false), property);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override IClrPropertyGetter CreateGeneric<TEntity, TStructuralType, TValue, TNonNullableEnumValue>(
        MemberInfo memberInfo,
        IPropertyBase? propertyBase)
    {
        var entityClrType = propertyBase?.DeclaringType.ContainingEntityType.ClrType ?? typeof(TEntity);
        var propertyDeclaringType = propertyBase?.DeclaringType.ClrType ?? typeof(TEntity);
        var entityParameter = Expression.Parameter(entityClrType, "entity");
        var structuralParameter = Expression.Parameter(propertyDeclaringType, "instance");

        var readExpression = CreateReadExpression(entityParameter, false);
        var structuralReadExpression = CreateReadExpression(structuralParameter, true);

        var hasSentinelValueExpression = readExpression.MakeHasSentinel(propertyBase);
        var hasStructuralSentinelValueExpression = structuralReadExpression.MakeHasSentinel(propertyBase);

        readExpression = ConvertReadExpression(readExpression, hasSentinelValueExpression);
        structuralReadExpression = ConvertReadExpression(structuralReadExpression, hasStructuralSentinelValueExpression);

        return new ClrPropertyGetter<TEntity, TStructuralType, TValue>(
            Expression.Lambda<Func<TEntity, TValue>>(readExpression, entityParameter).Compile(),
            Expression.Lambda<Func<TEntity, bool>>(hasSentinelValueExpression, entityParameter).Compile(),
            Expression.Lambda<Func<TStructuralType, TValue>>(structuralReadExpression, structuralParameter).Compile(),
            Expression.Lambda<Func<TStructuralType, bool>>(hasStructuralSentinelValueExpression, structuralParameter).Compile());

        Expression CreateReadExpression(ParameterExpression parameter, bool fromContainingType)
        {
            if (memberInfo.DeclaringType!.IsAssignableFrom(propertyDeclaringType))
            {
                return PropertyBase.CreateMemberAccess(propertyBase, parameter, memberInfo, fromContainingType);
            }

            // This path handles properties that exist only on proxy types and so only exist if the instance is a proxy
            var converted = Expression.Variable(memberInfo.DeclaringType, "converted");

            return Expression.Block(
                new[] { converted },
                new List<Expression>
                {
                    Expression.Assign(
                        converted,
                        Expression.TypeAs(parameter, memberInfo.DeclaringType)),
                    Expression.Condition(
                        Expression.ReferenceEqual(converted, Expression.Constant(null)),
                        Expression.Default(memberInfo.GetMemberType()),
                        PropertyBase.CreateMemberAccess(propertyBase, converted, memberInfo, fromContainingType))
                });
        }

        static Expression ConvertReadExpression(Expression expression, Expression sentinelExpression)
            => expression.Type != typeof(TValue)
                ? Expression.Condition(
                    sentinelExpression,
                    Expression.Constant(default(TValue), typeof(TValue)),
                    Expression.Convert(expression, typeof(TValue)))
                : expression;
    }
}
