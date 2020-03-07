// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query.SqlExpressions
{
    public class CrossApplyExpression : JoinExpressionBase
    {
        public CrossApplyExpression([NotNull] TableExpressionBase table)
            : base(table)
        {
        }

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            Check.NotNull(visitor, nameof(visitor));

            return Update((TableExpressionBase)visitor.Visit(Table));
        }

        public virtual CrossApplyExpression Update([NotNull] TableExpressionBase table)
        {
            Check.NotNull(table, nameof(table));

            return table != Table
                ? new CrossApplyExpression(table)
                : this;
        }

        public override void Print(ExpressionPrinter expressionPrinter)
        {
            Check.NotNull(expressionPrinter, nameof(expressionPrinter));

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
