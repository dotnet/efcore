// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Parsing;

namespace Microsoft.Data.Entity.Relational.Query.Expressions
{
    public class DiscriminatorPredicateExpression : ExtensionExpression
    {
        private readonly Expression _predicate;

        public DiscriminatorPredicateExpression(
            [NotNull] Expression predicate, [CanBeNull] IQuerySource querySource)
            : base(predicate.Type)
        {
            Check.NotNull(predicate, nameof(predicate));

            _predicate = predicate;

            QuerySource = querySource;
        }

        public virtual IQuerySource QuerySource { get; }

        public override bool CanReduce => true;

        public override Expression Reduce()
        {
            return _predicate;
        }

        public override string ToString()
        {
            return _predicate.ToString();
        }

        protected override Expression VisitChildren(ExpressionTreeVisitor visitor)
        {
            return this;
        }
    }
}
