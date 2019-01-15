// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Relational.Query.Pipeline.SqlExpressions
{
    public class SqlNullExpression : SqlExpression
    {
        public SqlNullExpression(
            SqlExpression operand,
            bool negated,
            RelationalTypeMapping typeMapping)
            : base(typeof(bool), typeMapping, true, false)
        {
            Check.NotNull(operand, nameof(operand));

            Operand = operand.ConvertToValue(true);
            Negated = negated;
        }

        private SqlNullExpression(
            SqlExpression operand,
            bool negated,
            RelationalTypeMapping typeMapping,
            bool treatAsValue)
            : base(typeof(bool), typeMapping, true, treatAsValue)
        {
            Operand = operand;
            Negated = negated;
        }

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            var operand = (SqlExpression)visitor.Visit(Operand);

            return operand != Operand
                ? new SqlNullExpression(operand, Negated, TypeMapping, ShouldBeValue)
                : this;
        }

        public override SqlExpression ConvertToValue(bool treatAsValue)
        {
            return new SqlNullExpression(Operand, Negated, TypeMapping, treatAsValue);
        }

        public SqlExpression Operand { get; }
        public bool Negated { get; }

        public override bool Equals(object obj)
            => obj != null
            && (ReferenceEquals(this, obj)
                || obj is SqlNullExpression sqlNullExpression
                    && Equals(sqlNullExpression));

        private bool Equals(SqlNullExpression sqlNullExpression)
            => base.Equals(sqlNullExpression)
            && Operand.Equals(sqlNullExpression.Operand)
            && Negated == sqlNullExpression.Negated;

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ Operand.GetHashCode();
                hashCode = (hashCode * 397) ^ Negated.GetHashCode();

                return hashCode;
            }
        }
    }
}
