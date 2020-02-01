// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class GroupByShaperExpression : Expression, IPrintableExpression
    {
        public GroupByShaperExpression(
            [NotNull] Expression keySelector,
            [NotNull] Expression elementSelector)
        {
            Check.NotNull(keySelector, nameof(keySelector));
            Check.NotNull(elementSelector, nameof(elementSelector));

            KeySelector = keySelector;
            ElementSelector = elementSelector;
        }

        public virtual Expression KeySelector { get; }
        public virtual Expression ElementSelector { get; }

        public override Type Type => typeof(IGrouping<,>).MakeGenericType(KeySelector.Type, ElementSelector.Type);
        public sealed override ExpressionType NodeType => ExpressionType.Extension;

        public virtual void Print(ExpressionPrinter expressionPrinter)
        {
            Check.NotNull(expressionPrinter, nameof(expressionPrinter));

            expressionPrinter.AppendLine($"{nameof(GroupByShaperExpression)}:");
            expressionPrinter.Append("KeySelector: ");
            expressionPrinter.Visit(KeySelector);
            expressionPrinter.AppendLine(", ");
            expressionPrinter.Append("ElementSelector:");
            expressionPrinter.Visit(ElementSelector);
            expressionPrinter.AppendLine();
        }

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            Check.NotNull(visitor, nameof(visitor));

            var keySelector = visitor.Visit(KeySelector);
            var elementSelector = visitor.Visit(ElementSelector);

            return Update(keySelector, elementSelector);
        }

        public virtual GroupByShaperExpression Update([NotNull] Expression keySelector, [NotNull] Expression elementSelector)
        {
            Check.NotNull(keySelector, nameof(keySelector));
            Check.NotNull(elementSelector, nameof(elementSelector));

            return keySelector != KeySelector || elementSelector != ElementSelector
                ? new GroupByShaperExpression(keySelector, elementSelector)
                : this;
        }
    }
}
