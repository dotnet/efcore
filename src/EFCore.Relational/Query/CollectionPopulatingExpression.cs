// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class CollectionPopulatingExpression : Expression, IPrintable
    {
        public CollectionPopulatingExpression(RelationalCollectionShaperExpression parent, Type type, bool include)
        {
            Parent = parent;
            Type = type;
            IsInclude = include;
        }

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            var parent = (RelationalCollectionShaperExpression)visitor.Visit(Parent);

            return parent != Parent
                ? new CollectionPopulatingExpression(parent, Type, IsInclude)
                : this;
        }

        public virtual void Print(ExpressionPrinter expressionPrinter)
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
        public virtual RelationalCollectionShaperExpression Parent { get; }
        public virtual bool IsInclude { get; }
    }
}
