// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query.SqlExpressions
{
    public class RowNumberExpression : SqlExpression
    {
        public RowNumberExpression(
            IReadOnlyList<SqlExpression> partitions, IReadOnlyList<OrderingExpression> orderings, RelationalTypeMapping typeMapping)
            : base(typeof(long), typeMapping)
        {
            Check.NotEmpty(orderings, nameof(orderings));

            Partitions = partitions;
            Orderings = orderings;
        }

        public virtual IReadOnlyList<SqlExpression> Partitions { get; }
        public virtual IReadOnlyList<OrderingExpression> Orderings { get; }

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
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

        public virtual RowNumberExpression Update(IReadOnlyList<SqlExpression> partitions, IReadOnlyList<OrderingExpression> orderings)
        {
            return (Partitions == null ? partitions == null : Partitions.SequenceEqual(partitions))
                && Orderings.SequenceEqual(orderings)
                    ? this
                    : new RowNumberExpression(partitions, orderings, TypeMapping);
        }

        public override void Print(ExpressionPrinter expressionPrinter)
        {
            expressionPrinter.Append("ROW_NUMBER() OVER(");
            if (Partitions.Any())
            {
                expressionPrinter.Append("PARTITION BY ");
                expressionPrinter.VisitList(Partitions);
                expressionPrinter.Append(" ");
            }

            expressionPrinter.Append("ORDER BY ");
            expressionPrinter.VisitList(Orderings);
            expressionPrinter.Append(")");
        }

        public override bool Equals(object obj)
            => obj != null
                && (ReferenceEquals(this, obj)
                    || obj is RowNumberExpression rowNumberExpression
                    && Equals(rowNumberExpression));

        private bool Equals(RowNumberExpression rowNumberExpression)
            => base.Equals(rowNumberExpression)
                && (Partitions == null ? rowNumberExpression.Partitions == null : Partitions.SequenceEqual(rowNumberExpression.Partitions))
                && Orderings.SequenceEqual(rowNumberExpression.Orderings);

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
