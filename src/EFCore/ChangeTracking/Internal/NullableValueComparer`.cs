// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;

namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class NullableValueComparer
    <[DynamicallyAccessedMembers(
        DynamicallyAccessedMemberTypes.PublicMethods
        | DynamicallyAccessedMemberTypes.PublicProperties)]
    T> : ValueComparer<T?>, IInfrastructure<ValueComparer>
    where T : struct
{
    private static readonly PropertyInfo _hasValueProperty = typeof(T?).GetProperty("HasValue")!;
    private readonly ValueComparer _valueComparer;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public NullableValueComparer(ValueComparer valueComparer)
        : base(
            CreateEquals(valueComparer),
            CreateHashCode(valueComparer),
            CreateSnapshot(valueComparer))
        => _valueComparer = valueComparer;

    private static Expression<Func<T?, T?, bool>> CreateEquals(ValueComparer valueComparer)
    {
        var clrType = typeof(T?);
        var newEqualsParam1 = Expression.Parameter(clrType, "v1");
        var newEqualsParam2 = Expression.Parameter(clrType, "v2");
        var v1HasValue = Expression.MakeMemberAccess(newEqualsParam1, _hasValueProperty);
        var v2HasValue = Expression.MakeMemberAccess(newEqualsParam2, _hasValueProperty);
        return Expression.Lambda<Func<T?, T?, bool>>(
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
            newEqualsParam1, newEqualsParam2);
    }

    private static Expression<Func<T?, int>> CreateHashCode(ValueComparer valueComparer)
    {
        var clrType = typeof(T?);
        var newHashCodeParam = Expression.Parameter(clrType, "v");
        return Expression.Lambda<Func<T?, int>>(
            Expression.Condition(
                Expression.MakeMemberAccess(newHashCodeParam, _hasValueProperty),
                valueComparer.ExtractHashCodeBody(
                    Expression.Convert(newHashCodeParam, valueComparer.Type)),
                Expression.Constant(0, typeof(int))),
            newHashCodeParam);
    }

    private static Expression<Func<T?, T?>> CreateSnapshot(ValueComparer valueComparer)
    {
        var clrType = typeof(T?);
        var newSnapshotParam = Expression.Parameter(clrType, "v");
        return Expression.Lambda<Func<T?, T?>>(
            Expression.Condition(
                Expression.MakeMemberAccess(newSnapshotParam, _hasValueProperty),
                Expression.Convert(
                    valueComparer.ExtractSnapshotBody(
                        Expression.Convert(newSnapshotParam, valueComparer.Type)), clrType),
                Expression.Default(clrType)),
            newSnapshotParam);
    }

    ValueComparer IInfrastructure<ValueComparer>.Instance
        => _valueComparer;
}
