// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query
{
    /// <summary>
    ///     <para>
    ///         An expression that represents creation of a collection in <see cref="ShapedQueryExpression.ShaperExpression" />.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    public class CollectionShaperExpression : Expression, IPrintableExpression
    {
        /// <summary>
        ///     Creates a new instance of the <see cref="CollectionShaperExpression" /> class.
        /// </summary>
        /// <param name="projection"> An expression reprensenting how to get value from query to create the collection. </param>
        /// <param name="innerShaper"> An expression used to create individual elements of the collection. </param>
        /// <param name="navigation"> A navigation associated with this collection, if any. </param>
        /// <param name="elementType"> The clr type of individual elements in the collection. </param>
        public CollectionShaperExpression(
            [NotNull] Expression projection,
            [NotNull] Expression innerShaper,
            [CanBeNull] INavigationBase navigation,
            [CanBeNull] Type elementType)
        {
            Check.NotNull(projection, nameof(projection));
            Check.NotNull(innerShaper, nameof(innerShaper));

            Projection = projection;
            InnerShaper = innerShaper;
            Navigation = navigation;
            ElementType = elementType ?? navigation.ClrType.TryGetSequenceType();
        }

        /// <summary>
        ///     The expression to get value from query for this collection.
        /// </summary>
        public virtual Expression Projection { get; }

        /// <summary>
        ///     The expression to create inner elements.
        /// </summary>
        public virtual Expression InnerShaper { get; }

        /// <summary>
        ///     The navigation if associated with the collection.
        /// </summary>
        public virtual INavigationBase Navigation { get; }

        /// <summary>
        ///     The clr type of elements of the collection.
        /// </summary>
        public virtual Type ElementType { get; }

        /// <inheritdoc />
        public sealed override ExpressionType NodeType
            => ExpressionType.Extension;

        /// <inheritdoc />
        public override Type Type
            => Navigation?.ClrType ?? typeof(List<>).MakeGenericType(ElementType);

        /// <inheritdoc />
        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            Check.NotNull(visitor, nameof(visitor));

            var projection = visitor.Visit(Projection);
            var innerShaper = visitor.Visit(InnerShaper);

            return Update(projection, innerShaper);
        }

        /// <summary>
        ///     Creates a new expression that is like this one, but using the supplied children. If all of the children are the same, it will
        ///     return this expression.
        /// </summary>
        /// <param name="projection"> The <see cref="Projection" /> property of the result. </param>
        /// <param name="innerShaper"> The <see cref="InnerShaper" /> property of the result. </param>
        /// <returns> This expression if no children changed, or an expression with the updated children. </returns>
        public virtual CollectionShaperExpression Update(
            [NotNull] Expression projection,
            [NotNull] Expression innerShaper)
        {
            Check.NotNull(projection, nameof(projection));
            Check.NotNull(innerShaper, nameof(innerShaper));

            return projection != Projection || innerShaper != InnerShaper
                ? new CollectionShaperExpression(projection, innerShaper, Navigation, ElementType)
                : this;
        }

        /// <inheritdoc />
        void IPrintableExpression.Print(ExpressionPrinter expressionPrinter)
        {
            Check.NotNull(expressionPrinter, nameof(expressionPrinter));

            expressionPrinter.AppendLine("CollectionShaper:");
            using (expressionPrinter.Indent())
            {
                expressionPrinter.Append("(");
                expressionPrinter.Visit(Projection);
                expressionPrinter.Append(", ");
                expressionPrinter.Visit(InnerShaper);
                expressionPrinter.AppendLine($", {Navigation?.Name})");
            }
        }
    }
}
