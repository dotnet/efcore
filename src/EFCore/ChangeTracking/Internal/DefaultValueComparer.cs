// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;

namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
// PublicMethods is required to preserve e.g. GetHashCode
public class DefaultValueComparer
    <[DynamicallyAccessedMembers(
            DynamicallyAccessedMemberTypes.PublicMethods
            | DynamicallyAccessedMemberTypes.PublicProperties)]
        T> : ValueComparer<T>
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static new ValueComparer<T> Default { get; } = new DefaultValueComparer<T>(favorStructuralComparisons: false);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static new ValueComparer<T> DefaultWithStructuralComparisons { get; } =
        (typeof(IStructuralEquatable).IsAssignableFrom(typeof(T)) || typeof(T).IsArray)
            ? new DefaultValueComparer<T>(favorStructuralComparisons: true)
            : Default;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public DefaultValueComparer(bool favorStructuralComparisons)
        : base(favorStructuralComparisons)
    {
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public DefaultValueComparer(Expression<Func<T?, T?, bool>> equalsExpression, bool favorStructuralComparisons)
        : base(
            equalsExpression,
            CreateDefaultHashCodeExpression(favorStructuralComparisons),
            CreateDefaultSnapshotExpression(favorStructuralComparisons))
    {
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override Expression ExtractEqualsBody(Expression leftExpression, Expression rightExpression)
        => Expression.Equal(leftExpression, rightExpression);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override Expression ExtractSnapshotBody(Expression expression)
        => expression;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override object? Snapshot(object? instance)
        => instance;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override T Snapshot(T instance)
        => instance;
}

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
// The equality operator returns false for NaNs, but the Equals method returns true.
public sealed class DefaultDoubleValueComparer(bool favorStructuralComparisons)
    : DefaultValueComparer<double>((v1, v2) => v1.Equals(v2), favorStructuralComparisons)
{
    private static readonly MethodInfo DoubleEqualsMethodInfo
        = typeof(double).GetRuntimeMethod(nameof(double.Equals), [typeof(double)])!;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static new ValueComparer<double> Default { get; } = new DefaultDoubleValueComparer(favorStructuralComparisons: false);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static new ValueComparer<double> DefaultWithStructuralComparisons { get; } = Default;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override Expression ExtractEqualsBody(Expression leftExpression, Expression rightExpression)
        => Expression.Call(leftExpression, DoubleEqualsMethodInfo, rightExpression);
}

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
// The equality operator returns false for NaNs, but the Equals method returns true.
public sealed class DefaultFloatValueComparer(bool favorStructuralComparisons)
    : DefaultValueComparer<float>((v1, v2) => v1.Equals(v2), favorStructuralComparisons)
{
    private static readonly MethodInfo FloatEqualsMethodInfo
        = typeof(float).GetRuntimeMethod(nameof(float.Equals), [typeof(float)])!;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static new ValueComparer<float> Default { get; } = new DefaultFloatValueComparer(favorStructuralComparisons: false);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static new ValueComparer<float> DefaultWithStructuralComparisons { get; } = Default;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override Expression ExtractEqualsBody(Expression leftExpression, Expression rightExpression)
        => Expression.Call(leftExpression, FloatEqualsMethodInfo, rightExpression);
}

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
// In .NET, two DateTimeOffset instances are considered equal if they represent the same point in time but with different
// time zone offsets. This comparer uses EqualsExact, which considers such DateTimeOffset as non-equal.
public sealed class DefaultDateTimeOffsetValueComparer(bool favorStructuralComparisons)
    : DefaultValueComparer<DateTimeOffset>((v1, v2) => v1.EqualsExact(v2), favorStructuralComparisons)
{
    private static readonly MethodInfo EqualsExactMethodInfo
        = typeof(DateTimeOffset).GetRuntimeMethod(nameof(DateTimeOffset.EqualsExact), [typeof(DateTimeOffset)])!;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static new ValueComparer<DateTimeOffset> Default { get; } = new DefaultDateTimeOffsetValueComparer(favorStructuralComparisons: false);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static new ValueComparer<DateTimeOffset> DefaultWithStructuralComparisons { get; } = Default;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override Expression ExtractEqualsBody(Expression leftExpression, Expression rightExpression)
        => Expression.Call(leftExpression, EqualsExactMethodInfo, rightExpression);
}
