// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.ExceptionServices;
using static System.Linq.Expressions.Expression;

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
        CreateExpressions<TRoot, TDeclaring, TValue>(
            memberInfo, propertyBase,
            out var setterUsingContainingEntityExpression,
            out var setterExpression);
        return new ClrPropertySetter<TRoot, TDeclaring, TValue>(
            setterUsingContainingEntityExpression.Compile(),
            setterExpression.Compile());
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
        out Expression setterUsingContainingEntityExpression,
        out Expression setterExpression)
    {
        var boundMethod = GenericCreateExpression.MakeGenericMethod(
            propertyBase.DeclaringType.ContainingEntityType.ClrType,
            propertyBase.DeclaringType.ClrType,
            propertyBase.ClrType);

        try
        {
            var parameters = new object?[] { GetMemberInfo(propertyBase), propertyBase, null, null };
            boundMethod.Invoke(this, parameters);
            setterUsingContainingEntityExpression = (Expression)parameters[2]!;
            setterExpression = (Expression)parameters[3]!;
        }
        catch (TargetInvocationException e) when (e.InnerException != null)
        {
            ExceptionDispatchInfo.Capture(e.InnerException).Throw();
            throw;
        }
    }

    private static readonly MethodInfo GenericCreateExpression
        = typeof(ClrPropertySetterFactory).GetMethod(nameof(CreateExpressions), BindingFlags.Instance | BindingFlags.NonPublic)!;

    private static readonly MethodInfo ComplexCollectionNullElementSetterException =
        typeof(CoreStrings).GetMethod(nameof(CoreStrings.ComplexCollectionNullElementSetter))!;

    private void CreateExpressions<TRoot, TDeclaring, TValue>(
        MemberInfo memberInfo,
        IPropertyBase? propertyBase,
        out Expression<Action<TRoot, IReadOnlyList<int>, TValue>> setterUsingContainingEntityExpression,
        out Expression<Func<TDeclaring, TValue, TDeclaring>> setterExpression)
        where TRoot : class
    {
        CreateExpressionUsingContainingEntity<TRoot, TDeclaring, TValue>(
            memberInfo, propertyBase, out setterUsingContainingEntityExpression);
        CreateDirectExpression(memberInfo, propertyBase, out setterExpression);
    }

    private static Expression CreateConvertedValueExpression(Expression valueParameter, MemberInfo memberInfo, IPropertyBase? propertyBase)
    {
        var memberType = memberInfo.GetMemberType();
        var convertedParameter = valueParameter;

        var propertyType = propertyBase?.ClrType ?? memberType;
        if (propertyType.IsNullableType())
        {
            var unwrappedType = propertyType.UnwrapNullableType();
            if (unwrappedType.IsEnum)
            {
                convertedParameter = Condition(
                    Equal(convertedParameter, Constant(null, convertedParameter.Type)),
                    convertedParameter,
                    Convert(Convert(convertedParameter, unwrappedType), convertedParameter.Type));
            }
        }

        if (memberType != convertedParameter.Type)
        {
            convertedParameter = Convert(convertedParameter, memberType);
        }

        return convertedParameter;
    }

    private static Expression CreateSimplePropertyAssignment(
        MemberInfo memberInfo,
        IPropertyBase? propertyBase,
        Expression instanceParameter,
        Expression convertedParameter)
        => propertyBase?.IsIndexerProperty() == true
            ? Assign(
                MakeIndex(
                    instanceParameter, (PropertyInfo)memberInfo, [Constant(propertyBase.Name)]),
                convertedParameter)
            : MakeMemberAccess(instanceParameter, memberInfo).Assign(convertedParameter);

    private void CreateExpressionUsingContainingEntity<TRoot, TDeclaring, TValue>(
        MemberInfo memberInfo,
        IPropertyBase? propertyBase,
        out Expression<Action<TRoot, IReadOnlyList<int>, TValue>> setterExpression)
        where TRoot : class
    {
        var entityClrType = propertyBase?.DeclaringType.ContainingEntityType.ClrType ?? typeof(TRoot);
        var propertyDeclaringType = propertyBase?.DeclaringType.ClrType ?? typeof(TDeclaring);
        var entityParameter = Parameter(entityClrType, "entity");
        var indicesParameter = Parameter(typeof(IReadOnlyList<int>), "indices");
        var valueParameter = Parameter(typeof(TValue), "value");
        var convertedParameter = CreateConvertedValueExpression(valueParameter, memberInfo, propertyBase);

        Expression writeExpression;
        if (memberInfo.DeclaringType!.IsAssignableFrom(propertyDeclaringType))
        {
            writeExpression = CreateMemberAssignment(memberInfo, propertyBase, entityParameter, indicesParameter, convertedParameter);
        }
        else
        {
            // This path handles properties that exist only on proxy types and so only exist if the instance is a proxy
            var converted = Variable(memberInfo.DeclaringType, "converted");

            writeExpression = Block(
                [converted],
                new List<Expression>
                {
                    Assign(
                        converted,
                        TypeAs(entityParameter, memberInfo.DeclaringType)),
                    IfThen(
                        ReferenceNotEqual(converted, Constant(null)),
                        CreateMemberAssignment(memberInfo, propertyBase, converted, indicesParameter, convertedParameter))
                });
        }

        setterExpression = Lambda<Action<TRoot, IReadOnlyList<int>, TValue>>(
            writeExpression,
            entityParameter,
            indicesParameter,
            valueParameter);

        static Expression CreateMemberAssignment(
            MemberInfo memberInfo,
            IPropertyBase? propertyBase,
            Expression instanceParameter,
            ParameterExpression indicesParameter,
            Expression convertedParameter)
        {
            if (propertyBase?.DeclaringType is not IComplexType complexType)
            {
                return CreateSimplePropertyAssignment(memberInfo, propertyBase, instanceParameter, convertedParameter);
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
            // This is necessary for the case without a backing field, because the value type property getter will return a copy of the value

            var chain = complexType.ComplexProperty.GetChainToComplexProperty(fromEntity: true);
            var previousLevel = instanceParameter;

            var variables = new List<ParameterExpression>();
            var statements = new List<Expression>();
            var chainCount = chain.Count;
            for (var i = chainCount; i >= 1; i--)
            {
                var currentProperty = chain[chainCount - i];
                if (currentProperty.IsCollection)
                {
                    var complexElementType = currentProperty.ComplexType.ClrType;
                    var currentLevel = Variable(complexElementType, $"level{chainCount + 1 - i}");
                    variables.Add(currentLevel);
                    statements.Add(
                        Assign(
                            currentLevel,
                            PropertyAccessorsFactory.CreateComplexCollectionElementAccess(
                                currentProperty,
                                previousLevel,
                                indicesParameter,
                                fromDeclaringType: true,
                                fromEntity: false)));

                    var indexExpression = MakeIndex(
                        indicesParameter,
                        indicesParameter.Type.GetProperty("Item"),
                        [Constant(((IRuntimeComplexType)currentProperty.ComplexType).CollectionDepth - 1)]);

                    statements.Add(
                        IfThen(
                            ReferenceEqual(currentLevel, Constant(null)),
                            Throw(
                                New(
                                    PropertyAccessorsFactory.InvalidOperationConstructor!,
                                    Call(
                                        ComplexCollectionNullElementSetterException,
                                        Constant(propertyBase.DeclaringType.DisplayName()),
                                        Constant(propertyBase.Name),
                                        Constant(currentProperty.DeclaringType.DisplayName()),
                                        Constant(currentProperty.Name),
                                        Convert(indexExpression, typeof(object)))))));

                    previousLevel = currentLevel;
                }
                else
                {
                    var complexMemberInfo = currentProperty.GetMemberInfo(forMaterialization: false, forSet: false);
                    var complexPropertyType = complexMemberInfo.GetMemberType();
                    var currentLevel = Variable(complexPropertyType, $"level{chainCount + 1 - i}");
                    variables.Add(currentLevel);
                    statements.Add(
                        Assign(
                            currentLevel,
                            PropertyAccessorsFactory.CreateMemberAccess(
                                currentProperty,
                                previousLevel,
                                indicesParameter,
                                complexMemberInfo,
                                fromDeclaringType: true,
                                fromEntity: false)));
                    previousLevel = currentLevel;
                }
            }

            var propertyMemberInfo = propertyBase.GetMemberInfo(forMaterialization: false, forSet: true);
            statements.Add(
                CreateSimplePropertyAssignment(
                    propertyMemberInfo, propertyBase, previousLevel, convertedParameter));

            for (var i = 0; i <= chainCount - 1; i++)
            {
                var currentProperty = chain[chainCount - 1 - i];
                if (currentProperty.IsCollection)
                {
                    if (currentProperty.ComplexType.ClrType.IsValueType)
                    {
                        var memberExpression = (MemberExpression)PropertyAccessorsFactory.CreateComplexCollectionElementAccess(
                            currentProperty,
                            i == (chainCount - 1) ? instanceParameter : variables[chainCount - 2 - i],
                            indicesParameter,
                            fromDeclaringType: true,
                            fromEntity: false);

                        statements.Add(memberExpression.Assign(variables[chainCount - 1 - i]));
                    }
                }
                else
                {
                    var complexMemberInfo = currentProperty.GetMemberInfo(forMaterialization: false, forSet: true);
                    if (complexMemberInfo.GetMemberType().IsValueType)
                    {
                        var memberExpression = (MemberExpression)PropertyAccessorsFactory.CreateMemberAccess(
                            currentProperty,
                            i == (chainCount - 1) ? instanceParameter : variables[chainCount - 2 - i],
                            indicesParameter,
                            complexMemberInfo,
                            fromDeclaringType: true,
                            fromEntity: false,
                            addNullCheck: false);

                        statements.Add(memberExpression.Assign(variables[chainCount - 1 - i]));
                    }
                }
            }

            return Block(variables, statements);
        }
    }

    private void CreateDirectExpression<TDeclaring, TValue>(
        MemberInfo memberInfo,
        IPropertyBase? propertyBase,
        out Expression<Func<TDeclaring, TValue, TDeclaring>> setterExpression)
    {
        var propertyDeclaringType = propertyBase?.DeclaringType.ClrType ?? typeof(TDeclaring);
        var instanceParameter = Parameter(typeof(TDeclaring), "instance");
        var valueParameter = Parameter(typeof(TValue), "value");
        var convertedParameter = CreateConvertedValueExpression(valueParameter, memberInfo, propertyBase);

        Expression writeExpression = null!;
        if (memberInfo.DeclaringType!.IsAssignableFrom(propertyDeclaringType))
        {
            writeExpression = Block(
                CreateSimplePropertyAssignment(memberInfo, propertyBase, instanceParameter, convertedParameter),
                instanceParameter);
        }
        else
        {
            // This path handles properties that exist only on proxy types and so only exist if the instance is a proxy
            var converted = Variable(memberInfo.DeclaringType, "converted");

            writeExpression = Block(
                [converted],
                new List<Expression>
                {
                    Assign(
                        converted,
                        TypeAs(instanceParameter, memberInfo.DeclaringType)),
                    IfThen(
                        ReferenceNotEqual(converted, Constant(null)),
                        CreateSimplePropertyAssignment(memberInfo, propertyBase, converted, convertedParameter)),
                    instanceParameter
                });
        }

        setterExpression = Lambda<Func<TDeclaring, TValue, TDeclaring>>(
            writeExpression,
            instanceParameter,
            valueParameter);
    }
}
