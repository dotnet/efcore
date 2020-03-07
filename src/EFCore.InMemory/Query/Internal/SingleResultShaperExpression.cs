// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.InMemory.Query.Internal
{
    public class SingleResultShaperExpression : Expression, IPrintableExpression
    {
        public SingleResultShaperExpression(
            [NotNull] Expression projection,
            [NotNull] Expression innerShaper,
            [NotNull] Type type)
        {
            Projection = projection;
            InnerShaper = innerShaper;
            Type = type;
        }

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            Check.NotNull(visitor, nameof(visitor));

            var projection = visitor.Visit(Projection);
            var innerShaper = visitor.Visit(InnerShaper);

            return Update(projection, innerShaper);
        }

        public virtual SingleResultShaperExpression Update([NotNull] Expression projection, [NotNull] Expression innerShaper)
            => projection != Projection || innerShaper != InnerShaper
                ? new SingleResultShaperExpression(projection, innerShaper, Type)
                : this;

        public sealed override ExpressionType NodeType => ExpressionType.Extension;
        public override Type Type { get; }

        public virtual Expression Projection { get; }
        public virtual Expression InnerShaper { get; }

        public virtual void Print(ExpressionPrinter expressionPrinter)
        {
            Check.NotNull(expressionPrinter, nameof(expressionPrinter));

            expressionPrinter.AppendLine($"{nameof(SingleResultShaperExpression)}:");
            using (expressionPrinter.Indent())
            {
                expressionPrinter.Append("(");
                expressionPrinter.Visit(Projection);
                expressionPrinter.Append(", ");
                expressionPrinter.Visit(InnerShaper);
                expressionPrinter.AppendLine(")");
            }
        }
    }
}
