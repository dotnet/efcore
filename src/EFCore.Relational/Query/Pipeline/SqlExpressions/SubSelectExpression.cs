// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query.Internal;

namespace Microsoft.EntityFrameworkCore.Relational.Query.Pipeline.SqlExpressions
{
    public class SubSelectExpression : SqlExpression
    {
        #region Fields & Constructors
        public SubSelectExpression(SelectExpression subquery)
            : base(Verify(subquery).Projection[0].Type, subquery.Projection[0].Expression.TypeMapping)
        {
            Subquery = subquery;
        }
        private static SelectExpression Verify(SelectExpression selectExpression)
        {
            if (selectExpression.Projection.Count != 1)
            {
                throw new InvalidOperationException();
            }

            return selectExpression;
        }
        #endregion

        #region Public Properties
        public SelectExpression Subquery { get; }
        #endregion

        #region Expression-based methods
        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            var subquery = (SelectExpression)visitor.Visit(Subquery);

            return Update(subquery);
        }

        public SubSelectExpression Update(SelectExpression subquery)
        {
            return subquery != Subquery
                ? new SubSelectExpression(subquery)
                : this;
        }
        #endregion

        #region Equality & HashCode
        public override bool Equals(object obj)
            => obj != null
            && (ReferenceEquals(this, obj)
                || obj is SubSelectExpression subSelectExpression
                    && Equals(subSelectExpression));

        private bool Equals(SubSelectExpression subSelectExpression)
            => base.Equals(subSelectExpression)
            && Subquery.Equals(subSelectExpression.Subquery);

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ Subquery.GetHashCode();

                return hashCode;
            }
        }

        #endregion

        public override void Print(ExpressionPrinter expressionPrinter)
        {
            expressionPrinter.StringBuilder.Append("(");
            using (expressionPrinter.StringBuilder.Indent())
            {
                expressionPrinter.Visit(Subquery);
            }
            expressionPrinter.StringBuilder.Append(")");
        }
    }
}
