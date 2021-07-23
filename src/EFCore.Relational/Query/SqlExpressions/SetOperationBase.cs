// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query.SqlExpressions
{
    /// <summary>
    ///     <para>
    ///         An expression that represents a set operation between two table sources.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    public abstract class SetOperationBase : TableExpressionBase
    {
        /// <summary>
        ///     Creates a new instance of the <see cref="SetOperationBase" /> class.
        /// </summary>
        /// <param name="alias"> A string alias for the table source. </param>
        /// <param name="source1"> A table source which is first source in the set operation. </param>
        /// <param name="source2"> A table source which is second source in the set operation. </param>
        /// <param name="distinct"> A bool value indicating whether result will remove duplicate rows. </param>
        protected SetOperationBase(
            string alias,
            SelectExpression source1,
            SelectExpression source2,
            bool distinct)
            : base(Check.NotEmpty(alias, nameof(alias)))
        {
            Check.NotNull(source1, nameof(source1));
            Check.NotNull(source2, nameof(source2));

            IsDistinct = distinct;
            Source1 = source1;
            Source2 = source2;
        }

        /// <summary>
        ///     The alias assigned to this table source.
        /// </summary>
        [NotNull]
        public override string? Alias
        {
            get => base.Alias!;
            internal set => base.Alias = value;
        }

        /// <summary>
        ///     The bool value indicating whether result will remove duplicate rows.
        /// </summary>
        public virtual bool IsDistinct { get; }

        /// <summary>
        ///     The first source of the set operation.
        /// </summary>
        public virtual SelectExpression Source1 { get; }

        /// <summary>
        ///     The second source of the set operation.
        /// </summary>
        public virtual SelectExpression Source2 { get; }

        /// <inheritdoc />
        public override bool Equals(object? obj)
            => obj != null
                && (ReferenceEquals(this, obj)
                    || obj is SetOperationBase setOperationBase
                    && Equals(setOperationBase));

        private bool Equals(SetOperationBase setOperationBase)
            => IsDistinct == setOperationBase.IsDistinct
                && Source1.Equals(setOperationBase.Source1)
                && Source2.Equals(setOperationBase.Source2);

        /// <inheritdoc />
        public override int GetHashCode()
            => HashCode.Combine(base.GetHashCode(), IsDistinct, Source1, Source2);
    }
}
