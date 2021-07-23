// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query.SqlExpressions
{
    /// <summary>
    ///     <para>
    ///         An expression that represents an EXISTS operation in a SQL tree.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    public class ExistsExpression : SqlExpression
    {
        /// <summary>
        ///     Creates a new instance of the <see cref="ExistsExpression" /> class.
        /// </summary>
        /// <param name="subquery"> A subquery to check existence of. </param>
        /// <param name="negated"> A value indicating if the existence check is negated. </param>
        /// <param name="typeMapping"> The <see cref="RelationalTypeMapping" /> associated with the expression. </param>
        public ExistsExpression(
            SelectExpression subquery,
            bool negated,
            RelationalTypeMapping? typeMapping)
            : base(typeof(bool), typeMapping)
        {
            Check.NotNull(subquery, nameof(subquery));

            Subquery = subquery;
            IsNegated = negated;
        }

        /// <summary>
        ///     The subquery to check existence of.
        /// </summary>
        public virtual SelectExpression Subquery { get; }

        /// <summary>
        ///     The value indicating if the existence check is negated.
        /// </summary>
        public virtual bool IsNegated { get; }

        /// <inheritdoc />
        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            Check.NotNull(visitor, nameof(visitor));

            return Update((SelectExpression)visitor.Visit(Subquery));
        }

        /// <summary>
        ///     Creates a new expression that is like this one, but using the supplied children. If all of the children are the same, it will
        ///     return this expression.
        /// </summary>
        /// <param name="subquery"> The <see cref="Subquery" /> property of the result. </param>
        /// <returns> This expression if no children changed, or an expression with the updated children. </returns>
        public virtual ExistsExpression Update(SelectExpression subquery)
        {
            Check.NotNull(subquery, nameof(subquery));

            return subquery != Subquery
                ? new ExistsExpression(subquery, IsNegated, TypeMapping)
                : this;
        }

        /// <inheritdoc />
        protected override void Print(ExpressionPrinter expressionPrinter)
        {
            Check.NotNull(expressionPrinter, nameof(expressionPrinter));

            if (IsNegated)
            {
                expressionPrinter.Append("NOT ");
            }

            expressionPrinter.AppendLine("EXISTS (");
            using (expressionPrinter.Indent())
            {
                expressionPrinter.Visit(Subquery);
            }

            expressionPrinter.Append(")");
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
            => obj != null
                && (ReferenceEquals(this, obj)
                    || obj is ExistsExpression existsExpression
                    && Equals(existsExpression));

        private bool Equals(ExistsExpression existsExpression)
            => base.Equals(existsExpression)
                && Subquery.Equals(existsExpression.Subquery)
                && IsNegated == existsExpression.IsNegated;

        /// <inheritdoc />
        public override int GetHashCode()
            => HashCode.Combine(base.GetHashCode(), Subquery, IsNegated);
    }
}
