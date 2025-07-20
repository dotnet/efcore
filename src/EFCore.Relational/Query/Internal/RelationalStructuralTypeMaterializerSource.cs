// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Internal;

#pragma warning disable EF1001 // EntityMaterializerSource is pubternal

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class RelationalStructuralTypeMaterializerSource(StructuralTypeMaterializerSourceDependencies dependencies)
    : StructuralTypeMaterializerSource(dependencies)
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
        // JSON complex properties are not handled in the initial materialization expression, since they're not
        // simply e.g. DbDataReader.GetFieldValue<>() calls. So they're handled afterwards in the shaper, and need
        // to be skipped here.
        if (property is IComplexProperty { ComplexType: var complexType } && complexType.IsMappedToJson())
        {
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
        = typeof(RelationalStructuralTypeMaterializerSource).GetTypeInfo().GetDeclaredMethod(nameof(MaterializeJsonComplexType))!;

    private static T MaterializeJsonComplexType<T>(in ValueBuffer valueBuffer, IComplexProperty complexProperty)
        => throw new UnreachableException();
}
