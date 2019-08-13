// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;

namespace Microsoft.EntityFrameworkCore.Query.SqlExpressions
{
    public class SqlFragmentExpression : SqlExpression
    {
        internal SqlFragmentExpression(string sql)
            : base(typeof(string), null)
        {
            Sql = sql;
        }

        public string Sql { get; }
        protected override Expression VisitChildren(ExpressionVisitor visitor) => this;
        public override void Print(ExpressionPrinter expressionPrinter)
            => expressionPrinter.Append(Sql);

        public override bool Equals(object obj)
            => obj != null
            && (ReferenceEquals(this, obj)
                || obj is SqlFragmentExpression sqlFragmentExpression
                    && Equals(sqlFragmentExpression));

        private bool Equals(SqlFragmentExpression sqlFragmentExpression)
            => base.Equals(sqlFragmentExpression)
            && string.Equals(Sql, sqlFragmentExpression.Sql);

        public override int GetHashCode() => HashCode.Combine(base.GetHashCode(), Sql);
    }
}
