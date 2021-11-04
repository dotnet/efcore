// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Microsoft.EntityFrameworkCore.Query.SqlExpressions
{
    /// <summary>
    ///     <para>
    ///         An expression that represents projecting a scalar SQL value from a subquery.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    public class ScalarSubqueryExpression : SqlExpression
    {
        /// <summary>
        ///     Creates a new instance of the <see cref="ScalarSubqueryExpression" /> class.
        /// </summary>
        /// <param name="subquery">A subquery projecting single row with a single scalar projection.</param>
        public ScalarSubqueryExpression(SelectExpression subquery)
            : base(Verify(subquery).Projection[0].Type, subquery.Projection[0].Expression.TypeMapping)
        {
            Subquery = subquery;
        }

        private static SelectExpression Verify(SelectExpression selectExpression)
        {
            if (selectExpression.Projection.Count != 1)
            {
                throw new InvalidOperationException(CoreStrings.TranslationFailed(selectExpression.Print()));
            }

            return selectExpression;
        }

        /// <summary>
        ///     The subquery projecting single row with single scalar projection.
        /// </summary>
        public virtual SelectExpression Subquery { get; }

        /// <inheritdoc />
        protected override Expression VisitChildren(ExpressionVisitor visitor)
            => Update((SelectExpression)visitor.Visit(Subquery));

        /// <summary>
        ///     Creates a new expression that is like this one, but using the supplied children. If all of the children are the same, it will
        ///     return this expression.
        /// </summary>
        /// <param name="subquery">The <see cref="Subquery" /> property of the result.</param>
        /// <returns>This expression if no children changed, or an expression with the updated children.</returns>
        public virtual ScalarSubqueryExpression Update(SelectExpression subquery)
            => subquery != Subquery
                ? new ScalarSubqueryExpression(subquery)
                : this;

        /// <inheritdoc />
        protected override void Print(ExpressionPrinter expressionPrinter)
        {
            expressionPrinter.Append("(");
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
                    || obj is ScalarSubqueryExpression scalarSubqueryExpression
                    && Equals(scalarSubqueryExpression));

        private bool Equals(ScalarSubqueryExpression scalarSubqueryExpression)
            => base.Equals(scalarSubqueryExpression)
                && Subquery.Equals(scalarSubqueryExpression.Subquery);

        /// <inheritdoc />
        public override int GetHashCode()
            => HashCode.Combine(base.GetHashCode(), Subquery);
    }
}
