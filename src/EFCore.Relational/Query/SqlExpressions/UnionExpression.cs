// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;

namespace Microsoft.EntityFrameworkCore.Query.SqlExpressions
{
    public class UnionExpression : SetOperationBase
    {
        public UnionExpression(string alias, SelectExpression source1, SelectExpression source2, bool distinct)
            : base(alias, source1, source2, distinct)
        {
        }

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            var source1 = (SelectExpression)visitor.Visit(Source1);
            var source2 = (SelectExpression)visitor.Visit(Source2);

            return Update(source1, source2);
        }

        public virtual UnionExpression Update(SelectExpression source1, SelectExpression source2)
            => source1 != Source1 || source2 != Source2
                ? new UnionExpression(Alias, source1, source2, IsDistinct)
                : this;

        public override void Print(ExpressionPrinter expressionPrinter)
        {
            expressionPrinter.Append("(");
            using (expressionPrinter.Indent())
            {
                expressionPrinter.Visit(Source1);
                expressionPrinter.AppendLine();
                expressionPrinter.Append("UNION");
                if (!IsDistinct)
                {
                    expressionPrinter.AppendLine(" ALL");
                }

                expressionPrinter.Visit(Source2);
            }

            expressionPrinter.AppendLine()
                .AppendLine($") AS {Alias}");
        }

        public override bool Equals(object obj)
            => obj != null
                && (ReferenceEquals(this, obj)
                    || obj is UnionExpression unionExpression
                    && Equals(unionExpression));

        private bool Equals(UnionExpression unionExpression)
            => base.Equals(unionExpression);

        public override int GetHashCode()
            => HashCode.Combine(base.GetHashCode(), GetType());
    }
}
