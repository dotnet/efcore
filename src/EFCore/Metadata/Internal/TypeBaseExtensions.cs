// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public static class TypeBaseExtensions
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static bool UseEagerSnapshots(this IReadOnlyTypeBase complexType)
        => complexType.GetChangeTrackingStrategy() is ChangeTrackingStrategy.Snapshot or ChangeTrackingStrategy.ChangedNotifications;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static string ShortNameChain(this IReadOnlyTypeBase structuralType)
        => (structuralType is IReadOnlyComplexType complexType) && (complexType.ComplexProperty is IReadOnlyComplexProperty complexProperty)
            ? complexProperty.DeclaringType.ShortNameChain() + (complexProperty.IsCollection ? "[]" : ".") + structuralType.ShortName()
            : structuralType.ShortName();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static T CheckContains<T>(this IReadOnlyTypeBase structuralType, T property)
        where T : IReadOnlyPropertyBase
    {
        Check.NotNull(property);

        return !property.DeclaringType.IsAssignableFrom(structuralType) && !property.DeclaringType.ContainingType.IsAssignableFrom(structuralType)
            ? throw new InvalidOperationException(
                CoreStrings.PropertyDoesNotBelong(property.Name, property.DeclaringType.DisplayName(), structuralType.DisplayName()))
            : property;
    }
}
