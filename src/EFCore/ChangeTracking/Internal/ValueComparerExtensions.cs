// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public static class ValueComparerExtensions
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static ValueComparer? ToNullableComparer(this ValueComparer? valueComparer, Type clrType)
        => valueComparer == null
            || !clrType.IsNullableValueType()
            || valueComparer.Type.IsNullableValueType()
                ? valueComparer
                : (ValueComparer)Activator.CreateInstance(
                    typeof(NullableValueComparer<>).MakeGenericType(valueComparer.Type),
                    valueComparer)!;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static ValueComparer? ComposeConversion(this ValueComparer? valueComparer, Type targetClrType)
    {
        if (valueComparer is null || valueComparer.Type == targetClrType)
        {
            return valueComparer;
        }

        if (targetClrType.IsNullableValueType() && valueComparer.Type.IsValueType)
        {
            var nonNullableTargetClrType = targetClrType.UnwrapNullableType();

            // we call ComposeConversion to apply ConvertingValueComparer if necessary
            // for cases where target type and element type differ by nullability AND the base type itself
            // e.g int? vs long
            return (ValueComparer)Activator.CreateInstance(
                typeof(NullableValueComparer<>).MakeGenericType(nonNullableTargetClrType),
                valueComparer.ComposeConversion(nonNullableTargetClrType))!;
        }

        return (ValueComparer)Activator.CreateInstance(
            typeof(ConvertingValueComparer<,>).MakeGenericType(targetClrType, valueComparer.Type),
            valueComparer)!;
    }
}
