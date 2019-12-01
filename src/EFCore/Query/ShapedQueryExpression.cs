// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class ShapedQueryExpression : Expression, IPrintableExpression
    {
        private Expression _queryExpression;
        private Expression _shaperExpression;

        public ShapedQueryExpression([NotNull] Expression queryExpression, [NotNull] Expression shaperExpression)
        {
            Check.NotNull(queryExpression, nameof(queryExpression));
            Check.NotNull(shaperExpression, nameof(shaperExpression));

            QueryExpression = queryExpression;
            ShaperExpression = shaperExpression;
        }

        public virtual Expression QueryExpression
        {
            get => _queryExpression;
            [param: NotNull] set => _queryExpression = Check.NotNull(value, nameof(value));
        }

        public virtual ResultCardinality ResultCardinality { get; set; }

        public virtual Expression ShaperExpression
        {
            get => _shaperExpression;
            [param: NotNull] set => _shaperExpression = Check.NotNull(value, nameof(value));
        }

        public override Type Type => ResultCardinality == ResultCardinality.Enumerable
            ? typeof(IQueryable<>).MakeGenericType(ShaperExpression.Type)
            : ShaperExpression.Type;

        public sealed override ExpressionType NodeType => ExpressionType.Extension;

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            Check.NotNull(visitor, nameof(visitor));

            QueryExpression = visitor.Visit(QueryExpression);

            return this;
        }

        public virtual void Print(ExpressionPrinter expressionPrinter)
        {
            Check.NotNull(expressionPrinter, nameof(expressionPrinter));

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
