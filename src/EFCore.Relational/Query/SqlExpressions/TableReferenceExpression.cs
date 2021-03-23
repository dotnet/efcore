// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Linq.Expressions;

namespace Microsoft.EntityFrameworkCore.Query.SqlExpressions
{
#pragma warning disable CS1591
    // TODO: Make this nested inside SelectExpression and same for ColumnExpression
    public class TableReferenceExpression : Expression
    {
        private SelectExpression _selectExpression;

        public TableReferenceExpression(SelectExpression selectExpression, string alias)
        {
            _selectExpression = selectExpression;
            Alias = alias;
        }

        public virtual TableExpressionBase Table
            => _selectExpression.Tables.Single(
                e => string.Equals((e as JoinExpressionBase)?.Table.Alias ?? e.Alias, Alias, StringComparison.OrdinalIgnoreCase));

        public virtual string Alias { get; internal set; }

        public override Type Type => typeof(object);

        public override ExpressionType NodeType => ExpressionType.Extension;
        public virtual void UpdateTableReference(SelectExpression oldSelect, SelectExpression newSelect)
        {
            if (ReferenceEquals(oldSelect, _selectExpression))
            {
                _selectExpression = newSelect;
            }
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
            => obj != null
                && (ReferenceEquals(this, obj)
                    || obj is TableReferenceExpression tableReferenceExpression
                    && Equals(tableReferenceExpression));

        // Since table reference is owned by SelectExpression, the select expression should be the same reference if they are matching.
        // That means we also don't need to compute the hashcode for it.
        // This allows us to break the cycle in computation when traversing this graph.
        private bool Equals(TableReferenceExpression tableReferenceExpression)
            => string.Equals(Alias, tableReferenceExpression.Alias, StringComparison.OrdinalIgnoreCase)
                && ReferenceEquals(_selectExpression, tableReferenceExpression._selectExpression);

        /// <inheritdoc />
        public override int GetHashCode()
            => Alias.GetHashCode();
    }
}
