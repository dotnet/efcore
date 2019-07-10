// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query.Expressions.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;

namespace Microsoft.EntityFrameworkCore.Query.Pipeline
{
    public class GroupByShaperExpression : Expression, IPrintable
    {
        public GroupByShaperExpression(Expression keySelector, Expression elementSelector)
        {
            KeySelector = keySelector;
            ElementSelector = elementSelector;
        }

        public Expression KeySelector { get; }
        public Expression ElementSelector { get; }

        public override Type Type => typeof(IGrouping<,>).MakeGenericType(KeySelector.Type, ElementSelector.Type);
        public override ExpressionType NodeType => ExpressionType.Extension;

        public void Print(ExpressionPrinter expressionPrinter)
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

        public GroupByShaperExpression Update(Expression keySelector, Expression elementSelector)
            => keySelector != KeySelector || elementSelector != ElementSelector
                ? new GroupByShaperExpression(keySelector, elementSelector)
                : this;
    }
}
