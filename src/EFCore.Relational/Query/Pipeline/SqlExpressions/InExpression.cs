// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Relational.Query.Pipeline.SqlExpressions
{
    public class InExpression : SqlExpression
    {
        public InExpression(SqlExpression item, bool negated, SelectExpression subquery, RelationalTypeMapping typeMapping)
            : this(item, negated, null, subquery, typeMapping)
        {
        }

        public InExpression(SqlExpression item, bool negated, SqlExpression values, RelationalTypeMapping typeMapping)
            : this(item, negated, values, null, typeMapping)
        {
        }

        private InExpression(SqlExpression item, bool negated, SqlExpression values, SelectExpression subquery,
            RelationalTypeMapping typeMapping)
            : base(typeof(bool), typeMapping)
        {
            Item = item;
            Negated = negated;
            Subquery = subquery;
            Values = values;
        }

        public SqlExpression Item { get; }
        public bool Negated { get; }
        public SqlExpression Values { get; }
        public SelectExpression Subquery { get; }

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            var newItem = (SqlExpression)visitor.Visit(Item);
            var subquery = (SelectExpression)visitor.Visit(Subquery);
            var values = (SqlExpression)visitor.Visit(Values);

            return Update(newItem, values, subquery);
        }

        public InExpression Negate() => new InExpression(Item, !Negated, Values, Subquery, TypeMapping);

        public InExpression Update(SqlExpression item, SqlExpression values, SelectExpression subquery)
            => item != Item || subquery != Subquery || values != Values
                ? new InExpression(item, Negated, values, subquery, TypeMapping)
                : this;

        public override void Print(ExpressionPrinter expressionPrinter)
        {
            expressionPrinter.Visit(Item);
            expressionPrinter.StringBuilder.Append(Negated ? " NOT IN " : " IN ");
            expressionPrinter.StringBuilder.Append("(");

            if (Values is SqlConstantExpression constantValuesExpression
                && constantValuesExpression.Value is IEnumerable constantValues)
            {
                var first = true;
                foreach (var item in constantValues)
                {
                    if (!first)
                    {
                        expressionPrinter.StringBuilder.Append(", ");
                    }

                    first = false;
                    expressionPrinter.StringBuilder.Append(constantValuesExpression.TypeMapping?.GenerateSqlLiteral(item) ?? item?.ToString() ?? "NULL");
                }
            }
            else
            {
                expressionPrinter.Visit(Values);
            }

            expressionPrinter.StringBuilder.Append(")");
        }

        public override bool Equals(object obj)
            => obj != null
            && (ReferenceEquals(this, obj)
                || obj is InExpression inExpression
                    && Equals(inExpression));

        private bool Equals(InExpression inExpression)
            => base.Equals(inExpression)
            && Item.Equals(inExpression.Item)
            && Negated.Equals(inExpression.Negated)
            && (Values == null ? inExpression.Values == null : Values.Equals(inExpression.Values))
            && (Subquery == null ? inExpression.Subquery == null : Subquery.Equals(inExpression.Subquery));

        public override int GetHashCode() => HashCode.Combine(base.GetHashCode(), Item, Negated, Values, Subquery);
    }
}
