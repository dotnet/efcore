// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query.SqlExpressions
{
    /// <summary>
    ///     <para>
    ///         An expression that represents a UNION operation in a SQL tree.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    public class UnionExpression : SetOperationBase
    {
        /// <summary>
        ///     Creates a new instance of the <see cref="UnionExpression" /> class.
        /// </summary>
        /// <param name="alias"> A string alias for the table source. </param>
        /// <param name="source1"> A table source which is first source in the set operation. </param>
        /// <param name="source2"> A table source which is second source in the set operation. </param>
        /// <param name="distinct"> A bool value indicating whether result will remove duplicate rows. </param>
        public UnionExpression(
            string alias,
            SelectExpression source1,
            SelectExpression source2,
            bool distinct)
            : base(alias, source1, source2, distinct)
        {
        }

        /// <inheritdoc />
        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            Check.NotNull(visitor, nameof(visitor));

            var source1 = (SelectExpression)visitor.Visit(Source1);
            var source2 = (SelectExpression)visitor.Visit(Source2);

            return Update(source1, source2);
        }

        /// <summary>
        ///     Creates a new expression that is like this one, but using the supplied children. If all of the children are the same, it will
        ///     return this expression.
        /// </summary>
        /// <param name="source1"> The <see cref="P:Source1" /> property of the result. </param>
        /// <param name="source2"> The <see cref="P:Source2" /> property of the result. </param>
        /// <returns> This expression if no children changed, or an expression with the updated children. </returns>
        public virtual UnionExpression Update(SelectExpression source1, SelectExpression source2)
        {
            Check.NotNull(source1, nameof(source1));
            Check.NotNull(source2, nameof(source2));

            return source1 != Source1 || source2 != Source2
                ? new UnionExpression(Alias, source1, source2, IsDistinct)
                : this;
        }

        /// <inheritdoc />
        protected override void Print(ExpressionPrinter expressionPrinter)
        {
            Check.NotNull(expressionPrinter, nameof(expressionPrinter));

            expressionPrinter.Append("(");
            using (expressionPrinter.Indent())
            {
                expressionPrinter.Visit(Source1);
                expressionPrinter.AppendLine();
                expressionPrinter.Append("UNION");
                if (!IsDistinct)
                {
                    expressionPrinter.AppendLine(" ALL");
                }

                expressionPrinter.Visit(Source2);
            }

            expressionPrinter.AppendLine()
                .AppendLine($") AS {Alias}");
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
            => obj != null
                && (ReferenceEquals(this, obj)
                    || obj is UnionExpression unionExpression
                    && Equals(unionExpression));

        private bool Equals(UnionExpression unionExpression)
            => base.Equals(unionExpression);

        /// <inheritdoc />
        public override int GetHashCode()
            => HashCode.Combine(base.GetHashCode(), GetType());
    }
}
