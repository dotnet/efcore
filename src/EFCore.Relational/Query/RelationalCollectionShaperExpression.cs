// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class RelationalCollectionShaperExpression : Expression, IPrintableExpression
    {
        public RelationalCollectionShaperExpression(
            int collectionId,
            Expression parentIdentifier,
            Expression outerIdentifier,
            Expression selfIdentifier,
            Expression innerShaper,
            INavigation navigation,
            Type elementType)
        {
            CollectionId = collectionId;
            ParentIdentifier = parentIdentifier;
            OuterIdentifier = outerIdentifier;
            SelfIdentifier = selfIdentifier;
            InnerShaper = innerShaper;
            Navigation = navigation;
            ElementType = elementType;
        }

        public virtual int CollectionId { get; }
        public virtual Expression ParentIdentifier { get; }
        public virtual Expression OuterIdentifier { get; }
        public virtual Expression SelfIdentifier { get; }
        public virtual Expression InnerShaper { get; }
        public virtual INavigation Navigation { get; }
        public virtual Type ElementType { get; }

        public override Type Type => Navigation?.ClrType ?? typeof(List<>).MakeGenericType(ElementType);
        public sealed override ExpressionType NodeType => ExpressionType.Extension;

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            var parentIdentifier = visitor.Visit(ParentIdentifier);
            var outerIdentifier = visitor.Visit(OuterIdentifier);
            var selfIdentifier = visitor.Visit(SelfIdentifier);
            var innerShaper = visitor.Visit(InnerShaper);

            return Update(parentIdentifier, outerIdentifier, selfIdentifier, innerShaper);
        }

        public virtual RelationalCollectionShaperExpression Update(
            Expression parentIdentifier, Expression outerIdentifier, Expression selfIdentifier, Expression innerShaper)
        {
            return parentIdentifier != ParentIdentifier
                || outerIdentifier != OuterIdentifier
                || selfIdentifier != SelfIdentifier
                || innerShaper != InnerShaper
                    ? new RelationalCollectionShaperExpression(
                        CollectionId, parentIdentifier, outerIdentifier, selfIdentifier, innerShaper, Navigation, ElementType)
                    : this;
        }

        public virtual void Print(ExpressionPrinter expressionPrinter)
        {
            expressionPrinter.AppendLine("RelationalCollectionShaper:");
            using (expressionPrinter.Indent())
            {
                expressionPrinter.AppendLine($"CollectionId: {CollectionId}");
                expressionPrinter.Append("ParentIdentifier:");
                expressionPrinter.Visit(ParentIdentifier);
                expressionPrinter.AppendLine();
                expressionPrinter.Append("OuterIdentifier:");
                expressionPrinter.Visit(OuterIdentifier);
                expressionPrinter.AppendLine();
                expressionPrinter.Append("SelfIdentifier:");
                expressionPrinter.Visit(SelfIdentifier);
                expressionPrinter.AppendLine();
                expressionPrinter.Append("InnerShaper:");
                expressionPrinter.Visit(InnerShaper);
                expressionPrinter.AppendLine();
                expressionPrinter.AppendLine($"Navigation: {Navigation?.Name}");
            }
        }
    }
}
