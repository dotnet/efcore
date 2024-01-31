// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.EntityFrameworkCore.ChangeTracking;

/// <summary>
///     Specifies custom value snapshotting and comparison for
///     CLR types that cannot be compared with <see cref="object.Equals(object, object)" />
///     and/or need a deep/structural copy when taking a snapshot. For example, arrays of primitive types
///     will require both if mutation is to be detected.
/// </summary>
/// <remarks>
///     <para>
///         Snapshotting is the process of creating a copy of the value into a snapshot so it can
///         later be compared to determine if it has changed. For some types, such as collections,
///         this needs to be a deep copy of the collection rather than just a shallow copy of the
///         reference.
///     </para>
///     <para>
///         See <see href="https://aka.ms/efcore-docs-value-comparers">EF Core value comparers</see> for more information and examples.
///     </para>
/// </remarks>
public abstract class ValueComparer : IEqualityComparer, IEqualityComparer<object>
{
    private static readonly MethodInfo DoubleEqualsMethodInfo
        = typeof(double).GetRuntimeMethod(nameof(double.Equals), [typeof(double)])!;

    private static readonly MethodInfo FloatEqualsMethodInfo
        = typeof(float).GetRuntimeMethod(nameof(float.Equals), [typeof(float)])!;

    internal static readonly MethodInfo EqualityComparerHashCodeMethod
        = typeof(IEqualityComparer).GetRuntimeMethod(nameof(IEqualityComparer.GetHashCode), [typeof(object)])!;

    internal static readonly MethodInfo EqualityComparerEqualsMethod
        = typeof(IEqualityComparer).GetRuntimeMethod(nameof(IEqualityComparer.Equals), [typeof(object), typeof(object)])!;

    internal static readonly MethodInfo ObjectGetHashCodeMethod
        = typeof(object).GetRuntimeMethod(nameof(object.GetHashCode), Type.EmptyTypes)!;

    private static readonly ConcurrentDictionary<Type, MethodInfo> _genericSnapshotMethodMap = new();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public static MethodInfo GetGenericSnapshotMethod(Type type)
        => _genericSnapshotMethodMap.GetOrAdd(type, t =>
            typeof(ValueComparer<>).MakeGenericType(t).GetGenericMethod(
                nameof(ValueComparer<object>.Snapshot),
                genericParameterCount: 0,
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly,
                (a, b) => new[] { a[0] },
                @override: false)!);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    protected static readonly MethodInfo HashCodeAddMethod
        = typeof(ValueComparer).GetRuntimeMethod(nameof(Add), [typeof(HashCode), typeof(int)])!;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    protected static readonly MethodInfo ToHashCodeMethod
        = typeof(HashCode).GetRuntimeMethod(nameof(HashCode.ToHashCode), new Type[0])!;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    protected static readonly Expression<Func<bool, bool>> BoolIdentity;

    static ValueComparer()
    {
        var param = Expression.Parameter(typeof(bool), "v");

        BoolIdentity = (Expression<Func<bool, bool>>)Expression.Lambda(param, param);
    }

    /// <summary>
    ///     Creates a new <see cref="ValueComparer" /> with the given comparison and
    ///     snapshotting expressions.
    /// </summary>
    /// <param name="equalsExpression">The comparison expression.</param>
    /// <param name="hashCodeExpression">The associated hash code generator.</param>
    /// <param name="snapshotExpression">The snapshot expression.</param>
    protected ValueComparer(
        LambdaExpression equalsExpression,
        LambdaExpression hashCodeExpression,
        LambdaExpression snapshotExpression)
    {
        Check.NotNull(equalsExpression, nameof(equalsExpression));
        Check.NotNull(hashCodeExpression, nameof(hashCodeExpression));
        Check.NotNull(snapshotExpression, nameof(snapshotExpression));

        EqualsExpression = equalsExpression;
        HashCodeExpression = hashCodeExpression;
        SnapshotExpression = snapshotExpression;
    }

    /// <summary>
    ///     The type.
    /// </summary>
    public abstract Type Type { get; }

    /// <summary>
    ///     Compares the two instances to determine if they are equal.
    /// </summary>
    /// <param name="left">The first instance.</param>
    /// <param name="right">The second instance.</param>
    /// <returns><see langword="true" /> if they are equal; <see langword="false" /> otherwise.</returns>
    public new abstract bool Equals(object? left, object? right);

    /// <summary>
    ///     Returns the hash code for the given instance.
    /// </summary>
    /// <param name="instance">The instance.</param>
    /// <returns>The hash code.</returns>
    public abstract int GetHashCode(object? instance);

    /// <summary>
    ///     Creates a snapshot of the given instance.
    /// </summary>
    /// <remarks>
    ///     Snapshotting is the process of creating a copy of the value into a snapshot so it can
    ///     later be compared to determine if it has changed. For some types, such as collections,
    ///     this needs to be a deep copy of the collection rather than just a shallow copy of the
    ///     reference.
    /// </remarks>
    /// <param name="instance">The instance.</param>
    /// <returns>The snapshot.</returns>
    [return: NotNullIfNotNull("instance")]
    public abstract object? Snapshot(object? instance);

    /// <summary>
    ///     The comparison expression.
    /// </summary>
    public virtual LambdaExpression EqualsExpression { get; }

    /// <summary>
    ///     The hash code expression.
    /// </summary>
    public virtual LambdaExpression HashCodeExpression { get; }

    /// <summary>
    ///     The snapshot expression.
    /// </summary>
    /// <remarks>
    ///     Snapshotting is the process of creating a copy of the value into a snapshot so it can
    ///     later be compared to determine if it has changed. For some types, such as collections,
    ///     this needs to be a deep copy of the collection rather than just a shallow copy of the
    ///     reference.
    /// </remarks>
    public virtual LambdaExpression SnapshotExpression { get; }

    /// <summary>
    ///     Takes <see cref="EqualsExpression" /> and replaces the two parameters with the given expressions,
    ///     returning the transformed body.
    /// </summary>
    /// <param name="leftExpression">The new left expression.</param>
    /// <param name="rightExpression">The new right expression.</param>
    /// <returns>The body of the lambda with left and right parameters replaced.</returns>
    public virtual Expression ExtractEqualsBody(
        Expression leftExpression,
        Expression rightExpression)
    {
        Check.NotNull(leftExpression, nameof(leftExpression));
        Check.NotNull(rightExpression, nameof(rightExpression));

        var original1 = EqualsExpression.Parameters[0];
        var original2 = EqualsExpression.Parameters[1];

        return new ReplacingExpressionVisitor(
                new Expression[] { original1, original2 }, new[] { leftExpression, rightExpression })
            .Visit(EqualsExpression.Body);
    }

    /// <summary>
    ///     Takes the <see cref="HashCodeExpression" /> and replaces the parameter with the given expression,
    ///     returning the transformed body.
    /// </summary>
    /// <param name="expression">The new expression.</param>
    /// <returns>The body of the lambda with the parameter replaced.</returns>
    public virtual Expression ExtractHashCodeBody(Expression expression)
    {
        Check.NotNull(expression, nameof(expression));

        return ReplacingExpressionVisitor.Replace(
            HashCodeExpression.Parameters[0],
            expression,
            HashCodeExpression.Body);
    }

    /// <summary>
    ///     Takes the <see cref="SnapshotExpression" /> and replaces the parameter with the given expression,
    ///     returning the transformed body.
    /// </summary>
    /// <param name="expression">The new expression.</param>
    /// <returns>The body of the lambda with the parameter replaced.</returns>
    public virtual Expression ExtractSnapshotBody(Expression expression)
    {
        Check.NotNull(expression, nameof(expression));

        return ReplacingExpressionVisitor.Replace(
            SnapshotExpression.Parameters[0],
            expression,
            SnapshotExpression.Body);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public static HashCode Add(HashCode hash, int code)
    {
        hash.Add(code);
        return hash;
    }

    /// <summary>
    ///     Creates a default <see cref="ValueComparer{T}" /> for the given type.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <param name="favorStructuralComparisons">
    ///     If <see langword="true" />, then EF will use <see cref="IStructuralEquatable" /> if the type
    ///     implements it. This is usually used when byte arrays act as keys.
    /// </param>
    /// <returns>The <see cref="ValueComparer{T}" />.</returns>
    public static ValueComparer CreateDefault(
        [DynamicallyAccessedMembers(
            DynamicallyAccessedMemberTypes.PublicMethods
            | DynamicallyAccessedMemberTypes.NonPublicMethods
            | DynamicallyAccessedMemberTypes.PublicProperties)]
        Type type,
        bool favorStructuralComparisons)
        => (ValueComparer)CreateDefaultMethod.MakeGenericMethod(type).Invoke(null, [favorStructuralComparisons])!;

    private static readonly MethodInfo CreateDefaultMethod = typeof(ValueComparer).GetMethod(
        nameof(CreateDefault),
        genericParameterCount: 1,
        BindingFlags.Static | BindingFlags.Public,
        null,
        [typeof(bool)],
        null)!;

    /// <summary>
    ///     Creates a default <see cref="ValueComparer{T}" /> for the given type.
    /// </summary>
    /// <param name="favorStructuralComparisons">
    ///     If <see langword="true" />, then EF will use <see cref="IStructuralEquatable" /> if the type
    ///     implements it. This is usually used when byte arrays act as keys.
    /// </param>
    /// <typeparam name="T">The type.</typeparam>
    /// <returns>The <see cref="ValueComparer{T}" />.</returns>
    public static ValueComparer CreateDefault<T>(bool favorStructuralComparisons)
    {
        var nonNullableType = typeof(T).UnwrapNullableType();

        // The equality operator returns false for NaNs, but the Equals methods returns true
        if (nonNullableType == typeof(double))
        {
            return new DefaultDoubleValueComparer(favorStructuralComparisons);
        }

        if (nonNullableType == typeof(float))
        {
            return new DefaultFloatValueComparer(favorStructuralComparisons);
        }

        if (nonNullableType == typeof(DateTimeOffset))
        {
            return new DefaultDateTimeOffsetValueComparer(favorStructuralComparisons);
        }

        return nonNullableType.IsInteger()
            || nonNullableType == typeof(decimal)
            || nonNullableType == typeof(bool)
            || nonNullableType == typeof(string)
            || nonNullableType == typeof(DateTime)
            || nonNullableType == typeof(DateOnly)
            || nonNullableType == typeof(Guid)
            || nonNullableType == typeof(TimeSpan)
            || nonNullableType == typeof(TimeOnly)
                ? new DefaultValueComparer<T>(favorStructuralComparisons)
                : new ValueComparer<T>(favorStructuralComparisons);
    }

    // PublicMethods is required to preserve e.g. GetHashCode
    internal class DefaultValueComparer<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)] T> : ValueComparer<T>
    {
        public DefaultValueComparer(bool favorStructuralComparisons)
            : base(favorStructuralComparisons)
        {
        }

        public DefaultValueComparer(Expression<Func<T?, T?, bool>> equalsExpression, bool favorStructuralComparisons)
            : base(
                equalsExpression,
                CreateDefaultHashCodeExpression(favorStructuralComparisons),
                CreateDefaultSnapshotExpression(favorStructuralComparisons))
        {
        }

        public override Expression ExtractEqualsBody(Expression leftExpression, Expression rightExpression)
            => Expression.Equal(leftExpression, rightExpression);

        public override Expression ExtractSnapshotBody(Expression expression)
            => expression;

        public override object? Snapshot(object? instance)
            => instance;

        public override T Snapshot(T instance)
            => instance;
    }

    internal sealed class DefaultDoubleValueComparer : DefaultValueComparer<double>
    {
        public DefaultDoubleValueComparer(bool favorStructuralComparisons)
            : base((v1, v2) => v1.Equals(v2), favorStructuralComparisons)
        {
        }

        public override Expression ExtractEqualsBody(Expression leftExpression, Expression rightExpression)
            => Expression.Call(leftExpression, DoubleEqualsMethodInfo, rightExpression);
    }

    internal sealed class DefaultFloatValueComparer : DefaultValueComparer<float>
    {
        public DefaultFloatValueComparer(bool favorStructuralComparisons)
            : base((v1, v2) => v1.Equals(v2), favorStructuralComparisons)
        {
        }

        public override Expression ExtractEqualsBody(Expression leftExpression, Expression rightExpression)
            => Expression.Call(leftExpression, FloatEqualsMethodInfo, rightExpression);
    }

    internal sealed class DefaultDateTimeOffsetValueComparer : DefaultValueComparer<DateTimeOffset>
    {
        private static readonly MethodInfo EqualsExactMethodInfo
            = typeof(DateTimeOffset).GetRuntimeMethod(nameof(DateTimeOffset.EqualsExact), [typeof(DateTimeOffset)])!;

        // In .NET, two DateTimeOffset instances are considered equal if they represent the same point in time but with different
        // time zone offsets. This comparer uses EqualsExact, which considers such DateTimeOffset as non-equal.
        public DefaultDateTimeOffsetValueComparer(bool favorStructuralComparisons)
            : base((v1, v2) => v1.EqualsExact(v2), favorStructuralComparisons)
        {
        }

        public override Expression ExtractEqualsBody(Expression leftExpression, Expression rightExpression)
            => Expression.Call(leftExpression, EqualsExactMethodInfo, rightExpression);
    }
}
