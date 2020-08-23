// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query.SqlExpressions
{
    /// <summary>
    ///     <para>
    ///         An expression that represents a LIKE in a SQL tree.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    public class LikeExpression : SqlExpression
    {
        /// <summary>
        ///     Creates a new instance of the <see cref="LikeExpression" /> class.
        /// </summary>
        /// <param name="match"> An expression on which LIKE is applied. </param>
        /// <param name="pattern"> A pattern to search. </param>
        /// <param name="escapeChar"> An optional escape character to use in LIKE. </param>
        /// <param name="typeMapping"> The <see cref="RelationalTypeMapping" /> associated with the expression. </param>
        public LikeExpression(
            [NotNull] SqlExpression match,
            [NotNull] SqlExpression pattern,
            [CanBeNull] SqlExpression escapeChar,
            [CanBeNull] RelationalTypeMapping typeMapping)
            : base(typeof(bool), typeMapping)
        {
            Check.NotNull(match, nameof(match));
            Check.NotNull(pattern, nameof(pattern));

            Match = match;
            Pattern = pattern;
            EscapeChar = escapeChar;
        }

        /// <summary>
        ///     The expression on which LIKE is applied.
        /// </summary>
        public virtual SqlExpression Match { get; }

        /// <summary>
        ///     The pattern to search in <see cref="Match" />.
        /// </summary>
        public virtual SqlExpression Pattern { get; }

        /// <summary>
        ///     The escape chater to use in LIKE.
        /// </summary>
        public virtual SqlExpression EscapeChar { get; }

        /// <inheritdoc />
        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            Check.NotNull(visitor, nameof(visitor));

            var match = (SqlExpression)visitor.Visit(Match);
            var pattern = (SqlExpression)visitor.Visit(Pattern);
            var escapeChar = (SqlExpression)visitor.Visit(EscapeChar);

            return Update(match, pattern, escapeChar);
        }

        /// <summary>
        ///     Creates a new expression that is like this one, but using the supplied children. If all of the children are the same, it will
        ///     return this expression.
        /// </summary>
        /// <param name="match"> The <see cref="Match" /> property of the result. </param>
        /// <param name="pattern"> The <see cref="Pattern" /> property of the result. </param>
        /// <param name="escapeChar"> The <see cref="EscapeChar" /> property of the result. </param>
        /// <returns> This expression if no children changed, or an expression with the updated children. </returns>
        public virtual LikeExpression Update(
            [NotNull] SqlExpression match,
            [NotNull] SqlExpression pattern,
            [CanBeNull] SqlExpression escapeChar)
        {
            Check.NotNull(match, nameof(match));
            Check.NotNull(pattern, nameof(pattern));

            return match != Match || pattern != Pattern || escapeChar != EscapeChar
                ? new LikeExpression(match, pattern, escapeChar, TypeMapping)
                : this;
        }

        /// <inheritdoc />
        protected override void Print(ExpressionPrinter expressionPrinter)
        {
            Check.NotNull(expressionPrinter, nameof(expressionPrinter));

            expressionPrinter.Visit(Match);
            expressionPrinter.Append(" LIKE ");
            expressionPrinter.Visit(Pattern);

            if (EscapeChar != null)
            {
                expressionPrinter.Append(" ESCAPE ");
                expressionPrinter.Visit(EscapeChar);
            }
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
            => obj != null
                && (ReferenceEquals(this, obj)
                    || obj is LikeExpression likeExpression
                    && Equals(likeExpression));

        private bool Equals(LikeExpression likeExpression)
            => base.Equals(likeExpression)
                && Match.Equals(likeExpression.Match)
                && Pattern.Equals(likeExpression.Pattern)
                && EscapeChar.Equals(likeExpression.EscapeChar);

        /// <inheritdoc />
        public override int GetHashCode()
            => HashCode.Combine(base.GetHashCode(), Match, Pattern, EscapeChar);
    }
}
