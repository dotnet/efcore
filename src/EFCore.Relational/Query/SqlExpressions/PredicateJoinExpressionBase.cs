// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query.SqlExpressions
{
    /// <summary>
    ///     <para>
    ///         An expression that represents a JOIN with a search condition in a SQL tree.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    public abstract class PredicateJoinExpressionBase : JoinExpressionBase
    {
        /// <summary>
        ///     Creates a new instance of the <see cref="PredicateJoinExpressionBase" /> class.
        /// </summary>
        /// <param name="table"> A table source to join with. </param>
        /// <param name="joinPredicate"> A predicate to use for the join. </param>
        protected PredicateJoinExpressionBase([NotNull] TableExpressionBase table, [NotNull] SqlExpression joinPredicate)
            : base(table)
        {
            Check.NotNull(joinPredicate, nameof(joinPredicate));

            JoinPredicate = joinPredicate;
        }

        /// <summary>
        ///     The predicate used in join.
        /// </summary>
        public virtual SqlExpression JoinPredicate { get; }

        /// <inheritdoc />
        public override bool Equals(object obj)
            => obj != null
                && (ReferenceEquals(this, obj)
                    || obj is PredicateJoinExpressionBase predicateJoinExpressionBase
                    && Equals(predicateJoinExpressionBase));

        private bool Equals(PredicateJoinExpressionBase predicateJoinExpressionBase)
            => base.Equals(predicateJoinExpressionBase)
                && JoinPredicate.Equals(predicateJoinExpressionBase.JoinPredicate);

        /// <inheritdoc />
        public override int GetHashCode()
            => HashCode.Combine(base.GetHashCode(), JoinPredicate);
    }
}
