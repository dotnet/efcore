// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Relational.Query.Pipeline.SqlExpressions
{
    public class SqlCastExpression : SqlExpression
    {
        public SqlCastExpression(
            SqlExpression operand,
            Type type,
            RelationalTypeMapping typeMapping)
            : base(type, typeMapping, false, true)
        {
            Check.NotNull(operand, nameof(operand));

            Operand = operand.ConvertToValue(true);
        }

        private SqlCastExpression(
            SqlExpression operand,
            Type type,
            RelationalTypeMapping typeMapping,
            bool treatAsValue)
            : base(type, typeMapping, false, treatAsValue)
        {
            Operand = operand;
        }

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            var operand = (SqlExpression)visitor.Visit(Operand);

            return operand != Operand
                ? new SqlCastExpression(operand, Type, TypeMapping, ShouldBeValue)
                : this;
        }

        public override SqlExpression ConvertToValue(bool treatAsValue)
        {
            return new SqlCastExpression(Operand, Type, TypeMapping, treatAsValue);
        }

        public SqlExpression Operand { get; }

        public override bool Equals(object obj)
            => obj != null
            && (ReferenceEquals(this, obj)
                || obj is SqlCastExpression sqlCastExpression
                    && Equals(sqlCastExpression));

        private bool Equals(SqlCastExpression sqlCastExpression)
            => base.Equals(sqlCastExpression)
            && Operand.Equals(sqlCastExpression.Operand);

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ Operand.GetHashCode();

                return hashCode;
            }
        }
    }
}
