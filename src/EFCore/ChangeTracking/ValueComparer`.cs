// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;

namespace Microsoft.EntityFrameworkCore.ChangeTracking
{
    /// <summary>
    ///     <para>
    ///         Specifies custom value snapshotting and comparison for
    ///         CLR types that cannot be compared with <see cref="object.Equals(object, object)" />
    ///         and/or need a deep copy when taking a snapshot. For example, arrays of primitive types
    ///         will require both if mutation is to be detected.
    ///     </para>
    ///     <para>
    ///         Snapshotting is the process of creating a copy of the value into a snapshot so it can
    ///         later be compared to determine if it has changed. For some types, such as collections,
    ///         this needs to be a deep copy of the collection rather than just a shallow copy of the
    ///         reference.
    ///     </para>
    /// </summary>
    /// <typeparam name="T"> The type. </typeparam>
    public class ValueComparer<T> : ValueComparer
    {
        private Func<object, object, bool> _equals;
        private Func<object, int> _hashCode;
        private Func<object, object> _snapshotFunc;

        /// <summary>
        ///     Creates a new <see cref="ValueComparer{T}" /> with a default comparison
        ///     expression and a shallow copy for the snapshot.
        /// </summary>
        /// <param name="favorStructuralComparisons">
        ///     If <c>true</c>, then EF will use <see cref="IStructuralEquatable" /> if the type
        ///     implements it. This is usually used when byte arrays act as keys.
        /// </param>
        public ValueComparer(bool favorStructuralComparisons)
            : this(
                CreateDefaultEqualsExpression(favorStructuralComparisons),
                CreateDefaultHashCodeExpression(favorStructuralComparisons))
        {
        }

        /// <summary>
        ///     Creates a new <see cref="ValueComparer{T}" /> with the given comparison expression.
        ///     A shallow copy will be used for the snapshot.
        /// </summary>
        /// <param name="equalsExpression"> The comparison expression. </param>
        /// <param name="hashCodeExpression"> The associated hash code generator. </param>
        public ValueComparer(
            [NotNull] Expression<Func<T, T, bool>> equalsExpression,
            [NotNull] Expression<Func<T, int>> hashCodeExpression)
            : this(equalsExpression, hashCodeExpression, v => v)
        {
        }

        /// <summary>
        ///     <para>
        ///         Creates a new <see cref="ValueComparer{T}" /> with the given comparison and
        ///         snapshotting expressions.
        ///     </para>
        ///     <para>
        ///         Snapshotting is the process of creating a copy of the value into a snapshot so it can
        ///         later be compared to determine if it has changed. For some types, such as collections,
        ///         this needs to be a deep copy of the collection rather than just a shallow copy of the
        ///         reference.
        ///     </para>
        /// </summary>
        /// <param name="equalsExpression"> The comparison expression. </param>
        /// <param name="hashCodeExpression"> The associated hash code generator. </param>
        /// <param name="snapshotExpression"> The snapshot expression. </param>
        public ValueComparer(
            [NotNull] Expression<Func<T, T, bool>> equalsExpression,
            [NotNull] Expression<Func<T, int>> hashCodeExpression,
            [NotNull] Expression<Func<T, T>> snapshotExpression)
            : base(equalsExpression, hashCodeExpression, snapshotExpression)
        {
        }

        private static Expression<Func<T, T, bool>> CreateDefaultEqualsExpression(bool favorStructuralComparisons)
        {
            var type = typeof(T);
            var param1 = Expression.Parameter(type, "v1");
            var param2 = Expression.Parameter(type, "v2");

            if (favorStructuralComparisons
                && typeof(IStructuralEquatable).GetTypeInfo().IsAssignableFrom(type.GetTypeInfo()))
            {
                return Expression.Lambda<Func<T, T, bool>>(
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
                || unwrappedType == typeof(double)
                || unwrappedType == typeof(float)
                || unwrappedType == typeof(object)
            )
            {
                return Expression.Lambda<Func<T, T, bool>>(
                    Expression.Equal(param1, param2),
                    param1, param2);
            }

            var typedEquals = type.GetRuntimeMethods().FirstOrDefault(
                m => m.ReturnType == typeof(bool)
                     && !m.IsStatic
                     && nameof(object.Equals).Equals(m.Name, StringComparison.Ordinal)
                     && m.GetParameters().Length == 1
                     && m.GetParameters()[0].ParameterType == typeof(T));

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

            return Expression.Lambda<Func<T, T, bool>>(
                typedEquals == null
                    ? Expression.Call(
                        ObjectEqualsMethod,
                        Expression.Convert(param1, typeof(object)),
                        Expression.Convert(param2, typeof(object)))
                    : typedEquals.IsStatic
                        ? Expression.Call(typedEquals, param1, param2)
                        : Expression.Call(param1, typedEquals, param2),
                param1, param2);
        }

        private static Expression<Func<T, int>> CreateDefaultHashCodeExpression(bool favorStructuralComparisons)
        {
            var type = typeof(T);
            var unwrappedType = type.UnwrapNullableType();
            var param = Expression.Parameter(type, "v");

            if (favorStructuralComparisons
                && typeof(IStructuralEquatable).GetTypeInfo().IsAssignableFrom(type.GetTypeInfo()))
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
                        : Expression.Call(
                            Expression.Convert(param, typeof(object)), ObjectGetHashCodeMethod);

            return Expression.Lambda<Func<T, int>>(expression, param);
        }

        /// <summary>
        ///     The comparison expression compiled into an untyped delegate.
        /// </summary>
        public override Func<object, object, bool> Equals
            => NonCapturingLazyInitializer.EnsureInitialized(
                ref _equals, this, c => HandleNulls(c.EqualsExpression));

        /// <summary>
        ///     The hash code expression compiled into an untyped delegate.
        /// </summary>
        public override Func<object, int> HashCode
            => NonCapturingLazyInitializer.EnsureInitialized(
                ref _hashCode, this, c => HandleNulls(c.HashCodeExpression));

        private static Func<object, object, bool> HandleNulls(Expression<Func<T, T, bool>> expression)
        {
            var compiled = expression.Compile();

            return (v1, v2) =>
            {
                var v1Null = v1 == null;
                var v2Null = v2 == null;

                return v1Null || v2Null ? v1Null && v2Null : compiled((T)v1, (T)v2);
            };
        }

        private static Func<object, int> HandleNulls(Expression<Func<T, int>> expression)
        {
            var compiled = expression.Compile();

            return v => v == null ? 0 : compiled((T)v);
        }

        /// <summary>
        ///     <para>
        ///         The snapshot expression compiled into an untyped delegate.
        ///     </para>
        ///     <para>
        ///         Snapshotting is the process of creating a copy of the value into a snapshot so it can
        ///         later be compared to determine if it has changed. For some types, such as collections,
        ///         this needs to be a deep copy of the collection rather than just a shallow copy of the
        ///         reference.
        ///     </para>
        /// </summary>
        public override Func<object, object> Snapshot
            => NonCapturingLazyInitializer.EnsureInitialized(
                ref _snapshotFunc, this, c => Compile(c.SnapshotExpression));

        /// <summary>
        ///     The type.
        /// </summary>
        public override Type Type => typeof(T);

        /// <summary>
        ///     The comparison expression.
        /// </summary>
        public new virtual Expression<Func<T, T, bool>> EqualsExpression
            => (Expression<Func<T, T, bool>>)base.EqualsExpression;

        /// <summary>
        ///     The hash code expression.
        /// </summary>
        public new virtual Expression<Func<T, int>> HashCodeExpression
            => (Expression<Func<T, int>>)base.HashCodeExpression;

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
        public new virtual Expression<Func<T, T>> SnapshotExpression
            => (Expression<Func<T, T>>)base.SnapshotExpression;

        private static Func<object, object> Compile(Expression<Func<T, T>> snapshotExpression)
        {
            var compiled = snapshotExpression.Compile();

            return v => compiled((T)v);
        }
    }
}
