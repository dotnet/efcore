// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public static class InMemoryPropertyExtensions
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    /// <remarks>
    ///     A non-nullable property declared on a nullable complex type (directly or through a containing complex type)
    ///     is stored as null when the complex type instance is null.
    /// </remarks>
    public static bool IsStoreNullable(this IProperty property)
        => property.IsNullable
            || (property.DeclaringType is IComplexType complexType
                && IsNullable(complexType.ComplexProperty));

    private static bool IsNullable(IComplexProperty complexProperty)
        => complexProperty.IsNullable
            || (complexProperty.DeclaringType is IComplexType complexType
                && IsNullable(complexType.ComplexProperty));
}
