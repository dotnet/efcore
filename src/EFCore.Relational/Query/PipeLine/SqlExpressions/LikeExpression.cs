// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Relational.Query.Pipeline.SqlExpressions
{
    public class LikeExpression : SqlExpression
    {
        public LikeExpression(SqlExpression match, SqlExpression pattern, SqlExpression escapeChar, RelationalTypeMapping typeMapping)
            : base(typeof(bool), typeMapping, true, false)
        {
            Match = match.ConvertToValue(true);
            Pattern = pattern.ConvertToValue(true);
            EscapeChar = escapeChar?.ConvertToValue(true);
        }

        private LikeExpression(
            SqlExpression match, SqlExpression pattern, SqlExpression escapeChar, RelationalTypeMapping typeMapping, bool treatAsValue)
            : base(typeof(bool), typeMapping, true, treatAsValue)
        {
            Match = match;
            Pattern = pattern;
            EscapeChar = escapeChar;
        }

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            var match = (SqlExpression)visitor.Visit(Match);
            var pattern = (SqlExpression)visitor.Visit(Pattern);
            var escapeChar = (SqlExpression)visitor.Visit(EscapeChar);

            return match != Match || pattern != Pattern || escapeChar != EscapeChar
                ? new LikeExpression(match, pattern, escapeChar, TypeMapping, ShouldBeValue)
                : this;
        }

        public override SqlExpression ConvertToValue(bool treatAsValue)
        {
            return new LikeExpression(Match, Pattern, EscapeChar, TypeMapping, treatAsValue);
        }

        public SqlExpression Match { get; }
        public SqlExpression Pattern { get; }
        public SqlExpression EscapeChar { get; }

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

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ Match.GetHashCode();
                hashCode = (hashCode * 397) ^ Pattern.GetHashCode();
                hashCode = (hashCode * 397) ^ EscapeChar.GetHashCode();

                return hashCode;
            }
        }
    }
}
