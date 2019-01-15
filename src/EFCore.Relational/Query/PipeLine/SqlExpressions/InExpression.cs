// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Relational.Query.Pipeline.SqlExpressions
{
    public class InExpression : SqlExpression
    {
        public InExpression(SqlExpression item, bool negated, SelectExpression subquery, RelationalTypeMapping typeMapping)
            : base(typeof(bool), typeMapping, true, false)
        {
            Item = item;
            Negated = negated;
            Subquery = subquery;
        }

        public InExpression(SqlExpression item, bool negated, SqlExpression values, RelationalTypeMapping typeMapping)
            : base(typeof(bool), typeMapping, true, false)
        {
            Item = item;
            Negated = negated;
            Values = values;
        }

        private InExpression(SqlExpression item, bool negated, SqlExpression values, SelectExpression subquery,
            RelationalTypeMapping typeMapping, bool treatAsValue)
            : base(typeof(bool), typeMapping, true, treatAsValue)
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

            return newItem != Item || subquery != Subquery || values != Values
                ? new InExpression(newItem, Negated, values, subquery, TypeMapping, ShouldBeValue)
                : this;
        }

        public override SqlExpression ConvertToValue(bool treatAsValue)
        {
            return new InExpression(Item, Negated, Values, Subquery, TypeMapping, treatAsValue);
        }

        public override bool Equals(object obj)
            => obj != null
            && (ReferenceEquals(this, obj)
                || obj is InExpression inExpression
                    && Equals(inExpression));

        private bool Equals(InExpression inExpression)
            => base.Equals(inExpression)
            && Item.Equals(inExpression.Item)
            && Values?.Equals(inExpression.Values) == true
            && Subquery?.Equals(inExpression.Subquery) == true;

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ Item.GetHashCode();
                hashCode = (hashCode * 397) ^ (Values?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (Subquery?.GetHashCode() ?? 0);

                return hashCode;
            }
        }
    }
}
