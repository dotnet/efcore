// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class CollectionInitializingExpression : Expression, IPrintable
    {
        public CollectionInitializingExpression(
            int collectionId, Expression parent, Expression parentIdentifier, Expression outerIdentifier, INavigation navigation, Type type)
        {
            CollectionId = collectionId;
            Parent = parent;
            ParentIdentifier = parentIdentifier;
            OuterIdentifier = outerIdentifier;
            Navigation = navigation;
            Type = type;
        }

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            var parent = visitor.Visit(Parent);
            var parentIdentifier = visitor.Visit(ParentIdentifier);
            var outerIdentifier = visitor.Visit(OuterIdentifier);

            return parent != Parent || parentIdentifier != ParentIdentifier || outerIdentifier != OuterIdentifier
                ? new CollectionInitializingExpression(CollectionId, parent, parentIdentifier, outerIdentifier, Navigation, Type)
                : this;
        }

        public virtual void Print(ExpressionPrinter expressionPrinter)
        {
            expressionPrinter.StringBuilder.AppendLine("InitializeCollection:");
            using (expressionPrinter.StringBuilder.Indent())
            {
                expressionPrinter.StringBuilder.AppendLine($"CollectionId: {CollectionId}");
                expressionPrinter.StringBuilder.AppendLine($"Navigation: {Navigation?.Name}");
                expressionPrinter.StringBuilder.Append("Parent:");
                expressionPrinter.Visit(Parent);
                expressionPrinter.StringBuilder.AppendLine();
                expressionPrinter.StringBuilder.Append("ParentIdentifier:");
                expressionPrinter.Visit(ParentIdentifier);
                expressionPrinter.StringBuilder.AppendLine();
                expressionPrinter.StringBuilder.Append("OuterIdentifier:");
                expressionPrinter.Visit(OuterIdentifier);
                expressionPrinter.StringBuilder.AppendLine();
            }
        }

        public override Type Type { get; }

        public override ExpressionType NodeType => ExpressionType.Extension;

        public virtual int CollectionId { get; }
        public virtual Expression Parent { get; }
        public virtual Expression ParentIdentifier { get; }
        public virtual Expression OuterIdentifier { get; }
        public virtual INavigation Navigation { get; }
    }
}
