// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query.Sql;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query.Expressions
{
    /// <summary>
    ///     Represents a SQL LIKE expression.
    /// </summary>
    public class LikeExpression : Expression
    {
        /// <summary>
        ///     Creates a new instance of LikeExpression.
        /// </summary>
        /// <param name="match"> The expression to match. </param>
        /// <param name="pattern"> The pattern to match. </param>
        public LikeExpression([NotNull] Expression match, [NotNull] Expression pattern)
        {
            Check.NotNull(match, nameof(match));
            Check.NotNull(pattern, nameof(pattern));

            Match = match;
            Pattern = pattern;
        }

        /// <summary>
        ///     Gets the match expression.
        /// </summary>
        /// <value>
        ///     The match expression.
        /// </value>
        public virtual Expression Match { get; }

        /// <summary>
        ///     Gets the pattern to match.
        /// </summary>
        /// <value>
        ///     The pattern to match.
        /// </value>
        public virtual Expression Pattern { get; }

        /// <summary>
        ///     Returns the node type of this <see cref="Expression" />. (Inherited from <see cref="Expression" />.)
        /// </summary>
        /// <returns>The <see cref="ExpressionType" /> that represents this expression.</returns>
        public override ExpressionType NodeType => ExpressionType.Extension;

        /// <summary>
        ///     Gets the static type of the expression that this <see cref="Expression" /> represents. (Inherited from <see cref="Expression" />.)
        /// </summary>
        /// <returns>The <see cref="Type" /> that represents the static type of the expression.</returns>
        public override Type Type => typeof(bool);

        /// <summary>
        ///     Dispatches to the specific visit method for this node type.
        /// </summary>
        protected override Expression Accept(ExpressionVisitor visitor)
        {
            Check.NotNull(visitor, nameof(visitor));

            var specificVisitor = visitor as ISqlExpressionVisitor;

            return specificVisitor != null
                ? specificVisitor.VisitLike(this)
                : base.Accept(visitor);
        }

        /// <summary>
        ///     Reduces the node and then calls the <see cref="ExpressionVisitor.Visit(System.Linq.Expressions.Expression)" /> method passing the
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
            var newMatchExpression = visitor.Visit(Match);
            var newPatternExpression = visitor.Visit(Pattern);

            return (newMatchExpression != Match)
                   || (newPatternExpression != Pattern)
                ? new LikeExpression(newMatchExpression, newPatternExpression)
                : this;
        }

        /// <summary>
        ///     Creates a <see cref="string" /> representation of the Expression.
        /// </summary>
        /// <returns>A <see cref="string" /> representation of the Expression.</returns>
        public override string ToString() => Match + " LIKE " + Pattern;
    }
}
