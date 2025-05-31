// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.ExceptionServices;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class ClrPropertySetterFactory : ClrAccessorFactory<IClrPropertySetter>
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected ClrPropertySetterFactory()
    {
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static readonly ClrPropertySetterFactory Instance = new();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override IClrPropertySetter Create(IPropertyBase property)
        => property as IClrPropertySetter ?? CreateBase(property);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override IClrPropertySetter CreateGeneric<TRoot, TDeclaring, TValue>(
        MemberInfo memberInfo,
        IPropertyBase? propertyBase)
    {
        CreateExpression<TRoot, TDeclaring, TValue>(memberInfo, propertyBase, out var setter);
        return new ClrPropertySetter<TRoot, TValue>(setter.Compile());
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override MemberInfo GetMemberInfo(IPropertyBase propertyBase)
        => propertyBase.GetMemberInfo(forMaterialization: false, forSet: true);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void Create(
        IPropertyBase propertyBase,
        out Expression setterExpression)
    {
        var boundMethod = GenericCreateExpression.MakeGenericMethod(
            propertyBase.DeclaringType.ContainingType.ClrType,
            propertyBase.DeclaringType.ClrType,
            propertyBase.ClrType);

        try
        {
            var parameters = new object?[] { GetMemberInfo(propertyBase), propertyBase, null };
            boundMethod.Invoke(this, parameters);
            setterExpression = (Expression)parameters[2]!;
        }
        catch (TargetInvocationException e) when (e.InnerException != null)
        {
            ExceptionDispatchInfo.Capture(e.InnerException).Throw();
            throw;
        }
    }

    private static readonly MethodInfo GenericCreateExpression
        = typeof(ClrPropertySetterFactory).GetMethod(nameof(CreateExpression), BindingFlags.Instance | BindingFlags.NonPublic)!;

    private void CreateExpression<TRoot, TDeclaring, TValue>(
        MemberInfo memberInfo,
        IPropertyBase? propertyBase,
        out Expression<Action<TRoot, TValue>> setterExpression)
        where TRoot : class
    {
        var entityClrType = propertyBase?.DeclaringType.ContainingType.ClrType ?? typeof(TRoot);
        var propertyDeclaringType = propertyBase?.DeclaringType.ClrType ?? typeof(TDeclaring);
        var entityParameter = Expression.Parameter(entityClrType, "entity");
        var valueParameter = Expression.Parameter(typeof(TValue), "value");
        var memberType = memberInfo.GetMemberType();
        var convertedParameter = (Expression)valueParameter;

        var propertyType = propertyBase?.ClrType ?? memberType;
        if (propertyType.IsNullableType())
        {
            var unwrappedType = propertyType.UnwrapNullableType();
            if (unwrappedType.IsEnum)
            {
                convertedParameter = Expression.Condition(
                    Expression.Equal(convertedParameter, Expression.Constant(null, convertedParameter.Type)),
                    convertedParameter,
                    Expression.Convert(Expression.Convert(convertedParameter, unwrappedType), convertedParameter.Type));
            }
        }

        if (memberType != convertedParameter.Type)
        {
            convertedParameter = Expression.Convert(convertedParameter, memberType);
        }

        Expression writeExpression;
        if (memberInfo.DeclaringType!.IsAssignableFrom(propertyDeclaringType))
        {
            writeExpression = CreateMemberAssignment(memberInfo, propertyBase, entityParameter, convertedParameter);
        }
        else
        {
            // This path handles properties that exist only on proxy types and so only exist if the instance is a proxy
            var converted = Expression.Variable(memberInfo.DeclaringType, "converted");

            writeExpression = Expression.Block(
                [converted],
                new List<Expression>
                {
                    Expression.Assign(
                        converted,
                        Expression.TypeAs(entityParameter, memberInfo.DeclaringType)),
                    Expression.IfThen(
                        Expression.ReferenceNotEqual(converted, Expression.Constant(null)),
                        CreateMemberAssignment(memberInfo, propertyBase, converted, convertedParameter))
                });
        }

        setterExpression = Expression.Lambda<Action<TRoot, TValue>>(
            writeExpression,
            entityParameter,
            valueParameter);

        static Expression CreateMemberAssignment(
            MemberInfo memberInfo,
            IPropertyBase? propertyBase,
            Expression instanceParameter,
            Expression convertedParameter)
        {
            if (propertyBase?.DeclaringType is not IComplexType complexType)
            {
                return propertyBase?.IsIndexerProperty() == true
                    ? Expression.Assign(
                        Expression.MakeIndex(
                            instanceParameter, (PropertyInfo)memberInfo, [Expression.Constant(propertyBase.Name)]),
                        convertedParameter)
                    : Expression.MakeMemberAccess(instanceParameter, memberInfo).Assign(convertedParameter);
            }

            // The idea here is to create something like this:
            //
            // $level1 = $entity.<Culture>k__BackingField;
            // $level2 = $level1.<License>k__BackingField;
            // $level3 = $level2.<Tog>k__BackingField;
            // $level3.<Text>k__BackingField = $value;
            // $level2.<Tog>k__BackingField = $level3;
            // $level1.<License>k__BackingField = $level2;
            // $entity.<Culture>k__BackingField = $level1;
            //
            // That is, we create copies of value types, make the assignment, and then copy the value back.
            // This is necessary for the case without a backing field, because the value type property getter will always return a copy of the value

            var chain = complexType.ComplexProperty.GetChainToComplexProperty();
            var previousLevel = instanceParameter;

            var variables = new List<ParameterExpression>();
            var assignments = new List<Expression>();
            var chainCount = chain.Count;
            for (var i = chainCount; i >= 1; i--)
            {
                var currentProperty = chain[chainCount - i];
                var complexMemberInfo = currentProperty.GetMemberInfo(forMaterialization: false, forSet: false);
                var complexPropertyType = complexMemberInfo.GetMemberType();
                var currentLevel = Expression.Variable(complexPropertyType, $"level{chainCount + 1 - i}");
                variables.Add(currentLevel);
                assignments.Add(
                    Expression.Assign(
                        currentLevel, PropertyAccessorsFactory.CreateMemberAccess(
                            currentProperty,
                            previousLevel,
                            complexMemberInfo,
                            fromDeclaringType: true)));
                previousLevel = currentLevel;
            }

            var propertyMemberInfo = propertyBase.GetMemberInfo(forMaterialization: false, forSet: true);
            assignments.Add(Expression.MakeMemberAccess(previousLevel, propertyMemberInfo).Assign(convertedParameter));

            for (var i = 0; i <= chainCount - 1; i++)
            {
                var currentProperty = chain[chainCount - 1 - i];
                var complexMemberInfo = currentProperty.GetMemberInfo(forMaterialization: false, forSet: true);
                if (complexMemberInfo.GetMemberType().IsValueType)
                {
                    var memberExpression = (MemberExpression)PropertyAccessorsFactory.CreateMemberAccess(
                        currentProperty,
                        i == (chainCount - 1) ? instanceParameter : variables[chainCount - 2 - i],
                        complexMemberInfo,
                        fromDeclaringType: true);

                    assignments.Add(memberExpression.Assign(variables[chainCount - 1 - i]));
                }
            }

            return Expression.Block(variables, assignments);
        }
    }
}
