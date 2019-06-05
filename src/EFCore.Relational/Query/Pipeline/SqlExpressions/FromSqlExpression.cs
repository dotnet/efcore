// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query.Internal;

namespace Microsoft.EntityFrameworkCore.Relational.Query.Pipeline.SqlExpressions
{
    public class FromSqlExpression : TableExpressionBase
    {
        public FromSqlExpression([NotNull] string sql, Expression arguments, [NotNull] string alias)
            : base(alias)
        {
            Sql = sql;
            Arguments = arguments;
        }

        public string Sql { get; }
        public Expression Arguments { get; }

        protected override Expression VisitChildren(ExpressionVisitor visitor) => this;

        public override void Print(ExpressionPrinter expressionPrinter)
            => expressionPrinter.StringBuilder.Append(Sql);

        public override bool Equals(object obj)
            => obj != null
               && (ReferenceEquals(this, obj)
                   || obj is FromSqlExpression fromSqlExpression
                   && Equals(fromSqlExpression));

        private bool Equals(FromSqlExpression fromSqlExpression)
            => base.Equals(fromSqlExpression)
               && string.Equals(Sql, fromSqlExpression.Sql);

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
