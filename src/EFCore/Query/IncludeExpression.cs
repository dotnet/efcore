// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query
{
    /// <summary>
    ///     <para>
    ///         An expression that represents include operation in <see cref="ShapedQueryExpression.ShaperExpression" />.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    public class IncludeExpression : Expression, IPrintableExpression
    {
        /// <summary>
        ///     Creates a new instance of the <see cref="IncludeExpression" /> class.
        /// </summary>
        /// <param name="entityExpression"> An expression to get entity which is performing include. </param>
        /// <param name="navigationExpression"> An expression to get included navigation element. </param>
        /// <param name="navigation"> The navigation for this include operation. </param>
        public IncludeExpression(
            [NotNull] Expression entityExpression,
            [NotNull] Expression navigationExpression,
            [NotNull] INavigationBase navigation)
        {
            Check.NotNull(entityExpression, nameof(entityExpression));
            Check.NotNull(navigationExpression, nameof(navigationExpression));
            Check.NotNull(navigation, nameof(navigation));

            EntityExpression = entityExpression;
            NavigationExpression = navigationExpression;
            Navigation = navigation;
            Type = EntityExpression.Type;
        }

        /// <summary>
        ///     The expression representing entity perfoming this include.
        /// </summary>
        public virtual Expression EntityExpression { get; }

        /// <summary>
        ///     The expression representing included navigation element.
        /// </summary>
        public virtual Expression NavigationExpression { get; }

        /// <summary>
        ///     The navigation associated with this include operation.
        /// </summary>
        public virtual INavigationBase Navigation { get; }

        /// <inheritdoc />
        public sealed override ExpressionType NodeType
            => ExpressionType.Extension;

        /// <inheritdoc />
        public override Type Type { get; }

        /// <inheritdoc />
        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            Check.NotNull(visitor, nameof(visitor));

            var newEntityExpression = visitor.Visit(EntityExpression);
            var newNavigationExpression = visitor.Visit(NavigationExpression);

            return Update(newEntityExpression, newNavigationExpression);
        }

        /// <summary>
        ///     Creates a new expression that is like this one, but using the supplied children. If all of the children are the same, it will
        ///     return this expression.
        /// </summary>
        /// <param name="entityExpression"> The <see cref="EntityExpression" /> property of the result. </param>
        /// <param name="navigationExpression"> The <see cref="NavigationExpression" /> property of the result. </param>
        /// <returns> This expression if no children changed, or an expression with the updated children. </returns>
        public virtual IncludeExpression Update([NotNull] Expression entityExpression, [NotNull] Expression navigationExpression)
        {
            Check.NotNull(entityExpression, nameof(entityExpression));
            Check.NotNull(navigationExpression, nameof(navigationExpression));

            return entityExpression != EntityExpression || navigationExpression != NavigationExpression
                ? new IncludeExpression(entityExpression, navigationExpression, Navigation)
                : this;
        }

        /// <inheritdoc />
        void IPrintableExpression.Print(ExpressionPrinter expressionPrinter)
        {
            Check.NotNull(expressionPrinter, nameof(expressionPrinter));

            expressionPrinter.AppendLine("IncludeExpression(");
            using (expressionPrinter.Indent())
            {
                expressionPrinter.Visit(EntityExpression);
                expressionPrinter.AppendLine(", ");
                expressionPrinter.Visit(NavigationExpression);
                expressionPrinter.AppendLine($", {Navigation.Name})");
            }
        }
    }
}
