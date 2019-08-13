// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Query.SqlExpressions
{
    public class SqlConstantExpression : SqlExpression
    {
        private readonly ConstantExpression _constantExpression;

        public SqlConstantExpression(ConstantExpression constantExpression, RelationalTypeMapping typeMapping)
            : base(constantExpression.Type, typeMapping)
        {
            _constantExpression = constantExpression;
        }

        public virtual object Value => _constantExpression.Value;

        public virtual SqlExpression ApplyTypeMapping(RelationalTypeMapping typeMapping)
            => new SqlConstantExpression(_constantExpression, typeMapping);
        protected override Expression VisitChildren(ExpressionVisitor visitor) => this;
        public override void Print(ExpressionPrinter expressionPrinter) => Print(Value, expressionPrinter);

        private void Print(object value, ExpressionPrinter expressionPrinter)
            => expressionPrinter.Append(TypeMapping?.GenerateSqlLiteral(value) ?? Value?.ToString() ?? "NULL");

        public override bool Equals(object obj)
            => obj != null
            && (ReferenceEquals(this, obj)
                || obj is SqlConstantExpression sqlConstantExpression
                    && Equals(sqlConstantExpression));

        private bool Equals(SqlConstantExpression sqlConstantExpression)
            => base.Equals(sqlConstantExpression)
            && (Value == null
                ? sqlConstantExpression.Value == null
                : Value.Equals(sqlConstantExpression.Value));

        public override int GetHashCode() => HashCode.Combine(base.GetHashCode(), Value);
    }
}
