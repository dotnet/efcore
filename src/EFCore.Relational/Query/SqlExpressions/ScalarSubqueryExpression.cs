// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Microsoft.EntityFrameworkCore.Query.SqlExpressions
{
    public class ScalarSubqueryExpression : SqlExpression
    {
        public ScalarSubqueryExpression(SelectExpression subquery)
            : base(Verify(subquery).Projection[0].Type, subquery.Projection[0].Expression.TypeMapping)
        {
            Subquery = subquery;
        }

        private static SelectExpression Verify(SelectExpression selectExpression)
        {
            if (selectExpression.Projection.Count != 1)
            {
                throw new InvalidOperationException(CoreStrings.TranslationFailed(selectExpression.Print()));
            }

            return selectExpression;
        }

        public virtual SelectExpression Subquery { get; }

        protected override Expression VisitChildren(ExpressionVisitor visitor)
            => Update((SelectExpression)visitor.Visit(Subquery));

        public virtual ScalarSubqueryExpression Update(SelectExpression subquery)
            => subquery != Subquery
                ? new ScalarSubqueryExpression(subquery)
                : this;

        public override void Print(ExpressionPrinter expressionPrinter)
        {
            expressionPrinter.Append("(");
            using (expressionPrinter.Indent())
            {
                expressionPrinter.Visit(Subquery);
            }

            expressionPrinter.Append(")");
        }

        public override bool Equals(object obj)
            => obj != null
                && (ReferenceEquals(this, obj)
                    || obj is ScalarSubqueryExpression subSelectExpression
                    && Equals(subSelectExpression));

        private bool Equals(ScalarSubqueryExpression scalarSubqueryExpression)
            => base.Equals(scalarSubqueryExpression)
                && Subquery.Equals(scalarSubqueryExpression.Subquery);

        public override int GetHashCode() => HashCode.Combine(base.GetHashCode(), Subquery);
    }
}
