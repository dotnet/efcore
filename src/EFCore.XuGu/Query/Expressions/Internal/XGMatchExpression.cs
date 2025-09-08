// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.EntityFrameworkCore.XuGu.Query.ExpressionVisitors.Internal;

namespace Microsoft.EntityFrameworkCore.XuGu.Query.Expressions.Internal
{
    public class XGMatchExpression : SqlExpression
    {
        private static ConstructorInfo _quotingConstructor;

        public XGMatchExpression(
            SqlExpression match,
            SqlExpression against,
            XGMatchSearchMode searchMode,
            RelationalTypeMapping typeMapping)
            : base(typeof(double), typeMapping)
        {
            Check.NotNull(match, nameof(match));
            Check.NotNull(against, nameof(against));

            Match = match;
            Against = against;
            SearchMode = searchMode;
        }

        public virtual XGMatchSearchMode SearchMode { get; }

        public virtual SqlExpression Match { get; }
        public virtual SqlExpression Against { get; }

        protected override Expression Accept(ExpressionVisitor visitor)
        {
            Check.NotNull(visitor, nameof(visitor));

            return visitor is XGQuerySqlGenerator xgQuerySqlGenerator // TODO: Move to VisitExtensions
                ? xgQuerySqlGenerator.VisitXGMatch(this)
                : base.Accept(visitor);
        }

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            var match = (SqlExpression)visitor.Visit(Match);
            var against = (SqlExpression)visitor.Visit(Against);

            return Update(match, against);
        }

        /// <inheritdoc />
        public override Expression Quote()
            => New(
                _quotingConstructor ??= typeof(XGInlinedParameterExpression).GetConstructor(
                    [typeof(SqlExpression), typeof(SqlExpression), typeof(XGMatchSearchMode), typeof(RelationalTypeMapping)])!,
                Match.Quote(),
                Against.Quote(),
                Constant(SearchMode),
                RelationalExpressionQuotingUtilities.QuoteTypeMapping(TypeMapping));

        public virtual XGMatchExpression Update(SqlExpression match, SqlExpression against)
            => match != Match || against != Against
                ? new XGMatchExpression(
                    match,
                    against,
                    SearchMode,
                    TypeMapping)
                : this;

        public override bool Equals(object obj)
            => obj != null && ReferenceEquals(this, obj)
            || obj is XGMatchExpression matchExpression && Equals(matchExpression);

        private bool Equals(XGMatchExpression matchExpression)
            => base.Equals(matchExpression)
            && SearchMode == matchExpression.SearchMode
            && Match.Equals(matchExpression.Match)
            && Against.Equals(matchExpression.Against);

        public override int GetHashCode() => HashCode.Combine(base.GetHashCode(), SearchMode, Match, Against);

        protected override void Print(ExpressionPrinter expressionPrinter)
        {
            expressionPrinter.Append("MATCH ");
            expressionPrinter.Append($"({expressionPrinter.Visit(Match)})");
            expressionPrinter.Append(" AGAINST ");
            expressionPrinter.Append($"({expressionPrinter.Visit(Against)}");

            switch (SearchMode)
            {
                case XGMatchSearchMode.NaturalLanguage:
                    break;
                case XGMatchSearchMode.NaturalLanguageWithQueryExpansion:
                    expressionPrinter.Append(" WITH QUERY EXPANSION");
                    break;
                case XGMatchSearchMode.Boolean:
                    expressionPrinter.Append(" IN BOOLEAN MODE");
                    break;
            }

            expressionPrinter.Append(")");
        }
    }
}
