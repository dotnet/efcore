// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class CollectionShaperExpression : Expression, IPrintableExpression
    {
        public CollectionShaperExpression(
            [NotNull] Expression projection,
            [NotNull] Expression innerShaper,
            [CanBeNull] INavigation navigation,
            [CanBeNull] Type elementType)
        {
            Check.NotNull(projection, nameof(projection));
            Check.NotNull(innerShaper, nameof(innerShaper));

            Projection = projection;
            InnerShaper = innerShaper;
            Navigation = navigation;
            ElementType = elementType ?? navigation.ClrType.TryGetSequenceType();
        }

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            Check.NotNull(visitor, nameof(visitor));

            var projection = visitor.Visit(Projection);
            var innerShaper = visitor.Visit(InnerShaper);

            return Update(projection, innerShaper);
        }

        public virtual CollectionShaperExpression Update(
            [NotNull] Expression projection,
            [NotNull] Expression innerShaper)
        {
            Check.NotNull(projection, nameof(projection));
            Check.NotNull(innerShaper, nameof(innerShaper));

            return projection != Projection || innerShaper != InnerShaper
                ? new CollectionShaperExpression(projection, innerShaper, Navigation, ElementType)
                : this;
        }

        public sealed override ExpressionType NodeType => ExpressionType.Extension;
        public override Type Type => Navigation?.ClrType ?? typeof(List<>).MakeGenericType(ElementType);

        public virtual Expression Projection { get; }
        public virtual Expression InnerShaper { get; }
        public virtual INavigation Navigation { get; }
        public virtual Type ElementType { get; }

        public virtual void Print(ExpressionPrinter expressionPrinter)
        {
            Check.NotNull(expressionPrinter, nameof(expressionPrinter));

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
