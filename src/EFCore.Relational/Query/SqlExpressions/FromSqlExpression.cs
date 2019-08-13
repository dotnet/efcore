// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Query.SqlExpressions
{
    public class FromSqlExpression : TableExpressionBase
    {
        public FromSqlExpression([NotNull] string sql, Expression arguments, [NotNull] string alias)
            : base(alias)
        {
            Sql = sql;
            Arguments = arguments;
        }

        public virtual string Sql { get; }
        public virtual Expression Arguments { get; }

        protected override Expression VisitChildren(ExpressionVisitor visitor) => this;

        public override void Print(ExpressionPrinter expressionPrinter)
            => expressionPrinter.Append(Sql);

        public override bool Equals(object obj)
            => obj != null
               && (ReferenceEquals(this, obj)
                   || obj is FromSqlExpression fromSqlExpression
                   && Equals(fromSqlExpression));

        private bool Equals(FromSqlExpression fromSqlExpression)
            => base.Equals(fromSqlExpression)
               && string.Equals(Sql, fromSqlExpression.Sql);

        public override int GetHashCode() => HashCode.Combine(base.GetHashCode(), Sql);
    }
}
