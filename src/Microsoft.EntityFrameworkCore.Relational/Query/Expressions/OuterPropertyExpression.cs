// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Utilities;
using Remotion.Linq.Clauses;
using Microsoft.EntityFrameworkCore.Query.Sql;

namespace Microsoft.EntityFrameworkCore.Query.Expressions
{
    /// <summary>
    ///     An expression that represents an AliasExpression from an outer query
    ///     but may later be bound to a PropertyParameterExpression instead.
    /// </summary>
    public class OuterPropertyExpression : Expression
    {
        /// <summary>
        ///     Creates a new instance of a ColumnExpression.
        /// </summary>
        /// <param name="sourceExpression"> The column name. </param>
        /// <param name="property"> The corresponding property. </param>
        /// <param name="querySource"> The target table expression. </param>
        /// <param name="aliasExpression"> The target table expression. </param>
        public OuterPropertyExpression(
            [NotNull] Expression sourceExpression,
            [NotNull] IProperty property,
            [NotNull] IQuerySource querySource,
            [NotNull] AliasExpression aliasExpression)
            : this(sourceExpression, property, querySource)
        {
            BoundExpression = Check.NotNull(aliasExpression, nameof(aliasExpression));
        }

        private OuterPropertyExpression(
            [NotNull] Expression sourceExpression,
            [NotNull] IProperty property,
            [NotNull] IQuerySource querySource)
        {
            SourceExpression = Check.NotNull(sourceExpression, nameof(sourceExpression));
            Property = Check.NotNull(property, nameof(property));
            QuerySource = Check.NotNull(querySource, nameof(querySource));
        }

        /// <summary>
        ///     The target table.
        /// </summary>
        public virtual Expression SourceExpression { get; }

#pragma warning disable 108

        /// <summary>
        ///     The corresponding property.
        /// </summary>
        public virtual IProperty Property { get; }

#pragma warning restore 108

        /// <summary>
        ///     The target table alias.
        /// </summary>
        public virtual IQuerySource QuerySource { get; }

        /// <summary>
        ///     Gets the column name.
        /// </summary>
        /// <value>
        ///     The column name.
        /// </value>
        public virtual Expression BoundExpression { get; private set; }

        /// <summary>
        ///     Gets a value indicating whether or not this outer property expression
        ///     has been resolved.
        /// </summary>
        public virtual bool Resolved { get; private set; }

        /// <summary>
        ///     Returns the node type of this <see cref="Expression" />. (Inherited from <see cref="Expression" />.)
        /// </summary>
        /// <returns> The <see cref="ExpressionType" /> that represents this expression. </returns>
        public override ExpressionType NodeType => ExpressionType.Extension;

        /// <summary>
        ///     Gets the static type of the expression that this <see cref="Expression" /> represents. (Inherited from <see cref="Expression" />.)
        /// </summary>
        /// <returns> The <see cref="Type" /> that represents the static type of the expression. </returns>
        public override Type Type => Property.ClrType;

        /// <summary>
        ///     Resolves this outer property expression to its current bound expression.
        /// </summary>
        public virtual void Resolve()
        {
            Resolved = true;
        }

        /// <summary>
        ///     Resolves this outer property expression to a new bound expression.
        /// </summary>
        /// <param name="boundExpression"> The property parameter. </param>
        public virtual void Resolve([NotNull] Expression boundExpression)
        {
            Check.NotNull(boundExpression, nameof(boundExpression));

            BoundExpression = boundExpression;
            Resolved = true;
        }

        /// <summary>
        ///     Dispatches to the specific visit method for this node type.
        /// </summary>
        protected override Expression Accept(ExpressionVisitor visitor)
        {
            Check.NotNull(visitor, nameof(visitor));

            if (Resolved)
            {
                var newBoundExpression = visitor.Visit(BoundExpression);

                if (newBoundExpression != BoundExpression)
                {
                    var newOuterPropertyExpression
                        = new OuterPropertyExpression(
                            SourceExpression,
                            Property,
                            QuerySource);

                    newOuterPropertyExpression.Resolve(newBoundExpression);

                    return newOuterPropertyExpression;
                }

                return this;
            }

            if (visitor is ISqlExpressionVisitor)
            {
                return visitor.Visit(BoundExpression);
            }

            return base.Accept(visitor);
        }

        /// <summary>
        ///     Reduces the node and then calls the <see cref="ExpressionVisitor.Visit(Expression)" /> method passing the
        ///     reduced expression.
        ///     Throws an exception if the node isn't reducible.
        /// </summary>
        /// <param name="visitor"> An instance of <see cref="ExpressionVisitor" />. </param>
        /// <returns> The expression being visited, or an expression which should replace it in the tree. </returns>
        /// <remarks>
        ///     Override this method to provide logic to walk the node's children.
        ///     A typical implementation will call visitor.Visit on each of its
        ///     children, and if any of them change, should return a new copy of
        ///     itself with the modified children.
        /// </remarks>
        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            Check.NotNull(visitor, nameof(visitor));

            visitor.Visit(BoundExpression);

            return this;
        }

        /// <summary>
        ///     Creates a <see cref="string" /> representation of the Expression.
        /// </summary>
        /// <returns>A <see cref="string" /> representation of the Expression.</returns>
        public override string ToString()
        {
            if (!Resolved)
            {
                return $"Unresolved({BoundExpression})";
            }

            return BoundExpression.ToString();
        }
    }
}
