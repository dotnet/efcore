// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using static System.Linq.Expressions.Expression;

namespace Microsoft.EntityFrameworkCore.Cosmos.ChangeTracking.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public sealed class SingleDimensionalArrayComparer<TElement> : ValueComparer<TElement[]>
{
    internal static readonly PropertyInfo ArrayLengthProperty
        = typeof(Array).GetRuntimeProperty(nameof(Array.Length))!;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SingleDimensionalArrayComparer(ValueComparer elementComparer)
        : base(
            CreateEqualsExpression(elementComparer),
            CreateHashCodeExpression(elementComparer),
            CreateSnapshotExpression(elementComparer))
    {
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override Type Type
        => typeof(TElement[]);

    private static Expression<Func<TElement[]?, TElement[]?, bool>> CreateEqualsExpression(ValueComparer elementComparer)
    {
        var type = typeof(TElement[]);
        var param1 = Parameter(type, "v1");
        var param2 = Parameter(type, "v2");

        return Lambda<Func<TElement[]?, TElement[]?, bool>>(
            Condition(
                Equal(param1, Constant(null, type)),
                Equal(param2, Constant(null, type)),
                AndAlso(
                    NotEqual(param2, Constant(null, type)),
                    AndAlso(
                        Equal(MakeMemberAccess(param1, ArrayLengthProperty), MakeMemberAccess(param2, ArrayLengthProperty)),
                        OrElse(
                            ReferenceEqual(param1, param2),
                            Call(
                                EnumerableMethods.All.MakeGenericMethod(typeof(bool)),
                                Call(
                                    EnumerableMethods.ZipWithSelector.MakeGenericMethod(typeof(TElement), typeof(TElement), typeof(bool)),
                                    param1,
                                    param2,
                                    elementComparer.EqualsExpression),
#pragma warning disable EF1001 // Internal EF Core API usage.
                                BoolIdentity))))),
#pragma warning restore EF1001 // Internal EF Core API usage.
            param1, param2);
    }

    private static Expression<Func<TElement[], int>> CreateHashCodeExpression(ValueComparer elementComparer)
    {
        var elementType = typeof(TElement);
        var param = Parameter(typeof(TElement[]), "v");

        var aggregateParam = Parameter(typeof(HashCode), "h");
        var aggregateElementParam = Parameter(elementType, "e");
#pragma warning disable EF1001 // Internal EF Core API usage.
        var aggregateFunc = Lambda<Func<HashCode, TElement, HashCode>>(
            Call(HashCodeAddMethod, aggregateParam, elementComparer.ExtractHashCodeBody(aggregateElementParam)),
            aggregateParam, aggregateElementParam);

        var selector = Lambda<Func<HashCode, int>>(
            Call(aggregateParam, ToHashCodeMethod),
            aggregateParam);
#pragma warning restore EF1001 // Internal EF Core API usage.

        return Lambda<Func<TElement[], int>>(
            Call(
                EnumerableMethods.AggregateWithSeedSelector.MakeGenericMethod(elementType, typeof(HashCode), typeof(int)),
                param,
                New(typeof(HashCode)),
                aggregateFunc,
                selector),
            param);
    }

    private static Expression<Func<TElement[], TElement[]>> CreateSnapshotExpression(ValueComparer elementComparer)
    {
        var elementType = typeof(TElement);
        var param = Parameter(typeof(TElement[]), "v");

        var elementParam = Parameter(elementType, "e");

        var selector = elementType.IsValueType
            ? elementComparer.SnapshotExpression
            : Lambda<Func<TElement, TElement>>(
                Condition(
                    Equal(elementParam, Constant(null, elementType)),
                    Constant(null, elementType),
                    elementComparer.ExtractSnapshotBody(elementParam)),
                elementParam);

        return Lambda<Func<TElement[], TElement[]>>(
            Call(
                EnumerableMethods.ToArray.MakeGenericMethod(elementType),
                Call(
                    EnumerableMethods.Select.MakeGenericMethod(elementType, elementType),
                    param,
                    selector)),
            param);
    }
}
