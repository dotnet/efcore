// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query.SqlExpressions
{
    /// <summary>
    ///     <para>
    ///         An expression that represents an INNER JOIN in a SQL tree.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    public class InnerJoinExpression : PredicateJoinExpressionBase
    {
        /// <summary>
        ///     Creates a new instance of the <see cref="InnerJoinExpression" /> class.
        /// </summary>
        /// <param name="table"> A table source to INNER JOIN with. </param>
        /// <param name="joinPredicate"> A predicate to use for the join. </param>
        public InnerJoinExpression([NotNull] TableExpressionBase table, [NotNull] SqlExpression joinPredicate)
            : base(table, joinPredicate)
        {
        }

        /// <inheritdoc />
        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            Check.NotNull(visitor, nameof(visitor));

            var table = (TableExpressionBase)visitor.Visit(Table);
            var joinPredicate = (SqlExpression)visitor.Visit(JoinPredicate);

            return Update(table, joinPredicate);
        }

        /// <summary>
        ///     Creates a new expression that is like this one, but using the supplied children. If all of the children are the same, it will
        ///     return this expression.
        /// </summary>
        /// <param name="table"> The <see cref="P:Table" /> property of the result. </param>
        /// <param name="joinPredicate"> The <see cref="P:JoinPredicate" /> property of the result. </param>
        /// <returns> This expression if no children changed, or an expression with the updated children. </returns>
        public virtual InnerJoinExpression Update([NotNull] TableExpressionBase table, [NotNull] SqlExpression joinPredicate)
        {
            Check.NotNull(table, nameof(table));
            Check.NotNull(joinPredicate, nameof(joinPredicate));

            return table != Table || joinPredicate != JoinPredicate
                ? new InnerJoinExpression(table, joinPredicate)
                : this;
        }

        /// <inheritdoc />
        protected override void Print(ExpressionPrinter expressionPrinter)
        {
            Check.NotNull(expressionPrinter, nameof(expressionPrinter));

            expressionPrinter.Append("INNER JOIN ");
            expressionPrinter.Visit(Table);
            expressionPrinter.Append(" ON ");
            expressionPrinter.Visit(JoinPredicate);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
            => obj != null
                && (ReferenceEquals(this, obj)
                    || obj is InnerJoinExpression innerJoinExpression
                    && Equals(innerJoinExpression));

        private bool Equals(InnerJoinExpression innerJoinExpression)
            => base.Equals(innerJoinExpression);

        /// <inheritdoc />
        public override int GetHashCode()
            => base.GetHashCode();
    }
}
