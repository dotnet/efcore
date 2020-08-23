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
    ///         An expression that represents an ordering in a SQL tree.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    public class OrderingExpression : Expression, IPrintableExpression
    {
        /// <summary>
        ///     Creates a new instance of the <see cref="OrderingExpression" /> class.
        /// </summary>
        /// <param name="expression"> An expression used for ordering. </param>
        /// <param name="ascending"> A value indicating if the ordering is ascending. </param>
        public OrderingExpression([NotNull] SqlExpression expression, bool ascending)
        {
            Check.NotNull(expression, nameof(expression));

            Expression = expression;
            IsAscending = ascending;
        }

        /// <summary>
        ///     The expression used for ordering.
        /// </summary>
        public virtual SqlExpression Expression { get; }

        /// <summary>
        ///     The value indicating if the ordering is ascending.
        /// </summary>
        public virtual bool IsAscending { get; }

        /// <inheritdoc />
        public sealed override ExpressionType NodeType
            => ExpressionType.Extension;

        /// <inheritdoc />
        public override Type Type
            => Expression.Type;

        /// <inheritdoc />
        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            Check.NotNull(visitor, nameof(visitor));

            return Update((SqlExpression)visitor.Visit(Expression));
        }

        /// <summary>
        ///     Creates a new expression that is like this one, but using the supplied children. If all of the children are the same, it will
        ///     return this expression.
        /// </summary>
        /// <param name="expression"> The <see cref="Expression" /> property of the result. </param>
        /// <returns> This expression if no children changed, or an expression with the updated children. </returns>
        public virtual OrderingExpression Update([NotNull] SqlExpression expression)
        {
            Check.NotNull(expression, nameof(expression));

            return expression != Expression
                ? new OrderingExpression(expression, IsAscending)
                : this;
        }

        /// <inheritdoc />
        void IPrintableExpression.Print(ExpressionPrinter expressionPrinter)
        {
            Check.NotNull(expressionPrinter, nameof(expressionPrinter));

            expressionPrinter.Visit(Expression);

            expressionPrinter.Append(IsAscending ? " ASC" : " DESC");
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
            => obj != null
                && (ReferenceEquals(this, obj)
                    || obj is OrderingExpression orderingExpression
                    && Equals(orderingExpression));

        private bool Equals(OrderingExpression orderingExpression)
            => Expression.Equals(orderingExpression.Expression)
                && IsAscending == orderingExpression.IsAscending;

        /// <inheritdoc />
        public override int GetHashCode()
            => HashCode.Combine(Expression, IsAscending);
    }
}
