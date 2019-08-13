// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;

namespace Microsoft.EntityFrameworkCore.Query.SqlExpressions
{
    public class InnerJoinLateralExpression : JoinExpressionBase
    {
        public InnerJoinLateralExpression(TableExpressionBase table)
            : base(table)
        {
        }

        protected override Expression VisitChildren(ExpressionVisitor visitor)
            => Update((TableExpressionBase)visitor.Visit(Table));

        public virtual InnerJoinLateralExpression Update(TableExpressionBase table)
            => table != Table
                ? new InnerJoinLateralExpression(table)
                : this;

        public override void Print(ExpressionPrinter expressionPrinter)
        {
            expressionPrinter.StringBuilder.Append("INNER JOIN LATERAL ");
            expressionPrinter.Visit(Table);
        }

        public override bool Equals(object obj)
            => obj != null
            && (ReferenceEquals(this, obj)
                || obj is InnerJoinLateralExpression innerJoinLateralExpression
                    && Equals(innerJoinLateralExpression));

        private bool Equals(InnerJoinLateralExpression innerJoinLateralExpression)
            => base.Equals(innerJoinLateralExpression);

        public override int GetHashCode() => base.GetHashCode();
    }
}
