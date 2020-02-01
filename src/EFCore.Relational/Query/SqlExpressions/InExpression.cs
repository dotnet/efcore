// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query.SqlExpressions
{
    public class InExpression : SqlExpression
    {
        public InExpression(
            [NotNull] SqlExpression item,
            bool negated,
            [NotNull] SelectExpression subquery,
            [CanBeNull] RelationalTypeMapping typeMapping)
            : this(item, negated, null, subquery, typeMapping)
        {
            Check.NotNull(item, nameof(item));
            Check.NotNull(subquery, nameof(subquery));
        }

        public InExpression(
            [NotNull] SqlExpression item,
            bool negated,
            [NotNull] SqlExpression values,
            [CanBeNull] RelationalTypeMapping typeMapping)
            : this(item, negated, values, null, typeMapping)
        {
            Check.NotNull(item, nameof(item));
            Check.NotNull(values, nameof(values));
        }

        private InExpression(
            SqlExpression item, bool negated, SqlExpression values, SelectExpression subquery,
            RelationalTypeMapping typeMapping)
            : base(typeof(bool), typeMapping)
        {
            Item = item;
            IsNegated = negated;
            Subquery = subquery;
            Values = values;
        }

        public virtual SqlExpression Item { get; }
        public virtual bool IsNegated { get; }
        public virtual SqlExpression Values { get; }
        public virtual SelectExpression Subquery { get; }

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            Check.NotNull(visitor, nameof(visitor));

            var item = (SqlExpression)visitor.Visit(Item);
            var subquery = (SelectExpression)visitor.Visit(Subquery);
            var values = (SqlExpression)visitor.Visit(Values);

            return Update(item, values, subquery);
        }

        public virtual InExpression Negate() => new InExpression(Item, !IsNegated, Values, Subquery, TypeMapping);

        public virtual InExpression Update(
            [NotNull] SqlExpression item, [CanBeNull] SqlExpression values, [CanBeNull] SelectExpression subquery)
        {
            Check.NotNull(item, nameof(item));

            if (values != null
                && subquery != null)
            {
                throw new ArgumentException($"Either {nameof(values)} or {nameof(subquery)} must be null");
            }

            return item != Item || subquery != Subquery || values != Values
                ? new InExpression(item, IsNegated, values, subquery, TypeMapping)
                : this;
        }

        public override void Print(ExpressionPrinter expressionPrinter)
        {
            Check.NotNull(expressionPrinter, nameof(expressionPrinter));

            expressionPrinter.Visit(Item);
            expressionPrinter.Append(IsNegated ? " NOT IN " : " IN ");
            expressionPrinter.Append("(");

            if (Values is SqlConstantExpression constantValuesExpression
                && constantValuesExpression.Value is IEnumerable constantValues)
            {
                var first = true;
                foreach (var item in constantValues)
                {
                    if (!first)
                    {
                        expressionPrinter.Append(", ");
                    }

                    first = false;
                    expressionPrinter.Append(constantValuesExpression.TypeMapping?.GenerateSqlLiteral(item) ?? item?.ToString() ?? "NULL");
                }
            }
            else
            {
                expressionPrinter.Visit(Values);
            }

            expressionPrinter.Append(")");
        }

        public override bool Equals(object obj)
            => obj != null
                && (ReferenceEquals(this, obj)
                    || obj is InExpression inExpression
                    && Equals(inExpression));

        private bool Equals(InExpression inExpression)
            => base.Equals(inExpression)
                && Item.Equals(inExpression.Item)
                && IsNegated.Equals(inExpression.IsNegated)
                && (Values == null ? inExpression.Values == null : Values.Equals(inExpression.Values))
                && (Subquery == null ? inExpression.Subquery == null : Subquery.Equals(inExpression.Subquery));

        public override int GetHashCode() => HashCode.Combine(base.GetHashCode(), Item, IsNegated, Values, Subquery);
    }
}
