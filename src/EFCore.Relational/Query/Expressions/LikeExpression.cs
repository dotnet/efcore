// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query.Internal;
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
        ///     Creates a new instance of LikeExpression.
        /// </summary>
        /// <param name="match"> The expression to match. </param>
        /// <param name="pattern"> The pattern to match. </param>
        /// <param name="escapeChar"> The escape character to use in <paramref name="pattern" />. </param>
        public LikeExpression([NotNull] Expression match, [NotNull] Expression pattern, [CanBeNull] Expression escapeChar)
        {
            Check.NotNull(match, nameof(match));
            Check.NotNull(pattern, nameof(pattern));

            Match = match;
            Pattern = pattern;
            EscapeChar = escapeChar;
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
        ///     Gets the escape character to use in <see cref="Pattern" />.
        /// </summary>
        /// <value>
        ///     The escape character to use. If null, no escape character is used.
        /// </value>
        [CanBeNull]
        public virtual Expression EscapeChar { get; }

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

            return visitor is ISqlExpressionVisitor specificVisitor
                ? specificVisitor.VisitLike(this)
                : base.Accept(visitor);
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
            var newMatchExpression = visitor.Visit(Match);
            var newPatternExpression = visitor.Visit(Pattern);
            var newEscapeCharExpression = EscapeChar == null ? null : visitor.Visit(EscapeChar);

            return newMatchExpression != Match
                   || newPatternExpression != Pattern
                   || newEscapeCharExpression != EscapeChar
                ? new LikeExpression(newMatchExpression, newPatternExpression, newEscapeCharExpression)
                : this;
        }

        /// <summary>
        ///     Tests if this object is considered equal to another.
        /// </summary>
        /// <param name="obj"> The object to compare with the current object. </param>
        /// <returns>
        ///     true if the objects are considered equal, false if they are not.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (obj is null)
            {
                return false;
            }

            return ReferenceEquals(this, obj) ? true : obj.GetType() == GetType() && Equals((LikeExpression)obj);
        }
        private bool Equals(LikeExpression other)
            => ExpressionEqualityComparer.Instance.Equals(Match, other.Match)
               && ExpressionEqualityComparer.Instance.Equals(Pattern, other.Pattern)
               && ExpressionEqualityComparer.Instance.Equals(EscapeChar, other.EscapeChar);

        /// <summary>
        ///     Returns a hash code for this object.
        /// </summary>
        /// <returns>
        ///     A hash code for this object.
        /// </returns>
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Match.GetHashCode();
                hashCode = (hashCode * 397) ^ Pattern.GetHashCode();
                hashCode = (hashCode * 397) ^ (EscapeChar?.GetHashCode() ?? 0);

                return hashCode;
            }
        }

        /// <summary>
        ///     Creates a <see cref="string" /> representation of the Expression.
        /// </summary>
        /// <returns>A <see cref="string" /> representation of the Expression.</returns>
        public override string ToString() => $"{Match} LIKE {Pattern}{(EscapeChar == null ? "" : $" ESCAPE {EscapeChar}")}";
    }
}
