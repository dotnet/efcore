// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query.Internal;

namespace Microsoft.EntityFrameworkCore.Relational.Query.Pipeline.SqlExpressions
{
    public class CrossJoinExpression : JoinExpressionBase
    {
        public CrossJoinExpression(TableExpressionBase table)
            : base(table)
        {
        }

        protected override Expression VisitChildren(ExpressionVisitor visitor)
            => Update((TableExpressionBase)visitor.Visit(Table));

        public CrossJoinExpression Update(TableExpressionBase table)
            => table != Table
                ? new CrossJoinExpression(table)
                : this;

        public override void Print(ExpressionPrinter expressionPrinter)
        {
            expressionPrinter.StringBuilder.Append("CROSS JOIN ");
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
