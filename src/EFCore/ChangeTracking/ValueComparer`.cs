// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Internal;
using ExpressionExtensions = Microsoft.EntityFrameworkCore.Infrastructure.ExpressionExtensions;

namespace Microsoft.EntityFrameworkCore.ChangeTracking;

/// <summary>
///     Specifies custom value snapshotting and comparison for
///     CLR types that cannot be compared with <see cref="object.Equals(object?, object?)" />
///     and/or need a deep copy when taking a snapshot. For example, arrays of primitive types
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
/// <typeparam name="T">The type.</typeparam>
// PublicMethods is required to preserve e.g. GetHashCode
public class ValueComparer
    <[DynamicallyAccessedMembers(
            DynamicallyAccessedMemberTypes.PublicMethods
            | DynamicallyAccessedMemberTypes.NonPublicMethods
            | DynamicallyAccessedMemberTypes.PublicProperties)]
        T>
    : ValueComparer, IEqualityComparer<T>
{
    private Func<T?, T?, bool>? _equals;
    private Func<T, int>? _hashCode;
    private Func<T, T>? _snapshot;

    /// <summary>
    ///     Creates a new <see cref="ValueComparer{T}" /> with a default comparison
    ///     expression and a shallow copy for the snapshot.
    /// </summary>
    /// <param name="favorStructuralComparisons">
    ///     If <see langword="true" />, then EF will use <see cref="IStructuralEquatable" /> if the type
    ///     implements it. This is usually used when byte arrays act as keys.
    /// </param>
    public ValueComparer(bool favorStructuralComparisons)
        : this(
            CreateDefaultEqualsExpression(),
            CreateDefaultHashCodeExpression(favorStructuralComparisons),
            CreateDefaultSnapshotExpression(favorStructuralComparisons))
    {
    }

    /// <summary>
    ///     Creates a new <see cref="ValueComparer{T}" /> with the given comparison expression.
    ///     A shallow copy will be used for the snapshot.
    /// </summary>
    /// <param name="equalsExpression">The comparison expression.</param>
    /// <param name="hashCodeExpression">The associated hash code generator.</param>
    public ValueComparer(
        Expression<Func<T?, T?, bool>> equalsExpression,
        Expression<Func<T, int>> hashCodeExpression)
        : this(equalsExpression, hashCodeExpression, CreateDefaultSnapshotExpression(false))
    {
    }

    /// <summary>
    ///     Creates a new <see cref="ValueComparer{T}" /> with the given comparison and
    ///     snapshotting expressions.
    /// </summary>
    /// <remarks>
    ///     Snapshotting is the process of creating a copy of the value into a snapshot so it can
    ///     later be compared to determine if it has changed. For some types, such as collections,
    ///     this needs to be a deep copy of the collection rather than just a shallow copy of the
    ///     reference.
    /// </remarks>
    /// <param name="equalsExpression">The comparison expression.</param>
    /// <param name="hashCodeExpression">The associated hash code generator.</param>
    /// <param name="snapshotExpression">The snapshot expression.</param>
    public ValueComparer(
        Expression<Func<T?, T?, bool>> equalsExpression,
        Expression<Func<T, int>> hashCodeExpression,
        Expression<Func<T, T>> snapshotExpression)
        : base(equalsExpression, hashCodeExpression, snapshotExpression)
    {
    }

    /// <summary>
    ///     Creates an expression for equality.
    /// </summary>
    /// <returns>The equality expression.</returns>
    protected static Expression<Func<T?, T?, bool>> CreateDefaultEqualsExpression()
    {
        var type = typeof(T);
        var param1 = Expression.Parameter(type, "v1");
        var param2 = Expression.Parameter(type, "v2");

        // We exclude multi-dimensional arrays even though they're IStructuralEquatable because of
        // https://github.com/dotnet/runtime/issues/66472
        if (typeof(IStructuralEquatable).IsAssignableFrom(type))
        {
            return Expression.Lambda<Func<T?, T?, bool>>(
                Expression.Call(
                    Expression.Constant(StructuralComparisons.StructuralEqualityComparer, typeof(IEqualityComparer)),
                    EqualityComparerEqualsMethod,
                    Expression.Convert(param1, typeof(object)),
                    Expression.Convert(param2, typeof(object))
                ),
                param1, param2);
        }

        var unwrappedType = type.UnwrapNullableType();
        if (unwrappedType.IsInteger()
            || unwrappedType == typeof(string)
            || unwrappedType == typeof(Guid)
            || unwrappedType == typeof(bool)
            || unwrappedType == typeof(decimal)
            || unwrappedType == typeof(object))
        {
            return Expression.Lambda<Func<T?, T?, bool>>(
                Expression.Equal(param1, param2),
                param1, param2);
        }

        var typedEquals = type.GetRuntimeMethods().FirstOrDefault(
            m => m.ReturnType == typeof(bool)
                && !m.IsStatic
                && nameof(object.Equals).Equals(m.Name, StringComparison.Ordinal)
                && m.GetParameters().Length == 1
                && m.GetParameters()[0].ParameterType == typeof(T));

        if (typedEquals != null)
        {
            return Expression.Lambda<Func<T?, T?, bool>>(
                type.IsNullableType()
                    ? Expression.OrElse(
                        Expression.AndAlso(
                            Expression.Equal(param1, Expression.Constant(null, type)),
                            Expression.Equal(param2, Expression.Constant(null, type))),
                        Expression.AndAlso(
                            Expression.AndAlso(
                                Expression.NotEqual(param1, Expression.Constant(null, type)),
                                Expression.NotEqual(param2, Expression.Constant(null, type))),
                            Expression.Call(param1, typedEquals, param2)))
                    : Expression.Call(param1, typedEquals, param2),
                param1, param2);
        }

        while (typedEquals == null
               && type != null)
        {
            var declaredMethods = type.GetTypeInfo().DeclaredMethods;
            typedEquals = declaredMethods.FirstOrDefault(
                m => m.IsStatic
                    && m.ReturnType == typeof(bool)
                    && "op_Equality".Equals(m.Name, StringComparison.Ordinal)
                    && m.GetParameters().Length == 2
                    && m.GetParameters()[0].ParameterType == typeof(T)
                    && m.GetParameters()[1].ParameterType == typeof(T));

            type = type.BaseType;
        }

        return Expression.Lambda<Func<T?, T?, bool>>(
            typedEquals == null
                ? ExpressionExtensions.CreateEqualsExpression(param1, param2)
                : Expression.Call(typedEquals, param1, param2),
            param1, param2);
    }

    /// <summary>
    ///     Creates an expression for creating a snapshot of a value.
    /// </summary>
    /// <returns>The snapshot expression.</returns>
    protected static Expression<Func<T, T>> CreateDefaultSnapshotExpression(bool favorStructuralComparisons)
    {
        if (!favorStructuralComparisons
            || !typeof(T).IsArray
            || typeof(T).GetArrayRank() != 1)
        {
            return v => v;
        }

        var sourceParameter = Expression.Parameter(typeof(T), "source");
        return Expression.Lambda<Func<T, T>>(
            Expression.Call(
                EnumerableMethods.ToArray.MakeGenericMethod(typeof(T).GetElementType()!),
                sourceParameter),
            sourceParameter);
    }

    /// <summary>
    ///     Creates an expression for generating a hash code.
    /// </summary>
    /// <param name="favorStructuralComparisons">
    ///     If <see langword="true" />, then <see cref="IStructuralEquatable" /> is used if the type implements it.
    /// </param>
    /// <returns>The hash code expression.</returns>
    protected static Expression<Func<T, int>> CreateDefaultHashCodeExpression(bool favorStructuralComparisons)
    {
        var type = typeof(T);
        var unwrappedType = type.UnwrapNullableType();
        var param = Expression.Parameter(type, "v");

        if (favorStructuralComparisons
            && typeof(IStructuralEquatable).IsAssignableFrom(type))
        {
            return Expression.Lambda<Func<T, int>>(
                Expression.Call(
                    Expression.Constant(StructuralComparisons.StructuralEqualityComparer, typeof(IEqualityComparer)),
                    EqualityComparerHashCodeMethod,
                    Expression.Convert(param, typeof(object))
                ),
                param);
        }

        var expression
            = type == typeof(int)
                ? param
                : unwrappedType == typeof(int)
                || unwrappedType == typeof(short)
                || unwrappedType == typeof(byte)
                || unwrappedType == typeof(uint)
                || unwrappedType == typeof(ushort)
                || unwrappedType == typeof(sbyte)
                || unwrappedType == typeof(char)
                    ? (Expression)Expression.Convert(param, typeof(int))
                    : Expression.Call(param, ObjectGetHashCodeMethod);

        return Expression.Lambda<Func<T, int>>(expression, param);
    }

    /// <summary>
    ///     Compares the two instances to determine if they are equal.
    /// </summary>
    /// <param name="left">The first instance.</param>
    /// <param name="right">The second instance.</param>
    /// <returns><see langword="true" /> if they are equal; <see langword="false" /> otherwise.</returns>
    public override bool Equals(object? left, object? right)
    {
        var v1Null = left == null;
        var v2Null = right == null;

        return v1Null || v2Null ? v1Null && v2Null : Equals((T?)left, (T?)right);
    }

    /// <summary>
    ///     Returns the hash code for the given instance.
    /// </summary>
    /// <param name="instance">The instance.</param>
    /// <returns>The hash code.</returns>
    public override int GetHashCode(object? instance)
        => instance is null ? 0 : GetHashCode((T)instance);

    /// <summary>
    ///     Compares the two instances to determine if they are equal.
    /// </summary>
    /// <param name="left">The first instance.</param>
    /// <param name="right">The second instance.</param>
    /// <returns><see langword="true" /> if they are equal; <see langword="false" /> otherwise.</returns>
    public virtual bool Equals(T? left, T? right)
        => NonCapturingLazyInitializer.EnsureInitialized(
            ref _equals, this, static c => c.EqualsExpression.Compile())(left, right);

    /// <summary>
    ///     Returns the hash code for the given instance.
    /// </summary>
    /// <param name="instance">The instance.</param>
    /// <returns>The hash code.</returns>
    public virtual int GetHashCode(T instance)
        => NonCapturingLazyInitializer.EnsureInitialized(
            ref _hashCode, this, static c => c.HashCodeExpression.Compile())(instance);

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
    public override object? Snapshot(object? instance)
        => instance == null ? null : Snapshot((T)instance);

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
    public virtual T Snapshot(T instance)
        => NonCapturingLazyInitializer.EnsureInitialized(
            ref _snapshot, this, static c => c.SnapshotExpression.Compile())(instance);

    /// <summary>
    ///     The type.
    /// </summary>
    public override Type Type
        => typeof(T);

    /// <summary>
    ///     The comparison expression.
    /// </summary>
    public new virtual Expression<Func<T?, T?, bool>> EqualsExpression
        => (Expression<Func<T?, T?, bool>>)base.EqualsExpression;

    /// <summary>
    ///     The hash code expression.
    /// </summary>
    public new virtual Expression<Func<T, int>> HashCodeExpression
        => (Expression<Func<T, int>>)base.HashCodeExpression;

    /// <summary>
    ///     The snapshot expression.
    /// </summary>
    /// <remarks>
    ///     Snapshotting is the process of creating a copy of the value into a snapshot so it can
    ///     later be compared to determine if it has changed. For some types, such as collections,
    ///     this needs to be a deep copy of the collection rather than just a shallow copy of the
    ///     reference.
    /// </remarks>
    public new virtual Expression<Func<T, T>> SnapshotExpression
        => (Expression<Func<T, T>>)base.SnapshotExpression;
}
