// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.ChangeTracking
{
    /// <summary>
    ///     <para>
    ///         Specifies custom value snapshotting and comparison for
    ///         CLR types that cannot be compared with <see cref="object.Equals(object, object)" />
    ///         and/or need a deep/structural copy when taking a snapshot. For example, arrays of primitive types
    ///         will require both if mutation is to be detected.
    ///     </para>
    ///     <para>
    ///         Snapshotting is the process of creating a copy of the value into a snapshot so it can
    ///         later be compared to determine if it has changed. For some types, such as collections,
    ///         this needs to be a deep copy of the collection rather than just a shallow copy of the
    ///         reference.
    ///     </para>
    /// </summary>
    /// <remarks>
    ///     For more information, see <see href="https://aka.ms/efcore-docs-value-comparers">EF Core value comparers</see>.
    /// </remarks>
    public abstract class ValueComparer : IEqualityComparer, IEqualityComparer<object>
    {
        private static readonly MethodInfo _doubleEqualsMethodInfo
            = typeof(double).GetRequiredRuntimeMethod(nameof(double.Equals), new[] { typeof(double) });

        private static readonly MethodInfo _floatEqualsMethodInfo
            = typeof(float).GetRequiredRuntimeMethod(nameof(float.Equals), new[] { typeof(float) });

        internal static readonly MethodInfo ArrayCopyMethod
            = typeof(Array).GetRequiredRuntimeMethod(nameof(Array.Copy), new[] { typeof(Array), typeof(Array), typeof(int) });

        internal static readonly MethodInfo EqualityComparerHashCodeMethod
            = typeof(IEqualityComparer).GetRequiredRuntimeMethod(nameof(IEqualityComparer.GetHashCode), new[] { typeof(object) });

        internal static readonly MethodInfo EqualityComparerEqualsMethod
            = typeof(IEqualityComparer).GetRequiredRuntimeMethod(nameof(IEqualityComparer.Equals), new[] { typeof(object), typeof(object) });

        internal static readonly MethodInfo ObjectEqualsMethod
            = typeof(object).GetRequiredRuntimeMethod(nameof(object.Equals), new[] { typeof(object), typeof(object) });

        internal static readonly MethodInfo ObjectGetHashCodeMethod
            = typeof(object).GetRequiredRuntimeMethod(nameof(object.GetHashCode), Type.EmptyTypes);

        /// <summary>
        ///     Creates a new <see cref="ValueComparer" /> with the given comparison and
        ///     snapshotting expressions.
        /// </summary>
        /// <param name="equalsExpression"> The comparison expression. </param>
        /// <param name="hashCodeExpression"> The associated hash code generator. </param>
        /// <param name="snapshotExpression"> The snapshot expression. </param>
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
        /// <param name="left"> The first instance. </param>
        /// <param name="right"> The second instance. </param>
        /// <returns> <see langword="true" /> if they are equal; <see langword="false" /> otherwise. </returns>
        public new abstract bool Equals(object? left, object? right);

        /// <summary>
        ///     Returns the hash code for the given instance.
        /// </summary>
        /// <param name="instance"> The instance. </param>
        /// <returns> The hash code. </returns>
        public abstract int GetHashCode(object instance);

        /// <summary>
        ///     <para>
        ///         Creates a snapshot of the given instance.
        ///     </para>
        ///     <para>
        ///         Snapshotting is the process of creating a copy of the value into a snapshot so it can
        ///         later be compared to determine if it has changed. For some types, such as collections,
        ///         this needs to be a deep copy of the collection rather than just a shallow copy of the
        ///         reference.
        ///     </para>
        /// </summary>
        /// <param name="instance"> The instance. </param>
        /// <returns> The snapshot. </returns>
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
        ///     <para>
        ///         The snapshot expression.
        ///     </para>
        ///     <para>
        ///         Snapshotting is the process of creating a copy of the value into a snapshot so it can
        ///         later be compared to determine if it has changed. For some types, such as collections,
        ///         this needs to be a deep copy of the collection rather than just a shallow copy of the
        ///         reference.
        ///     </para>
        /// </summary>
        public virtual LambdaExpression SnapshotExpression { get; }

        /// <summary>
        ///     Takes <see cref="EqualsExpression" /> and replaces the two parameters with the given expressions,
        ///     returning the transformed body.
        /// </summary>
        /// <param name="leftExpression"> The new left expression. </param>
        /// <param name="rightExpression"> The new right expression. </param>
        /// <returns> The body of the lambda with left and right parameters replaced.</returns>
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
        /// <param name="expression"> The new expression. </param>
        /// <returns> The body of the lambda with the parameter replaced.</returns>
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
        /// <param name="expression"> The new expression. </param>
        /// <returns> The body of the lambda with the parameter replaced.</returns>
        public virtual Expression ExtractSnapshotBody(Expression expression)
        {
            Check.NotNull(expression, nameof(expression));

            return ReplacingExpressionVisitor.Replace(
                SnapshotExpression.Parameters[0],
                expression,
                SnapshotExpression.Body);
        }

        /// <summary>
        ///     Creates a default <see cref="ValueComparer{T}" /> for the given type.
        /// </summary>
        /// <param name="type"> The type. </param>
        /// <param name="favorStructuralComparisons">
        ///     If <see langword="true" />, then EF will use <see cref="IStructuralEquatable" /> if the type
        ///     implements it. This is usually used when byte arrays act as keys.
        /// </param>
        /// <returns> The <see cref="ValueComparer{T}" />. </returns>
        public static ValueComparer CreateDefault(Type type, bool favorStructuralComparisons)
        {
            var nonNullabletype = type.UnwrapNullableType();

            // The equality operator returns false for NaNs, but the Equals methods returns true
            if (nonNullabletype == typeof(double))
            {
                return new DefaultDoubleValueComparer(favorStructuralComparisons);
            }

            if (nonNullabletype == typeof(float))
            {
                return new DefaultFloatValueComparer(favorStructuralComparisons);
            }

            if (nonNullabletype == typeof(DateTimeOffset))
            {
                return new DefaultDateTimeOffsetValueComparer(favorStructuralComparisons);
            }

            var comparerType = nonNullabletype.IsInteger()
                || nonNullabletype == typeof(decimal)
                || nonNullabletype == typeof(bool)
                || nonNullabletype == typeof(string)
                || nonNullabletype == typeof(DateTime)
                || nonNullabletype == typeof(Guid)
                || nonNullabletype == typeof(TimeSpan)
                    ? typeof(DefaultValueComparer<>)
                    : typeof(ValueComparer<>);

            return (ValueComparer)Activator.CreateInstance(
                comparerType.MakeGenericType(type),
                new object[] { favorStructuralComparisons })!;
        }

        internal class DefaultValueComparer<T> : ValueComparer<T>
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

            public override T? Snapshot(T? instance)
                => instance;
        }

        internal sealed class DefaultDoubleValueComparer : DefaultValueComparer<double>
        {
            public DefaultDoubleValueComparer(bool favorStructuralComparisons)
                : base((v1, v2) => v1.Equals(v2), favorStructuralComparisons)
            {
            }

            public override Expression ExtractEqualsBody(Expression leftExpression, Expression rightExpression)
                => Expression.Call(leftExpression, _doubleEqualsMethodInfo, rightExpression);
        }

        internal sealed class DefaultFloatValueComparer : DefaultValueComparer<float>
        {
            public DefaultFloatValueComparer(bool favorStructuralComparisons)
                : base((v1, v2) => v1.Equals(v2), favorStructuralComparisons)
            {
            }

            public override Expression ExtractEqualsBody(Expression leftExpression, Expression rightExpression)
                => Expression.Call(leftExpression, _floatEqualsMethodInfo, rightExpression);
        }

        internal sealed class DefaultDateTimeOffsetValueComparer : DefaultValueComparer<DateTimeOffset>
        {
            private static readonly PropertyInfo _offsetPropertyInfo = typeof(DateTimeOffset).GetProperty(nameof(DateTimeOffset.Offset))!;

            // In .NET, two DateTimeOffset instances are considered equal if they represent the same point in time but with different
            // time zone offsets. This comparer considers such DateTimeOffset as non-equal.
            public DefaultDateTimeOffsetValueComparer(bool favorStructuralComparisons)
                : base((v1, v2) => v1 == v2 && v1.Offset == v2.Offset, favorStructuralComparisons)
            {
            }

            public override Expression ExtractEqualsBody(Expression leftExpression, Expression rightExpression)
                => Expression.And(
                    Expression.Equal(leftExpression, rightExpression),
                    Expression.Equal(
                        Expression.Property(leftExpression, _offsetPropertyInfo),
                        Expression.Property(rightExpression, _offsetPropertyInfo)));
        }
    }
}
