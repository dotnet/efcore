// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query
{
    /// <summary>
    ///     <para>
    ///         An expression that represents creation of a grouping element in <see cref="ShapedQueryExpression.ShaperExpression" />.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    public class GroupByShaperExpression : Expression, IPrintableExpression
    {
        /// <summary>
        ///     Creates a new instance of the <see cref="GroupByShaperExpression" /> class.
        /// </summary>
        /// <param name="keySelector"> An expression representing key selector for the grouping element. </param>
        /// <param name="elementSelector"> An expression representing element selector for the grouping element. </param>
        public GroupByShaperExpression(
            [NotNull] Expression keySelector,
            [NotNull] Expression elementSelector)
        {
            Check.NotNull(keySelector, nameof(keySelector));
            Check.NotNull(elementSelector, nameof(elementSelector));

            KeySelector = keySelector;
            ElementSelector = elementSelector;
        }

        /// <summary>
        ///     The expression representing the key selector for this grouping element.
        /// </summary>
        public virtual Expression KeySelector { get; }

        /// <summary>
        ///     The expression representing the element selector for this grouping element.
        /// </summary>
        public virtual Expression ElementSelector { get; }

        /// <inheritdoc />
        public override Type Type
            => typeof(IGrouping<,>).MakeGenericType(KeySelector.Type, ElementSelector.Type);

        /// <inheritdoc />
        public sealed override ExpressionType NodeType
            => ExpressionType.Extension;

        /// <inheritdoc />
        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            Check.NotNull(visitor, nameof(visitor));

            var keySelector = visitor.Visit(KeySelector);
            var elementSelector = visitor.Visit(ElementSelector);

            return Update(keySelector, elementSelector);
        }

        /// <summary>
        ///     Creates a new expression that is like this one, but using the supplied children. If all of the children are the same, it will
        ///     return this expression.
        /// </summary>
        /// <param name="keySelector"> The <see cref="KeySelector" /> property of the result. </param>
        /// <param name="elementSelector"> The <see cref="ElementSelector" /> property of the result. </param>
        /// <returns> This expression if no children changed, or an expression with the updated children. </returns>
        public virtual GroupByShaperExpression Update([NotNull] Expression keySelector, [NotNull] Expression elementSelector)
        {
            Check.NotNull(keySelector, nameof(keySelector));
            Check.NotNull(elementSelector, nameof(elementSelector));

            return keySelector != KeySelector || elementSelector != ElementSelector
                ? new GroupByShaperExpression(keySelector, elementSelector)
                : this;
        }

        /// <inheritdoc />
        void IPrintableExpression.Print(ExpressionPrinter expressionPrinter)
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
    }
}
