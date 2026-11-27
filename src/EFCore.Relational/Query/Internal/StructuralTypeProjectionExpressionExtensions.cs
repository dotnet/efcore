// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public static class StructuralTypeProjectionExpressionExtensions
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static ColumnExpression BindColumn(this StructuralTypeProjectionExpression projection, IPropertyBase property)
        => property switch
        {
            IProperty p => projection.BindProperty(p),
            IComplexProperty complexProperty => BindComplexProperty(projection, complexProperty) switch
            {
                StructuralTypeShaperExpression { ValueBufferExpression: JsonQueryExpression jsonQuery } => jsonQuery.JsonColumn,
                CollectionResultExpression { QueryExpression: JsonQueryExpression jsonQuery } => jsonQuery.JsonColumn,
                _ => throw new UnreachableException()
            },
            _ => throw new UnreachableException()
        };

    // A JSON complex property can be nested in complex properties mapped to columns (table splitting)
    private static Expression BindComplexProperty(StructuralTypeProjectionExpression projection, IComplexProperty complexProperty)
        => complexProperty.DeclaringType is IComplexType { ComplexProperty: var containingProperty } declaringComplexType
            && !declaringComplexType.IsMappedToJson()
            && declaringComplexType != projection.StructuralType
            && BindComplexProperty(projection, containingProperty) is StructuralTypeShaperExpression
            {
                ValueBufferExpression: StructuralTypeProjectionExpression containingProjection
            }
                ? containingProjection.BindComplexProperty(complexProperty)
                : projection.BindComplexProperty(complexProperty);
}
