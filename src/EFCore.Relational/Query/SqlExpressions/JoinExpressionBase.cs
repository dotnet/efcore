// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query.SqlExpressions
{
    /// <summary>
    ///     <para>
    ///         An expression that represents a JOIN in a SQL tree.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    public abstract class JoinExpressionBase : TableExpressionBase
    {
        /// <summary>
        ///     Creates a new instance of the <see cref="JoinExpressionBase" /> class.
        /// </summary>
        /// <param name="table"> A table source to join with. </param>
        protected JoinExpressionBase([NotNull] TableExpressionBase table)
            : base(null)
        {
            Check.NotNull(table, nameof(table));

            Table = table;
        }

        /// <summary>
        ///     Gets the underlying table source to join with.
        /// </summary>
        public virtual TableExpressionBase Table { get; }

        /// <inheritdoc />
        public override bool Equals(object obj)
            => obj != null
                && (ReferenceEquals(this, obj)
                    || obj is JoinExpressionBase joinExpressionBase
                    && Equals(joinExpressionBase));

        private bool Equals(JoinExpressionBase joinExpressionBase)
            => base.Equals(joinExpressionBase)
                && Table.Equals(joinExpressionBase.Table);

        /// <inheritdoc />
        public override int GetHashCode()
            => HashCode.Combine(base.GetHashCode(), Table);
    }
}
