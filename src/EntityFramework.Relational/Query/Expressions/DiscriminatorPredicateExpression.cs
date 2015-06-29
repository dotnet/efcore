// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;
using Remotion.Linq.Clauses;

namespace Microsoft.Data.Entity.Query.Expressions
{
    public class DiscriminatorPredicateExpression : Expression
    {
        private readonly Expression _predicate;

        public DiscriminatorPredicateExpression(
            [NotNull] Expression predicate, [CanBeNull] IQuerySource querySource)
        {
            Check.NotNull(predicate, nameof(predicate));

            _predicate = predicate;

            QuerySource = querySource;
        }

        public virtual IQuerySource QuerySource { get; }

        public override ExpressionType NodeType => ExpressionType.Extension;

        public override Type Type => _predicate.Type;

        public override bool CanReduce => true;

        public override Expression Reduce() => _predicate;

        public override string ToString() => _predicate.ToString();

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            var newPredicate = visitor.Visit(_predicate);

            return _predicate != newPredicate
                ? new DiscriminatorPredicateExpression(newPredicate, QuerySource)
                : this;
        }
    }
}
