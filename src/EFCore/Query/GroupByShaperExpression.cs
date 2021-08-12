// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Linq;
using System.Linq.Expressions;
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
        /// <param name="groupingEnumerable"> An expression representing element selector for the grouping element. </param>
        public GroupByShaperExpression(
            Expression keySelector,
            ShapedQueryExpression groupingEnumerable)
        {
            Check.NotNull(keySelector, nameof(keySelector));
            Check.NotNull(groupingEnumerable, nameof(groupingEnumerable));

            KeySelector = keySelector;
            GroupingEnumerable = groupingEnumerable;
        }

        /// <summary>
        ///     The expression representing the key selector for this grouping element.
        /// </summary>
        public virtual Expression KeySelector { get; }

        /// <summary>
        ///     The expression representing the element selector for this grouping element.
        /// </summary>
        public virtual ShapedQueryExpression GroupingEnumerable { get; }

        /// <inheritdoc />
        public override Type Type
            => typeof(IGrouping<,>).MakeGenericType(KeySelector.Type, GroupingEnumerable.ShaperExpression.Type);

        /// <inheritdoc />
        public sealed override ExpressionType NodeType
            => ExpressionType.Extension;

        /// <inheritdoc />
        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            Check.NotNull(visitor, nameof(visitor));

            var keySelector = visitor.Visit(KeySelector);
            var groupingEnumerable = (ShapedQueryExpression)visitor.Visit(GroupingEnumerable);

            return Update(keySelector, groupingEnumerable);
        }

        /// <summary>
        ///     Creates a new expression that is like this one, but using the supplied children. If all of the children are the same, it will
        ///     return this expression.
        /// </summary>
        /// <param name="keySelector"> The <see cref="KeySelector" /> property of the result. </param>
        /// <param name="groupingEnumerable"> The <see cref="GroupingEnumerable" /> property of the result. </param>
        /// <returns> This expression if no children changed, or an expression with the updated children. </returns>
        public virtual GroupByShaperExpression Update(Expression keySelector, ShapedQueryExpression groupingEnumerable)
        {
            Check.NotNull(keySelector, nameof(keySelector));
            Check.NotNull(groupingEnumerable, nameof(groupingEnumerable));

            return keySelector != KeySelector || groupingEnumerable != GroupingEnumerable
                ? new GroupByShaperExpression(keySelector, groupingEnumerable)
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
            expressionPrinter.Append("GroupingEnumerable:");
            expressionPrinter.Visit(GroupingEnumerable);
            expressionPrinter.AppendLine();
        }
    }
}
