// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query.SqlExpressions
{
    public class CrossJoinExpression : JoinExpressionBase
    {
        public CrossJoinExpression([NotNull] TableExpressionBase table)
            : base(table)
        {
        }

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            Check.NotNull(visitor, nameof(visitor));

            return Update((TableExpressionBase)visitor.Visit(Table));
        }

        public virtual CrossJoinExpression Update([NotNull] TableExpressionBase table)
        {
            Check.NotNull(table, nameof(table));

            return table != Table
                ? new CrossJoinExpression(table)
                : this;
        }

        public override void Print(ExpressionPrinter expressionPrinter)
        {
            Check.NotNull(expressionPrinter, nameof(expressionPrinter));

            expressionPrinter.Append("CROSS JOIN ");
            expressionPrinter.Visit(Table);
        }

        public override bool Equals(object obj)
            => obj != null
                && (ReferenceEquals(this, obj)
                    || obj is CrossJoinExpression crossJoinExpression
                    && Equals(crossJoinExpression));

        private bool Equals(CrossJoinExpression crossJoinExpression)
            => base.Equals(crossJoinExpression);

        public override int GetHashCode() => base.GetHashCode();
    }
}
