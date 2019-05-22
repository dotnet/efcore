// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query.Expressions.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;

namespace Microsoft.EntityFrameworkCore.Query.Pipeline
{
    public abstract class ShapedQueryExpression : Expression, IPrintable
    {
        public Expression QueryExpression { get; set; }
        public ResultType ResultType { get; set; }

        public Expression ShaperExpression { get; set; }

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

    public enum ResultType
    {
#pragma warning disable SA1602 // Enumeration items should be documented
        Enumerable,
        Single,
        SingleWithDefault
#pragma warning restore SA1602 // Enumeration items should be documented
    }

}
