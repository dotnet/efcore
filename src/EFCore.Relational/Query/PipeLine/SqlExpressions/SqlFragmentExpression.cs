// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;

namespace Microsoft.EntityFrameworkCore.Relational.Query.Pipeline.SqlExpressions
{
    public class SqlFragmentExpression : SqlExpression
    {
        public SqlFragmentExpression(string sql)
            : base(typeof(string), null, false, true)
        {
            Sql = sql;
        }

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            return this;
        }

        public override SqlExpression ConvertToValue(bool treatAsValue)
        {
            return this;
        }

        public string Sql { get; }

        public override bool Equals(object obj)
            => obj != null
            && (ReferenceEquals(this, obj)
                || obj is SqlFragmentExpression sqlFragmentExpression
                    && Equals(sqlFragmentExpression));

        private bool Equals(SqlFragmentExpression sqlFragmentExpression)
            => base.Equals(sqlFragmentExpression)
            && string.Equals(Sql, sqlFragmentExpression.Sql);

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ Sql.GetHashCode();

                return hashCode;
            }
        }
    }
}
