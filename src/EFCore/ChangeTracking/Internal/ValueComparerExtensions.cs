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
    public static ValueComparer? ToNullableComparer(this ValueComparer? valueComparer, IReadOnlyProperty property)
    {
        if (valueComparer == null
            || !property.ClrType.IsNullableValueType()
            || valueComparer.Type.IsNullableValueType())
        {
            return valueComparer;
        }

        var newEqualsParam1 = Expression.Parameter(property.ClrType, "v1");
        var newEqualsParam2 = Expression.Parameter(property.ClrType, "v2");
        var newHashCodeParam = Expression.Parameter(property.ClrType, "v");
        var newSnapshotParam = Expression.Parameter(property.ClrType, "v");
        var hasValueMethod = property.ClrType.GetMethod("get_HasValue")!;
        var v1HasValue = Expression.Parameter(typeof(bool), "v1HasValue");
        var v2HasValue = Expression.Parameter(typeof(bool), "v2HasValue");

        return (ValueComparer)Activator.CreateInstance(
            typeof(ValueComparer<>).MakeGenericType(property.ClrType),
            Expression.Lambda(
                Expression.Block(
                    typeof(bool),
                    new[] { v1HasValue, v2HasValue },
                    Expression.Assign(v1HasValue, Expression.Call(newEqualsParam1, hasValueMethod)),
                    Expression.Assign(v2HasValue, Expression.Call(newEqualsParam2, hasValueMethod)),
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
                            Expression.Not(v2HasValue)))),
                newEqualsParam1, newEqualsParam2),
            Expression.Lambda(
                Expression.Condition(
                    Expression.Call(newHashCodeParam, hasValueMethod),
                    valueComparer.ExtractHashCodeBody(
                        Expression.Convert(newHashCodeParam, valueComparer.Type)),
                    Expression.Constant(0, typeof(int))),
                newHashCodeParam),
            Expression.Lambda(
                Expression.Condition(
                    Expression.Call(newSnapshotParam, hasValueMethod),
                    Expression.Convert(
                        valueComparer.ExtractSnapshotBody(
                            Expression.Convert(newSnapshotParam, valueComparer.Type)), property.ClrType),
                    Expression.Default(property.ClrType)),
                newSnapshotParam))!;
    }
}
