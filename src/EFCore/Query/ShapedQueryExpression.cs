// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Linq.Expressions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class ShapedQueryExpression : Expression, IPrintable
    {
        public ShapedQueryExpression(Expression queryExpression, Expression shaperExpression)
        {
            QueryExpression = queryExpression;
            ShaperExpression = shaperExpression;
        }

        public virtual Expression QueryExpression { get; set; }
        public virtual ResultCardinality ResultCardinality { get; set; }
        public virtual Expression ShaperExpression { get; set; }
        public override Type Type => typeof(IQueryable<>).MakeGenericType(ShaperExpression.Type);
        public override ExpressionType NodeType => ExpressionType.Extension;
        public override bool CanReduce => false;

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            QueryExpression = visitor.Visit(QueryExpression);

            return this;
        }

        public virtual void Print(ExpressionPrinter expressionPrinter)
        {
            expressionPrinter.StringBuilder.AppendLine(nameof(ShapedQueryExpression) + ": ");
            using (expressionPrinter.StringBuilder.Indent())
            {
                expressionPrinter.StringBuilder.AppendLine(nameof(QueryExpression) + ": ");
                using (expressionPrinter.StringBuilder.Indent())
                {
                    expressionPrinter.Visit(QueryExpression);
                }

                expressionPrinter.StringBuilder.AppendLine().Append(nameof(ShaperExpression) + ": ");
                using (expressionPrinter.StringBuilder.Indent())
                {
                    expressionPrinter.Visit(ShaperExpression);
                }

                expressionPrinter.StringBuilder.AppendLine();
            }
        }
    }
}
