// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Parsing;

namespace Microsoft.Data.Entity.Relational.Query.Expressions
{
    public abstract class AggregateExpression : ExtensionExpression
    {
        protected AggregateExpression([NotNull] Expression expression)
            : base(Check.NotNull(expression, nameof(expression)).Type)
        {
            Expression = expression;
        }

        public virtual Expression Expression { get; }

        protected override Expression VisitChildren(ExpressionTreeVisitor visitor) => this;
    }
}
