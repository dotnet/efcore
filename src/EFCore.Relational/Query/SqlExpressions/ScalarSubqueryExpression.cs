// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query.SqlExpressions
{
    public class ScalarSubqueryExpression : SqlExpression
    {
        public ScalarSubqueryExpression([NotNull] SelectExpression subquery)
            : base(Verify(subquery).Projection[0].Type, subquery.Projection[0].Expression.TypeMapping)
        {
            Check.NotNull(subquery, nameof(subquery));

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
        {
            Check.NotNull(visitor, nameof(visitor));

            return Update((SelectExpression)visitor.Visit(Subquery));
        }

        public virtual ScalarSubqueryExpression Update([NotNull] SelectExpression subquery)
        {
            Check.NotNull(subquery, nameof(subquery));

            return subquery != Subquery
                ? new ScalarSubqueryExpression(subquery)
                : this;
        }

        public override void Print(ExpressionPrinter expressionPrinter)
        {
            Check.NotNull(expressionPrinter, nameof(expressionPrinter));

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
                    || obj is ScalarSubqueryExpression scalarSubqueryExpression
                    && Equals(scalarSubqueryExpression));

        private bool Equals(ScalarSubqueryExpression scalarSubqueryExpression)
            => base.Equals(scalarSubqueryExpression)
                && Subquery.Equals(scalarSubqueryExpression.Subquery);

        public override int GetHashCode() => HashCode.Combine(base.GetHashCode(), Subquery);
    }
}
