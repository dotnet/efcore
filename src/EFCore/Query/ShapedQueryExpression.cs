// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Linq.Expressions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class ShapedQueryExpression : Expression, IPrintableExpression
    {
        public ShapedQueryExpression(Expression queryExpression, Expression shaperExpression)
        {
            QueryExpression = queryExpression;
            ShaperExpression = shaperExpression;
        }

        public virtual Expression QueryExpression { get; set; }
        public virtual ResultCardinality ResultCardinality { get; set; }
        public virtual Expression ShaperExpression { get; set; }

        public override Type Type => ResultCardinality == ResultCardinality.Enumerable
            ? typeof(IQueryable<>).MakeGenericType(ShaperExpression.Type)
            : ShaperExpression.Type;

        public sealed override ExpressionType NodeType => ExpressionType.Extension;

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            QueryExpression = visitor.Visit(QueryExpression);

            return this;
        }

        public virtual void Print(ExpressionPrinter expressionPrinter)
        {
            expressionPrinter.AppendLine(nameof(ShapedQueryExpression) + ": ");
            using (expressionPrinter.Indent())
            {
                expressionPrinter.AppendLine(nameof(QueryExpression) + ": ");
                using (expressionPrinter.Indent())
                {
                    expressionPrinter.Visit(QueryExpression);
                }

                expressionPrinter.AppendLine().Append(nameof(ShaperExpression) + ": ");
                using (expressionPrinter.Indent())
                {
                    expressionPrinter.Visit(ShaperExpression);
                }

                expressionPrinter.AppendLine();
            }
        }
    }
}
