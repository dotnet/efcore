// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query.SqlExpressions
{
    /// <summary>
    ///     <para>
    ///         An expression that represents a CROSS JOIN in a SQL tree.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    public class CrossJoinExpression : JoinExpressionBase
    {
        /// <summary>
        ///     Creates a new instance of the <see cref="CrossJoinExpression" /> class.
        /// </summary>
        /// <param name="table"> A table source to CROSS JOIN with. </param>
        public CrossJoinExpression([NotNull] TableExpressionBase table)
            : base(table)
        {
        }

        /// <inheritdoc />
        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            Check.NotNull(visitor, nameof(visitor));

            return Update((TableExpressionBase)visitor.Visit(Table));
        }

        /// <summary>
        ///     Creates a new expression that is like this one, but using the supplied children. If all of the children are the same, it will
        ///     return this expression.
        /// </summary>
        /// <param name="table"> The <see cref="P:Table" /> property of the result. </param>
        /// <returns> This expression if no children changed, or an expression with the updated children. </returns>
        public virtual CrossJoinExpression Update([NotNull] TableExpressionBase table)
        {
            Check.NotNull(table, nameof(table));

            return table != Table
                ? new CrossJoinExpression(table)
                : this;
        }

        /// <inheritdoc />
        protected override void Print(ExpressionPrinter expressionPrinter)
        {
            Check.NotNull(expressionPrinter, nameof(expressionPrinter));

            expressionPrinter.Append("CROSS JOIN ");
            expressionPrinter.Visit(Table);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
            => obj != null
                && (ReferenceEquals(this, obj)
                    || obj is CrossJoinExpression crossJoinExpression
                    && Equals(crossJoinExpression));

        private bool Equals(CrossJoinExpression crossJoinExpression)
            => base.Equals(crossJoinExpression);

        /// <inheritdoc />
        public override int GetHashCode()
            => base.GetHashCode();
    }
}
