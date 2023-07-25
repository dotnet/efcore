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
    {
        if (valueComparer == null
            || !clrType.IsNullableValueType()
            || valueComparer.Type.IsNullableValueType())
        {
            return valueComparer;
        }

        var newEqualsParam1 = Expression.Parameter(clrType, "v1");
        var newEqualsParam2 = Expression.Parameter(clrType, "v2");
        var newHashCodeParam = Expression.Parameter(clrType, "v");
        var newSnapshotParam = Expression.Parameter(clrType, "v");
        var hasValueProperty = clrType.GetProperty("HasValue")!;
        var v1HasValue = Expression.MakeMemberAccess(newEqualsParam1, hasValueProperty);
        var v2HasValue = Expression.MakeMemberAccess(newEqualsParam2, hasValueProperty);

        return (ValueComparer)Activator.CreateInstance(
            typeof(ValueComparer<>).MakeGenericType(clrType),
            Expression.Lambda(
                Expression.OrElse(
                    Expression.AndAlso(
                        v1HasValue,
                        Expression.AndAlso(
                            v2HasValue,
                            valueComparer.ExtractEqualsBody(
                                Expression.Convert(newEqualsParam1, valueComparer.Type),
                                Expression.Convert(newEqualsParam2, valueComparer.Type)))),
                    Expression.AndAlso(
                        Expression.Not(v1HasValue),
                        Expression.Not(v2HasValue))),
                newEqualsParam1, newEqualsParam2),
            Expression.Lambda(
                Expression.Condition(
                    Expression.MakeMemberAccess(newHashCodeParam, hasValueProperty),
                    valueComparer.ExtractHashCodeBody(
                        Expression.Convert(newHashCodeParam, valueComparer.Type)),
                    Expression.Constant(0, typeof(int))),
                newHashCodeParam),
            Expression.Lambda(
                Expression.Condition(
                    Expression.MakeMemberAccess(newSnapshotParam, hasValueProperty),
                    Expression.Convert(
                        valueComparer.ExtractSnapshotBody(
                            Expression.Convert(newSnapshotParam, valueComparer.Type)), clrType),
                    Expression.Default(clrType)),
                newSnapshotParam))!;
    }
}
