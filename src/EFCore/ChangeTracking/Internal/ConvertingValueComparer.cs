// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal;

using System.Diagnostics.CodeAnalysis;
using static Expression;

/// <summary>
///     A composable value comparer that accepts a value comparer, and exposes it as a value comparer for a base type.
///     Used when a collection comparer over e.g. object[] is needed over a specific element type (e.g. int)
/// </summary>
/// <remarks>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </remarks>
public class ConvertingValueComparer
    <[DynamicallyAccessedMembers(
        DynamicallyAccessedMemberTypes.PublicMethods
        | DynamicallyAccessedMemberTypes.PublicProperties)]
    TTo, TFrom> : ValueComparer<TTo>, IInfrastructure<ValueComparer>
{
    private readonly ValueComparer<TFrom> _valueComparer;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public ConvertingValueComparer(ValueComparer<TFrom> valueComparer)
        : base(
            CreateEquals(valueComparer),
            CreateHashCode(valueComparer),
            CreateSnapshot(valueComparer))
        => _valueComparer = valueComparer;

    private static Expression<Func<TTo?, TTo?, bool>> CreateEquals(ValueComparer<TFrom> valueComparer)
    {
        var p1 = Parameter(typeof(TTo), "v1");
        var p2 = Parameter(typeof(TTo), "v2");

        var body = typeof(TTo).IsAssignableFrom(typeof(TFrom))
            ? valueComparer.EqualsExpression.Body
            : valueComparer.ExtractEqualsBody(
                Convert(p1, typeof(TFrom)),
                Convert(p2, typeof(TFrom)));

        return Lambda<Func<TTo?, TTo?, bool>>(
            body,
            p1,
            p2);
    }

    private static Expression<Func<TTo, int>> CreateHashCode(ValueComparer<TFrom> valueComparer)
    {
        var p = Parameter(typeof(TTo), "v");

        var body = typeof(TTo).IsAssignableFrom(typeof(TFrom))
            ? valueComparer.HashCodeExpression.Body
            : valueComparer.ExtractHashCodeBody(
                Convert(p, typeof(TFrom)));

        return Lambda<Func<TTo, int>>(
            body,
            p);
    }

    private static Expression<Func<TTo, TTo>> CreateSnapshot(ValueComparer<TFrom> valueComparer)
    {
        var p = Parameter(typeof(TTo), "v");

        // types must match exactly as we have both covariance and contravariance case here
        var body = typeof(TTo) == typeof(TFrom)
            ? valueComparer.SnapshotExpression.Body
            : Convert(
                valueComparer.ExtractSnapshotBody(
                    Convert(p, typeof(TFrom))),
                typeof(TTo));

        return Lambda<Func<TTo, TTo>>(
            body,
            p);
    }

    ValueComparer IInfrastructure<ValueComparer>.Instance
        => _valueComparer;
}
