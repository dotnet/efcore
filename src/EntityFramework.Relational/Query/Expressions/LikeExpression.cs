// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
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
        private readonly Expression _match;
        private readonly Expression _pattern;

        public LikeExpression([NotNull] Expression match, [NotNull] Expression pattern)
            : base(typeof(bool))
        {
            Check.NotNull(match, nameof(match));
            Check.NotNull(pattern, nameof(pattern));

            _match = match;
            _pattern = pattern;
        }

        public virtual Expression Match
        {
            get { return _match; }
        }

        public virtual Expression Pattern
        {
            get { return _pattern; }
        }

        public override Expression Accept([NotNull] ExpressionTreeVisitor visitor)
        {
            Check.NotNull(visitor, nameof(visitor));

            var specificVisitor = visitor as ISqlExpressionVisitor;

            if (specificVisitor != null)
            {
                return specificVisitor.VisitLikeExpression(this);
            }

            return base.Accept(visitor);
        }

        protected override Expression VisitChildren(ExpressionTreeVisitor visitor)
        {
            var newMatchExpression = visitor.VisitExpression(_match);
            var newPatternExpression = visitor.VisitExpression(_pattern);

            return newMatchExpression != _match
                   || newPatternExpression != _pattern
                ? new LikeExpression(newMatchExpression, newPatternExpression)
                : this;
        }

        public override string ToString()
        {
            return _match + " LIKE " + _pattern;
        }
    }
}
