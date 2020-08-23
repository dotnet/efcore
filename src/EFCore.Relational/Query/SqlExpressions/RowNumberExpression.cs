// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query.SqlExpressions
{
    /// <summary>
    ///     <para>
    ///         An expression that represents a ROW_NUMBER operation in a SQL tree.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    public class RowNumberExpression : SqlExpression
    {
        /// <summary>
        ///     Creates a new instance of the <see cref="RowNumberExpression" /> class.
        /// </summary>
        /// <param name="partitions"> A list expressions to partition by. </param>
        /// <param name="orderings"> A list of ordering expressions to order by. </param>
        /// <param name="typeMapping"> The <see cref="RelationalTypeMapping" /> associated with the expression. </param>
        public RowNumberExpression(
            [CanBeNull] IReadOnlyList<SqlExpression> partitions,
            [NotNull] IReadOnlyList<OrderingExpression> orderings,
            [CanBeNull] RelationalTypeMapping typeMapping)
            : base(typeof(long), typeMapping)
        {
            Check.NotEmpty(orderings, nameof(orderings));

            Partitions = partitions ?? Array.Empty<SqlExpression>();
            Orderings = orderings;
        }

        /// <summary>
        ///     The list of expressions used in partitioning.
        /// </summary>
        public virtual IReadOnlyList<SqlExpression> Partitions { get; }

        /// <summary>
        ///     The list of ordering expressions used to order inside the given partition.
        /// </summary>
        public virtual IReadOnlyList<OrderingExpression> Orderings { get; }

        /// <inheritdoc />
        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            Check.NotNull(visitor, nameof(visitor));

            var changed = false;
            var partitions = new List<SqlExpression>();
            foreach (var partition in Partitions)
            {
                var newPartition = (SqlExpression)visitor.Visit(partition);
                changed |= newPartition != partition;
                partitions.Add(newPartition);
            }

            var orderings = new List<OrderingExpression>();
            foreach (var ordering in Orderings)
            {
                var newOrdering = (OrderingExpression)visitor.Visit(ordering);
                changed |= newOrdering != ordering;
                orderings.Add(newOrdering);
            }

            return changed
                ? new RowNumberExpression(partitions, orderings, TypeMapping)
                : this;
        }

        /// <summary>
        ///     Creates a new expression that is like this one, but using the supplied children. If all of the children are the same, it will
        ///     return this expression.
        /// </summary>
        /// <param name="partitions"> The <see cref="Partitions" /> property of the result. </param>
        /// <param name="orderings"> The <see cref="Orderings" /> property of the result. </param>
        /// <returns> This expression if no children changed, or an expression with the updated children. </returns>
        public virtual RowNumberExpression Update(
            [CanBeNull] IReadOnlyList<SqlExpression> partitions,
            [NotNull] IReadOnlyList<OrderingExpression> orderings)
        {
            Check.NotNull(orderings, nameof(orderings));

            return (Partitions == null ? partitions == null : Partitions.SequenceEqual(partitions))
                && Orderings.SequenceEqual(orderings)
                    ? this
                    : new RowNumberExpression(partitions, orderings, TypeMapping);
        }

        /// <inheritdoc />
        protected override void Print(ExpressionPrinter expressionPrinter)
        {
            Check.NotNull(expressionPrinter, nameof(expressionPrinter));

            expressionPrinter.Append("ROW_NUMBER() OVER(");
            if (Partitions.Any())
            {
                expressionPrinter.Append("PARTITION BY ");
                expressionPrinter.VisitCollection(Partitions);
                expressionPrinter.Append(" ");
            }

            expressionPrinter.Append("ORDER BY ");
            expressionPrinter.VisitCollection(Orderings);
            expressionPrinter.Append(")");
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
            => obj != null
                && (ReferenceEquals(this, obj)
                    || obj is RowNumberExpression rowNumberExpression
                    && Equals(rowNumberExpression));

        private bool Equals(RowNumberExpression rowNumberExpression)
            => base.Equals(rowNumberExpression)
                && (Partitions == null ? rowNumberExpression.Partitions == null : Partitions.SequenceEqual(rowNumberExpression.Partitions))
                && Orderings.SequenceEqual(rowNumberExpression.Orderings);

        /// <inheritdoc />
        public override int GetHashCode()
        {
            var hash = new HashCode();
            hash.Add(base.GetHashCode());
            foreach (var partition in Partitions)
            {
                hash.Add(partition);
            }

            foreach (var ordering in Orderings)
            {
                hash.Add(ordering);
            }

            return hash.ToHashCode();
        }
    }
}
