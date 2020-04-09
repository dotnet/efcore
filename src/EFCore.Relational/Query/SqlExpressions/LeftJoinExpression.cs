// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query.SqlExpressions
{
    public class LeftJoinExpression : PredicateJoinExpressionBase
    {
        public LeftJoinExpression([NotNull] TableExpressionBase table, [NotNull] SqlExpression joinPredicate)
            : base(table, joinPredicate)
        {
        }

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            Check.NotNull(visitor, nameof(visitor));

            var table = (TableExpressionBase)visitor.Visit(Table);
            var joinPredicate = (SqlExpression)visitor.Visit(JoinPredicate);

            return Update(table, joinPredicate);
        }

        public virtual LeftJoinExpression Update([NotNull] TableExpressionBase table, [NotNull] SqlExpression joinPredicate)
        {
            Check.NotNull(table, nameof(table));
            Check.NotNull(joinPredicate, nameof(joinPredicate));

            return table != Table || joinPredicate != JoinPredicate
                ? new LeftJoinExpression(table, joinPredicate)
                : this;
        }

        public override void Print(ExpressionPrinter expressionPrinter)
        {
            Check.NotNull(expressionPrinter, nameof(expressionPrinter));

            expressionPrinter.Append("LEFT JOIN ");
            expressionPrinter.Visit(Table);
            expressionPrinter.Append(" ON ");
            expressionPrinter.Visit(JoinPredicate);
        }

        public override bool Equals(object obj)
            => obj != null
                && (ReferenceEquals(this, obj)
                    || obj is LeftJoinExpression leftJoinExpression
                    && Equals(leftJoinExpression));

        private bool Equals(LeftJoinExpression leftJoinExpression)
            => base.Equals(leftJoinExpression);

        public override int GetHashCode() => base.GetHashCode();
    }
}
