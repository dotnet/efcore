// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query.SqlExpressions
{
    /// <summary>
    ///     <para>
    ///         An expression that represents a DISTINCT in a SQL tree.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    public class DistinctExpression : SqlExpression
    {
        /// <summary>
        ///     Creates a new instance of the <see cref="DistinctExpression" /> class.
        /// </summary>
        /// <param name="operand"> An expression on which DISTINCT is applied. </param>
        public DistinctExpression([NotNull] SqlExpression operand)
            : base(operand.Type, operand.TypeMapping)
        {
            Check.NotNull(operand, nameof(operand));

            Operand = operand;
        }

        /// <summary>
        ///     The expression on which DISTINCT is applied.
        /// </summary>
        public virtual SqlExpression Operand { get; }

        /// <inheritdoc />
        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            Check.NotNull(visitor, nameof(visitor));

            return Update((SqlExpression)visitor.Visit(Operand));
        }

        /// <summary>
        ///     Creates a new expression that is like this one, but using the supplied children. If all of the children are the same, it will
        ///     return this expression.
        /// </summary>
        /// <param name="operand"> The <see cref="Operand" /> property of the result. </param>
        /// <returns> This expression if no children changed, or an expression with the updated children. </returns>
        public virtual DistinctExpression Update([NotNull] SqlExpression operand)
        {
            Check.NotNull(operand, nameof(operand));

            return operand != Operand
                ? new DistinctExpression(operand)
                : this;
        }

        /// <inheritdoc />
        protected override void Print(ExpressionPrinter expressionPrinter)
        {
            Check.NotNull(expressionPrinter, nameof(expressionPrinter));

            expressionPrinter.Append("(DISTINCT ");
            expressionPrinter.Visit(Operand);
            expressionPrinter.Append(")");
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
            => obj != null
                && (ReferenceEquals(this, obj)
                    || obj is DistinctExpression distinctExpression
                    && Equals(distinctExpression));

        private bool Equals(DistinctExpression distinctExpression)
            => base.Equals(distinctExpression)
                && Operand.Equals(distinctExpression.Operand);

        /// <inheritdoc />
        public override int GetHashCode()
            => HashCode.Combine(base.GetHashCode(), Operand);
    }
}
