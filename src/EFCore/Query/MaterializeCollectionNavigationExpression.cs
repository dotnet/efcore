// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query
{
    /// <summary>
    ///     <para>
    ///         An expression that represents materialization of a collection navigation in <see cref="ShapedQueryExpression.ShaperExpression" />.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    public class MaterializeCollectionNavigationExpression : Expression, IPrintableExpression
    {
        /// <summary>
        ///     Creates a new instance of the <see cref="CollectionShaperExpression" /> class.
        /// </summary>
        /// <param name="subquery"> An expression reprensenting how to get value from query to create the collection. </param>
        /// <param name="navigation"> A navigation associated with this collection. </param>
        public MaterializeCollectionNavigationExpression(Expression subquery, INavigationBase navigation)
        {
            Check.NotNull(subquery, nameof(subquery));
            Check.NotNull(navigation, nameof(navigation));

            Subquery = subquery;
            Navigation = navigation;
        }

        /// <summary>
        ///     The expression that returns the values from query used to create the collection.
        /// </summary>
        public virtual Expression Subquery { get; }

        /// <summary>
        ///     The navigation associated with this collection.
        /// </summary>
        public virtual INavigationBase Navigation { get; }

        /// <inheritdoc />
        public sealed override ExpressionType NodeType
            => ExpressionType.Extension;

        /// <inheritdoc />
        public override Type Type
            => Navigation.ClrType;

        /// <inheritdoc />
        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            Check.NotNull(visitor, nameof(visitor));

            return Update(visitor.Visit(Subquery));
        }

        /// <summary>
        ///     Creates a new expression that is like this one, but using the supplied children. If all of the children are the same, it will
        ///     return this expression.
        /// </summary>
        /// <param name="subquery"> The <see cref="Subquery" /> property of the result. </param>
        /// <returns> This expression if no children changed, or an expression with the updated children. </returns>
        public virtual MaterializeCollectionNavigationExpression Update(Expression subquery)
        {
            Check.NotNull(subquery, nameof(subquery));

            return subquery != Subquery
                ? new MaterializeCollectionNavigationExpression(subquery, Navigation)
                : this;
        }

        /// <inheritdoc />
        void IPrintableExpression.Print(ExpressionPrinter expressionPrinter)
        {
            Check.NotNull(expressionPrinter, nameof(expressionPrinter));

            expressionPrinter.AppendLine("MaterializeCollectionNavigation(");
            using (expressionPrinter.Indent())
            {
                expressionPrinter.AppendLine($"Navigation: {Navigation.DeclaringEntityType.DisplayName()}.{Navigation.Name},");
                expressionPrinter.Append("subquery: ");
                expressionPrinter.Visit(Subquery);
                expressionPrinter.Append(")");
            }
        }
    }
}
