// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.XuGu.Query.ExpressionVisitors.Internal;

namespace Microsoft.EntityFrameworkCore.XuGu.Query.Expressions.Internal
{
    public class XGRegexpExpression : SqlExpression
    {
        private static ConstructorInfo _quotingConstructor;

        public XGRegexpExpression(
            [NotNull] SqlExpression match,
            [NotNull] SqlExpression pattern,
            RelationalTypeMapping typeMapping)
            : base(typeof(bool), typeMapping)
        {
            Check.NotNull(match, nameof(match));
            Check.NotNull(pattern, nameof(pattern));

            Match = match;
            Pattern = pattern;
        }

        public virtual SqlExpression Match { get; }
        public virtual SqlExpression Pattern { get; }

        protected override Expression Accept(ExpressionVisitor visitor)
        {
            Check.NotNull(visitor, nameof(visitor));

            return visitor is XGQuerySqlGenerator xgQuerySqlGenerator // TODO: Move to VisitExtensions
                ? xgQuerySqlGenerator.VisitXGRegexp(this)
                : base.Accept(visitor);
        }

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            var match = (SqlExpression)visitor.Visit(Match);
            var pattern = (SqlExpression)visitor.Visit(Pattern);

            return Update(match, pattern);
        }

        /// <inheritdoc />
        public override Expression Quote()
            => New(
                _quotingConstructor ??= typeof(XGInlinedParameterExpression).GetConstructor(
                    [typeof(SqlExpression), typeof(SqlExpression), typeof(RelationalTypeMapping)])!,
                Match.Quote(),
                Pattern.Quote(),
                RelationalExpressionQuotingUtilities.QuoteTypeMapping(TypeMapping));

        public virtual XGRegexpExpression Update(SqlExpression match, SqlExpression pattern)
            => match != Match ||
               pattern != Pattern
                ? new XGRegexpExpression(match, pattern, TypeMapping)
                : this;

        public override bool Equals(object obj)
        {
            if (obj is null)
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            return obj.GetType() == GetType() && Equals((XGRegexpExpression)obj);
        }

        private bool Equals(XGRegexpExpression other)
            => Equals(Match, other.Match)
               && Equals(Pattern, other.Pattern);

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Match.GetHashCode();
                hashCode = (hashCode * 397) ^ Pattern.GetHashCode();

                return hashCode;
            }
        }

        public override string ToString() => $"{Match} REGEXP {Pattern}";

        protected override void Print(ExpressionPrinter expressionPrinter)
            => expressionPrinter.Append(ToString());
    }
}
