// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query.SqlExpressions
{
    public class ExistsExpression : SqlExpression
    {
        public ExistsExpression(
            [NotNull] SelectExpression subquery,
            bool negated,
            [CanBeNull] RelationalTypeMapping typeMapping)
            : base(typeof(bool), typeMapping)
        {
            Check.NotNull(subquery, nameof(subquery));

            Subquery = subquery;
            IsNegated = negated;
        }

        public virtual SelectExpression Subquery { get; }
        public virtual bool IsNegated { get; }

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            Check.NotNull(visitor, nameof(visitor));

            return Update((SelectExpression)visitor.Visit(Subquery));
        }

        public virtual ExistsExpression Update([NotNull] SelectExpression subquery)
        {
            Check.NotNull(subquery, nameof(subquery));

            return subquery != Subquery
                ? new ExistsExpression(subquery, IsNegated, TypeMapping)
                : this;
        }

        public override void Print(ExpressionPrinter expressionPrinter)
        {
            Check.NotNull(expressionPrinter, nameof(expressionPrinter));

            if (IsNegated)
            {
                expressionPrinter.Append("NOT ");
            }

            expressionPrinter.AppendLine("EXISTS (");
            using (expressionPrinter.Indent())
            {
                expressionPrinter.Visit(Subquery);
            }

            expressionPrinter.Append(")");
        }

        public override bool Equals(object obj)
            => obj != null
                && (ReferenceEquals(this, obj)
                    || obj is ExistsExpression existsExpression
                    && Equals(existsExpression));

        private bool Equals(ExistsExpression existsExpression)
            => base.Equals(existsExpression)
                && Subquery.Equals(existsExpression.Subquery)
                && IsNegated == existsExpression.IsNegated;

        public override int GetHashCode() => HashCode.Combine(base.GetHashCode(), Subquery, IsNegated);
    }
}
