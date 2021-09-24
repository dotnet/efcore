// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query.SqlExpressions
{
    /// <summary>
    ///     <para>
    ///         An expression that represents an OUTER APPLY in a SQL tree.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    public class OuterApplyExpression : JoinExpressionBase
    {
        /// <summary>
        ///     Creates a new instance of the <see cref="OuterApplyExpression" /> class.
        /// </summary>
        /// <param name="table">A table source to OUTER APPLY with.</param>
        public OuterApplyExpression(TableExpressionBase table)
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
        /// <param name="table">The <see cref="P:Table" /> property of the result.</param>
        /// <returns>This expression if no children changed, or an expression with the updated children.</returns>
        public virtual OuterApplyExpression Update(TableExpressionBase table)
        {
            Check.NotNull(table, nameof(table));

            return table != Table
                ? new OuterApplyExpression(table)
                : this;
        }

        /// <inheritdoc />
        protected override void Print(ExpressionPrinter expressionPrinter)
        {
            Check.NotNull(expressionPrinter, nameof(expressionPrinter));

            expressionPrinter.Append("OUTER APPLY ");
            expressionPrinter.Visit(Table);
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
            => obj != null
                && (ReferenceEquals(this, obj)
                    || obj is OuterApplyExpression outerApplyExpression
                    && Equals(outerApplyExpression));

        private bool Equals(OuterApplyExpression outerApplyExpression)
            => base.Equals(outerApplyExpression);

        /// <inheritdoc />
        public override int GetHashCode()
            => base.GetHashCode();
    }
}
