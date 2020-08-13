// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;

namespace Microsoft.EntityFrameworkCore.Query.SqlExpressions
{
    public class LeftJoinExpression : PredicateJoinExpressionBase
    {
        public LeftJoinExpression(TableExpressionBase table, SqlExpression joinPredicate)
            : base(table, joinPredicate)
        {
        }

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            var table = (TableExpressionBase)visitor.Visit(Table);
            var joinPredicate = (SqlExpression)visitor.Visit(JoinPredicate);

            return Update(table, joinPredicate);
        }

        public virtual LeftJoinExpression Update(TableExpressionBase table, SqlExpression joinPredicate)
            => table != Table || joinPredicate != JoinPredicate
                ? new LeftJoinExpression(table, joinPredicate)
                : this;

        public override void Print(ExpressionPrinter expressionPrinter)
        {
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
