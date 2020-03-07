// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class ShapedQueryExpression : Expression, IPrintableExpression
    {
        public ShapedQueryExpression([NotNull] Expression queryExpression, [NotNull] Expression shaperExpression)
            : this(Check.NotNull(queryExpression, nameof(queryExpression)),
                  Check.NotNull(shaperExpression, nameof(shaperExpression)),
                  ResultCardinality.Enumerable)
        {
        }

        private ShapedQueryExpression(
            [NotNull] Expression queryExpression, [NotNull] Expression shaperExpression, ResultCardinality resultCardinality)
        {
            QueryExpression = queryExpression;
            ShaperExpression = shaperExpression;
            ResultCardinality = resultCardinality;
        }

        public virtual Expression QueryExpression { get; }

        public virtual ResultCardinality ResultCardinality { get; }

        public virtual Expression ShaperExpression { get; }

        public override Type Type => ResultCardinality == ResultCardinality.Enumerable
            ? typeof(IQueryable<>).MakeGenericType(ShaperExpression.Type)
            : ShaperExpression.Type;

        public sealed override ExpressionType NodeType => ExpressionType.Extension;

        protected override Expression VisitChildren(ExpressionVisitor visitor)
            => throw new InvalidOperationException(CoreStrings.VisitIsNotAllowed($"{nameof(ShapedQueryExpression)}.{nameof(VisitChildren)}"));

        public virtual ShapedQueryExpression Update([NotNull] Expression queryExpression, [NotNull] Expression shaperExpression)
        {
            Check.NotNull(queryExpression, nameof(queryExpression));
            Check.NotNull(shaperExpression, nameof(shaperExpression));

            return queryExpression != QueryExpression || shaperExpression != ShaperExpression
                ? new ShapedQueryExpression(queryExpression, shaperExpression, ResultCardinality)
                : this;
        }

        public virtual ShapedQueryExpression UpdateShaperExpression([NotNull] Expression shaperExpression)
        {
            Check.NotNull(shaperExpression, nameof(shaperExpression));

            return shaperExpression != ShaperExpression
                ? new ShapedQueryExpression(QueryExpression, shaperExpression, ResultCardinality)
                : this;
        }

        public virtual ShapedQueryExpression UpdateResultCardinality(ResultCardinality resultCardinality)
            => new ShapedQueryExpression(QueryExpression, ShaperExpression, resultCardinality);

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
