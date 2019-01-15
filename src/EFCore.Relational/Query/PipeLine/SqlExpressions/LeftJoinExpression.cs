// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;

namespace Microsoft.EntityFrameworkCore.Relational.Query.Pipeline.SqlExpressions
{
    public class LeftJoinExpression : PredicateJoinExpressionBase
    {
        #region Fields & Constructors
        public LeftJoinExpression(TableExpressionBase table, SqlExpression joinPredicate)
            : base(table, joinPredicate)
        {
        }
        #endregion

        #region Expression-based methods
        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            var table = (TableExpressionBase)visitor.Visit(Table);
            var joinPredicate = (SqlExpression)visitor.Visit(JoinPredicate);

            return Update(table, joinPredicate);
        }

        public LeftJoinExpression Update(TableExpressionBase table, SqlExpression joinPredicate)
        {
            return table != Table || joinPredicate != JoinPredicate
                ? new LeftJoinExpression(table, joinPredicate)
                : this;
        }
        #endregion

        #region Equality & HashCode
        public override bool Equals(object obj)
            => obj != null
            && (ReferenceEquals(this, obj)
                || obj is LeftJoinExpression leftJoinExpression
                    && Equals(leftJoinExpression));

        private bool Equals(LeftJoinExpression leftJoinExpression)
            => base.Equals(leftJoinExpression);

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = base.GetHashCode();

                return hashCode;
            }
        }
        #endregion
    }
}
