// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Relational.Query.Pipeline.SqlExpressions
{
    public class SqlNegateExpression : SqlExpression
    {
        public SqlNegateExpression(SqlExpression operand, RelationalTypeMapping typeMapping)
            : base(operand.Type, typeMapping, false, true)
        {
            Operand = operand.ConvertToValue(true);
        }

        private SqlNegateExpression(SqlExpression operand, RelationalTypeMapping typeMapping, bool treatAsValue)
            : base(operand.Type, typeMapping, false, treatAsValue)
        {
            Operand = operand;
        }

        public SqlExpression Operand { get; }

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            var operand = (SqlExpression)visitor.Visit(Operand);

            return operand != Operand
                ? new SqlNegateExpression(operand, TypeMapping, ShouldBeValue)
                : this;
        }

        public override SqlExpression ConvertToValue(bool treatAsValue)
        {
            return new SqlNegateExpression(Operand, TypeMapping, treatAsValue);
        }

        public override bool Equals(object obj)
            => obj != null
            && (ReferenceEquals(this, obj)
                || obj is SqlNotExpression sqlNotExpression
                    && Equals(sqlNotExpression));

        private bool Equals(SqlNotExpression sqlNotExpression)
            => base.Equals(sqlNotExpression)
            && Operand.Equals(sqlNotExpression.Operand);

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
