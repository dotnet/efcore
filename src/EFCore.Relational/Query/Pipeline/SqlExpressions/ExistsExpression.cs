// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Relational.Query.Pipeline.SqlExpressions
{
    public class ExistsExpression : SqlExpression
    {
        public ExistsExpression(SelectExpression subquery, bool negated, RelationalTypeMapping typeMapping)
            : base(typeof(bool), typeMapping)
        {
            Subquery = subquery;
            Negated = negated;
        }

        public SelectExpression Subquery { get; }
        public bool Negated { get; }

        protected override Expression VisitChildren(ExpressionVisitor visitor)
            => Update((SelectExpression)visitor.Visit(Subquery));

        public ExistsExpression Update(SelectExpression subquery)
            => subquery != Subquery
                ? new ExistsExpression(subquery, Negated, TypeMapping)
                : this;


        public override void Print(ExpressionPrinter expressionPrinter)
        {
            if (Negated)
            {
                expressionPrinter.StringBuilder.Append("NOT ");
            }

            expressionPrinter.StringBuilder.AppendLine("EXISTS (");
            using (expressionPrinter.StringBuilder.Indent())
            {
                expressionPrinter.Visit(Subquery);
            }

            expressionPrinter.StringBuilder.Append(")");
        }

        public override bool Equals(object obj)
            => obj != null
            && (ReferenceEquals(this, obj)
                || obj is ExistsExpression existsExpression
                    && Equals(existsExpression));

        private bool Equals(ExistsExpression existsExpression)
            => base.Equals(existsExpression)
            && Subquery.Equals(existsExpression.Subquery)
            && Negated == existsExpression.Negated;

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ Subquery.GetHashCode();
                hashCode = (hashCode * 397) ^ Negated.GetHashCode();

                return hashCode;
            }
        }
    }
}
