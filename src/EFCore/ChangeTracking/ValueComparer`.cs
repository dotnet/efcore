// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using JetBrains.Annotations;

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
        /// <summary>
        ///     Creates a new <see cref="ValueComparer{T}" /> with the given comparison expression.
        ///     A shallow copy will be used for the snapshot.
        /// </summary>
        /// <param name="compareExpression"> The comparison expression. </param>
        public ValueComparer(
            [NotNull] Expression<Func<T, T, bool>> compareExpression)
            : this(compareExpression, v => v)
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
        /// <param name="compareExpression"> The comparison expression. </param>
        /// <param name="snapshotExpression"> The snapshot expression. </param>
        public ValueComparer(
            [NotNull] Expression<Func<T, T, bool>> compareExpression,
            [NotNull] Expression<Func<T, T>> snapshotExpression)
            : base(
                Compile(compareExpression),
                Compile(snapshotExpression),
                compareExpression,
                snapshotExpression)
        {
        }

        /// <summary>
        ///     The type.
        /// </summary>
        public override Type Type => typeof(T);

        /// <summary>
        ///     The comparison expression.
        /// </summary>
        public new virtual Expression<Func<T, T, bool>> CompareExpression
            => (Expression<Func<T, T, bool>>)base.CompareExpression;

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

        private static Func<object, object, bool> Compile(Expression<Func<T, T, bool>> compareExpression)
        {
            var compiled = compareExpression.Compile();

            return (l, r) => compiled((T)l, (T)r);
        }

        private static Func<object, object> Compile(Expression<Func<T, T>> snapshotExpression)
        {
            var compiled = snapshotExpression.Compile();

            return v => compiled((T)v);
        }
    }
}
