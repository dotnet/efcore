// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Relational.Query.Pipeline.SqlExpressions
{
    public class InExpression : SqlExpression
    {
        #region Fields & Constructors
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
        #endregion

        #region Public Properties

        public SqlExpression Item { get; }
        public bool Negated { get; }
        public SqlExpression Values { get; }
        public SelectExpression Subquery { get; }
        #endregion

        #region Expression-based methods
        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            var newItem = (SqlExpression)visitor.Visit(Item);
            var subquery = (SelectExpression)visitor.Visit(Subquery);
            var values = (SqlExpression)visitor.Visit(Values);

            return Update(newItem, values, subquery);
        }

        public InExpression Negate()
        {
            return new InExpression(Item, !Negated, Values, Subquery, TypeMapping);
        }

        public InExpression Update(SqlExpression item, SqlExpression values, SelectExpression subquery)
        {
            return item != Item || subquery != Subquery || values != Values
                ? new InExpression(item, Negated, values, subquery, TypeMapping)
                : this;
        }
        #endregion

        #region Equality & HashCode
        public override bool Equals(object obj)
            => obj != null
            && (ReferenceEquals(this, obj)
                || obj is InExpression inExpression
                    && Equals(inExpression));

        private bool Equals(InExpression inExpression)
            => base.Equals(inExpression)
            && Item.Equals(inExpression.Item)
            && Negated.Equals(inExpression.Negated)
            && Values == null ? inExpression.Values == null : Values.Equals(inExpression.Values)
            && Subquery == null ? inExpression.Subquery == null : Subquery.Equals(inExpression.Subquery);

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ Item.GetHashCode();
                hashCode = (hashCode * 397) ^ Negated.GetHashCode();
                hashCode = (hashCode * 397) ^ (Values?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (Subquery?.GetHashCode() ?? 0);

                return hashCode;
            }
        }
        #endregion
    }
}
