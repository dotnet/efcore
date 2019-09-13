// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Query.SqlExpressions
{
    public class SelectDistinctExpression : SqlExpression
    {
        public SelectDistinctExpression(ColumnExpression column, RelationalTypeMapping typeMapping)
            : base(typeof(bool), typeMapping)
        {
            Column = column;
        }

        public virtual ColumnExpression Column { get; }

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            visitor.Visit(new SqlFragmentExpression("DISTINCT "));
            var column = (ColumnExpression)visitor.Visit(Column);
            return new SelectDistinctExpression(column, base.TypeMapping);
        }

        public override void Print(ExpressionPrinter expressionPrinter)
        {
            expressionPrinter.Append("DISTINCT ");
            expressionPrinter.Visit(Column);
        }

        public override bool Equals(object obj)
            => obj != null
                && (ReferenceEquals(this, obj)
                    || obj is SelectDistinctExpression distinctCountExpression
                    && Equals(distinctCountExpression));

        private bool Equals(SelectDistinctExpression distinctCountExpression)
            => base.Equals(distinctCountExpression)
               && Column.Equals(distinctCountExpression.Column);

        public override int GetHashCode() => HashCode.Combine(base.GetHashCode(), Column);
    }
}
