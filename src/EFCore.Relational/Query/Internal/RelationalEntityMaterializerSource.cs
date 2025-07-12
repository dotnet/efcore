// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using static System.Linq.Expressions.Expression;

namespace Microsoft.EntityFrameworkCore.Query.Internal;

#pragma warning disable EF1001 // EntityMaterializerSource is pubternal

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class RelationalEntityMaterializerSource(EntityMaterializerSourceDependencies dependencies)
    : EntityMaterializerSource(dependencies)
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override void AddInitializeExpression(
        IPropertyBase property,
        ParameterBindingInfo bindingInfo,
        Expression instanceVariable,
        MethodCallExpression valueBufferExpression,
        List<Expression> blockExpressions)
    {
        if (property is IComplexProperty { ComplexType: var complexType } complexProperty
            && complexType.IsMappedToJson())
        {
            // var memberInfo = property.GetMemberInfo(forMaterialization: true, forSet: true);

            // var valueExpression = Call(
            //     MaterializeJsonComplexTypeMethod.MakeGenericMethod(complexProperty.ClrType),
            //     valueBufferExpression,
            //     Constant(complexProperty, typeof(IComplexProperty)));

            // blockExpressions.Add(
            //     property.IsIndexerProperty()
            //         ? Assign(
            //             MakeIndex(instanceVariable, (PropertyInfo)memberInfo, [Constant(property.Name)]),
            //             valueExpression)
            //         : MakeMemberAccess(instanceVariable, memberInfo).Assign(valueExpression));
            return;
        }

        base.AddInitializeExpression(property, bindingInfo, instanceVariable, valueBufferExpression, blockExpressions);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public static readonly MethodInfo MaterializeJsonComplexTypeMethod
        = typeof(RelationalEntityMaterializerSource).GetTypeInfo().GetDeclaredMethod(nameof(MaterializeJsonComplexType))!;

    private static T MaterializeJsonComplexType<T>(in ValueBuffer valueBuffer, IComplexProperty complexProperty)
        => throw new UnreachableException();
}
