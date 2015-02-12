// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Parsing;

namespace Microsoft.Data.Entity.Relational.Query.Expressions
{
    public abstract class ColumnAggregateExpression : ExtensionExpression
    {
        protected ColumnAggregateExpression([NotNull] ColumnExpression columnExpression)
            : base(Check.NotNull(columnExpression, "columnExpression").Type)
        {
            ColumnExpression = columnExpression;
        }

        public virtual ColumnExpression ColumnExpression { get; }

        protected override Expression VisitChildren(ExpressionTreeVisitor visitor)
        {
            return this;
        }
    }
}
