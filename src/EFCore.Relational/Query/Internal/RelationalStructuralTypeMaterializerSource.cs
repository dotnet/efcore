// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Internal;

#pragma warning disable EF1001 // StructuralTypeMaterializerSource is pubternal

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class RelationalStructuralTypeMaterializerSource(StructuralTypeMaterializerSourceDependencies dependencies)
    : StructuralTypeMaterializerSource(dependencies)
{
    // Whether the materializer being created reads the values of all the properties from the value buffer, e.g. for
    // PropertyValues.ToObject(), rather than the results of a query
    [ThreadStatic]
    private static bool _readingAllValues;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override Func<MaterializationContext, object> GetMaterializer(IEntityType entityType)
    {
        var readingAllValues = _readingAllValues;
        _readingAllValues = true;
        try
        {
            return base.GetMaterializer(entityType);
        }
        finally
        {
            _readingAllValues = readingAllValues;
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override Func<MaterializationContext, object> GetMaterializer(IComplexType complexType)
    {
        var readingAllValues = _readingAllValues;
        _readingAllValues = true;
        try
        {
            return base.GetMaterializer(complexType);
        }
        finally
        {
            _readingAllValues = readingAllValues;
        }
    }

    /// <summary>
    ///     JSON complex properties are not handled in the initial materialization expression of a query,
    ///     since they're not simply e.g. DbDataReader.GetFieldValue calls.
    ///     So they're handled afterwards in the shaper, and need to be skipped.
    ///     Outside of queries, e.g. for PropertyValues.ToObject(), the value buffer contains the values of their properties.
    /// </summary>
    protected override bool ReadComplexTypeDirectly(IComplexType complexType)
        => _readingAllValues || !complexType.IsMappedToJson();

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
        MethodCallExpression getValueBufferExpression,
        List<Expression> blockExpressions,
        bool nullable)
    {
        if (_readingAllValues)
        {
            // Complex collections are populated afterwards from their own values, e.g. by PropertyValues.ToObject()
            if (property is IComplexProperty { IsCollection: true })
            {
                return;
            }
        }
        // A JSON complex property declared on a complex type mapped to columns (table splitting) is assigned with the
        // other properties of that complex type, so that the containing complex property is set to the complete instance.
        // The placeholder is replaced by the shaper with the materialization from the JSON column.
        else if (property is IComplexProperty
                 {
                     ComplexType: var complexType,
                     DeclaringType: IComplexType declaringComplexType
                 } complexProperty
                 && complexType.IsMappedToJson()
                 && !declaringComplexType.IsMappedToJson())
        {
            var memberInfo = complexProperty.GetMemberInfo(forMaterialization: true, forSet: true);
            blockExpressions.Add(
                Expression.MakeMemberAccess(instanceVariable, memberInfo)
                    .Assign(new NestedJsonComplexPropertyExpression(complexProperty, memberInfo.GetMemberType())));

            return;
        }

        base.AddInitializeExpression(
            property, bindingInfo, instanceVariable, getValueBufferExpression, blockExpressions, nullable);
    }
}

/// <summary>
///     A placeholder for the value of a JSON complex property declared on a complex type mapped to columns (table splitting),
///     replaced by the shaper with the materialization from the JSON column. It reduces to the default value if it isn't replaced.
/// </summary>
internal sealed class NestedJsonComplexPropertyExpression(IComplexProperty complexProperty, Type type) : Expression
{
    public IComplexProperty ComplexProperty { get; } = complexProperty;

    public override Type Type { get; } = type;

    public override ExpressionType NodeType
        => ExpressionType.Extension;

    public override bool CanReduce
        => true;

    public override Expression Reduce()
        => Default(Type);

    protected override Expression VisitChildren(ExpressionVisitor visitor)
        => this;
}
