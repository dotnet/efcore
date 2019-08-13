// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Query.SqlExpressions
{
    public class LikeExpression : SqlExpression
    {
        public LikeExpression(SqlExpression match, SqlExpression pattern, SqlExpression escapeChar, RelationalTypeMapping typeMapping)
            : base(typeof(bool), typeMapping)
        {
            Match = match;
            Pattern = pattern;
            EscapeChar = escapeChar;
        }

        public virtual SqlExpression Match { get; }
        public virtual SqlExpression Pattern { get; }
        public virtual SqlExpression EscapeChar { get; }

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            var match = (SqlExpression)visitor.Visit(Match);
            var pattern = (SqlExpression)visitor.Visit(Pattern);
            var escapeChar = (SqlExpression)visitor.Visit(EscapeChar);

            return Update(match, pattern, escapeChar);
        }

        public virtual LikeExpression Update(SqlExpression match, SqlExpression pattern, SqlExpression escapeChar)
            => match != Match || pattern != Pattern || escapeChar != EscapeChar
                ? new LikeExpression(match, pattern, escapeChar, TypeMapping)
                : this;

        public override void Print(ExpressionPrinter expressionPrinter)
        {
            expressionPrinter.Visit(Match);
            expressionPrinter.Append(" LIKE ");
            expressionPrinter.Visit(Pattern);

            if (EscapeChar != null)
            {
                expressionPrinter.Append(" ESCAPE ");
                expressionPrinter.Visit(EscapeChar);
            }
        }

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

        public override int GetHashCode() => HashCode.Combine(base.GetHashCode(), Match, Pattern, EscapeChar);
    }
}
