// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Relational.Query.Sql;
using Microsoft.Data.Entity.Utilities;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Parsing;

namespace Microsoft.Data.Entity.Relational.Query.Expressions
{
    public class LikeExpression : ExtensionExpression
    {
        public LikeExpression([NotNull] Expression match, [NotNull] Expression pattern)
            : base(typeof(bool))
        {
            Check.NotNull(match, nameof(match));
            Check.NotNull(pattern, nameof(pattern));

            Match = match;
            Pattern = pattern;
        }

        public virtual Expression Match { get; }

        public virtual Expression Pattern { get; }

        public override Expression Accept([NotNull] ExpressionTreeVisitor visitor)
        {
            Check.NotNull(visitor, nameof(visitor));

            var specificVisitor = visitor as ISqlExpressionVisitor;

            return specificVisitor != null 
                ? specificVisitor.VisitLikeExpression(this) 
                : base.Accept(visitor);
        }

        protected override Expression VisitChildren(ExpressionTreeVisitor visitor)
        {
            var newMatchExpression = visitor.VisitExpression(Match);
            var newPatternExpression = visitor.VisitExpression(Pattern);

            return newMatchExpression != Match
                   || newPatternExpression != Pattern
                ? new LikeExpression(newMatchExpression, newPatternExpression)
                : this;
        }

        public override string ToString() => Match + " LIKE " + Pattern;
    }
}
