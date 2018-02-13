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
    public abstract class ValueComparer
    {
        /// <summary>
        ///     Creates a new <see cref="ValueComparer" /> with the given comparison and
        ///     snapshotting expressions.
        /// </summary>
        /// <param name="compareFunc"> The compare expression compiled into a untyped delegate. </param>
        /// <param name="snapshotFunc"> The snapshot expression compiled into a untyped delegate. </param>
        /// <param name="compareExpression"> The comparison expression. </param>
        /// <param name="snapshotExpression"> The snapshot expression. </param>
        protected ValueComparer(
            [NotNull] Func<object, object, bool> compareFunc,
            [NotNull] Func<object, object> snapshotFunc,
            [NotNull] LambdaExpression compareExpression,
            [NotNull] LambdaExpression snapshotExpression)
        {
            CompareFunc = compareFunc;
            SnapshotFunc = snapshotFunc;
            CompareExpression = compareExpression;
            SnapshotExpression = snapshotExpression;
        }

        /// <summary>
        ///     The type.
        /// </summary>
        public abstract Type Type { get; }

        /// <summary>
        ///     The comparison expression compiled into an untyped delegate.
        /// </summary>
        public virtual Func<object, object, bool> CompareFunc { get; }

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
        public virtual Func<object, object> SnapshotFunc { get; }

        /// <summary>
        ///     The comparison expression.
        /// </summary>
        public virtual LambdaExpression CompareExpression { get; }

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
    }
}
