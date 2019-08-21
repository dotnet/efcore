// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;

namespace Microsoft.EntityFrameworkCore.Query.SqlExpressions
{
    public class OuterApplyExpression : JoinExpressionBase
    {
        public OuterApplyExpression(TableExpressionBase table)
            : base(table)
        {
        }

        protected override Expression VisitChildren(ExpressionVisitor visitor)
            => Update((TableExpressionBase)visitor.Visit(Table));

        public virtual OuterApplyExpression Update(TableExpressionBase table)
            => table != Table
                ? new OuterApplyExpression(table)
                : this;

        public override void Print(ExpressionPrinter expressionPrinter)
        {
            expressionPrinter.Append("OUTER APPLY ");
            expressionPrinter.Visit(Table);
        }

        public override bool Equals(object obj)
            => obj != null
            && (ReferenceEquals(this, obj)
                || obj is OuterApplyExpression outerApplyExpression
                    && Equals(outerApplyExpression));

        private bool Equals(OuterApplyExpression outerApplyExpression)
            => base.Equals(outerApplyExpression);

        public override int GetHashCode() => base.GetHashCode();
    }
}
