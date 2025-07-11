// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.ExceptionServices;
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
    private ClrPropertyGetterFactory()
    {
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static readonly ClrPropertyGetterFactory Instance = new();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override IClrPropertyGetter Create(IPropertyBase property)
        => property as IClrPropertyGetter ?? CreateBase(property);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override IClrPropertyGetter CreateGeneric<TRoot, TDeclaring, TValue>(
        MemberInfo memberInfo,
        IPropertyBase? propertyBase)
    {
        CreateExpressions<TRoot, TDeclaring, TValue>(
            memberInfo, propertyBase,
            out var getClrValueUsingContainingEntityExpression,
            out var hasSentinelValueUsingContainingEntityExpression,
            out var getClrValueExpression,
            out var hasSentinelValueExpression);
        return new ClrPropertyGetter<TRoot, TDeclaring, TValue>(
            getClrValueUsingContainingEntityExpression.Compile(),
            hasSentinelValueUsingContainingEntityExpression.Compile(),
            getClrValueExpression.Compile(),
            hasSentinelValueExpression.Compile());
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override MemberInfo GetMemberInfo(IPropertyBase propertyBase)
        => propertyBase.GetMemberInfo(forMaterialization: false, forSet: false);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void Create(
        IPropertyBase propertyBase,
        out Expression getClrValueUsingContainingEntityExpression,
        out Expression hasSentinelValueUsingContainingEntityExpression,
        out Expression getClrValueExpression,
        out Expression hasSentinelValueExpression)
    {
        var boundMethod = GenericCreateExpressions.MakeGenericMethod(
            propertyBase.DeclaringType.ContainingEntityType.ClrType,
            propertyBase.DeclaringType.ClrType,
            propertyBase.ClrType);

        try
        {
            var parameters = new object?[] { GetMemberInfo(propertyBase), propertyBase, null, null, null, null };
            boundMethod.Invoke(this, parameters);
            getClrValueUsingContainingEntityExpression = (Expression)parameters[2]!;
            hasSentinelValueUsingContainingEntityExpression = (Expression)parameters[3]!;
            getClrValueExpression = (Expression)parameters[4]!;
            hasSentinelValueExpression = (Expression)parameters[5]!;
        }
        catch (TargetInvocationException e) when (e.InnerException != null)
        {
            ExceptionDispatchInfo.Capture(e.InnerException).Throw();
            throw;
        }
    }

    private static readonly MethodInfo GenericCreateExpressions
        = typeof(ClrPropertyGetterFactory).GetMethod(nameof(CreateExpressions), BindingFlags.Instance | BindingFlags.NonPublic)!;

    private void CreateExpressions<TRoot, TDeclaring, TValue>(
        MemberInfo memberInfo,
        IPropertyBase? propertyBase,
        out Expression<Func<TRoot, IReadOnlyList<int>, TValue>> getClrValueUsingContainingEntityExpression,
        out Expression<Func<TRoot, IReadOnlyList<int>, bool>> hasSentinelValueUsingContainingEntityExpression,
        out Expression<Func<TDeclaring, TValue>> getClrValueExpression,
        out Expression<Func<TDeclaring, bool>> hasSentinelValueExpression)
    {
        var entityClrType = propertyBase?.DeclaringType.ContainingEntityType.ClrType ?? typeof(TRoot);
        var propertyDeclaringType = propertyBase?.DeclaringType.ClrType ?? typeof(TDeclaring);
        var entityParameter = Expression.Parameter(entityClrType, "entity");
        var indicesParameter = Expression.Parameter(typeof(IReadOnlyList<int>), "indices");
        var structuralParameter = Expression.Parameter(propertyDeclaringType, "instance");

        var readExpression = CreateReadExpression(entityParameter, indicesParameter, fromDeclaringType: false);
        var structuralReadExpression = CreateReadExpression(structuralParameter, indicesParameter, fromDeclaringType: true);

        var hasClrSentinelValueExpression = readExpression.MakeHasSentinel(propertyBase);
        var hasStructuralSentinelValueExpression = structuralReadExpression.MakeHasSentinel(propertyBase);

        readExpression = ConvertReadExpression(readExpression, hasClrSentinelValueExpression);
        structuralReadExpression = ConvertReadExpression(structuralReadExpression, hasStructuralSentinelValueExpression);

        getClrValueUsingContainingEntityExpression = Expression.Lambda<Func<TRoot, IReadOnlyList<int>, TValue>>(readExpression, entityParameter, indicesParameter);
        hasSentinelValueUsingContainingEntityExpression = Expression.Lambda<Func<TRoot, IReadOnlyList<int>, bool>>(hasClrSentinelValueExpression, entityParameter, indicesParameter);
        getClrValueExpression = Expression.Lambda<Func<TDeclaring, TValue>>(structuralReadExpression, structuralParameter);
        hasSentinelValueExpression =
            Expression.Lambda<Func<TDeclaring, bool>>(hasStructuralSentinelValueExpression, structuralParameter);

        Expression CreateReadExpression(ParameterExpression instanceParameter, ParameterExpression indicesParameter, bool fromDeclaringType)
        {
            if (memberInfo.DeclaringType!.IsAssignableFrom(propertyDeclaringType))
            {
                return PropertyAccessorsFactory.CreateMemberAccess(propertyBase, instanceParameter, indicesParameter, memberInfo, fromDeclaringType, fromEntity: !fromDeclaringType);
            }

            // This path handles properties that exist only on proxy types and so only exist if the instance is a proxy
            var converted = Expression.Variable(memberInfo.DeclaringType, "converted");

            return Expression.Block(
                [converted],
                new List<Expression>
                {
                    Expression.Assign(
                        converted,
                        Expression.TypeAs(instanceParameter, memberInfo.DeclaringType)),
                    Expression.Condition(
                        Expression.ReferenceEqual(converted, Expression.Constant(null)),
                        Expression.Default(memberInfo.GetMemberType()),
                        PropertyAccessorsFactory.CreateMemberAccess(propertyBase, converted, indicesParameter, memberInfo, fromDeclaringType, fromEntity: !fromDeclaringType))
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
