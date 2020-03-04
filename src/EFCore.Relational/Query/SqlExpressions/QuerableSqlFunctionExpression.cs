// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query.SqlExpressions
{
    /// <summary>
    ///     Represents a SQL Table Valued Fuction in the sql generation tree.
    /// </summary>
    public sealed class QueryableSqlFunctionExpression : TableExpressionBase
    {
        public QueryableSqlFunctionExpression([NotNull] SqlFunctionExpression expression, [CanBeNull] string alias)
            : base(alias)
        {
            Check.NotNull(expression, nameof(expression));

            SqlFunctionExpression = expression;
        }

        public SqlFunctionExpression SqlFunctionExpression { get; }
        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            Check.NotNull(visitor, nameof(visitor));

            var sqlFunctionExpression = (SqlFunctionExpression)visitor.Visit(SqlFunctionExpression);

            return Update(sqlFunctionExpression);
        }
        public QueryableSqlFunctionExpression Update([NotNull] SqlFunctionExpression sqlFunctionExpression)
        {
            Check.NotNull(sqlFunctionExpression, nameof(sqlFunctionExpression));

            return sqlFunctionExpression != SqlFunctionExpression
                ? new QueryableSqlFunctionExpression(sqlFunctionExpression, Alias)
                : this;
        }

        public override void Print(ExpressionPrinter expressionPrinter)
        {
            expressionPrinter.Append("(");
            expressionPrinter.Visit(SqlFunctionExpression);
            expressionPrinter.AppendLine()
                .AppendLine($") AS {Alias}");
        }

        public override bool Equals(object obj)
            => obj != null
                && (ReferenceEquals(this, obj)
                    || obj is QueryableSqlFunctionExpression queryableExpression
                    && Equals(queryableExpression));

        private bool Equals(QueryableSqlFunctionExpression queryableExpression)
            => base.Equals(queryableExpression)
                && SqlFunctionExpression.Equals(queryableExpression.SqlFunctionExpression);

        public override int GetHashCode() => HashCode.Combine(base.GetHashCode(), SqlFunctionExpression);
    }
}
