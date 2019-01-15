// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Relational.Query.Pipeline.SqlExpressions
{
    public class SqlConstantExpression : SqlExpression
    {
        private readonly ConstantExpression _constantExpression;

        public SqlConstantExpression(ConstantExpression constantExpression, RelationalTypeMapping typeMapping)
            : base(constantExpression.Type, typeMapping, false, true)
        {
            _constantExpression = constantExpression;
        }

        private SqlConstantExpression(ConstantExpression constantExpression, RelationalTypeMapping typeMapping, bool treatAsValue)
            : base(constantExpression.Type, typeMapping, false, treatAsValue)
        {
            _constantExpression = constantExpression;
        }

        public SqlExpression ApplyTypeMapping(RelationalTypeMapping typeMapping)
        {
            return new SqlConstantExpression(_constantExpression, typeMapping);
        }

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            return this;
        }

        public override SqlExpression ConvertToValue(bool treatAsValue)
        {
            return new SqlConstantExpression(_constantExpression, TypeMapping, treatAsValue);
        }

        public object Value => _constantExpression.Value;

        public override bool Equals(object obj)
            => obj != null
            && (ReferenceEquals(this, obj)
                || obj is SqlConstantExpression sqlConstantExpression
                    && Equals(sqlConstantExpression));

        private bool Equals(SqlConstantExpression sqlConstantExpression)
            => base.Equals(sqlConstantExpression)
            && Value.Equals(sqlConstantExpression.Value);

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ Value.GetHashCode();

                return hashCode;
            }
        }
    }
}
