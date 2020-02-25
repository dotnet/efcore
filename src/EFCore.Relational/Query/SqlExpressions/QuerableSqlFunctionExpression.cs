// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query.SqlExpressions
{
    /// <summary>
    ///     Represents a SQL Table Valued Fuction in the sql generation tree.
    /// </summary>
    public class QueryableSqlFunctionExpression : TableExpressionBase
    {
        public QueryableSqlFunctionExpression([NotNull] SqlFunctionExpression expression, [CanBeNull] string alias)
            : base(alias)
        {
            Check.NotNull(expression, nameof(expression));

            SqlFunctionExpression = expression;
        }

        public virtual SqlFunctionExpression SqlFunctionExpression { get; }

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
