// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query.Expressions.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;

namespace Microsoft.EntityFrameworkCore.Relational.Query.Pipeline
{
    public class CollectionPopulatingExpression : Expression, IPrintable
    {
        public CollectionPopulatingExpression(RelationalCollectionShaperExpression parent, Type type, bool include)
        {
            Parent = parent;
            Type = type;
            Include = include;
        }

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            var parent = (RelationalCollectionShaperExpression)visitor.Visit(Parent);

            return parent != Parent
                ? new CollectionPopulatingExpression(parent, Type, Include)
                : this;
        }

        public void Print(ExpressionPrinter expressionPrinter)
        {
            expressionPrinter.StringBuilder.AppendLine("PopulateCollection:");
            using (expressionPrinter.StringBuilder.Indent())
            {
                expressionPrinter.StringBuilder.Append("Parent:");
                expressionPrinter.Visit(Parent);
            }
        }

        public override Type Type { get; }

        public override ExpressionType NodeType => ExpressionType.Extension;
        public RelationalCollectionShaperExpression Parent { get; }
        public bool Include { get; }
    }
}
