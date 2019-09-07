// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class CollectionShaperExpression : Expression, IPrintableExpression
    {
        public CollectionShaperExpression(
            Expression projection,
            Expression innerShaper,
            INavigation navigation,
            Type elementType)
        {
            Projection = projection;
            InnerShaper = innerShaper;
            Navigation = navigation;
            ElementType = elementType ?? navigation.ClrType.TryGetSequenceType();
        }

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            var projection = visitor.Visit(Projection);
            var innerShaper = visitor.Visit(InnerShaper);

            return Update(projection, innerShaper);
        }

        public virtual CollectionShaperExpression Update(Expression projection, Expression innerShaper)
            => projection != Projection || innerShaper != InnerShaper
                ? new CollectionShaperExpression(projection, innerShaper, Navigation, ElementType)
                : this;

        public sealed override ExpressionType NodeType => ExpressionType.Extension;
        public override Type Type => Navigation?.ClrType ?? typeof(List<>).MakeGenericType(ElementType);

        public virtual Expression Projection { get; }
        public virtual Expression InnerShaper { get; }
        public virtual INavigation Navigation { get; }
        public virtual Type ElementType { get; }

        public virtual void Print(ExpressionPrinter expressionPrinter)
        {
            expressionPrinter.AppendLine("CollectionShaper:");
            using (expressionPrinter.Indent())
            {
                expressionPrinter.Append("(");
                expressionPrinter.Visit(Projection);
                expressionPrinter.Append(", ");
                expressionPrinter.Visit(InnerShaper);
                expressionPrinter.AppendLine($", {Navigation?.Name})");
            }
        }
    }
}
