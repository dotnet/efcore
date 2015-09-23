// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Query.Sql;
using Microsoft.Data.Entity.Utilities;
using Remotion.Linq.Clauses;

namespace Microsoft.Data.Entity.Query.Expressions.Internal
{
    public class RowNumberExpression : Expression
    {
        private readonly List<Ordering> _orderings = new List<Ordering>();

        public RowNumberExpression(
            [NotNull] ColumnExpression columnExpression,
            [NotNull] IReadOnlyList<Ordering> orderings)
        {
            Check.NotNull(columnExpression, nameof(columnExpression));
            Check.NotNull(orderings, nameof(orderings));

            ColumnExpression = columnExpression;
            _orderings.AddRange(orderings);
        }

        public virtual ColumnExpression ColumnExpression { get; }

        public override ExpressionType NodeType => ExpressionType.Extension;
        public override bool CanReduce => false;
        public override Type Type => ColumnExpression.Type;

        public virtual IReadOnlyList<Ordering> Orderings => _orderings;

        protected override Expression Accept(ExpressionVisitor visitor)
        {
            Check.NotNull(visitor, nameof(visitor));

            var specificVisitor = visitor as ISqlServerExpressionVisitor;

            return specificVisitor != null
                ? specificVisitor.VisitRowNumber(this)
                : base.Accept(visitor);
        }

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            var newColumnExpression = visitor.Visit(ColumnExpression) as ColumnExpression;

            var newOrderings = new List<Ordering>();
            var recreate = false;
            foreach (var ordering in _orderings)
            {
                var newOrdering = new Ordering(visitor.Visit(ordering.Expression), ordering.OrderingDirection);
                newOrderings.Add(newOrdering);
                recreate |= newOrdering.Expression != ordering.Expression;
            }

            if (recreate ||
                (newColumnExpression != null && ColumnExpression != newColumnExpression))
            {
                return new RowNumberExpression(newColumnExpression, newOrderings);
            }

            return this;
        }
    }
}
