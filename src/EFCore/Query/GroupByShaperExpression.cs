// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Linq.Expressions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class GroupByShaperExpression : Expression, IPrintable
    {
        public GroupByShaperExpression(Expression keySelector, Expression elementSelector)
        {
            KeySelector = keySelector;
            ElementSelector = elementSelector;
        }

        public virtual Expression KeySelector { get; }
        public virtual Expression ElementSelector { get; }

        public override Type Type => typeof(IGrouping<,>).MakeGenericType(KeySelector.Type, ElementSelector.Type);
        public override ExpressionType NodeType => ExpressionType.Extension;

        public virtual void Print(ExpressionPrinter expressionPrinter)
        {
            expressionPrinter.StringBuilder.AppendLine("GroupBy(");
            expressionPrinter.StringBuilder.Append("KeySelector: ");
            expressionPrinter.Visit(KeySelector);
            expressionPrinter.StringBuilder.AppendLine(", ");
            expressionPrinter.StringBuilder.Append("ElementSelector:");
            expressionPrinter.Visit(ElementSelector);
            expressionPrinter.StringBuilder.AppendLine(")");
        }

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            var keySelector = visitor.Visit(KeySelector);
            var elementSelector = visitor.Visit(ElementSelector);

            return Update(keySelector, elementSelector);
        }

        public virtual GroupByShaperExpression Update(Expression keySelector, Expression elementSelector)
            => keySelector != KeySelector || elementSelector != ElementSelector
                ? new GroupByShaperExpression(keySelector, elementSelector)
                : this;
    }
}
