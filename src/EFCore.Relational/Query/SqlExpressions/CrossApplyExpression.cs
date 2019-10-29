// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;

namespace Microsoft.EntityFrameworkCore.Query.SqlExpressions
{
    public class CrossApplyExpression : JoinExpressionBase
    {
        public CrossApplyExpression(TableExpressionBase table)
            : base(table)
        {
        }

        protected override Expression VisitChildren(ExpressionVisitor visitor)
            => Update((TableExpressionBase)visitor.Visit(Table));

        public virtual CrossApplyExpression Update(TableExpressionBase table)
            => table != Table
                ? new CrossApplyExpression(table)
                : this;

        public override void Print(ExpressionPrinter expressionPrinter)
        {
            expressionPrinter.Append("CROSS APPLY ");
            expressionPrinter.Visit(Table);
        }

        public override bool Equals(object obj)
            => obj != null
                && (ReferenceEquals(this, obj)
                    || obj is CrossApplyExpression crossApplyExpression
                    && Equals(crossApplyExpression));

        private bool Equals(CrossApplyExpression crossApplyExpression)
            => base.Equals(crossApplyExpression);

        public override int GetHashCode() => base.GetHashCode();
    }
}
