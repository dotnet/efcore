// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query.Sql.Internal;
using Microsoft.EntityFrameworkCore.Utilities;
using Remotion.Linq.Clauses;

namespace Microsoft.EntityFrameworkCore.Query.Expressions.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class RowNumberExpression : Expression
    {
        private readonly List<Ordering> _orderings = new List<Ordering>();

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public RowNumberExpression(
            [NotNull] ColumnExpression columnExpression,
            [NotNull] IReadOnlyList<Ordering> orderings)
        {
            Check.NotNull(columnExpression, nameof(columnExpression));
            Check.NotNull(orderings, nameof(orderings));

            ColumnExpression = columnExpression;
            _orderings.AddRange(orderings);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ColumnExpression ColumnExpression { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override ExpressionType NodeType => ExpressionType.Extension;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override bool CanReduce => false;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override Type Type => ColumnExpression.Type;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IReadOnlyList<Ordering> Orderings => _orderings;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override Expression Accept(ExpressionVisitor visitor)
        {
            Check.NotNull(visitor, nameof(visitor));

            var specificVisitor = visitor as ISqlServerExpressionVisitor;

            return specificVisitor != null
                ? specificVisitor.VisitRowNumber(this)
                : base.Accept(visitor);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
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
                ((newColumnExpression != null) && (ColumnExpression != newColumnExpression)))
            {
                return new RowNumberExpression(newColumnExpression ?? ColumnExpression, newOrderings);
            }

            return this;
        }
    }
}
