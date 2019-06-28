// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query.Expressions.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;

namespace Microsoft.EntityFrameworkCore.Relational.Query.Pipeline
{
    public class RelationalCollectionShaperExpression : Expression, IPrintable
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

        public int CollectionId { get; }
        public Expression ParentIdentifier { get; }
        public Expression OuterIdentifier { get; }
        public Expression SelfIdentifier { get; }
        public Expression InnerShaper { get; }
        public INavigation Navigation { get; }
        public Type ElementType { get; }

        public override Type Type => Navigation?.ClrType ?? typeof(List<>).MakeGenericType(ElementType);
        public override ExpressionType NodeType => ExpressionType.Extension;

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            var parentIdentifier = visitor.Visit(ParentIdentifier);
            var outerIdentifier = visitor.Visit(OuterIdentifier);
            var selfIdentifier = visitor.Visit(SelfIdentifier);
            var innerShaper = visitor.Visit(InnerShaper);

            return Update(parentIdentifier, outerIdentifier, selfIdentifier, innerShaper);
        }

        public RelationalCollectionShaperExpression Update(
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

        public void Print(ExpressionPrinter expressionPrinter)
        {
            expressionPrinter.StringBuilder.AppendLine("RelationalCollectionShaper:");
            using (expressionPrinter.StringBuilder.Indent())
            {
                expressionPrinter.StringBuilder.AppendLine($"CollectionId: {CollectionId}");
                expressionPrinter.StringBuilder.Append("ParentIdentifier:");
                expressionPrinter.Visit(ParentIdentifier);
                expressionPrinter.StringBuilder.AppendLine();
                expressionPrinter.StringBuilder.Append("OuterIdentifier:");
                expressionPrinter.Visit(OuterIdentifier);
                expressionPrinter.StringBuilder.AppendLine();
                expressionPrinter.StringBuilder.Append("SelfIdentifier:");
                expressionPrinter.Visit(SelfIdentifier);
                expressionPrinter.StringBuilder.AppendLine();
                expressionPrinter.StringBuilder.Append("InnerShaper:");
                expressionPrinter.Visit(InnerShaper);
                expressionPrinter.StringBuilder.AppendLine();
                expressionPrinter.StringBuilder.AppendLine($"Navigation: {Navigation?.Name}");

            }
        }
    }
}
