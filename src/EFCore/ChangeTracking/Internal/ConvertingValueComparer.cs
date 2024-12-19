// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal;

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
public class ConvertingValueComparer<T, TBase> : ValueComparer<TBase>, IInfrastructure<ValueComparer>
    where T : TBase
{
    private readonly ValueComparer _valueComparer;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public ConvertingValueComparer(ValueComparer valueComparer)
        : base(
            CreateEquals(valueComparer),
            CreateHashCode(valueComparer),
            CreateSnapshot(valueComparer))
        => _valueComparer = valueComparer;

    private static Expression<Func<TBase?, TBase?, bool>> CreateEquals(ValueComparer valueComparer)
    {
        var p1 = Parameter(typeof(TBase), "v1");
        var p2 = Parameter(typeof(TBase), "v2");
        var body = valueComparer.ExtractEqualsBody(Convert(p1, valueComparer.Type), Convert(p2, valueComparer.Type));
        var nullConstant = Constant(null, typeof(TBase));

        return Lambda<Func<TBase?, TBase?, bool>>(
            typeof(TBase).IsNullableType() && !typeof(T).IsNullableType()
                ? OrElse(
                    AndAlso(
                        NotEqual(p1, nullConstant),
                        AndAlso(
                            NotEqual(p2, nullConstant),
                            body)),
                    AndAlso(
                        Equal(p1, nullConstant),
                        Equal(p2, nullConstant)))
                : body,
            p1, p2);

        // var p1 = Parameter(typeof(TBase), "v1");
        // var p2 = Parameter(typeof(TBase), "v2");
        // var body = valueComparer.ExtractEqualsBody(Convert(p1, valueComparer.Type), Convert(p2, valueComparer.Type));
        // var p1IsNull = Equal(p1, Constant(null, typeof(TBase)));
        // var p2IsNull = Equal(p2, Constant(null, typeof(TBase)));
        //
        // return Lambda<Func<TBase?, TBase?, bool>>(
        //     typeof(TBase).IsNullableType() && !typeof(T).IsNullableType()
        //         ? OrElse(
        //             AndAlso(
        //                 Not(p1IsNull),
        //                 AndAlso(
        //                     Not(p2IsNull),
        //                     body)),
        //             AndAlso(
        //                 p1IsNull,
        //                 p2IsNull))
        //         : body,
        //     p1, p2);
    }

    private static Expression<Func<TBase, int>> CreateHashCode(ValueComparer valueComparer)
    {
        var p = Parameter(typeof(TBase), "v");
        var body = valueComparer.ExtractHashCodeBody(Convert(p, typeof(T)));

        return Lambda<Func<TBase, int>>(
            typeof(TBase).IsNullableType() && !typeof(T).IsNullableType()
                ? Condition(Equal(p, Constant(null, typeof(TBase))), Constant(0, typeof(int)), body)
                : body,
            p);
    }

    private static Expression<Func<TBase, TBase>> CreateSnapshot(ValueComparer valueComparer)
    {
        var p = Parameter(typeof(TBase), "v");
        var body = Convert(valueComparer.ExtractSnapshotBody(Convert(p, valueComparer.Type)), typeof(TBase));
        var nullConstant = Constant(null, typeof(TBase));

        return Lambda<Func<TBase, TBase>>(
            typeof(TBase).IsNullableType() && !typeof(T).IsNullableType()
                ? Condition(Equal(p, nullConstant), nullConstant, body)
                : body,
            p);
    }

    ValueComparer IInfrastructure<ValueComparer>.Instance
        => _valueComparer;
}
