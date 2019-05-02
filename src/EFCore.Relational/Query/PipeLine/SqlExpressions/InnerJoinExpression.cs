// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;

namespace Microsoft.EntityFrameworkCore.Relational.Query.Pipeline.SqlExpressions
{
    public class InnerJoinExpression : PredicateJoinExpressionBase
    {
        #region Fields & Constructors
        public InnerJoinExpression(TableExpressionBase table, SqlExpression joinPredicate)
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

        public InnerJoinExpression Update(TableExpressionBase table, SqlExpression joinPredicate)
        {
            return table != Table || joinPredicate != JoinPredicate
                ? new InnerJoinExpression(table, joinPredicate)
                : this;
        }
        #endregion

        #region Equality & HashCode
        public override bool Equals(object obj)
            => obj != null
            && (ReferenceEquals(this, obj)
                || obj is InnerJoinExpression innerJoinExpression
                    && Equals(innerJoinExpression));

        private bool Equals(InnerJoinExpression innerJoinExpression)
            => base.Equals(innerJoinExpression);

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

    public class CrossJoinExpression : JoinExpressionBase
    {
        #region Fields & Constructors
        public CrossJoinExpression(TableExpressionBase table)
            : base(table)
        {
        }
        #endregion

        #region Expression-based methods
        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            var table = (TableExpressionBase)visitor.Visit(Table);

            return Update(table);
        }

        public CrossJoinExpression Update(TableExpressionBase table)
        {
            return table != Table
                ? new CrossJoinExpression(table)
                : this;
        }
        #endregion

        #region Equality & HashCode
        public override bool Equals(object obj)
            => obj != null
            && (ReferenceEquals(this, obj)
                || obj is CrossJoinExpression crossJoinExpression
                    && Equals(crossJoinExpression));

        private bool Equals(CrossJoinExpression crossJoinExpression)
            => base.Equals(crossJoinExpression);

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
