// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class CollectionPopulatingExpression : Expression, IPrintableExpression
    {
        public CollectionPopulatingExpression([NotNull] RelationalCollectionShaperExpression parent, [NotNull] Type type, bool include)
        {
            Check.NotNull(parent, nameof(parent));
            Check.NotNull(type, nameof(type));

            Parent = parent;
            Type = type;
            IsInclude = include;
        }

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            Check.NotNull(visitor, nameof(visitor));

            var parent = (RelationalCollectionShaperExpression)visitor.Visit(Parent);

            return parent != Parent
                ? new CollectionPopulatingExpression(parent, Type, IsInclude)
                : this;
        }

        public virtual void Print(ExpressionPrinter expressionPrinter)
        {
            Check.NotNull(expressionPrinter, nameof(expressionPrinter));

            expressionPrinter.AppendLine("PopulateCollection:");
            using (expressionPrinter.Indent())
            {
                expressionPrinter.Append("Parent:");
                expressionPrinter.Visit(Parent);
            }
        }

        public override Type Type { get; }

        public sealed override ExpressionType NodeType => ExpressionType.Extension;
        public virtual RelationalCollectionShaperExpression Parent { get; }
        public virtual bool IsInclude { get; }
    }
}
